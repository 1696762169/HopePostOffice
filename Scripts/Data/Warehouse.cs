#define DEBUG_WAREHOUSE
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Warehouse
{
    private static Warehouse instance = new Warehouse();
    public static Warehouse Instance => instance;
    protected const string warehousePath = "Warehouse.json";

    // 钱和属性值
    public int money = 0;
    public int courage = 0;
    public int wisdom = 0;
    public int kindness = 0;

    // 查看列表的元素个数 方便遍历
    public int EmployeeCount => employees.Count;
    public int ItemCount => items.Count;
    public int TotalItemCount => totalItems.Count;

    [SerializeField]
    private List<EmployeeData> employees = new List<EmployeeData>();    // 已招募员工列表
    [SerializeField]
    private List<ItemData> items = new List<ItemData>();                // 当天使用的物品
    [SerializeField]
    private List<ItemData> totalItems = new List<ItemData>();           // 玩家拥有的所有物品

    // 由游戏管理器进行初始化 防止在TimeMgr对象生成前初始化导致的错误
    private bool inited = false;
    public void Init()
    {
        if (inited)
            return;
        inited = true;

        // 加载数据
        LoadOrInitData();

        // 一天结束时清空当天要用的物品
        TimeMgr.Instance.dayEndAction += () =>
        {
            items.Clear();
        };
    }

    // 获得一个员工信息
    public EmployeeData GetEmployee(int index)
    {
        if (index < 0 || index >= employees.Count)
            return null;
        else
            return employees[index];
    }
    // 加一个员工 尝试加入不存在的id会失败
    public bool AddEmployee(int id)
    {
        EmployeeData data = EmployeeMgr.Instance.GetEmployee(id);
        if (data != null)
            employees.Add(data);
        return data != null;
    }

    // 获得一个今天要用的物品
    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;
        else
            return items[index];
    }
    // 加今天要用的物品 尝试加入仓库里没有的东西会失败
    public bool AddItem(int id)
    {
        // 已经存在的物品就不需要加入了 但仍然返回加入成功
        foreach (ItemData item in items)
        {
            if (item.Id == id)
                return true;
        }

        // 查找仓库里的物品 加入其引用
        for (int i = 0; i < TotalItemCount; i++)
        {
            if (totalItems[i].Id == id)
            {
                items.Add(totalItems[i]);
                return true;
            }
        }
        return false;
    }

    // 获得一个仓库里的物品
    public ItemData GetTotalItem(int index)
    {
        if (index < 0 || index >= totalItems.Count)
            return null;
        else
            return totalItems[index];
    }
    // 加物品到仓库 尝试加入不存在的id会失败
    public bool AddTotalItem(int id, int num)
    {
        // 若已有该类型的物品 则直接增加数量
        for (int i = 0; i < TotalItemCount; i++)
        {
            if (totalItems[i].Id == id)
            {
                totalItems[i].AddNumber(num);
                return true;
            }
        }

        // 添加新物品
        ItemData item = ItemMgr.Instance.GetItem(id);
        if (item != null)
        {
            item.AddNumber(num);
            items.Add(item);
        }
        return item != null;
    }

    // 使用物品 参数是物品和员工ID
    public bool UseItem(int itemId, int empId)
    {
        int itemIndex = -1, empIndex = -1;
        // 查找物品
        for (int i = 0; i < ItemCount; i++)
        {
            if (items[i].Id == itemId)
            {
                itemIndex = i;
                break;
            }
        }
        if (itemIndex == -1)
            return false;

        // 查找员工
        for (int i = 0; i < EmployeeCount; i++)
        {
            if (employees[i].Id == empId)
            {
                empIndex = i;
                break;
            }
        }
        if (empIndex == -1)
            return false;

        items[itemIndex].UseTo(employees[empIndex]);
        RemoveEmptyItem();
        return true;
    }
    // 使用物品 参数是物品/员工在列表中的位置
    public bool UseItemInList(int itemIndex, int empIndex)
    {
        if (itemIndex < 0 || itemIndex >= items.Count ||
            empIndex < 0 || empIndex >= employees.Count)
            return false;
        items[itemIndex].UseTo(employees[empIndex]);
        return true;
    }

    /// <summary>
    /// 移除已经用完的物品
    /// </summary>
    /// <returns>移除的物品数</returns>
    protected int RemoveEmptyItem()
    {
        int removeCount = 0;
        for (int i = TotalItemCount - 1; i >= 0; i++)
        {
            if (totalItems[i] == null || totalItems[i].Number <= 0)
            {
                totalItems.RemoveAt(i);
                removeCount++;
            }
        }
        for (int i = ItemCount - 1; i >= 0; i++)
        {
            if (items[i] == null || items[i].Number <= 0)
                items.RemoveAt(i);
        }
        return removeCount;
    }

    /// <summary>
    /// 将当前仓库数据写入文件
    /// </summary>
    public void SaveData()
    {
        string path = Application.persistentDataPath + "/" + warehousePath;
        File.WriteAllText(path, JsonUtility.ToJson(instance, true));
    }

    // 从文件中读取数据 若找不到文件则使用初始化数据
    protected void LoadOrInitData()
    {
        string path = Application.persistentDataPath + "/" + warehousePath;
        if (File.Exists(path))
            instance = JsonUtility.FromJson<Warehouse>(File.ReadAllText(path));
        else
            InitData();
    }
    // 初始化数据并存入文件
    protected void InitData()
    {
#if DEBUG_WAREHOUSE
        employees.Add(EmployeeMgr.Instance.GetEmployee(1));
        totalItems.Add(ItemMgr.Instance.GetItem(1));
#endif
        SaveData();
    }
}
