using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameManager : MonoBehaviour
{
    public static int curRound;// ��ǰ�غ���
    public static int curDay;// ��ǰ����
    public static int DayRoundCount; // �����ܻغ���
    public static int DayCount; // ������
    public static UnityAction roundEndAction;// �غϽ����¼�

    public static BasePostMan PostManReadyToMove;                //����׼���ƶ����ʵ�Ա
    public static List<BasePostMan> MyPostMen;                   //�������е��ʵ�Ա
    public static void ChangePostManReadyToMove(BasePostMan postMan) {
        PostManReadyToMove = postMan;
    }

    public static void AddRoundEndMonitor(UnityAction action) {
        roundEndAction += action;
    }

    public void Init(int dayNum, int[] dayRound) { } // ��ʼ�������ã�
}
