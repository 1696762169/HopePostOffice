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

    // 该面板所在的地块
    protected BaseBlock m_Block;

    protected void Start()
    {
        cancelBtn.onClick.AddListener(() => Hide(null, null));
    }
    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="task">任务数据</param>
    /// <param name="summit">提交后做的事</param>
    public void Show(TaskData task, UnityAction summit)
    {
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
        gameObject.SetActive(false);
        summitBtn.onClick.RemoveAllListeners();
        m_Block.OnPostManGetOut -= this.Hide;
    }
}
