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
    // �������ñ�·��
    protected readonly string m_FilePath = Application.streamingAssetsPath + "/Tasks.xlsx";

    // ��ǰ/����������
    public int Progress { get; protected set; }
    public int MaxProgress { get; protected set; } = 3000;

    // ȫ��/��ǰ�����
    protected Dictionary<int, TaskData> m_AllTasks = new Dictionary<int, TaskData>();
    protected List<TaskData> m_CurTasks = new List<TaskData>();
    //��ǰ����δ��ȡ/�ѽ�ȡ������ 
    protected Dictionary<BaseBlock, Task> m_WaitingTasks = new Dictionary<BaseBlock, Task>();
    protected Dictionary<BasePostMan, Task> m_PickedTasks = new Dictionary<BasePostMan, Task>();

    // �м�ص��
    protected Dictionary<int, MiddlePlace> m_AllMiddlePlaces = new Dictionary<int, MiddlePlace>();

    // ���ڲ������������
    protected int m_CurTaskIndex;
    // ���������
    protected const int m_MaxTaskNum = 5;

    // ����Ԥ����
    protected GameObject m_CanvasObj;
    // ��ǰ�����ϵĻ�������
    protected List<GameObject> m_CanvasList = new List<GameObject>();

    // ����ͼ����Դ
    protected GameObject m_StartObj;
    protected GameObject m_MiddleObj;
    protected GameObject m_EndObj;

    protected TaskMgr()
    {
        // ��ȡ�м�ص�����
        foreach (MiddlePlaceRaw raw in MiniExcel.Query<MiddlePlaceRaw>(m_FilePath, sheetName: "MiddlePlace", startCell: "A3"))
        {
            if (raw.Name != null)
                m_AllMiddlePlaces.Add(raw.ID, new MiddlePlace(raw));
        }
        // ��ȡ��������
        foreach (TaskDataRaw raw in MiniExcel.Query<TaskDataRaw>(m_FilePath, sheetName: "Task", startCell: "A3"))
        {
            if (raw.StartName != null)
            {
                TaskData data = new TaskData(raw, m_AllMiddlePlaces);
                m_AllTasks.Add(raw.ID, data);
            }
        }
        // ��ʼ��ǰ����������
        foreach (TaskData task in m_AllTasks.Values)
        {
            if (task.Next <= 0)
                continue;
            if (m_AllTasks.ContainsKey(task.Next))
                ++m_AllTasks[task.Next].Prev;
            else
                Debug.LogError($"δ�ҵ�IDΪ{task.Next}�ĺ�������");
        }

        // ����Ԥ����
        m_StartObj = Resources.Load<GameObject>("TaskPick");
        m_MiddleObj = Resources.Load<GameObject>("TaskMiddle");
        m_EndObj = Resources.Load<GameObject>("TaskEnd");
        m_CanvasObj = Resources.Load<GameObject>("TaskCanvas");

        // ����¼�
        TimeMgr.Instance.dayEndAction += AddTaskToQueue;
        TimeMgr.Instance.dayEndAction += () => Progress = 0;

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
#if DEBUG_WAREHOUSE
        Warehouse.Instance.courage += 1000;
        Warehouse.Instance.wisdom += 1000;
        Warehouse.Instance.kindness += 1000;
#endif
    }

    // �غϽ���ʱ�������� �����غϽ����¼���δ��ɣ�
    protected void GenerateTask()
    {
        bool error;
        int count = 0;
        int maxCount = m_CurTasks.Count * 3;
        // ����̫�� �� ������û������ �� ���ѡ����û�к��ʵ����� �ͱ�ˢ��
        while (m_WaitingTasks.Count < m_MaxTaskNum && m_CurTasks.Count > 0 && count < maxCount)
        {
            error = false;
            ++count;
            // ���ѡ��һ�������������
            m_CurTaskIndex = Random.Range(0, m_CurTasks.Count);
            // ������������������ �� �����������Ա���ͻ�һ������
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
    // �ڳ����в���һ��������
    protected void GenerateTaskOnScene()
    {
        BaseBlock block = MapController.mapController.GameMap[m_CurTasks[m_CurTaskIndex].StartPoint];
        m_WaitingTasks.Add(block, new Task(m_CurTasks[m_CurTaskIndex], block));
        TaskProgress(m_WaitingTasks[block]);
        block.OnPostManGetin += StartTask;

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
        foreach (TaskData task in m_AllTasks.Values)
        {
            if (task.AddDay <= day && task.Prev <= 0)
                m_CurTasks.Add(task);
        }
    }

    // ��ʼ���� �����˽�������ʼ�ؿ�ʱ����
    protected void StartTask(BasePostMan postman, BaseBlock block)
    {
        // ���������Ѿ��������򷵻�
        if (m_PickedTasks.ContainsKey(postman))
            return;
        // �����������
        Task task = m_WaitingTasks[block];
        m_WaitingTasks[block].owner = postman;
        TaskProgress(task);
        // �����ұ�ʶ �� �ؿ��¼�
        task.owner.GenerateBuffIcon(task.data.TaskIcon);
        if (task.data.MiddleCount > 0)
            task.block.OnPostManGetin += MiddleTask;
        else
            task.block.OnPostManGetin += EndTask;
        // ������������
        m_PickedTasks.Add(task.owner, task);
        m_WaitingTasks.Remove(MapController.mapController.GameMap[task.data.StartPoint]);
        // ���ʰȡ�¼�
        block.OnPostManGetin -= StartTask;
    }
    // ·���м�� �����˽����м��ؿ�ʱ����
    protected void MiddleTask(BasePostMan postman, BaseBlock block)
    {
        // ȷ���������
        if (!m_PickedTasks.ContainsKey(postman) || m_PickedTasks[postman].block != block)
            return;
        // ����������� �� ��ɾ�¼�
        Task task = m_PickedTasks[postman];
        TaskProgress(task);
        if (task.progress <= task.data.MiddleCount)
            task.block.OnPostManGetin += MiddleTask;
        else
            task.block.OnPostManGetin += EndTask;
        block.OnPostManGetin -= MiddleTask;
    }
    // ������� �����˽��������ύ�ؿ�ʱ����
    protected void EndTask(BasePostMan postman, BaseBlock block)
    {
        // ȷ���������
        if (!m_PickedTasks.ContainsKey(postman) || m_PickedTasks[postman].block != block)
            return;
        // ��ȡ�򴴽�һ����������
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
        // ��ʾ���
        canvas.GetComponent<TaskCanvas>().Show(m_PickedTasks[postman].data, () =>
        {
            ReallyEndTask(postman, block);
            canvas.GetComponent<TaskCanvas>().Hide(postman, block);
         });
    }
    // ����������� ����ύ��ť�󴥷�
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

        // ���������� �� ִ�лص�����
        TaskProgress(task);
        m_PickedTasks.Remove(postman);
        postman.ClearBuffIcon(data.TaskIcon);
        if (data.FinishTask != null)
            data.FinishTask.Invoke();

        // �۳����� �� ���轱��
        Warehouse.Instance.money += data.Reward;
        Warehouse.Instance.courage -= data.CourageNeed;
        Warehouse.Instance.wisdom -= data.WisdomNeed;
        Warehouse.Instance.kindness -= data.KindnessNeed;
        Progress += data.Progress;

        // ������������������
        if (data.Next > 0)
        {
            --m_AllTasks[data.Next].Prev;
            if (m_AllTasks[data.Next].Prev <= 0)
                m_CurTasks.Add(m_AllTasks[data.Next]);
        }
        // ����������
        GenerateTask();
        // �Ƴ��¼�
        block.OnPostManGetin -= EndTask;
    }
    // �����Դ�Ƿ��������
    protected bool CheckAttr(TaskData task)
    {
        return task.CourageNeed <= Warehouse.Instance.courage &&
                task.WisdomNeed <= Warehouse.Instance.wisdom &&
                task.KindnessNeed <= Warehouse.Instance.kindness;
    }

    // �������������յ��ͼ��
    protected void TaskProgress(Task task)
    {
        // �ƽ�����
        ++task.progress;

        // ��������ؿ�
        if (task.progress > 0)
        {
            int point;
            if (task.progress > task.data.MiddleCount)
                point = task.data.EndPoint;
            else
                point = task.data.GetMiddlePlace(task.progress - 1).Point;
            task.block = MapController.mapController.GameMap[point];
        }

        // ����ԭ�ȵ�����ͼ��
        if (task.obj != null)
        {
            GameObject.Destroy(task.obj);
            task.obj = null;
        }
        // �½�����ͼ��
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

    // �������ϵ������������
    protected class Task
    {
        public TaskData data;       // ��������
        public GameObject obj;      // ��ǰ���ϵ�ͼ��
        public BaseBlock block;     // ͼ�����ڵ�ͼ����
        public BasePostMan owner;   // ��ȡ��������
        public int progress;        // ������� -1��ʾ����δ��ʼ�� 0��ʾ����δ����ȡ
        public Task(TaskData data, BaseBlock block)
        {
            this.data = data;
            this.block = block;
            progress = -1;
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
