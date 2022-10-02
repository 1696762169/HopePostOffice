using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LineController : MonoBehaviour
{
    private LineRenderer lr;
    public List<Vector3> pots;

    public GameObject TargetGraphPre;
    private GameObject TargetGraph;

    public void Setpots()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = pots.Count ;
        lr.SetPositions(pots.ToArray());
        if (TargetGraph == null)
        {
            TargetGraph = Instantiate(TargetGraphPre,pots[0],Quaternion.identity,transform);
        }
    }

    public void OnDestroy()
    {
        Destroy(TargetGraph.gameObject);
    }
}
