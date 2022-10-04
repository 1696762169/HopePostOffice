#define DEBUG_ITEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    // ������ID ID��װ����Ψһ��ʶ��
    public int Id { get; private set; }
    public string Name { get; private set; }

    // ֻ����������
    public int Speed { get;  private set; }
    public int Courage => Attr[0];
    public int Wisdom => Attr[1];
    public int Kindness => Attr[2];
    public int[] Attr { get; private set; }

    // ֻ���ٷֱ�����
    public float SpeedScale { get; private set; }
    public float CourageScale => AttrScale[0];
    public float WisdomScale => AttrScale[1];
    public float KindnessScale => AttrScale[2];
    public float[] AttrScale { get; private set; }

    public int LastRound { get; private set; }      // ���߳����غ���
    public int Number { get; private set; }         // ���ߵ�����

    // װ�д˵��ߵ�װ����
    private EmployeeData employee = null;

    // ���캯��
    public ItemData() : this(int.MaxValue, null, 0, new int[EmployeeData.attrNum], 0, new float[EmployeeData.attrNum], 0) { }
    public ItemData(int id, string name, int speed, int[] attr, float speedScale, float[] attrScale, int lastRound)
    {
        // ��鴫�������Ƿ�����
        if (attr == null || attr.Length != EmployeeData.attrNum ||
            attrScale == null || attrScale.Length != EmployeeData.attrNum)
            Debug.LogError($"װ��{Name}��������");

        Id = id;
        Name = name;

        Speed = speed;
        SpeedScale = speedScale;
        LastRound = lastRound;
        Number = 0;

        Attr = new int[EmployeeData.attrNum];
        AttrScale = new float[EmployeeData.attrNum];
        for (int i = 0; i < EmployeeData.attrNum; i++)
        {
            Attr[i] = attr[i];
            AttrScale[i] = attrScale[i];
        }

    }
    public ItemData(ItemData item) : this(item.Id, item.Name, item.Speed, item.Attr, item.SpeedScale, item.AttrScale, item.LastRound) { }

    // Ϊһ��Ա��ʹ��һ���˵���
    public void UseTo(EmployeeData employee)
    {
        if (employee == null)
            return;

        --Number;
        // �ڴ˴�����Id��ʵ�ָ�����Ʒ���⹦��
        switch (Id)
        {
        default:
            break;
        }

        // ������Ʒ�������ټ��
        if (LastRound > 0)
        {
            this.employee = employee;
            employee.AddItem(Id);
            TimeMgr.Instance.RemoveRoundEvent(this.WearDown);
        }
    }

    // �����Ʒ��
    public void AddNumber(int num) => Number += num;

    // ���ڼ�ⳤ�ڵ��߻غ������Ƴ�����
    private void WearDown()
    {
        // ���ٳ����غ���
        --LastRound;
        // �Ƴ�����
        if (LastRound == 0)
        {
            employee.RemoveItem(Id);
            employee = null;
            TimeMgr.Instance.RemoveRoundEvent(this.WearDown);
        }
        TimeMgr.Instance.RoundEventDone();
    }

    // ��д�Ƚ��߼� �������б���ɾ��
    public override bool Equals(object obj)
    {
        if (obj is ItemData)
            return (obj as ItemData).Id == this.Id;
        else
            return false;
    }
    public override int GetHashCode() => base.GetHashCode();
}
