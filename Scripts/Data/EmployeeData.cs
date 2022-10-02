using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeData
{
    // 名称与ID ID是员工的唯一标识符
    public int Id { get; private set; }
    public string Name { get; private set; }

    // 对外展示的只读属性
    public int Speed => CalcRealSpeed();
    public int Courage => CalcRealAttr(0);
    public int Wisdom => CalcRealAttr(1);
    public int Kindness => CalcRealAttr(2);
    // 道具数量
    public int ItemCount => itemList.Count;
    // 招募此人的价格
    public int Price { get; private set; }
    // Sprite在Resources的路径
    public string SpritePath { get; private set; }

    // 基础属性
    public const int attrNum = 3;
    private int basicSpeed;
    private int[] basicAttr = new int[attrNum];

    // 长期道具列表
    private List<ItemData> itemList = new List<ItemData>();

    // 构造函数
    public EmployeeData(int id, string name, int speed, int courage, int wisdom, int kindness, int price, string spritePath)
    {
        Id = id;
        Name = name;
        basicSpeed = speed;
        basicAttr[0] = courage;
        basicAttr[1] = wisdom;
        basicAttr[2] = kindness;
        Price = price;
        SpritePath = spritePath;
    }

    // 添加与减少道具
    public bool AddItem(int id)
    {
        ItemData item = ItemMgr.Instance.GetItem(id);
        if (item != null)
            itemList.Add(item);
        return item != null;
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

    /* 将速度/属性基础值乘以scale 然后增加num 传入负数就是减少 */
    public void UpdateSpeed(int num, float scale = 1) => UpdateValue(ref basicSpeed, num, scale);
    public void UpdateCourage(int num, float scale = 1) => UpdateValue(ref basicAttr[0], num, scale);
    public void UpdateWisdom(int num, float scale = 1) => UpdateValue(ref basicAttr[1], num, scale);
    public void UpdateKindness(int num, float scale = 1) => UpdateValue(ref basicAttr[2], num, scale);
    private void UpdateValue(ref int origin, int num, float scale)
    {
        origin = (int)(origin * scale);
        origin += num;
    }

    // 获取实际速度 逻辑暂时和属性相同
    private int CalcRealSpeed()
    {
        int speed = basicSpeed;
        // 先进行比例运算 将改变的比例线性相加
        float scale = 1;
        foreach (ItemData equip in itemList)
            scale += equip.SpeedScale - 1;
        speed = (int)(speed * scale);

        // 再增加绝对值
        foreach (ItemData equip in itemList)
            speed += equip.Speed;

        return speed;
    }

    /// <summary>
    /// 获取实际属性值 逻辑暂定
    /// </summary>
    /// <param name="index">属性值编号</param>
    private int CalcRealAttr(int index)
    {
        int attr = basicAttr[index];
        // 先进行比例运算 将改变的比例线性相加
        float scale = 1;
        foreach (ItemData equip in itemList)
            scale += equip.AttrScale[index] - 1;
        attr = (int)(attr * scale);

        // 再增加绝对值
        foreach (ItemData equip in itemList)
            attr += equip.Attr[index];

        return attr;
    }
}
