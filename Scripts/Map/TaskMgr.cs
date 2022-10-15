#define DEBUG_TASKMGR
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
    protected TaskMgr()
    {
        // ��ȡ�м�ص�����
        foreach (MiddlePlaceRaw raw in MiniExcel.Query<MiddlePlaceRaw>(filePath, sheetName: "MiddlePlace", startCell: "A3"))
        {
            if (raw.Name != null)
                allMiddlePlaces.Add(raw.ID, new MiddlePlace(raw));
        }
        // ��ȡ��������
        foreach (TaskDataRaw raw in MiniExcel.Query<TaskDataRaw>(filePath, sheetName: "Task", startCell: "A3"))
        {
            if (raw.StartName != null)
                allTasks.Add(raw.ID, new TaskData(raw, allMiddlePlaces));
        }

        // �����������¼�
        TimeMgr.Instance.roundEndAction += TurnTrigger;
        Cancel();
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
    }
    protected readonly string filePath = Application.streamingAssetsPath + "/Tasks.xlsx";

    // ȫ��/��ǰ�����
    protected Dictionary<int, TaskData> allTasks = new Dictionary<int, TaskData>();
    public List<TaskData> curTasks;
    // �м�ص��
    protected Dictionary<int, MiddlePlace> allMiddlePlaces = new Dictionary<int, MiddlePlace>();
    #region
    protected int TaskTimer = 0;
    protected int TaskCount = 0;

    public int[] TaskTurn;                                  //��ͼ��ÿ�����ٻغ�����һ������

    protected Dictionary<BaseBlock,TaskData> CurWaitingTaskDatas=new Dictionary<BaseBlock, TaskData>();                       //��ǰ��δ��ȡ������ 
    protected Dictionary<BasePostMan, TaskData> CurPickedDetas=new Dictionary<BasePostMan, TaskData>();                       //��ǰ�Ѿ���ȡ������

    protected GameObject canvas
    {
        get
        {
            if (m_canvas == null)
            {
                m_canvas = GameObject.Find("���񻭲�");
            }
            return m_canvas;
        }
        set => m_canvas = value;
    }
    protected GameObject m_canvas;

    protected Button PickButton;
    protected Button CancelButton;

    protected Text Discribtion;

    protected List<GameObject> TaskSigns = new List<GameObject>();
    protected List<BaseBlock> Pickblocks = new List<BaseBlock>();
    public GameObject TaskPick;

    protected List<GameObject> TaskFinishes = new List<GameObject>();
    protected List<BaseBlock> Finishblocks = new List<BaseBlock>();
    public GameObject TaskFinish;

    public void Start() {

    }
    public void TurnTrigger()
    {
        TaskTimer++;
        if (TaskCount < TaskTurn.Length)
        {
            if (TaskTimer >= TaskTurn[TaskCount])
            {
                GenerateTask();
                TaskTimer = 0;
                TaskCount++;
            }
            if (CurWaitingTaskDatas.Count == 0)
            {
                GenerateTask();
            }
        }
    }

    public void GenerateTask()
    {
        MapController.mapController.GameMap[curTasks[TaskCount].StartPoint].OnPostManGetin += ShowTaskCanvas;
        MapController.mapController.GameMap[curTasks[TaskCount].StartPoint].OnPostManGetOut += Cancel;

        CurWaitingTaskDatas.Add(MapController.mapController.GameMap[curTasks[TaskCount].StartPoint], curTasks[TaskCount]);

        ChangeTaskIcon(MapController.mapController.GameMap[curTasks[TaskCount].StartPoint]);

        TimeMgr.Instance.pause = true;
    }

    public void ShowTaskCanvas(BasePostMan postMan,BaseBlock block) {
        TaskData deta = new TaskData();
        if (CurWaitingTaskDatas.TryGetValue(block, out deta))
        {   
            canvas.SetActive(true);
            PickButton.onClick.RemoveAllListeners();
            PickButton.onClick.AddListener(delegate { PickTask(block, deta, postMan); });

            Discribtion.text = deta.StartDesc;

            TimeMgr.Instance.pause = true;
        }
    }

    public void Cancel(BasePostMan man= null,BaseBlock block=null) {
        canvas.SetActive(false);
    }

    public void PickTask(BaseBlock Block,TaskData deta,BasePostMan postMan) {
        Block.OnPostManGetin -= ShowTaskCanvas;
        Block.OnPostManGetOut -= Cancel;

        MapController.mapController.GameMap[deta.EndPoint].OnPostManGetin += FinishTask;
        ChangeTaskIcon(MapController.mapController.GameMap[deta.EndPoint],false,true);

        CurPickedDetas.Add(postMan, deta);
        CurWaitingTaskDatas.Remove(Block);

        postMan.GenerateBuffIcon(deta.TaskIcon);

        ChangeTaskIcon(MapController.mapController.GameMap[deta.StartPoint],true,false);
        Cancel();
    }

    public void FinishTask(BasePostMan postMan,BaseBlock block) {
        TaskData deta=new TaskData();
        if (CurPickedDetas.TryGetValue(postMan, out deta))
        {
            if(MapController.mapController.GameMap[deta.EndPoint]==block)
            {
                ChangeTaskIcon(block,false,false);

                postMan.ClearBuffIcon(deta.TaskIcon);

                deta.FinishTask.Invoke();
                CurPickedDetas.Remove(postMan);
            }
        }
    }

    public void ChangeTaskIcon(BaseBlock block,bool IsPicked=true, bool IsAdd = true)
    {
        if (IsAdd)
        {
            if (IsPicked) 
            {
                GameObject go = GameObject.Instantiate(TaskPick, block.transform.position, Quaternion.identity);
                TaskSigns.Add(go);
                Pickblocks.Add(MapController.mapController.GameMap[curTasks[TaskCount].StartPoint]);
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
    #endregion
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
