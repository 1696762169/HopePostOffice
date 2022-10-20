//#define DEBUG_TASKCANVAS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TaskCanvas : MonoBehaviour
{
    // 面板上的控件
    public Button summitBtn;
    public Button cancelBtn;

    public Text nameText;
    public Text courageText;
    public Text wisdomText;
    public Text kindnessText;

    // 面板位置
    protected RectTransform m_Trans;
    protected Vector3 m_TempPos;
    protected Vector2 m_Pos;

    // 面板所在的地块
    protected BaseBlock m_Block;
    [Tooltip("面板相对地块位置偏移量")]
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
    /// 显示面板
    /// </summary>
    /// <param name="task">任务数据</param>
    /// <param name="summit">提交后做的事</param>
    public void Show(TaskData task, UnityAction summit)
    {
        Time.timeScale = 0;
        // 设置控件
        gameObject.SetActive(true);
        summitBtn.onClick.AddListener(summit);
        nameText.text = task.EndName;
        courageText.text = task.CourageNeed.ToString();
        wisdomText.text = task.WisdomNeed.ToString();
        kindnessText.text = task.KindnessNeed.ToString();

        // 添加离开方法
        m_Block = MapController.mapController.GameMap[task.EndPoint];
        m_Block.OnPostManGetOut += this.Hide;
    }

    // 隐藏面板 参数无用 为适应委托类型而设
    public void Hide(BasePostMan postman, BaseBlock block)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        summitBtn.onClick.RemoveAllListeners();
        m_Block.OnPostManGetOut -= this.Hide;
    }
}
