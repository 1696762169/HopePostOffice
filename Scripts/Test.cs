using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        //EmployeeMgr.Instance.ToString();
        //ItemMgr.Instance.ToString();
        GlobalBuff.Instance.ToString();
        Warehouse.Instance.Init();
    }
}
