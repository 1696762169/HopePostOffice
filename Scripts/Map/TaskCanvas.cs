//#define DEBUG_TASKCANVAS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TaskCanvas : MonoBehaviour
{
    // ����ϵĿؼ�
    public Button summitBtn;
    public Button cancelBtn;

    public Text nameText;
    public Text courageText;
    public Text wisdomText;
    public Text kindnessText;

    // ���λ��
    protected RectTransform m_Trans;
    protected Vector3 m_TempPos;
    protected Vector2 m_Pos;

    // ������ڵĵؿ�
    protected BaseBlock m_Block;
    [Tooltip("�����Եؿ�λ��ƫ����")]
    public Vector3 DeltaPos = new Vector3(0, 0.5f, 0);

    protected void Start()
    {
        cancelBtn.onClick.AddListener(() => Hide(null, null));
        m_Trans = transform.GetChild(0) as RectTransform;
    }

//    protected void Update()
//    {
//        m_TempPos = Camera.main.WorldToScreenPoint(m_Block.transform.position + DeltaPos);
//        m_Pos.x = m_TempPos.x;
//        m_Pos.y = m_TempPos.y;
//        m_Trans.anchoredPosition = m_Pos;
//#if DEBUG_TASKCANVAS
//        print(m_Block.transform.position);
//        print(m_TempPos);
//#endif
//    }

    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <param name="task">��������</param>
    /// <param name="summit">�ύ��������</param>
    public void Show(TaskData task, UnityAction summit)
    {
        Time.timeScale = 0;
        // ���ÿؼ�
        gameObject.SetActive(true);
        summitBtn.onClick.AddListener(summit);
        nameText.text = task.EndName;
        courageText.text = task.CourageNeed.ToString();
        wisdomText.text = task.WisdomNeed.ToString();
        kindnessText.text = task.KindnessNeed.ToString();

        // ����뿪����
        m_Block = MapController.mapController.GameMap[task.EndPoint];
        m_Block.OnPostManGetOut += this.Hide;
    }

    // ������� �������� Ϊ��Ӧί�����Ͷ���
    public void Hide(BasePostMan postman, BaseBlock block)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        summitBtn.onClick.RemoveAllListeners();
        m_Block.OnPostManGetOut -= this.Hide;
    }
}
