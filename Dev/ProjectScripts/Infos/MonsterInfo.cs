using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterInfo
{
    private static MonsterInfo instance;
    public Dictionary<int, monster_data> monsterDic;
    private MonsterInfo()
    {
        Init();
    }
    public static MonsterInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new MonsterInfo();
            }
            return instance;
        }

    }
    void Init()
    {
        monsterDic = new Dictionary<int, monster_data>();
    }
    public void DisplayAll()
    {
        foreach (var monster in monsterDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=red>{monster.Value.name}</color>");
            Debug.Log($"<color=red>{monster.Value.desc}</color>");
#endif
        }
    }
}
