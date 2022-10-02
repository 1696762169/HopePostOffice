using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class PostMenCard : MonoBehaviour
{
    private bool isDraging = false;
    private bool isPointOverThis = false;

    private BasePostMan postMan;
    private Vector3 StartPos;
    public void Update()
    {
        if (isPointOverThis)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartPos = transform.position;
                isDraging = true;
            }
        }
        if (isDraging)
        {
            transform.position = Input.mousePosition;
            if (Input.GetMouseButtonUp(0))
            {
                isDraging = false;
                foreach (BaseBlock block in MapController.mapController.GameMap)
                {
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 Mypos = block.transform.position;
                    if ((pos - Mypos).magnitude < 0.5f)
                    {
                        BasePostMan bpm = Instantiate(postMan, block.transform.position, Quaternion.identity);
                        bpm.MoveToBlock(block);
                        GetComponent<Button>().interactable = false;
                        break;
                    }
                }
                transform.position = StartPos;
            }
        }

    }
    public void AddMoveIntroduce(Button button)
    {
        EventTrigger et = button.gameObject.GetComponent<EventTrigger>();
        EventTrigger.Entry pointerEntry = new EventTrigger.Entry();
        EventTrigger.Entry pointerOut = new EventTrigger.Entry();
        pointerEntry.eventID = EventTriggerType.PointerEnter;
        pointerOut.eventID = EventTriggerType.PointerExit;

        pointerEntry.callback = new EventTrigger.TriggerEvent();
        pointerOut.callback = new EventTrigger.TriggerEvent();

        UnityAction<BaseEventData> PointerEnterCB = new UnityAction<BaseEventData>(delegate { if (GetComponent<Button>().interactable) isPointOverThis = true; });
        UnityAction<BaseEventData> PointerOutCB = new UnityAction<BaseEventData>(delegate { isPointOverThis = false; });

        pointerEntry.callback.AddListener(PointerEnterCB);
        pointerOut.callback.AddListener(PointerOutCB);

        et.triggers.Add(pointerEntry);
        et.triggers.Add(pointerOut);
    }

    public void ChangePostMan(BasePostMan man) {
        postMan = man;
    }
}
