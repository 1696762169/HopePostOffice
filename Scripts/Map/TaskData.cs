using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 任务数据类
/// </summary>
[System.Serializable]
public class TaskData
{
    // 唯一标识符
    public int ID { get; protected set; }
    // 接取名称/描述/地点
    public string StartName { get; protected set; }
    public string StartDesc { get; protected set; }
    public int StartPoint { get; protected set; }
    // 完成名称/描述/地点
    public string EndName { get; protected set; }
    public string EndDesc { get; protected set; }
    public int EndPoint { get; protected set; }
    // 后续任务ID
    public int Next { get; protected set; }
    // 进度值
    public int Progress { get; protected set; }
    // 信用点奖励
    public int Reward { get; protected set; }
    // 信念点需求
    public int CourageNeed { get; protected set; }
    public int WisdomNeed { get; protected set; }
    public int KindnessNeed { get; protected set; }
    // 加入天数
    public int AddDay { get; protected set; }
    // 任务使用图标
    public Sprite TaskIcon { get; protected set; }
    protected const string m_SpritePath = "TaskIcon";
    // 中间地点个数/中间地点
    public int MiddleCount => m_MiddlePlaces.Count;
    protected List<MiddlePlace> m_MiddlePlaces = new List<MiddlePlace>();

    // 任务完成触发事件
    public UnityEvent FinishTask;


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
        Next = raw.Next;
        Progress = raw.Progress;
        Reward = raw.Reward;
        CourageNeed = raw.CourageNeed;
        WisdomNeed = raw.WisdomNeed;
        KindnessNeed = raw.KindnessNeed;
        AddDay = raw.AddDay;
        GetSprite(m_SpritePath);
        AddMiddlePlaces(raw.MiddlePlace, mps);
        InitFinishEvent();
    }
    /// <summary>
    /// 获取中间地点信息
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
    /// 解析坐标字符串
    /// </summary>
    public static int StringToPosition(string str)
    {
        string[] vec = str.Split(',');
        return int.Parse(vec[0]) * 20 + int.Parse(vec[1]);
    }

    // 获取任务图标资源
    protected void GetSprite(string str)
    {
        Sprite spr = Resources.Load<Sprite>(str);
        if (spr != null)
            TaskIcon = spr;
        else
            Debug.LogError($"找不到精灵图片，路径：{str}");
    }
    // 添加中间地点
    protected void AddMiddlePlaces(string str, Dictionary<int, MiddlePlace> mps)
    {
        foreach (string id_str in str.Split(','))
        {
            int id = int.Parse(id_str.Trim());
            if (mps.ContainsKey(id))
                m_MiddlePlaces.Add(mps[id]);
            else
                Debug.LogError($"未找到ID为{id}的中间地点");
        }
    }
    // 添加特殊行为
    protected void InitFinishEvent()
    {
        switch (ID)
        {
        case 0:
            FinishTask.AddListener(() =>
            {

            });
            break;
        }
    }
}

/// <summary>
/// 任务中间地点类
/// </summary>
public class MiddlePlace
{
    // 名称/描述/地点
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
