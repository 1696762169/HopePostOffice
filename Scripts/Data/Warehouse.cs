using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warehouse
{
    private static Warehouse instance = new Warehouse();
    public static Warehouse Instance => instance;
    private Warehouse() { }

    // Ǯ������ֵ
    public int money;
    public int courage;
    public int wisdom;
    public int kindness;

    // �鿴�б��Ԫ�ظ��� �������
    public int EmployeeCount => employees.Count;
    public int ItemCount => items.Count;
    public int TotalItemCount => totalItems.Count;

    private List<EmployeeData> employees = new List<EmployeeData>();    // ����ļԱ���б�
    private List<ItemData> items = new List<ItemData>();                // ����ʹ�õ���Ʒ
    private List<ItemData> totalItems = new List<ItemData>();           // ���ӵ�е�������Ʒ

    // ����Ϸ���������г�ʼ�� ��ֹ��TimeMgr��������ǰ��ʼ�����µĴ���
    private bool inited = false;
    public void Init()
    {
        if (inited)
            return;
        inited = true;

        // һ�����ʱ��յ���Ҫ�õ���Ʒ
        TimeMgr.Instance.dayEndAction += () =>
        {
            items.Clear();
        };
    }

    // ���һ��Ա����Ϣ
    public EmployeeData GetEmployee(int index)
    {
        if (index < 0 || index >= employees.Count)
            return null;
        else
            return employees[index];
    }
    // ��һ��Ա�� ���Լ��벻���ڵ�id��ʧ��
    public bool AddEmployee(int id)
    {
        EmployeeData data = EmployeeMgr.Instance.GetEmployee(id);
        if (data != null)
            employees.Add(data);
        return data != null;
    }

    // ���һ������Ҫ�õ���Ʒ
    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;
        else
            return items[index];
    }
    // �ӽ���Ҫ�õ���Ʒ ���Լ���ֿ���û�еĶ�����ʧ��
    public bool AddItem(int id)
    {
        // �Ѿ����ڵ���Ʒ�Ͳ���Ҫ������ ����Ȼ���ؼ���ɹ�
        foreach (ItemData item in items)
        {
            if (item.Id == id)
                return true;
        }

        // ���Ҳֿ������Ʒ ����������
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

    // ���һ���ֿ������Ʒ
    public ItemData GetTotalItem(int index)
    {
        if (index < 0 || index >= totalItems.Count)
            return null;
        else
            return totalItems[index];
    }
    // ����Ʒ���ֿ� ���Լ��벻���ڵ�id��ʧ��
    public bool AddTotalItem(int id, int num)
    {
        // �����и����͵���Ʒ ��ֱ����������
        for (int i = 0; i < TotalItemCount; i++)
        {
            if (totalItems[i].Id == id)
            {
                totalItems[i].AddNumber(num);
                return true;
            }
        }

        // �������Ʒ
        ItemData item = ItemMgr.Instance.GetItem(id);
        if (item != null)
        {
            item.AddNumber(num);
            items.Add(item);
        }
        return item != null;
    }

    // ʹ����Ʒ ��������Ʒ��Ա��ID
    public bool UseItem(int itemId, int empId)
    {
        int itemIndex = -1, empIndex = -1;
        // ������Ʒ
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

        // ����Ա��
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
        return true;
    }
    // ʹ����Ʒ ��������Ʒ/Ա�����б��е�λ��
    public bool UseItemInList(int itemIndex, int empIndex)
    {
        if (itemIndex < 0 || itemIndex >= items.Count ||
            empIndex < 0 || empIndex >= employees.Count)
            return false;
        items[itemIndex].UseTo(employees[empIndex]);
        return true;
    }

    /// <summary>
    /// �Ƴ��Ѿ��������Ʒ
    /// </summary>
    /// <returns>�Ƴ�����Ʒ��</returns>
    public int RemoveEmptyItem()
    {
        int removeCount = 0;
        for (int i = TotalItemCount; i > 0; i++)
        {
            if (totalItems[i] == null || totalItems[i].Number <= 0)
            {
                totalItems.RemoveAt(i);
                removeCount++;
            }
        }
        for (int i = ItemCount; i > 0; i++)
        {
            if (items[i] == null || items[i].Number <= 0)
                items.RemoveAt(i);
        }
        return removeCount;
    }
}
