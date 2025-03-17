using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardInfo
{
    private static RewardInfo instance;
    public Dictionary<int, reward_data> rewardDic;

    public RewardInfo()
    {
        Init();
    }
    
    public static RewardInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new RewardInfo();
            }
            return instance;
        }
    }

    public void Init()
    {
        rewardDic = new Dictionary<int, reward_data>();
    }
}
