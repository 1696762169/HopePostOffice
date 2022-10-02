using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniExcelLibs;
using System.Linq;

/// <summary>
/// 全局Buff类
/// </summary>
public class GlobalBuff
{
    private static GlobalBuff instance = new GlobalBuff();
    public static GlobalBuff Instance => instance;
    private GlobalBuff()
    {
        string filePath = Application.streamingAssetsPath + "/GlobalBuff.xlsx";
        GBRaw gb = MiniExcel.Query<GBRaw>(filePath, startCell: "A3").ToList()[0];

        Attr = new int[EmployeeData.attrNum];
        AttrScale = new float[EmployeeData.attrNum];

        Speed = gb.Speed;
        Courage = gb.Courage;
        Wisdom = gb.Wisdom;
        Kindness = gb.Kindness;
        SpeedScale = gb.SpeedScale;
        CourageScale = gb.CourageScale;
        WisdomScale = gb.WisdomScale;
        KindnessScale = gb.KindnessScale;
    }

    // 速度加成
    public int Speed;
    // 属性加成
    public int Courage { get => Attr[0]; set => Attr[0] = value; }
    public int Wisdom { get => Attr[1]; set => Attr[1] = value; }
    public int Kindness { get => Attr[2]; set => Attr[2] = value; }
    public int[] Attr;

    // 速度比例加成
    public float SpeedScale;
    // 属性比例加成
    public float CourageScale { get => AttrScale[0]; set => AttrScale[0] = value; }
    public float WisdomScale { get => AttrScale[1]; set => AttrScale[1] = value; }
    public float KindnessScale { get => AttrScale[2]; set => AttrScale[2] = value; }
    public float[] AttrScale;

    private class GBRaw
    {
        public int Speed { get; set; }
        // 属性加成
        public int Courage { get; set; }
        public int Wisdom { get; set; }
        public int Kindness { get; set; }

        // 速度比例加成
        public float SpeedScale { get; set; }
        // 属性比例加成
        public float CourageScale { get; set; }
        public float WisdomScale { get; set; }
        public float KindnessScale { get; set; }
    }
}
