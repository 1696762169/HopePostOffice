using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeMgr
{
    private static TimeMgr instance = new TimeMgr();
    public static TimeMgr Instance => instance;
    private TimeMgr() { }

    public int curRound;    // 当前回合数
    public int curDay;      // 当前天数
    public int DayRoundCount => dayRoundList[curDay - 1];   // 当前这一天的总回合数
    public int DayCount => dayRoundList.Count;              // 总天数
    private List<int> dayRoundList;                         // 各天的总回合数

    public event UnityAction roundEndAction;     // 回合结束事件
    public event UnityAction dayEndAction;       // 一天结束事件

    public bool pause;                           // 游戏暂停标识符
    public event UnityAction pauseAction;        // 游戏内部暂停事件  

    private bool inited = false;    // 初始化标识符

    /// <summary>
    /// 由游戏管理器进行初始化 读取配置数据
    /// </summary>
    public void Init()
    {
        // 防止重复初始化
        if (inited)
            return;
        inited = true;

        // 设定开始的天数和回合数
        curRound = 1;
        curDay = 1;

        // 读取各天回合数
        dayRoundList = new List<int>() { 20, 20, 20, 20, 25, 30, 30 };
        Debug.LogWarning("暂未实现各天回合数从文件读取");
        if (dayRoundList == null)
            Debug.LogError("未读取到各天回合数");
    }

    /// <summary>
    /// 由外部调用进行一个回合
    /// </summary>
    public void RoundEnd()
    {
        // 检测当天是否结束
        if (curRound == DayRoundCount)
        {
            DayEnd();
            return;
        }

        // 增加回合数
        ++curRound;
        // 调用所有回合结束事件
        if (pause)
            pauseAction();
        else
        {
            roundEndAction();
            if (!pause)
                RoundEnd();
        }
    }

    /// <summary>
    /// 在回合结束时若当天结束 则自动调用此函数
    /// </summary>
    private void DayEnd()
    {
        // 检测游戏是否结束
        if (curDay == DayCount)
        {
            // 暂时未定整个游戏的逻辑是否写在这里
            return;
        }

        // 增加天数并重置回合数
        ++curDay;
        curRound = 1;
        // 调用所有一天结束事件
        dayEndAction();
    }
}
