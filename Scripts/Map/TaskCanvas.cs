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

    // ��������ڵĵؿ�
    protected BaseBlock m_Block;

    protected void Start()
    {
        cancelBtn.onClick.AddListener(() => Hide(null, null));
    }
    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <param name="task">��������</param>
    /// <param name="summit">�ύ��������</param>
    public void Show(TaskData task, UnityAction summit)
    {
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
        gameObject.SetActive(false);
        summitBtn.onClick.RemoveAllListeners();
        m_Block.OnPostManGetOut -= this.Hide;
    }
}
