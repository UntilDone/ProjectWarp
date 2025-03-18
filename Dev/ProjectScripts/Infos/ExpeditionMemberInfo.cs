using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpeditionMemberInfo
{
    private static ExpeditionMemberInfo instance;
    public Dictionary<int, expedition_member_data> expedition_memberDic;
    private ExpeditionMemberInfo()
    {
        Init();
    }
    public static ExpeditionMemberInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new ExpeditionMemberInfo();
            }
            return instance;
        }
    }

    private void Init()
    {
        expedition_memberDic = new Dictionary<int, expedition_member_data>();
    }
    public void DisplayAll()
    {
        foreach(var expedition_member in expedition_memberDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=brown>{expedition_member.Value.name}</color>");
            Debug.Log($"<color=brown>{expedition_member.Value.desc}</color>");
#endif
        }
    }
}
