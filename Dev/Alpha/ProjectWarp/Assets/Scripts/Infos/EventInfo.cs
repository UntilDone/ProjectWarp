using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventInfo
{
    private static EventInfo instance;
    public Dictionary<int, event_data> eventDic;
    private EventInfo()
    {
        Init();
    }

    public static EventInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new EventInfo();
            }
            return instance;
        }
    }

    void Init()
    {
        eventDic = new Dictionary<int, event_data>();
    }

    public void DisplayAll()
    {
        foreach (var events in eventDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=magenta>{events.Value.name}</color>");
#endif
        }
    }
}
