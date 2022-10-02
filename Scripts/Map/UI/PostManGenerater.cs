using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PostManGenerater : MonoBehaviour
{
    public List<BasePostMan> PostMens;
    public PostMenCard Card;

    public Vector3 StartPos;
    public Vector3 offset;
    void Start()
    {
        var pos=Camera.main.ViewportToScreenPoint(StartPos);
        foreach (BasePostMan postMan in PostMens)
        {
            PostMenCard card = Instantiate(Card,pos,Quaternion.identity,transform);
            pos+= Camera.main.ViewportToScreenPoint(offset);
            card.AddMoveIntroduce(card.GetComponent<Button>());
            card.ChangePostMan(postMan);
        }
    }
}
