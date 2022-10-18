//#define DEBUG_TASKMGR
//#define DEBUG_MAP
#define DEBUG_WAREHOUSE
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

    // 当前/最大任务进度
    public int Progress { get; protected set; }
    public int MaxProgress { get; protected set; } = 3000;

    // 全部/当前任务池
    protected Dictionary<int, TaskData> m_AllTasks = new Dictionary<int, TaskData>();
    protected List<TaskData> m_CurTasks = new List<TaskData>();
    //当前场上未接取/已接取的任务 
    protected Dictionary<BaseBlock, Task> m_WaitingTasks = new Dictionary<BaseBlock, Task>();
    protected Dictionary<BasePostMan, Task> m_PickedTasks = new Dictionary<BasePostMan, Task>();

    // 中间地点池
    protected Dictionary<int, MiddlePlace> m_AllMiddlePlaces = new Dictionary<int, MiddlePlace>();

    // 正在产生的任务序号
    protected int m_CurTaskIndex;
    // 最大任务数
    protected const int m_MaxTaskNum = 5;

    // 画布预制体
    protected GameObject m_CanvasObj;
    // 当前场景上的画布对象
    protected List<GameObject> m_CanvasList = new List<GameObject>();

    // 任务图标资源
    protected GameObject m_StartObj;
    protected GameObject m_MiddleObj;
    protected GameObject m_EndObj;

    protected TaskMgr()
    {
        // 读取中间地点数据
        foreach (MiddlePlaceRaw raw in MiniExcel.Query<MiddlePlaceRaw>(m_FilePath, sheetName: "MiddlePlace", startCell: "A3"))
        {
            if (raw.Name != null)
                m_AllMiddlePlaces.Add(raw.ID, new MiddlePlace(raw));
        }
        // 读取任务数据
        foreach (TaskDataRaw raw in MiniExcel.Query<TaskDataRaw>(m_FilePath, sheetName: "Task", startCell: "A3"))
        {
            if (raw.StartName != null)
            {
                TaskData data = new TaskData(raw, m_AllMiddlePlaces);
                m_AllTasks.Add(raw.ID, data);
            }
        }
        // 初始化前置任务数量
        foreach (TaskData task in m_AllTasks.Values)
        {
            if (task.Next <= 0)
                continue;
            if (m_AllTasks.ContainsKey(task.Next))
                ++m_AllTasks[task.Next].Prev;
            else
                Debug.LogError($"未找到ID为{task.Next}的后续任务");
        }

        // 加载预设体
        m_StartObj = Resources.Load<GameObject>("TaskPick");
        m_MiddleObj = Resources.Load<GameObject>("TaskMiddle");
        m_EndObj = Resources.Load<GameObject>("TaskEnd");
        m_CanvasObj = Resources.Load<GameObject>("TaskCanvas");

        // 添加事件
        TimeMgr.Instance.dayEndAction += AddTaskToQueue;
        TimeMgr.Instance.dayEndAction += () => Progress = 0;

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
#if DEBUG_WAREHOUSE
        Warehouse.Instance.courage += 1000;
        Warehouse.Instance.wisdom += 1000;
        Warehouse.Instance.kindness += 1000;
#endif
    }

    // 回合结束时产生任务 需加入回合结束事件（未完成）
    protected void GenerateTask()
    {
        bool error;
        int count = 0;
        int maxCount = m_CurTasks.Count * 3;
        // 任务太多 或 池子里没任务了 或 随机选择多次没有合适的任务 就别刷了
        while (m_WaitingTasks.Count < m_MaxTaskNum && m_CurTasks.Count > 0 && count < maxCount)
        {
            error = false;
            ++count;
            // 随机选择一个池子里的任务
            m_CurTaskIndex = Random.Range(0, m_CurTasks.Count);
            // 该任务起点有任务起点 或 该任务起点有员工就换一个任务
            int start = m_CurTasks[m_CurTaskIndex].StartPoint;
            foreach (Task task in m_WaitingTasks.Values)
            {
                if (task.data.StartPoint == start)
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
        BaseBlock block = MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint];
        m_WaitingTasks.Add(block, new Task(m_CurTasks[m_CurTaskIndex], block));
        TaskProgress(m_WaitingTasks[block]);
        block.OnPostManGetin += StartTask;

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
        foreach (TaskData task in m_AllTasks.Values)
        {
            if (task.AddDay <= day && task.Prev <= 0)
                m_CurTasks.Add(task);
        }
    }

    // 开始任务 当有人进入任务开始地块时触发
    protected void StartTask(BasePostMan postman, BaseBlock block)
    {
        // 若进入者已经有任务则返回
        if (m_PickedTasks.ContainsKey(postman))
            return;
        // 处理任务对象
        Task task = m_WaitingTasks[block];
        m_WaitingTasks[block].owner = postman;
        TaskProgress(task);
        // 添加玩家标识 和 地块事件
        task.owner.GenerateBuffIcon(task.data.TaskIcon);
        if (task.data.MiddleCount > 0)
            task.block.OnPostManGetin += MiddleTask;
        else
            task.block.OnPostManGetin += EndTask;
        // 更换任务容器
        m_PickedTasks.Add(task.owner, task);
        m_WaitingTasks.Remove(MapController.mapController.GameMap[task.data.StartPoint]);
        // 清除拾取事件
        block.OnPostManGetin -= StartTask;
    }
    // 路过中间点 当有人进入中间点地块时触发
    protected void MiddleTask(BasePostMan postman, BaseBlock block)
    {
        // 确认来者身份
        if (!m_PickedTasks.ContainsKey(postman) || m_PickedTasks[postman].block != block)
            return;
        // 处理任务对象 并 增删事件
        Task task = m_PickedTasks[postman];
        TaskProgress(task);
        if (task.progress <= task.data.MiddleCount)
            task.block.OnPostManGetin += MiddleTask;
        else
            task.block.OnPostManGetin += EndTask;
        block.OnPostManGetin -= MiddleTask;
    }
    // 完成任务 当有人进入任务提交地块时触发
    protected void EndTask(BasePostMan postman, BaseBlock block)
    {
        // 确认来者身份
        if (!m_PickedTasks.ContainsKey(postman) || m_PickedTasks[postman].block != block)
            return;
        // 获取或创建一个画布对象
        GameObject canvas = null;
        foreach (GameObject obj in m_CanvasList)
        {
            if (!obj.activeInHierarchy)
            {
                canvas = obj;
                canvas.SetActive(true);
                break;
            }
        }
        if (canvas == null)
        {
            canvas = GameObject.Instantiate(m_CanvasObj);
            m_CanvasList.Add(canvas);
        }
        // 显示面板
        canvas.GetComponent<TaskCanvas>().Show(m_PickedTasks[postman].data, () =>
        {
            ReallyEndTask(postman, block);
            canvas.GetComponent<TaskCanvas>().Hide(postman, block);
         });
    }
    // 真正完成任务 点击提交按钮后触发
    protected void ReallyEndTask(BasePostMan postman, BaseBlock block)
    {
        if (!m_PickedTasks.ContainsKey(postman))
            return;
        Task task = m_PickedTasks[postman];
        TaskData data = task.data;
        if (MapController.mapController.GameMap[data.EndPoint] != block)
            return;
        if (!CheckAttr(data))
            return;

        // 清除这个任务 并 执行回调函数
        TaskProgress(task);
        m_PickedTasks.Remove(postman);
        postman.ClearBuffIcon(data.TaskIcon);
        if (data.FinishTask != null)
            data.FinishTask.Invoke();

        // 扣除点数 与 给予奖励
        Warehouse.Instance.money += data.Reward;
        Warehouse.Instance.courage -= data.CourageNeed;
        Warehouse.Instance.wisdom -= data.WisdomNeed;
        Warehouse.Instance.kindness -= data.KindnessNeed;
        Progress += data.Progress;

        // 将后续任务加入任务池
        if (data.Next > 0)
        {
            --m_AllTasks[data.Next].Prev;
            if (m_AllTasks[data.Next].Prev <= 0)
                m_CurTasks.Add(m_AllTasks[data.Next]);
        }
        // 产生新任务
        GenerateTask();
        // 移除事件
        block.OnPostManGetin -= EndTask;
    }
    // 检查资源是否够完成任务
    protected bool CheckAttr(TaskData task)
    {
        return task.CourageNeed <= Warehouse.Instance.courage &&
                task.WisdomNeed <= Warehouse.Instance.wisdom &&
                task.KindnessNeed <= Warehouse.Instance.kindness;
    }

    // 更改任务起点和终点的图标
    protected void TaskProgress(Task task)
    {
        // 推进进度
        ++task.progress;

        // 更新任务地块
        if (task.progress > 0)
        {
            int point;
            if (task.progress > task.data.MiddleCount)
                point = task.data.EndPoint;
            else
                point = task.data.GetMiddlePlace(task.progress - 1).Point;
            task.block = MapController.mapController.GameMap[point];
        }

        // 销毁原先的任务图标
        if (task.obj != null)
        {
            GameObject.Destroy(task.obj);
            task.obj = null;
        }
        // 新建任务图标
        if (task.progress <= task.data.MiddleCount + 1)
        {
            GameObject prefab;
            if (task.progress == 0)
                prefab = m_StartObj;
            else if (task.progress <= task.data.MiddleCount)
                prefab = m_MiddleObj;
            else
                prefab = m_EndObj;
            task.obj = GameObject.Instantiate(prefab, task.block.transform.position, Quaternion.identity);
        }
    }

    // 管理场景上的任务对象和面板
    protected class Task
    {
        public TaskData data;       // 任务数据
        public GameObject obj;      // 当前场上的图标
        public BaseBlock block;     // 图标所在地图区块
        public BasePostMan owner;   // 接取该任务者
        public int progress;        // 任务进度 -1表示任务未初始化 0表示任务未被接取
        public Task(TaskData data, BaseBlock block)
        {
            this.data = data;
            this.block = block;
            progress = -1;
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
