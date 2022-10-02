using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class BaseBlock : MonoBehaviour
{
    private bool Isoccupied=false;
    public bool CanBeoccupied=true;
    [HideInInspector]
    public List<PostManType> AllowTypes;                //允许通过的邮递员类型
    public List<PostManType> OriAllowTypes;

    private BaseBuilding OccupingBuilding;
    [HideInInspector]
    public List<BaseBlock> Neighbors;

    public UnityAction OnPostManPassBy;

    public void Start()
    {
        OnPostManPassBy += MyOnPostManPassBy;
        RevertPassType();
    }

    //public void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        Vector2 mypos = transform.position;
    //        if ((pos - mypos).magnitude < 0.3f)
    //        {
    //            foreach (BaseBlock baseBlock in Neighbors)
    //            {
    //                baseBlock.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 1);
    //            }
    //        }
    //    }
    //}
    public virtual bool BeenOccupied(BaseBuilding building) {
        if (!CanBeoccupied)
            return false;
        if (building != null)
        {
            Isoccupied = true;
            OccupingBuilding = building;
            AlwNoToPass();
        }
        else
        {
            Isoccupied = false;
            OccupingBuilding = building;
            RevertPassType();
        }
        return true;
    }

    public virtual void AlwNoToPass (){
        AllowTypes.Clear();
    }
    public virtual void RevertPassType() {
        AllowTypes = OriAllowTypes;
    }
    public virtual void MyOnPostManPassBy() {
        if(Isoccupied)
        {
            OccupingBuilding.OnPostManPassBy();
        }
    }                                                                //邮递员路过时发生的事件
}
