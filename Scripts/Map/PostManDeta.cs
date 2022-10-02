using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PostManDeta
{
    public int MovePoint;
    public int[] Attribute=new int[3];
    public int EquipMaxNum;

    PostManDeta(int movePoint,int courage,int wisdom,int kindness) {
        MovePoint = movePoint;
        Attribute[0] = courage;
        Attribute[1] = wisdom;
        Attribute[2] = kindness;
    }

    public void UpDateAttribue(int num,int index,float scale=1) {
        Attribute[index] = (int)(Attribute[index]* scale);
        Attribute[index] += num;
    }

}
