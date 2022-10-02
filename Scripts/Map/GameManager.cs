using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameManager : MonoBehaviour
{
    public static int curRound;// 当前回合数
    public static int curDay;// 当前天数
    public static int DayRoundCount; // 当天总回合数
    public static int DayCount; // 总天数
    public static UnityAction roundEndAction;// 回合结束事件

    public static BasePostMan PostManReadyToMove;                //正在准备移动的邮递员
    public static List<BasePostMan> MyPostMen;                   //场上所有的邮递员
    public static void ChangePostManReadyToMove(BasePostMan postMan) {
        PostManReadyToMove = postMan;
    }

    public static void AddRoundEndMonitor(UnityAction action) {
        roundEndAction += action;
    }

    public void Init(int dayNum, int[] dayRound) { } // 初始化（重置）
}
