using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeMgr
{
    private static EmployeeMgr instance = new EmployeeMgr();
    public static EmployeeMgr Instance => instance;
    private EmployeeMgr()
    {
        // ��������
    }

    // �ṩԱ�������� ���������
    public int employeeCount => employees.Count;
    private Dictionary<int, EmpDataRaw> employees = new Dictionary<int, EmpDataRaw>();

    // ��Ա��ID��ȡԱ����Ϣ
    public EmployeeData GetEmployee(int id)
    {
        if (employees.ContainsKey(id))
            return CreateEmployeeData(id, employees[id]);
        return null;
    }
    // ��ȡȫ��Ա����Ϣ
    public List<EmployeeData> GetAllEmployees()
    {
        List<EmployeeData> list = new List<EmployeeData>();
        foreach (var data in employees)
            list.Add(CreateEmployeeData(data.Key, data.Value));
        return list;
    }

    // �洢��Ҫ���õ���Ϣ
    private class EmpDataRaw
    {
        public string name;
        public int speed;
        public int courage;
        public int wisdom;
        public int kindness;
        public int price;
        public string spritePath;
    }
    private EmployeeData CreateEmployeeData(int id, EmpDataRaw raw)
    {
        return new EmployeeData(id, raw.name, raw.speed, raw.courage, raw.wisdom, raw.kindness, raw.price, raw.spritePath);
    }
}
