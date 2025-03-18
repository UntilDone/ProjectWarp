using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfo
{
    private static MapInfo instance;
    public Dictionary<int, map_data> mapDic;
    private MapInfo()
    {
        Init();
    }
    public static MapInfo Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MapInfo();
            }
            return instance;
        }
    }
    void Init()
    {
        mapDic = new Dictionary<int, map_data>();
    }
    public void DisplayAll()
    {
        foreach (var map in mapDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=green>{map.Value.name}</color>");
            Debug.Log($"<color=green>{map.Value.desc}</color>");
#endif
        }
    }
}
