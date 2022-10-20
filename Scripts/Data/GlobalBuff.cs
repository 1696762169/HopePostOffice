using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniExcelLibs;
using System.Linq;
using System.IO;

/// <summary>
/// ȫ��Buff��
/// </summary>
public class GlobalBuff
{
    private static GlobalBuff instance = new GlobalBuff();
    public static GlobalBuff Instance => instance;
    private GlobalBuff()
    {
        if (loaded)
            return;
        loaded = true;

        // ��ȡ�ļ�
        if (!File.Exists(filePath))
            Save();
        else
            instance = JsonUtility.FromJson<GlobalBuff>(File.ReadAllText(filePath));

        // �غϽ���ʱ����
        TimeMgr.Instance.roundEndAction += Save;

        // һ�����ʱ���������
        TimeMgr.Instance.dayEndAction += () =>
        {
            Warehouse.Instance.courage = Mathf.Max(Warehouse.Instance.courage + CourageEveryDay, 0);
            Warehouse.Instance.wisdom = Mathf.Max(Warehouse.Instance.wisdom + WisdomEveryDay, 0);
            Warehouse.Instance.kindness = Mathf.Max(Warehouse.Instance.kindness + KindnessEveryDay, 0);
        };
    }

    readonly string filePath = Application.persistentDataPath + "/GlobalBuff.json";
    public static bool loaded = false;

    protected void Save()
    {
        File.WriteAllText(filePath, JsonUtility.ToJson(instance));
    }

    // �ٶȼӳ�
    public int Speed;
    // ���Լӳ�
    public int Courage { get => Attr[0]; set => Attr[0] = value; }
    public int Wisdom { get => Attr[1]; set => Attr[1] = value; }
    public int Kindness { get => Attr[2]; set => Attr[2] = value; }
    public int[] Attr = new int[EmployeeData.attrNum];

    // �ٶȱ����ӳ�
    public float SpeedScale;
    // ���Ա����ӳ�
    public float CourageScale { get => AttrScale[0]; set => AttrScale[0] = value; }
    public float WisdomScale { get => AttrScale[1]; set => AttrScale[1] = value; }
    public float KindnessScale { get => AttrScale[2]; set => AttrScale[2] = value; }
    public float[] AttrScale = new float[EmployeeData.attrNum];

    // ÿ������������
    public int CourageEveryDay { get => PointEveryDay[0]; set => PointEveryDay[0] = value; }
    public int WisdomEveryDay { get => PointEveryDay[1]; set => PointEveryDay[1] = value; }
    public int KindnessEveryDay { get => PointEveryDay[2]; set => PointEveryDay[2] = value; }
    public int[] PointEveryDay = new int[EmployeeData.attrNum];

    private class GBRaw
    {
        // ���Լӳ�
        public int Speed { get; set; }
        public int Courage { get; set; }
        public int Wisdom { get; set; }
        public int Kindness { get; set; }

        // �����ӳ�
        public float SpeedScale { get; set; }
        public float CourageScale { get; set; }
        public float WisdomScale { get; set; }
        public float KindnessScale { get; set; }

        // ÿ������������
        public int CourageEveryDay { get; set; }
        public int WisdomEveryDay { get; set; }
        public int KindnessEveryDay { get; set; }
    }
}
