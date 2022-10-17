//#define DEBUG_TASKMGR
//#define DEBUG_MAP
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MiniExcelLibs;

public class TaskMgr
{
    public static TaskMgr Instance => instance;
    protected static TaskMgr instance = new TaskMgr();
    // 任务配置表路径
    protected readonly string m_FilePath = Application.streamingAssetsPath + "/Tasks.xlsx";

    // 全部/当前任务池
    protected List<Dictionary<int, TaskData>> m_AllTasks = new List<Dictionary<int, TaskData>>();
    public List<TaskData> m_CurTasks = new List<TaskData>();
    // 中间地点池
    protected Dictionary<int, MiddlePlace> allMiddlePlaces = new Dictionary<int, MiddlePlace>();

    // 正在产生的任务序号
    protected int m_CurTaskIndex;
    // 最大任务数
    protected const int m_MaxTaskNum = 5;

    //当前未接取/已接取的任务 
    protected Dictionary<BaseBlock,TaskData> CurWaitingTasks = new Dictionary<BaseBlock, TaskData>();
    protected Dictionary<BasePostMan, TaskData> CurPickedTasks = new Dictionary<BasePostMan, TaskData>();

    // 需要访问画布时自动获取场景上的画布对象
    protected GameObject CanvasObj
    {
        get
        {
            if (m_canvas == null)
            {
                // 绑定画布对象与控件
                m_canvas = GameObject.Find("任务画布");
                PickButton = m_canvas.transform.GetChild(0).GetComponent<Button>();
                CancelButton = m_canvas.transform.GetChild(1).GetComponent<Button>();
                Discribtion = m_canvas.transform.GetChild(2).GetComponent<Text>();

                // 初始化按钮功能
                CancelButton.onClick.AddListener(() => Cancel());
            }
            return m_canvas;
        }
        set => m_canvas = value;
    }
    protected GameObject m_canvas;
    // 画布控件
    protected Button PickButton;
    protected Button CancelButton;
    protected Text Discribtion;

    // 任务图标资源
    public GameObject TaskPick;
    public GameObject TaskFinish;

    // 用于清除场景上的图标所记录的对象
    protected List<GameObject> TaskSigns = new List<GameObject>();
    protected List<BaseBlock> Pickblocks = new List<BaseBlock>();
    protected List<GameObject> TaskFinishes = new List<GameObject>();
    protected List<BaseBlock> Finishblocks = new List<BaseBlock>();

    protected TaskMgr()
    {
        // 读取中间地点数据
        foreach (MiddlePlaceRaw raw in MiniExcel.Query<MiddlePlaceRaw>(m_FilePath, sheetName: "MiddlePlace", startCell: "A3"))
        {
            if (raw.Name != null)
                allMiddlePlaces.Add(raw.ID, new MiddlePlace(raw));
        }
        // 读取任务数据
        foreach (TaskDataRaw raw in MiniExcel.Query<TaskDataRaw>(m_FilePath, sheetName: "Task", startCell: "A3"))
        {
            if (raw.StartName != null)
            {
                TaskData data = new TaskData(raw, allMiddlePlaces);
                while (data.AddDay > m_AllTasks.Count)
                    m_AllTasks.Add(new Dictionary<int, TaskData>());
                m_AllTasks[data.AddDay - 1].Add(raw.ID, data);
            }
        }
        // 加载预设体
        const string prefabPath = "TaskPick";
        TaskPick = Resources.Load<GameObject>(prefabPath);
        TaskFinish = Resources.Load<GameObject>(prefabPath);

        // 添加任务事件
        TimeMgr.Instance.dayEndAction += AddTaskToQueue;
        Cancel();
        // 添加第一天的任务
        AddTaskToQueue();
        for (int i = 0; i < m_MaxTaskNum; i++)
            GenerateTask();

#if DEBUG_TASKMGR
        Debug.Log(allTasks[1].ID);
        Debug.Log(allTasks[1].StartName);
        Debug.Log(allTasks[1].StartDesc);
        Debug.Log(allTasks[1].StartPoint);
        Debug.Log(allTasks[1].EndName);
        Debug.Log(allTasks[1].EndDesc);
        Debug.Log(allTasks[1].EndPoint);
        Debug.Log(allTasks[1].Next);
        Debug.Log(allTasks[1].Progress);
        Debug.Log(allTasks[1].Reward);
        Debug.Log(allTasks[1].CourageNeed);
        Debug.Log(allTasks[1].WisdomNeed);
        Debug.Log(allTasks[1].KindnessNeed);
        Debug.Log(allTasks[1].AddDay);
        Debug.Log(allTasks[1].TaskIcon);
        for (int i = 0; i < allTasks[1].MiddleCount; ++i)
        {
            MiddlePlace mp = allTasks[1].GetMiddlePlace(i);
            Debug.Log($"{mp.Name} {mp.Desc} {mp.Point}");
        }

#endif
#if DEBUG_MAP
        string mapPath = Application.streamingAssetsPath + "/Map.xlsx";
        System.Data.DataTable table = MiniExcel.QueryAsDataTable(mapPath, sheetName: "Map");
        Debug.Log(table.Rows[0][0]);
        Debug.Log(table.Rows[19][0]);
        Debug.Log(table.Rows[0][19]);
        Debug.Log(table.Rows[19][19]);
#endif
    }

    // 回合结束时产生任务 需加入回合结束事件（未完成）
    protected void GenerateTask()
    {
        bool error;
        int count = 0;
        int maxCount = m_CurTasks.Count * 3;
        // 任务太多 或 池子里没任务了 或 随机选择多次没有合适的任务 就别刷了
        while (CurWaitingTasks.Count < m_MaxTaskNum && m_CurTasks.Count > 0 && count < maxCount)
        {
            error = false;
            ++count;
            // 随机选择一个池子里的任务
            m_CurTaskIndex = Random.Range(0, m_CurTasks.Count);
            // 该任务起点有任务起点 或 该任务起点有员工就换一个任务
            int start = m_CurTasks[m_CurTaskIndex].StartPoint;
            foreach (TaskData task in CurWaitingTasks.Values)
            {
                if (task.StartPoint == start)
                {
                    error = true;
                    break;
                }
            }
            error |= MapController.mapController.GameMap[start].OccupingMen.Count != 0;

            if (!error)
                GenerateTaskOnScene();
        }
    }
    // 在场景中产生一个新任务
    protected void GenerateTaskOnScene()
    {
        MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint].OnPostManGetin += ShowTaskCanvas;
        MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint].OnPostManGetOut += Cancel;
        CurWaitingTasks.Add(MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint], m_CurTasks[m_CurTaskIndex]);
        ChangeTaskIcon(MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint]);

        // 从池子中移除刚生成的任务
        m_CurTasks.RemoveAt(m_CurTaskIndex);
    }

    // 一天结束时将新任务加入任务产生序列 需加入一天结束事件
    protected void AddTaskToQueue()
    {
        int day = TimeMgr.Instance.curDay;
        // 某天之后没有任务了 就不需要加任务了
        if (day > m_AllTasks.Count)
            return;
        foreach (TaskData task in m_AllTasks[day - 1].Values)
            m_CurTasks.Add(task);
    }

    // 显示任务画布 当有人进入某地块时触发
    protected void ShowTaskCanvas(BasePostMan postMan,BaseBlock block)
    {
        TaskData deta = new TaskData();
        if (CurWaitingTasks.TryGetValue(block, out deta))
        {   
            CanvasObj.SetActive(true);
            PickButton.onClick.RemoveAllListeners();
            PickButton.onClick.AddListener(delegate { PickTask(block, deta, postMan); });

            Discribtion.text = deta.StartDesc;

            //TimeMgr.Instance.pause = true;
        }
    }

    // 关闭任务画布
    protected void Cancel(BasePostMan man= null,BaseBlock block=null)
    {
        CanvasObj.SetActive(false);
    }

    // 开始任务
    protected void PickTask(BaseBlock Block,TaskData data,BasePostMan postMan)
    {
        Block.OnPostManGetin -= ShowTaskCanvas;
        Block.OnPostManGetOut -= Cancel;

        MapController.mapController.GameMap[data.EndPoint].OnPostManGetin += FinishTask;
        ChangeTaskIcon(MapController.mapController.GameMap[data.EndPoint],false,true);

        CurPickedTasks.Add(postMan, data);
        CurWaitingTasks.Remove(Block);

        postMan.GenerateBuffIcon(data.TaskIcon);

        ChangeTaskIcon(MapController.mapController.GameMap[data.StartPoint],true,false);
        Cancel();
    }

    // 完成任务
    protected void FinishTask(BasePostMan postman, BaseBlock block)
    {
        if (CurPickedTasks.TryGetValue(postman, out TaskData task))
        {
            if (MapController.mapController.GameMap[task.EndPoint] != block || !CheckAttr(postman))
                return;
            // 清除这个任务
            ChangeTaskIcon(block,false,false);
            postman.ClearBuffIcon(task.TaskIcon);
            if (task.FinishTask != null)
                task.FinishTask.Invoke();
            CurPickedTasks.Remove(postman);

            // 给予奖励
            Warehouse.Instance.money += task.Reward;

            Debug.LogWarning("任务奖励有待完善");

            // 将后续任务加入任务池
            for (int i = 0; i < m_AllTasks.Count; ++i)
            {
                if (m_AllTasks[i].ContainsKey(task.Next))
                {
                    m_CurTasks.Add(m_AllTasks[i][task.Next]);
                    break;
                }
            }
            // 产生新任务
            GenerateTask();
        }
    }
    // 检查员工是否够资格完成任务
    protected bool CheckAttr(BasePostMan postman)
    {
        if (!CurPickedTasks.ContainsKey(postman))
            return false;

        TaskData task = CurPickedTasks[postman];
        EmployeeData postdData = postman.MyDeta;
        return task.CourageNeed <= postdData.Courage &&
                task.WisdomNeed <= postdData.Wisdom &&
                task.KindnessNeed <= postdData.Kindness;
    }

    // 更改任务起点和终点的图标
    protected void ChangeTaskIcon(BaseBlock block,bool IsPicked=true, bool IsAdd = true)
    {
        if (IsAdd)
        {
            if (IsPicked)
            {
                GameObject go = GameObject.Instantiate(TaskPick, block.transform.position, Quaternion.identity);
                TaskSigns.Add(go);
                Pickblocks.Add(MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint]);
            }
            else
            {
                GameObject go = GameObject.Instantiate(TaskFinish, block.transform.position, Quaternion.identity);
                TaskFinishes.Add(go);
                Finishblocks.Add(block);
            }
        }
        else
        {
            if (IsPicked) 
            {
                GameObject.Destroy(TaskSigns[Pickblocks.IndexOf(block)]);
                TaskSigns.RemoveAt(Pickblocks.IndexOf(block));
                Pickblocks.Remove(block);
            }
            else
            {
                GameObject.Destroy(TaskFinishes[Finishblocks.IndexOf(block)]);
                TaskFinishes.RemoveAt(Finishblocks.IndexOf(block));
                Finishblocks.Remove(block);
            }
        }
    }

    /* 方便文件读取的中间类 */
    public class TaskDataRaw
    {
        public int ID { get; set; }
        // 接取名称/描述/地点
        public string StartName { get; set; }
        public string StartDesc { get; set; }
        public string StartPoint { get; set; }
        // 完成名称/描述/地点
        public string EndName { get; set; }
        public string EndDesc { get; set; }
        public string EndPoint { get; set; }
        // 后续任务ID
        public int Next { get; set; }
        // 进度值
        public int Progress { get; set; }
        // 信用点奖励
        public int Reward { get; set; }
        // 信念点需求
        public int CourageNeed { get; set; }
        public int WisdomNeed { get; set; }
        public int KindnessNeed { get; set; }
        // 加入天数
        public int AddDay { get; set; }
        // 中间地点
        public string MiddlePlace { get; set; }
    }
    public class MiddlePlaceRaw
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Point { get; set; }
    }
}
