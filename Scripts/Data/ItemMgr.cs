//#define DEBUG_ITEMMGR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MiniExcelLibs;

public class ItemMgr
{
    private static ItemMgr instance = new ItemMgr();
    public static ItemMgr Instance => instance;
    private ItemMgr()
    {
        string filePath = Application.streamingAssetsPath + "/Items.xlsx";
        foreach (var item in MiniExcel.Query<ItemDataRaw>(filePath, startCell: "A3").ToList())
            items.Add(item.ID, item);
#if DEBUG_ITEMMGR
        ItemDataRaw data = items[1];
        Debug.Log(data.ID);
        Debug.Log(data.Name);
        Debug.Log(data.Speed);
        Debug.Log(data.Courage);
        Debug.Log(data.Wisdom);
        Debug.Log(data.Kindness);
        Debug.Log(data.SpeedScale);
        Debug.Log(data.CourageScale);
        Debug.Log(data.WisdomScale);
        Debug.Log(data.Kindness);
        Debug.Log(data.LastRound);
#endif
    }

    // 提供物品总数量 方便检查错误
    public int itemCount => items.Count;
    private Dictionary<int, ItemDataRaw> items = new Dictionary<int, ItemDataRaw>();

    // 按物品ID获取物品信息
    public ItemData GetItem(int id)
    {
        if (items.ContainsKey(id))
            return CreateItemData(id, items[id]);
        return null;
    }
    // 获取所有物品信息
    public List<ItemData> GetAllItems()
    {
        List<ItemData> list = new List<ItemData>();
        foreach (var data in items)
            list.Add(CreateItemData(data.Key, data.Value));
        return list;
    }

    // 存储需要配置的信息
    private class ItemDataRaw
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public int Speed { get; set; }
        public int Courage { get; set; }
        public int Wisdom { get; set; }
        public int Kindness { get; set; }

        public float SpeedScale { get; set; }
        public float CourageScale { get; set; }
        public float WisdomScale { get; set; }
        public float KindnessScale { get; set; }

        public int LastRound { get; set; }
    }
    private ItemData CreateItemData(int id, ItemDataRaw raw)
    {
        int[] attr = new int[EmployeeData.attrNum];
        attr[0] = raw.Courage;
        attr[1] = raw.Wisdom;
        attr[2] = raw.Kindness;
        float[] attrScale = new float[EmployeeData.attrNum];
        attrScale[0] = raw.CourageScale;
        attrScale[1] = raw.WisdomScale;
        attrScale[2] = raw.KindnessScale;
        return new ItemData(id, raw.Name, raw.Speed, attr, raw.SpeedScale, attrScale, raw.LastRound);
    }
}
