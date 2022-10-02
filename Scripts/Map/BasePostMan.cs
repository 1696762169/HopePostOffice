using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class BasePostMan : MonoBehaviour
{
    public PostManType MyType;

    private BaseBlock StayingBlock;
    private BaseBlock TargetBlock;
    
    public PostManDeta MyDeta;                          //埀垢議光倖奉來
    //！！！！！！！！！！！！！！！！！！！！！！！！卞強�犢慂�來！！！！！！！！！！！！！！！！
    public float Movespeed;                             //卞強扮卞強議堀業
    private int RemainMovePoint;                        //云指栽複噫議佩強泣
    private bool IsReadyToMove;
    private bool IsMoving;
    private List<BaseBlock> AllBlocksCanGo=new List<BaseBlock>();
    private List<BaseBlock> Route = new List<BaseBlock>();

    public GameObject RouteLinePre;
    private LineController RouteLine;
    //！！！！！！！！！！！！！！！！！！！！！！！！強鮫！！！！！！！！！！！！！！！！
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
        RemainMovePoint = MyDeta.MovePoint;
    }
    public void OnTurnStart() {
        RemainMovePoint = MyDeta.MovePoint;
    }
    public void MoveReady() {
        IsReadyToMove = true;
        if(GameManager.PostManReadyToMove!=null)
            GameManager.PostManReadyToMove.MoveReadyCancel();
        GameManager.ChangePostManReadyToMove(this);
        int[] distances = MapController.GetDidtance(StayingBlock,MyType);
        for (int i = 0; i < distances.Length; i++)
        {
            if(distances[i]<= RemainMovePoint)
            {
                AllBlocksCanGo.Add(MapController.mapController.GameMap[i]);
            }
        }
        foreach (BaseBlock baseBlock in AllBlocksCanGo)
        {
            baseBlock.GetComponent<SpriteRenderer>().color = new Color(1, 0.5f, 0.5f, 1);
        }
    }
    public void MoveReadyCancel() {
        IsReadyToMove = false;
        foreach (BaseBlock baseBlock in AllBlocksCanGo)
        {
            baseBlock.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        }
        AllBlocksCanGo.Clear();
        if (GameManager.PostManReadyToMove == this)
        {
            GameManager.ChangePostManReadyToMove(null);
        }
    }

    public void MoveToBlock(BaseBlock block)
    {
        StayingBlock = block;
        block.OnPostManPassBy();
    }
    public void SetTargetBlock(BaseBlock block) {
        TargetBlock = block;
        MapController.GetDidtance(StayingBlock,block,MyType,Route);

        if (RouteLine != null)
            Destroy(RouteLine.gameObject);
        RouteLine = Instantiate(RouteLinePre).GetComponent<LineController>();
        foreach (BaseBlock bb in Route)
        {
            Vector3 pos = bb.transform.position;
            pos.y += 0.15f;
            pos.z = -5;
            RouteLine.pots.Add(pos);
        }
        RouteLine.pots.Add(new Vector3(transform.position.x, transform.position.y+0.15f, -5));
        RouteLine.Setpots();

        IsMoving = true;

        if (GameManager.PostManReadyToMove == this)
        {
            GameManager.ChangePostManReadyToMove(null);
        }
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0)&&!EventSystem.current.IsPointerOverGameObject())
        {
            MoveJudge();
        }

    }
    public void FixedUpdate()
    {
        MoveAnimation();
    }
    protected void MoveJudge() {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        Vector3 myPos = transform.position;
        myPos.z = 0;
        if (!IsReadyToMove)
        {
            if ((pos - myPos).magnitude <= 0.3f)
            {
                MoveReady();
            }
        }
        else
        {
            foreach (BaseBlock block in AllBlocksCanGo)
            {
                Vector3 tempPos = block.transform.position;
                tempPos.z = 0;
                if ((pos - tempPos).magnitude < 0.3f)
                {
                    SetTargetBlock(block);
                    goto Label;
                }
            }
        Label: MoveReadyCancel();
        }
    }
    protected void MoveAnimation()
    {
        animator.SetBool("isRun", IsMoving);
        if (IsMoving)
        {
            Vector2 Mypos = transform.position;
            Vector2 BlockPos = Route[Route.Count - 1].transform.position;
            if ((Mypos - BlockPos).x > 0)
            {
                GetComponent<SpriteRenderer>().flipX=true;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = false;

            }
            if ((Mypos - BlockPos).magnitude > 0.1f)
            {
                Vector3 vec = Route[Route.Count-1].transform.position - new Vector3(0, 0, 1) - transform.position;
                transform.position += vec.normalized * Movespeed*Time.fixedDeltaTime;
                RouteLine.pots[RouteLine.pots.Count - 1] = new Vector3(transform.position.x, transform.position.y+0.15f, -5);
                RouteLine.Setpots();
            }
            else
            {
                if (IsReadyToMove)
                    MoveReadyCancel();
                MoveToBlock(Route[Route.Count - 1]);
                RemainMovePoint -= 1;
                if (Route.Count > 1)
                {
                    RouteLine.pots.RemoveAt(RouteLine.pots.Count-1);
                    RouteLine.Setpots();
                    Route.Remove(Route[Route.Count-1]);
                }
                else
                {
                    Route.Clear();
                    Destroy(RouteLine.gameObject);
                    IsMoving = false;
                }

            }
        }
    }
}
