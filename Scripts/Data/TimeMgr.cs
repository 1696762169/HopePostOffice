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

    public event UnityAction roundEndAction;     // �غϽ����¼�
    public event UnityAction dayEndAction;       // һ������¼�

    public bool pause;                           // ��Ϸ��ͣ��ʶ��
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

        // ��ȡ����غ���
        dayRoundList = new List<int>() { 20, 20, 20, 20, 25, 30, 30 };
        Debug.LogWarning("��δʵ�ָ���غ������ļ���ȡ");
        if (dayRoundList == null)
            Debug.LogError("δ��ȡ������غ���");
    }

    /// <summary>
    /// ���ⲿ���ý���һ���غ�
    /// </summary>
    public void RoundEnd()
    {
        // ��⵱���Ƿ����
        if (curRound == DayRoundCount)
        {
            DayEnd();
            return;
        }

        // ���ӻغ���
        ++curRound;
        // �������лغϽ����¼�
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
