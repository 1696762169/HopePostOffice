using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class BaseBuilding : MonoBehaviour
{
    private BaseBlock OccupingBlock;

    public UnityAction OnPostManPassBy;
    public UnityAction OnTurnUpdate;

    public void Start()
    {
        OnPostManPassBy += MyOnPostManPassBy;
        OnTurnUpdate += MyOnTurnUpdate;
        GameManager.AddRoundEndMonitor(OnTurnUpdate);
    }
    public virtual void OccupyBlock(BaseBlock block)
    {
        if(!block.BeenOccupied(this))
            Destroy(gameObject);
        OccupingBlock = block;
        transform.position = block.transform.position + new Vector3(0, 0, -1);
    }

    public virtual void MyOnTurnUpdate() { }              //回合结束时发生的事件
    public virtual void MyOnPostManPassBy() { }           //邮递员路过时发生的事件
}
