using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    private static PlayerInfo instance;
    public Dictionary<int, player_data> playerDic;

    private PlayerInfo()
    {
        Init();
    }
    public static PlayerInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new PlayerInfo();
            }
            return instance;
        }
    }
    void Init()
    {
        playerDic = new Dictionary<int, player_data>();
    }
    public void DisplayAll()
    {
        foreach (var player in playerDic)
        {
#if DEBUG_MODE
            Debug.Log($"<color=purple>{player.Value.name}</color>");
#endif
        }
    }
}
