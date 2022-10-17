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
    // �������ñ�·��
    protected readonly string m_FilePath = Application.streamingAssetsPath + "/Tasks.xlsx";

    // ȫ��/��ǰ�����
    protected List<Dictionary<int, TaskData>> m_AllTasks = new List<Dictionary<int, TaskData>>();
    public List<TaskData> m_CurTasks = new List<TaskData>();
    // �м�ص��
    protected Dictionary<int, MiddlePlace> allMiddlePlaces = new Dictionary<int, MiddlePlace>();

    // ���ڲ������������
    protected int m_CurTaskIndex;
    // ���������
    protected const int m_MaxTaskNum = 5;

    //��ǰδ��ȡ/�ѽ�ȡ������ 
    protected Dictionary<BaseBlock,TaskData> CurWaitingTasks = new Dictionary<BaseBlock, TaskData>();
    protected Dictionary<BasePostMan, TaskData> CurPickedTasks = new Dictionary<BasePostMan, TaskData>();

    // ��Ҫ���ʻ���ʱ�Զ���ȡ�����ϵĻ�������
    protected GameObject CanvasObj
    {
        get
        {
            if (m_canvas == null)
            {
                // �󶨻���������ؼ�
                m_canvas = GameObject.Find("���񻭲�");
                PickButton = m_canvas.transform.GetChild(0).GetComponent<Button>();
                CancelButton = m_canvas.transform.GetChild(1).GetComponent<Button>();
                Discribtion = m_canvas.transform.GetChild(2).GetComponent<Text>();

                // ��ʼ����ť����
                CancelButton.onClick.AddListener(() => Cancel());
            }
            return m_canvas;
        }
        set => m_canvas = value;
    }
    protected GameObject m_canvas;
    // �����ؼ�
    protected Button PickButton;
    protected Button CancelButton;
    protected Text Discribtion;

    // ����ͼ����Դ
    public GameObject TaskPick;
    public GameObject TaskFinish;

    // ������������ϵ�ͼ������¼�Ķ���
    protected List<GameObject> TaskSigns = new List<GameObject>();
    protected List<BaseBlock> Pickblocks = new List<BaseBlock>();
    protected List<GameObject> TaskFinishes = new List<GameObject>();
    protected List<BaseBlock> Finishblocks = new List<BaseBlock>();

    protected TaskMgr()
    {
        // ��ȡ�м�ص�����
        foreach (MiddlePlaceRaw raw in MiniExcel.Query<MiddlePlaceRaw>(m_FilePath, sheetName: "MiddlePlace", startCell: "A3"))
        {
            if (raw.Name != null)
                allMiddlePlaces.Add(raw.ID, new MiddlePlace(raw));
        }
        // ��ȡ��������
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
        // ����Ԥ����
        const string prefabPath = "TaskPick";
        TaskPick = Resources.Load<GameObject>(prefabPath);
        TaskFinish = Resources.Load<GameObject>(prefabPath);

        // ��������¼�
        TimeMgr.Instance.dayEndAction += AddTaskToQueue;
        Cancel();
        // ��ӵ�һ�������
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

    // �غϽ���ʱ�������� �����غϽ����¼���δ��ɣ�
    protected void GenerateTask()
    {
        bool error;
        int count = 0;
        int maxCount = m_CurTasks.Count * 3;
        // ����̫�� �� ������û������ �� ���ѡ����û�к��ʵ����� �ͱ�ˢ��
        while (CurWaitingTasks.Count < m_MaxTaskNum && m_CurTasks.Count > 0 && count < maxCount)
        {
            error = false;
            ++count;
            // ���ѡ��һ�������������
            m_CurTaskIndex = Random.Range(0, m_CurTasks.Count);
            // ������������������ �� �����������Ա���ͻ�һ������
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
    // �ڳ����в���һ��������
    protected void GenerateTaskOnScene()
    {
        MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint].OnPostManGetin += ShowTaskCanvas;
        MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint].OnPostManGetOut += Cancel;
        CurWaitingTasks.Add(MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint], m_CurTasks[m_CurTaskIndex]);
        ChangeTaskIcon(MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint]);

        // �ӳ������Ƴ������ɵ�����
        m_CurTasks.RemoveAt(m_CurTaskIndex);
    }

    // һ�����ʱ���������������������� �����һ������¼�
    protected void AddTaskToQueue()
    {
        int day = TimeMgr.Instance.curDay;
        // ĳ��֮��û�������� �Ͳ���Ҫ��������
        if (day > m_AllTasks.Count)
            return;
        foreach (TaskData task in m_AllTasks[day - 1].Values)
            m_CurTasks.Add(task);
    }

    // ��ʾ���񻭲� �����˽���ĳ�ؿ�ʱ����
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

    // �ر����񻭲�
    protected void Cancel(BasePostMan man= null,BaseBlock block=null)
    {
        CanvasObj.SetActive(false);
    }

    // ��ʼ����
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

    // �������
    protected void FinishTask(BasePostMan postman, BaseBlock block)
    {
        if (CurPickedTasks.TryGetValue(postman, out TaskData task))
        {
            if (MapController.mapController.GameMap[task.EndPoint] != block || !CheckAttr(postman))
                return;
            // ����������
            ChangeTaskIcon(block,false,false);
            postman.ClearBuffIcon(task.TaskIcon);
            if (task.FinishTask != null)
                task.FinishTask.Invoke();
            CurPickedTasks.Remove(postman);

            // ���轱��
            Warehouse.Instance.money += task.Reward;

            Debug.LogWarning("�������д�����");

            // ������������������
            for (int i = 0; i < m_AllTasks.Count; ++i)
            {
                if (m_AllTasks[i].ContainsKey(task.Next))
                {
                    m_CurTasks.Add(m_AllTasks[i][task.Next]);
                    break;
                }
            }
            // ����������
            GenerateTask();
        }
    }
    // ���Ա���Ƿ��ʸ��������
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

    // �������������յ��ͼ��
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

    /* �����ļ���ȡ���м��� */
    public class TaskDataRaw
    {
        public int ID { get; set; }
        // ��ȡ����/����/�ص�
        public string StartName { get; set; }
        public string StartDesc { get; set; }
        public string StartPoint { get; set; }
        // �������/����/�ص�
        public string EndName { get; set; }
        public string EndDesc { get; set; }
        public string EndPoint { get; set; }
        // ��������ID
        public int Next { get; set; }
        // ����ֵ
        public int Progress { get; set; }
        // ���õ㽱��
        public int Reward { get; set; }
        // ���������
        public int CourageNeed { get; set; }
        public int WisdomNeed { get; set; }
        public int KindnessNeed { get; set; }
        // ��������
        public int AddDay { get; set; }
        // �м�ص�
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
