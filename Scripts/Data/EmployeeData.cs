using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeData
{
    // ������ID ID��Ա����Ψһ��ʶ��
    public int Id { get; private set; }
    public string Name { get; private set; }

    // ����չʾ��ֻ��������ֵ
    public int Speed => CalcRealSpeed();
    public int Courage => CalcRealAttr(0);
    public int Wisdom => CalcRealAttr(1);
    public int Kindness => CalcRealAttr(2);
    // ֻ�����Եȼ�
    public int SpeedLevel { get; private set; }
    public int CourageLevel => attrLevel[0];
    public int WisdomLevel => attrLevel[1];
    public int KindnessLevel => attrLevel[2];

    // ��������
    public int ItemCount => itemList.Count;
    // ��ļ���˵ļ۸�
    public int Price { get; private set; }

    // �����ȼ�
    public const int attrNum = 3;
    private int[] attrLevel = new int[attrNum];

    // ���ڵ����б�
    private List<ItemData> itemList = new List<ItemData>();

    // ���캯��
    public EmployeeData(int id, string name, int speed, int courage, int wisdom, int kindness, int price)
    {
        Id = id;
        Name = name;
        SpeedLevel = speed;
        attrLevel[0] = courage;
        attrLevel[1] = wisdom;
        attrLevel[2] = kindness;
        Price = price;
    }

    // �������ٵ���
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

    /* �����ٶ�/����ֵ�ȼ� */
    public void AddSpeed(int num) => SpeedLevel += num;
    public void AddCourage(int num) => attrLevel[0] += num;
    public void AddWisdom(int num) => attrLevel[1] += num;
    public void AddKindness(int num) => attrLevel[2] += num;

    // ��ȡʵ���ٶ� �߼���ʱ��������ͬ
    private int CalcRealSpeed()
    {
        int speed = SpeedLevelToValue();
        // �Ƚ��б������� ���ı�ı����������
        float scale = 1;
        foreach (ItemData equip in itemList)
            scale += equip.SpeedScale - 1;
        speed = (int)(speed * scale);

        // �����Ӿ���ֵ
        foreach (ItemData equip in itemList)
            speed += equip.Speed;

        return speed;
    }
    // �����ٶȵȼ��õ������ٶ�ֵ��ת����ʽ
    private int SpeedLevelToValue()
    {
        return SpeedLevel;
    }

    /// <summary>
    /// ��ȡʵ������ֵ �߼��ݶ�
    /// </summary>
    /// <param name="index">����ֵ���</param>
    private int CalcRealAttr(int index)
    {
        int attr = AttrLevelToValue(index);
        // �Ƚ��б������� ���ı�ı����������
        float scale = 1;
        foreach (ItemData equip in itemList)
            scale += equip.AttrScale[index] - 1;
        attr = (int)(attr * scale);

        // �����Ӿ���ֵ
        foreach (ItemData equip in itemList)
            attr += equip.Attr[index];

        return attr;
    }
    /// <summary>
    /// ��������ֵ�ȼ��õ���������ֵ��ת����ʽ
    /// </summary>
    /// <param name="index">����ֵ���</param>
    private int AttrLevelToValue(int index)
    {
        return attrLevel[index];
    }
}
