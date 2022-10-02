using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeMgr
{
    private static EmployeeMgr instance = new EmployeeMgr();
    public static EmployeeMgr Instance => instance;
    private EmployeeMgr()
    {
        // 读入数据
    }

    // 提供员工总数量 方便检查错误
    public int employeeCount => employees.Count;
    private Dictionary<int, EmpDataRaw> employees = new Dictionary<int, EmpDataRaw>();

    // 按员工ID获取员工信息
    public EmployeeData GetEmployee(int id)
    {
        if (employees.ContainsKey(id))
            return CreateEmployeeData(id, employees[id]);
        return null;
    }
    // 获取全部员工信息
    public List<EmployeeData> GetAllEmployees()
    {
        List<EmployeeData> list = new List<EmployeeData>();
        foreach (var data in employees)
            list.Add(CreateEmployeeData(data.Key, data.Value));
        return list;
    }

    // 存储需要配置的信息
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
