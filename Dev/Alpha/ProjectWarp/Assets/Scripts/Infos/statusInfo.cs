using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class statusInfo
{
    private static statusInfo instance;
    public Dictionary<int, status_data> statusDic;
    
    private statusInfo()
    {
        Init();
    }
    public static statusInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new statusInfo();
            }
            return instance;
        }
    }
    void Init()
    {
        statusDic = new Dictionary<int, status_data>();
    }
    public void DisplayAll()
    {
        foreach (var status in statusDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=blue>{status.Value.name}</color>");
#endif
        }
    }

}
