using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmployeeData
{
    // ������ID ID��Ա����Ψһ��ʶ��
    public int Id => id;
    [SerializeField]
    private int id;
    public string Name => name;
    [SerializeField]
    private string name;

    // ����չʾ��ֻ��������ֵ
    public int Speed => CalcRealSpeed();
    public int Courage => CalcRealAttr(0);
    public int Wisdom => CalcRealAttr(1);
    public int Kindness => CalcRealAttr(2);
    // ֻ�����Եȼ�
    public int SpeedLevel => speedLevel;
    [SerializeField]
    private int speedLevel;
    public int CourageLevel => attrLevel[0];
    public int WisdomLevel => attrLevel[1];
    public int KindnessLevel => attrLevel[2];

    // ��������
    public int ItemCount => itemList.Count;
    // ��ļ���˵ļ۸�
    public int Price => price;
    [SerializeField]
    private int price;
    public List<int> UpgradePrice => upgradePrice;
    [SerializeField]
    private List<int> upgradePrice = new List<int>();

    // �����ȼ�
    public const int attrNum = 3;
    [SerializeField]
    private int[] attrLevel = new int[attrNum];

    // ���ڵ����б�
    [SerializeField]
    private List<ItemData> itemList = new List<ItemData>();

    // ���캯��
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
            Debug.LogError($"{id}��Ա�������������б�(UpgradePrice)Ԫ����������12��");
    }

    // �������ٵ���
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

    /* �����ٶ�/����ֵ�ȼ� */
    public void AddSpeed(int num) => speedLevel += num;
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
            if (equip.LastRound > 0)
                scale += equip.SpeedScale - 1;
        scale += GlobalBuff.Instance.SpeedScale;
        speed = (int)(speed * scale);

        // �����Ӿ���ֵ
        foreach (ItemData equip in itemList)
            if (equip.LastRound > 0)
                speed += equip.Speed;
        speed += GlobalBuff.Instance.Speed;

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
            if (equip.LastRound > 0)
                scale += equip.AttrScale[index] - 1;
        scale += GlobalBuff.Instance.AttrScale[index];
        attr = (int)(attr * scale);

        // �����Ӿ���ֵ
        foreach (ItemData equip in itemList)
            if (equip.LastRound > 0)
                attr += equip.Attr[index];
        attr += GlobalBuff.Instance.Attr[index];

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
