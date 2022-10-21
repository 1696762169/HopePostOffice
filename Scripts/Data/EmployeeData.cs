using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmployeeData
{
    // 名称与ID ID是员工的唯一标识符
    public int Id => id;
    [SerializeField]
    private int id;
    public string Name => name;
    [SerializeField]
    private string name;

    // 对外展示的只读属性数值
    public int Speed => CalcRealSpeed();
    public int Courage => CalcRealAttr(0);
    public int Wisdom => CalcRealAttr(1);
    public int Kindness => CalcRealAttr(2);
    // 只读属性等级
    public int SpeedLevel => speedLevel;
    [SerializeField]
    private int speedLevel;
    public int CourageLevel => attrLevel[0];
    public int WisdomLevel => attrLevel[1];
    public int KindnessLevel => attrLevel[2];

    // 道具数量
    public int ItemCount => itemList.Count;
    // 招募此人的价格
    public int Price => price;
    [SerializeField]
    private int price;
    public List<int> UpgradePrice => upgradePrice;
    [SerializeField]
    private List<int> upgradePrice = new List<int>();

    // 基础等级
    public const int attrNum = 3;
    [SerializeField]
    private int[] attrLevel = new int[attrNum];

    // 长期道具列表
    [SerializeField]
    private List<ItemData> itemList = new List<ItemData>();

    // 构造函数
    public EmployeeData(int id, string name, int speed, int courage, int wisdom, int kindness, int price, string up)
    {
        this.id = id;
        this.name = name;
        speedLevel = speed;
        attrLevel[0] = courage;
        attrLevel[1] = wisdom;
        attrLevel[2] = kindness;
        this.price = price;
        foreach (string str in up.Split(','))
            upgradePrice.Add(int.Parse(str.Trim()));
        if (upgradePrice.Count != 12)
            Debug.LogError($"{id}号员工的升级花费列表(UpgradePrice)元素数量不是12个");
    }

    // 添加与减少道具
    public ItemData AddItem(int id)
    {
        ItemData item = ItemMgr.Instance.GetItem(id);
        if (item != null)
            itemList.Add(item);
        return item;
    }
    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= itemList.Count)
            return null;
        return itemList[index];
    }
    public bool RemoveItem(int id)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] != null && itemList[i].Id == id && itemList[i].LastRound <= 0)
            {
                itemList.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /* 更新速度/属性值等级 */
    public void AddSpeed(int num) => speedLevel += num;
    public void AddCourage(int num) => attrLevel[0] += num;
    public void AddWisdom(int num) => attrLevel[1] += num;
    public void AddKindness(int num) => attrLevel[2] += num;

    // 获取实际速度 逻辑暂时和属性相同
    private int CalcRealSpeed()
    {
        int speed = SpeedLevelToValue();
        // 先进行比例运算 将改变的比例线性相加
        float scale = 1;
        foreach (ItemData equip in itemList)
            if (equip.LastRound > 0)
                scale += equip.SpeedScale - 1;
        scale += GlobalBuff.Instance.SpeedScale;
        speed = (int)(speed * scale);

        // 再增加绝对值
        foreach (ItemData equip in itemList)
            if (equip.LastRound > 0)
                speed += equip.Speed;
        speed += GlobalBuff.Instance.Speed;

        return speed;
    }
    // 根据速度等级得到基础速度值的转换公式
    private int SpeedLevelToValue()
    {
        return SpeedLevel;
    }

    /// <summary>
    /// 获取实际属性值 逻辑暂定
    /// </summary>
    /// <param name="index">属性值编号</param>
    private int CalcRealAttr(int index)
    {
        int attr = AttrLevelToValue(index);
        // 先进行比例运算 将改变的比例线性相加
        float scale = 1;
        foreach (ItemData equip in itemList)
            if (equip.LastRound > 0)
                scale += equip.AttrScale[index] - 1;
        scale += GlobalBuff.Instance.AttrScale[index];
        attr = (int)(attr * scale);

        // 再增加绝对值
        foreach (ItemData equip in itemList)
            if (equip.LastRound > 0)
                attr += equip.Attr[index];
        attr += GlobalBuff.Instance.Attr[index];

        return attr;
    }
    /// <summary>
    /// 根据属性值等级得到基础属性值的转换公式
    /// </summary>
    /// <param name="index">属性值编号</param>
    private int AttrLevelToValue(int index)
    {
        return attrLevel[index];
    }
}
