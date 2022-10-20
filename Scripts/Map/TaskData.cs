using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// ����������
/// </summary>
[System.Serializable]
public class TaskData
{
    // Ψһ��ʶ��
    public int ID { get; protected set; }
    // ��ȡ����/����/�ص�
    public string StartName { get; protected set; }
    public string StartDesc { get; protected set; }
    public int StartPoint { get; protected set; }
    // �������/����/�ص�
    public string EndName { get; protected set; }
    public string EndDesc { get; protected set; }
    public int EndPoint { get; protected set; }
    // ��������ID
    public List<int> Next { get; protected set; }
    // ����ֵ
    public int Progress { get; protected set; }

    // ���������
    public int CourageNeed { get; protected set; }
    public int WisdomNeed { get; protected set; }
    public int KindnessNeed { get; protected set; }

    // ���õ㽱��
    public int Reward { get; protected set; }
    // ÿ�������㽱��
    public int CourageReward { get; protected set; }
    public int WisdomReward { get; protected set; }
    public int KindnessReward { get; protected set; }

    // Ա�����Ե�ӳ�
    public int EmpSpeed { get; protected set; }
    public int EmpCourage { get; protected set; }
    public int EmpWisdom { get; protected set; }
    public int EmpKindness { get; protected set; }

    // ��������
    public int AddDay { get; protected set; }
    // ����ʹ��ͼ��
    public Sprite TaskIcon { get; protected set; }
    protected const string m_SpritePath = "TaskIcon";
    // �м�ص����/�м�ص�
    public int MiddleCount => m_MiddlePlaces.Count;
    protected List<MiddlePlace> m_MiddlePlaces = new List<MiddlePlace>();

    // ������ɴ����¼�
    public UnityEvent FinishTask = new UnityEvent();
    // �Ƿ�Ϊ��������
    public bool MainTask = true;

    public TaskData() { }
    public TaskData(TaskMgr.TaskDataRaw raw, Dictionary<int, MiddlePlace> mps)
    {
        ID = raw.ID;
        StartName = raw.StartName;
        StartDesc = raw.StartDesc;
        StartPoint = StringToPosition(raw.StartPoint);
        EndName = raw.EndName;
        EndDesc = raw.EndDesc;
        EndPoint = StringToPosition(raw.EndPoint);
        Next = StringToList(raw.Next);
        Progress = raw.Progress;
        CourageNeed = raw.CourageNeed;
        WisdomNeed = raw.WisdomNeed;
        KindnessNeed = raw.KindnessNeed;
        Reward = raw.Reward;
        CourageReward = raw.CourageReward;
        WisdomReward = raw.WisdomReward;
        KindnessReward = raw.KindnessReward;
        EmpSpeed = raw.EmpSpeed;
        EmpCourage = raw.EmpCourage;
        EmpWisdom = raw.EmpWisdom;
        EmpKindness = raw.EmpKindness;
        AddDay = raw.AddDay;
        GetSprite(m_SpritePath);
        AddMiddlePlaces(raw.MiddlePlace, mps);
        InitFinishEvent();
    }
    /// <summary>
    /// ��ȡ�м�ص���Ϣ
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MiddlePlace GetMiddlePlace(int index)
    {
        if (index < 0 || MiddleCount <= index)
            return null;
        return m_MiddlePlaces[index];
    }

    /// <summary>
    /// ���������ַ���
    /// </summary>
    public static int StringToPosition(string str)
    {
        string[] vec = str.Split(',');
        return int.Parse(vec[0]) * 20 + int.Parse(vec[1]);
    }
    // �����б�
    protected static List<int> StringToList(string str)
    {
        List<int> list = new List<int>();
        if (str != null)
            foreach (string s in str.Split(','))
                list.Add(int.Parse(s.Trim()));
        return list;
    }

    // ��ȡ����ͼ����Դ
    protected void GetSprite(string str)
    {
        Sprite spr = Resources.Load<Sprite>(str);
        if (spr != null)
            TaskIcon = spr;
        else
            Debug.LogError($"�Ҳ�������ͼƬ��·����{str}");
    }
    // ����м�ص�
    protected void AddMiddlePlaces(string str, Dictionary<int, MiddlePlace> mps)
    {
        if (str == null)
            return;
        foreach (string id_str in str.Split(','))
        {
            int id = int.Parse(id_str.Trim());
            if (mps.ContainsKey(id))
                m_MiddlePlaces.Add(mps[id]);
            else
                Debug.LogError($"δ�ҵ�IDΪ{id}���м�ص�");
        }
    }
    // ���������Ϊ
    protected void InitFinishEvent()
    {
        switch (ID)
        {
        case 0:
            FinishTask.AddListener(() =>
            {

            });
            break;
        default:
            FinishTask = null;
            break;
        }
    }
}

/// <summary>
/// �����м�ص���
/// </summary>
public class MiddlePlace
{
    // ����/����/�ص�
    public string Name { get; set; }
    public string Desc { get; set; }
    public int Point { get; set; }
    public MiddlePlace(TaskMgr.MiddlePlaceRaw raw)
    {
        Name = raw.Name;
        Desc = raw.Desc;
        Point = TaskData.StringToPosition(raw.Point);
    }
    public MiddlePlace(MiddlePlace other)
    {
        Name = other.Name;
        Desc = other.Desc;
        Point = other.Point;
    }
}
