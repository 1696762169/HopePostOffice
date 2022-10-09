//#define DEBUG_ITEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    // ������ID ID��װ����Ψһ��ʶ��
    public int Id => id;
    [SerializeField]
    private int id;
    public string Name => name;
    [SerializeField]
    private string name;

    // ֻ����������
    public int Speed => speed;
    [SerializeField]
    private int speed;
    public int Courage => Attr[0];
    public int Wisdom => Attr[1];
    public int Kindness => Attr[2];
    public int[] Attr => attr;
    [SerializeField]
    private int[] attr;

    // ֻ���ٷֱ�����
    public float SpeedScale => speedScale;
    [SerializeField]
    private float speedScale;
    public float CourageScale => AttrScale[0];
    public float WisdomScale => AttrScale[1];
    public float KindnessScale => AttrScale[2];
    public float[] AttrScale => attrScale;
    [SerializeField]
    private float[] attrScale;

    public int Price => price;              // ���߹���۸�
    [SerializeField]
    private int price;

    public int LastRound => lastRound;     // ���߳����غ���
    [SerializeField]
    private int lastRound;
    public int Number => number;         // ���ߵ�����
    [SerializeField]
    private int number;

    // ���캯��
    public ItemData() : this(int.MaxValue, null, 0, new int[EmployeeData.attrNum], 0, new float[EmployeeData.attrNum], 0, 0) { }
    public ItemData(int id, string name, int speed, int[] attr, float speedScale, float[] attrScale, int price, int lastRound)
    {
        // ��鴫�������Ƿ�����
        if (attr == null || attr.Length != EmployeeData.attrNum ||
            attrScale == null || attrScale.Length != EmployeeData.attrNum)
            Debug.LogError($"װ��{Name}��������");

        this.id = id;
        this.name = name;

        this.speed = speed;
        this.speedScale = speedScale;
        this.lastRound = lastRound;
        this.price = price;
        this.number = 0;

        this.attr = new int[EmployeeData.attrNum];
        this.attrScale = new float[EmployeeData.attrNum];
        for (int i = 0; i < EmployeeData.attrNum; i++)
        {
            Attr[i] = attr[i];
            AttrScale[i] = attrScale[i];
        }

    }
    public ItemData(ItemData item) : this(item.Id, item.Name, item.Speed, item.Attr, item.SpeedScale, item.AttrScale, item.price, item.LastRound) { }

    // Ϊһ��Ա��ʹ��һ���˵���
    public void UseTo(EmployeeData employee)
    {
        if (employee == null)
            return;

        --number;
        // �ڴ˴�����Id��ʵ�ָ�����Ʒ���⹦��
        switch (Id)
        {
        default:
            break;
        }

        // ������Ʒ�������ټ��
        if (LastRound > 0)
        {
            employee.AddItem(Id);
            TimeMgr.Instance.roundEndAction += this.WearDown;
        }
    }

    // �����Ʒ��
    public void AddNumber(int num) => number += num;

    // ���ڼ�ⳤ�ڵ��߻غ������Ƴ�����
    private void WearDown()
    {
        // ���ٳ����غ���
        --lastRound;
        // �Ƴ�����
        if (LastRound == 0)
        {
            TimeMgr.Instance.roundEndAction -= this.WearDown;
        }
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
