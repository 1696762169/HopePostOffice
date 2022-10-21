//#define DEBUG_EMPLOYEEMGR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MiniExcelLibs;
using System.Linq;

public class EmployeeMgr
{
    private static EmployeeMgr instance = new EmployeeMgr();
    public static EmployeeMgr Instance => instance;
    private EmployeeMgr()
    {
        string filePath = Application.streamingAssetsPath + "/Employees.xlsx";
        foreach (var employee in MiniExcel.Query<EmpDataRaw>(filePath, startCell: "A3").ToList())
            employees.Add(employee.ID, employee);
#if DEBUG_EMPLOYEEMGR
        EmpDataRaw data = employees[1];
        Debug.Log(data.ID);
        Debug.Log(data.Name);
        Debug.Log(data.Speed);
        Debug.Log(data.Courage);
        Debug.Log(data.Wisdom);
        Debug.Log(data.Kindness);
        Debug.Log(data.Price);
#endif
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
        public int ID { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }
        public int Courage { get; set; }
        public int Wisdom { get; set; }
        public int Kindness { get; set; }
        public int Price { get; set; }
        public string UpgradePrice { get; set; }
    }
    private EmployeeData CreateEmployeeData(int id, EmpDataRaw raw)
    {
        return new EmployeeData(id, raw.Name, raw.Speed, raw.Courage, raw.Wisdom, raw.Kindness, raw.Price, raw.UpgradePrice);
    }
}
