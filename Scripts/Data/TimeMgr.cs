using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeMgr
{
    private static TimeMgr instance = new TimeMgr();
    public static TimeMgr Instance => instance;
    private TimeMgr() { }

    public int curRound;    // ��ǰ�غ���
    public int curDay;      // ��ǰ����
    public int DayRoundCount => dayRoundList[curDay - 1];   // ��ǰ��һ����ܻغ���
    public int DayCount => dayRoundList.Count;              // ������
    private List<int> dayRoundList;                         // ������ܻغ���

    public event UnityAction dayEndAction;       // һ������¼�
    private event UnityAction roundEndAction;    // �غϽ����¼�
    private int roundEventCount = 0;             // ÿ���غ��¼�����
    private int lastRoundEvent = 0;              // ��ǰ�غ�ʣ��Ļ�û������¼�

    public bool Pause                            // ��Ϸ��ͣ��ʶ��
    {
        get => pause;
        set
        {
            pause = value;
            if (!pause && lastRoundEvent == 0)
                RoundEnd();
        }
    }
    private bool pause;
    public event UnityAction pauseAction;        // ��Ϸ�ڲ���ͣ�¼�  

    private bool inited = false;    // ��ʼ����ʶ��

    /// <summary>
    /// ����Ϸ���������г�ʼ�� ��ȡ��������
    /// </summary>
    public void Init()
    {
        // ��ֹ�ظ���ʼ��
        if (inited)
            return;
        inited = true;

        // �趨��ʼ�������ͻغ���
        curRound = 1;
        curDay = 1;
        // ȡ����ͣ
        pause = false;

        // ��ȡ����غ���
        dayRoundList = new List<int>() { 20, 20, 20, 20, 25, 30, 30 };
        Debug.LogWarning("��δʵ�ָ���غ������ļ���ȡ");
        if (dayRoundList == null)
            Debug.LogError("δ��ȡ������غ���");
    }

    // ���/�Ƴ��غϽ����¼�
    public void AddRoundEvent(UnityAction action)
    {
        roundEndAction += action;
        ++roundEventCount;
    }
    public void RemoveRoundEvent(UnityAction action)
    {
        roundEndAction -= action;
        --roundEventCount;
    }
    // �غϽ����¼���� ������ÿ����ӵ��¼������ʱ����
    public void RoundEventDone()
    {
        --lastRoundEvent;
        if (lastRoundEvent == 0)
            RoundEnd();
    }

    /// <summary>
    /// ���ⲿ���ý���һ���غ�
    /// </summary>
    public void RoundEnd()
    {
        // ��ͣʱ��������
        if (pause)
            pauseAction();
        else
        {
            // ��⵱���Ƿ����
            if (curRound == DayRoundCount)
            {
                DayEnd();
                return;
            }
            lastRoundEvent = roundEventCount;
            // ���ӻغ���
            ++curRound;
            // �������лغϽ����¼�
            roundEndAction();
        }
    }

    /// <summary>
    /// �ڻغϽ���ʱ��������� ���Զ����ô˺���
    /// </summary>
    private void DayEnd()
    {
        // �����Ϸ�Ƿ����
        if (curDay == DayCount)
        {
            // ��ʱδ��������Ϸ���߼��Ƿ�д������
            return;
        }

        // �������������ûغ���
        ++curDay;
        curRound = 1;
        // ��������һ������¼�
        dayEndAction();
    }
}
