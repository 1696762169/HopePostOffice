#define DEBUG_ITEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    // 名称与ID ID是装备的唯一标识符
    public int Id { get; private set; }
    public string Name { get; private set; }

    // 只读增加属性
    public int Speed { get;  private set; }
    public int Courage => Attr[0];
    public int Wisdom => Attr[1];
    public int Kindness => Attr[2];
    public int[] Attr { get; private set; }

    // 只读百分比属性
    public float SpeedScale { get; private set; }
    public float CourageScale => AttrScale[0];
    public float WisdomScale => AttrScale[1];
    public float KindnessScale => AttrScale[2];
    public float[] AttrScale { get; private set; }

    public int LastRound { get; private set; }      // 道具持续回合数
    public int Number { get; private set; }         // 道具的数量

    // 装有此道具的装备者
    private EmployeeData employee = null;

    // 构造函数
    public ItemData() : this(int.MaxValue, null, 0, new int[EmployeeData.attrNum], 0, new float[EmployeeData.attrNum], 0) { }
    public ItemData(int id, string name, int speed, int[] attr, float speedScale, float[] attrScale, int lastRound)
    {
        // 检查传入数据是否有误
        if (attr == null || attr.Length != EmployeeData.attrNum ||
            attrScale == null || attrScale.Length != EmployeeData.attrNum)
            Debug.LogError($"装备{Name}数据有误");

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

    // 为一个员工使用一个此道具
    public void UseTo(EmployeeData employee)
    {
        if (employee == null)
            return;

        --Number;
        // 在此处根据Id号实现各个物品特殊功能
        switch (Id)
        {
        default:
            break;
        }

        // 长期物品需添加损毁检测
        if (LastRound > 0)
        {
            this.employee = employee;
            employee.AddItem(Id);
            TimeMgr.Instance.RemoveRoundEvent(this.WearDown);
        }
    }

    // 添加物品数
    public void AddNumber(int num) => Number += num;

    // 用于检测长期道具回合数并移除自身
    private void WearDown()
    {
        // 减少持续回合数
        --LastRound;
        // 移除自身
        if (LastRound == 0)
        {
            employee.RemoveItem(Id);
            employee = null;
            TimeMgr.Instance.RemoveRoundEvent(this.WearDown);
        }
        TimeMgr.Instance.RoundEventDone();
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
