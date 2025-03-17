using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WatchInfo
{
    private static WatchInfo instance;
    public Dictionary<int, watch_data> watchDic;
    private WatchInfo()
    {
        Init();
    }
    public static WatchInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new WatchInfo();
            }
            return instance;
        }
    }
    void Init()
    {
        watchDic = new Dictionary<int, watch_data>();
    }
    public void DisplayAll()
    {
        foreach (var watch in watchDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=cyan>{watch.Value.name}</color>");
            Debug.Log($"<color=cyan>{watch.Value.desc}</color>");
            Debug.Log($"<color=cyan>{watch.Value.unlock}</color>");
#endif
        }
    }
}
