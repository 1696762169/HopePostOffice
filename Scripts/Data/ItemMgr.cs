using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMgr
{
    private static ItemMgr instance = new ItemMgr();
    public static ItemMgr Instance => instance;
    private ItemMgr()
    {

    }

    // �ṩ��Ʒ������ ���������
    public int itemCount => items.Count;
    private Dictionary<int, ItemDataRaw> items = new Dictionary<int, ItemDataRaw>();

    // ����ƷID��ȡ��Ʒ��Ϣ
    public ItemData GetItem(int id)
    {
        if (items.ContainsKey(id))
            return CreateItemData(id, items[id]);
        return null;
    }
    // ��ȡ������Ʒ��Ϣ
    public List<ItemData> GetAllItems()
    {
        List<ItemData> list = new List<ItemData>();
        foreach (var data in items)
            list.Add(CreateItemData(data.Key, data.Value));
        return list;
    }

    // �洢��Ҫ���õ���Ϣ
    private class ItemDataRaw
    {
        public string name;

        public int speed;
        public int courage;
        public int wisdom;
        public int kindness;

        public float speedScale;
        public float courageScale;
        public float wisdomScale;
        public float kindnessScale;

        public int lastRound;
        public string spritePath;
    }
    private ItemData CreateItemData(int id, ItemDataRaw raw)
    {
        int[] attr = new int[EmployeeData.attrNum];
        attr[0] = raw.courage;
        attr[1] = raw.wisdom;
        attr[2] = raw.kindness;
        float[] attrScale = new float[EmployeeData.attrNum];
        attrScale[0] = raw.courageScale;
        attrScale[1] = raw.wisdomScale;
        attrScale[2] = raw.kindnessScale;
        return new ItemData(id, raw.name, raw.speed, attr, raw.speedScale, attrScale, raw.lastRound, raw.spritePath);
    }
}
