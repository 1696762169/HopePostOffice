using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController mapController;

    public List<BaseBlock> GameMap;

    //————————————————生成地图相关————————————————————————
    public int Maplenth;                                    //地图的每一行的单元格数量
    public int MapSize;                                     //地图的所有单元格的数量
    public List<GameObject> BlockTypes;                     //地图中每一种单元格的预制体的集合  
    public List<GameObject> BuildingTypes;                  //地图中每一种建筑物的预制体的集合 
    public Vector3 offset;                                  //地图中两个单元格位置的差
    public int[] BlockTypesNum;                             //地图中每一个单元格对应的预制体在集合中的位置
    public int[] BuildingTypesNum;                          //地图中每一个建筑物对应的预制体在集合中的位置

    public void Start()
    {
        StartCoroutine(LoadGameMap());
        mapController = this;
    }
    public IEnumerator LoadGameMap() 
    {
        Vector3 TempPos = new Vector3(0, 0, 0);
        for (int i = 0; i < MapSize;)
        {
            for (int j = 0; j < Maplenth; i++,j++)
            {
                GameObject bblock = Instantiate(BlockTypes[BlockTypesNum[i]],TempPos,Quaternion.identity,this.transform);
                GameMap.Add(bblock.GetComponent<BaseBlock>());
                if (BuildingTypes[BuildingTypesNum[i]] != null)
                {
                    GameObject bbuilding = Instantiate(BuildingTypes[BuildingTypesNum[i]], TempPos+new Vector3(0,0,-1), Quaternion.identity, this.transform);
                    yield return 1;
                    bbuilding.GetComponent<BaseBuilding>().OccupyBlock(bblock.GetComponent<BaseBlock>());
                }
                TempPos += offset;
                yield return new WaitForSeconds(0.01f);
            }
            TempPos += new Vector3(-(offset.x*(Maplenth-1)),-(offset.y*(Maplenth+1)),-(offset.z*(Maplenth+1)));
        }
        yield return new WaitForSeconds(0.01f);
        for (int i=0;i<MapSize;i++)
        {
            if((i+1)%Maplenth!=0)
                GameMap[i].Neighbors.Add(GameMap[i + 1]);
            if(i%Maplenth!=0)
                GameMap[i].Neighbors.Add(GameMap[i - 1]);
            if(i/Maplenth>=1)
                GameMap[i].Neighbors.Add(GameMap[i - Maplenth]);
            if((MapSize-i-1)/Maplenth>=1)
                GameMap[i].Neighbors.Add(GameMap[i + Maplenth]);
        }
    }                    //生成地图
    //————————————————————————寻路相关函数————————————————————————————
    public static int GetDidtance(BaseBlock start,BaseBlock end,PostManType type) {
        if (start == end)
        {
            return 0;
        }
        int[] distance = new int[mapController.MapSize];
        for (int i = 0; i < mapController.MapSize; i++)
        {
            distance[i] = 100000;
        }
        distance[mapController.GameMap.IndexOf(start)] = 0;
        List<BaseBlock> BluePoint = new List<BaseBlock>();
        BluePoint.Add(start);
        for (int i = 0; i < BluePoint.Count; i++)
        {
            BaseBlock block = BluePoint[i];
            foreach (BaseBlock nblock in block.Neighbors)
            {
                if (!BluePoint.Contains(nblock) && nblock.AllowTypes.Contains(type))
                {
                    BluePoint.Add(nblock);
                    int tempdistance = distance[mapController.GameMap.IndexOf(BluePoint[i])]+1;
                    if (tempdistance < distance[mapController.GameMap.IndexOf(nblock)])
                        distance[mapController.GameMap.IndexOf(nblock)] = tempdistance;
                }
            }
            if (BluePoint.Contains(end))
            {
                return distance[mapController.GameMap.IndexOf(end)];
            }
        }
        return 100000;
    }                      //不可到达处的距离为100000,PostManType是邮递员的类型
    public static int GetDidtance(BaseBlock start, BaseBlock end,PostManType type,List<BaseBlock> results) //此函数可以通过results获得起点和终点的最短路径的单元格的集合，不可到达处的距离为100000
    {
        results.Clear();
        if (start == end)
        {
            results.Add(start);
            return 0;
        }
        int[] distance = new int[mapController.MapSize];
        int[] LastBlock= new int[mapController.MapSize];
        for (int i = 0; i < mapController.MapSize; i++)
        {
            distance[i] = 100000;
        }
        distance[mapController.GameMap.IndexOf(start)] = 0;
        List<BaseBlock> BluePoint = new List<BaseBlock>();
        BluePoint.Add(start);
        for (int i = 0; i < BluePoint.Count; i++)
        {
            BaseBlock block = BluePoint[i];
            foreach (BaseBlock nblock in block.Neighbors)
            {
                if (!BluePoint.Contains(nblock) && nblock.AllowTypes.Contains(type))
                {
                    BluePoint.Add(nblock);
                    int tempdistance = distance[mapController.GameMap.IndexOf(BluePoint[i])] + 1;
                    if (tempdistance < distance[mapController.GameMap.IndexOf(nblock)])
                    {
                        distance[mapController.GameMap.IndexOf(nblock)] = tempdistance;
                        LastBlock[mapController.GameMap.IndexOf(nblock)] = mapController.GameMap.IndexOf(block);
                    }
                }
            }
            if (BluePoint.Contains(end))
            {
                int temp = mapController.GameMap.IndexOf(end);
                while (mapController.GameMap.IndexOf(start) != temp)
                {
                    results.Add(mapController.GameMap[temp]);
                    temp = LastBlock[temp];
                }
                results.Add(start);
                return distance[mapController.GameMap.IndexOf(end)];
            }
        }
        return 100000;
    }

    public static int[] GetDidtance(BaseBlock start, PostManType type)                  //此函数可以获得地图中所有单元格与起点格的距离，不可到达处的距离为100000
    {
        int[] distance = new int[mapController.MapSize];
        for (int i = 0; i < mapController.MapSize; i++)
        {
            distance[i] = 100000;
        }
        distance[mapController.GameMap.IndexOf(start)] = 0;
        List<BaseBlock> BluePoint = new List<BaseBlock>();
        BluePoint.Add(start);
        for (int i = 0; i < BluePoint.Count; i++)
        {
            BaseBlock block = BluePoint[i];
            foreach (BaseBlock nblock in block.Neighbors)
            {
                if (!BluePoint.Contains(nblock) && nblock.AllowTypes.Contains(type))
                {
                    BluePoint.Add(nblock);
                    int tempdistance = distance[mapController.GameMap.IndexOf(BluePoint[i])] + 1;
                    if (tempdistance < distance[mapController.GameMap.IndexOf(nblock)])
                        distance[mapController.GameMap.IndexOf(nblock)] = tempdistance;
                }
            }
        }
        return distance;
    }
}
