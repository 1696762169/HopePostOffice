//#define DEBUG_ITEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    // 名称与ID ID是装备的唯一标识符
    public int Id => id;
    [SerializeField]
    private int id;
    public string Name => name;
    [SerializeField]
    private string name;

    // 只读增加属性
    public int Speed => speed;
    [SerializeField]
    private int speed;
    public int Courage => Attr[0];
    public int Wisdom => Attr[1];
    public int Kindness => Attr[2];
    public int[] Attr => attr;
    [SerializeField]
    private int[] attr;

    // 只读百分比属性
    public float SpeedScale => speedScale;
    [SerializeField]
    private float speedScale;
    public float CourageScale => AttrScale[0];
    public float WisdomScale => AttrScale[1];
    public float KindnessScale => AttrScale[2];
    public float[] AttrScale => attrScale;
    [SerializeField]
    private float[] attrScale;

    public int Price => price;              // 道具购买价格
    [SerializeField]
    private int price;

    public int LastRound => lastRound;     // 道具持续回合数
    [SerializeField]
    private int lastRound;
    public int Number => number;         // 道具的数量
    [SerializeField]
    private int number;

    // 构造函数
    public ItemData() : this(int.MaxValue, null, 0, new int[EmployeeData.attrNum], 0, new float[EmployeeData.attrNum], 0, 0) { }
    public ItemData(int id, string name, int speed, int[] attr, float speedScale, float[] attrScale, int price, int lastRound)
    {
        // 检查传入数据是否有误
        if (attr == null || attr.Length != EmployeeData.attrNum ||
            attrScale == null || attrScale.Length != EmployeeData.attrNum)
            Debug.LogError($"装备{Name}数据有误");

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

    // 为一个员工使用一个此道具
    public void UseTo(EmployeeData employee)
    {
        if (employee == null)
            return;

        --number;
        // 在此处根据Id号实现各个物品特殊功能
        switch (Id)
        {
        default:
            break;
        }

        // 长期物品需添加损毁检测
        if (LastRound > 0)
        {
            employee.AddItem(Id);
            TimeMgr.Instance.roundEndAction += this.WearDown;
        }
    }

    // 添加物品数
    public void AddNumber(int num) => number += num;

    // 用于检测长期道具回合数并移除自身
    private void WearDown()
    {
        // 减少持续回合数
        --lastRound;
        // 移除自身
        if (LastRound == 0)
        {
            TimeMgr.Instance.roundEndAction -= this.WearDown;
        }
    }

    // 重写比较逻辑 用于在列表中删除
    public override bool Equals(object obj)
    {
        if (obj is ItemData)
            return (obj as ItemData).Id == this.Id;
        else
            return false;
    }
    public override int GetHashCode() => base.GetHashCode();
}
