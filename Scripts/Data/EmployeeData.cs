using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeData
{
    // ������ID ID��Ա����Ψһ��ʶ��
    public int Id { get; private set; }
    public string Name { get; private set; }

    // ����չʾ��ֻ������
    public int Speed => CalcRealSpeed();
    public int Courage => CalcRealAttr(0);
    public int Wisdom => CalcRealAttr(1);
    public int Kindness => CalcRealAttr(2);
    // ��������
    public int ItemCount => itemList.Count;
    // ��ļ���˵ļ۸�
    public int Price { get; private set; }
    // Sprite��Resources��·��
    public string SpritePath { get; private set; }

    // ��������
    public const int attrNum = 3;
    private int basicSpeed;
    private int[] basicAttr = new int[attrNum];

    // ���ڵ����б�
    private List<ItemData> itemList = new List<ItemData>();

    // ���캯��
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

    /* ���ٶ�/���Ի���ֵ����scale Ȼ������num ���븺�����Ǽ��� */
    public void UpdateSpeed(int num, float scale = 1) => UpdateValue(ref basicSpeed, num, scale);
    public void UpdateCourage(int num, float scale = 1) => UpdateValue(ref basicAttr[0], num, scale);
    public void UpdateWisdom(int num, float scale = 1) => UpdateValue(ref basicAttr[1], num, scale);
    public void UpdateKindness(int num, float scale = 1) => UpdateValue(ref basicAttr[2], num, scale);
    private void UpdateValue(ref int origin, int num, float scale)
    {
        origin = (int)(origin * scale);
        origin += num;
    }

    // ��ȡʵ���ٶ� �߼���ʱ��������ͬ
    private int CalcRealSpeed()
    {
        int speed = basicSpeed;
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

    /// <summary>
    /// ��ȡʵ������ֵ �߼��ݶ�
    /// </summary>
    /// <param name="index">����ֵ���</param>
    private int CalcRealAttr(int index)
    {
        int attr = basicAttr[index];
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
}
