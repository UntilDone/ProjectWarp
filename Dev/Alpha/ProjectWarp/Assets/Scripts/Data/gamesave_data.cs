using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class gamesave_data
{
    private static gamesave_data _instance;
    public int currentSelectedEventId;
    public bool isDead;
    public bool isMonsterPoolReady;
    public bool isMonsterPackDownloaded;
    public int mapSeed;
    public int current_Chapter=1;
    public string currentSceneStatus;

    public Dictionary<int, player_data> playerData;
    public List<int> selectedEventIds;
    public List<expedition_member_data> expedition_member_list;
    public List<card_data> cardDeck;
    public List<monster_data> monsterList;
    public List<map_data> currentMapList;
    public Dictionary<int, monster_data> monsterData;
    public Dictionary<int, card_data> cardData;

    public List<monster_data> Chapter1;
    public List<monster_data> Chapter2;
    public List<monster_data> Chapter3;

    public List<monster_data> Chapter1_Elite;
    public List<monster_data> Chapter2_Elite;
    public List<monster_data> Chapter3_Elite;

    public List<monster_data> Chapter1_Boss;
    public List<monster_data> Chapter2_Boss;
    public List<monster_data> Chapter3_Boss;

    public List<monster_data> MonsterPool;

    private gamesave_data()
    {
        Init();

    }

    public static gamesave_data Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new gamesave_data();
            }
            return _instance;
        }
    }

    public void CheckPlayerHp()
    {
        if (playerData[1].current_hp > playerData[1].max_hp)
        {
            playerData[1].current_hp = playerData[1].max_hp;
        }
        else if (playerData[1].current_hp < 0)
        {
            playerData[1].current_hp = 0;
        }

        if (playerData[1].current_hp <= 0)
        {
            isDead = true;
        }
    }

    public void HealPlayer(int num)
    {
        playerData[1].current_hp += num;
        CheckPlayerHp();
    }

    public void DealPlayer(int num) 
    {
        playerData[1].current_hp -= num;
        CheckPlayerHp();
    }
    public void SaveScene()
    {
        this.currentSceneStatus =SceneManager.GetActiveScene().name;
        DataManager.Instance.SaveGame();
    }
    public void Init()
    {
        isDead = false;
        isMonsterPoolReady = false;

        monsterData = new Dictionary<int, monster_data>();
        monsterData = MonsterInfo.Instance.monsterDic;

        cardData = new Dictionary<int, card_data>();
        cardData = CardInfo.Instance.cardDic;

        monsterData = new Dictionary<int, monster_data>();
        monsterData = MonsterInfo.Instance.monsterDic;

        playerData = new Dictionary<int, player_data>();
        playerData = PlayerInfo.Instance.playerDic;

        selectedEventIds = new List<int>();
        currentSelectedEventId = 0;

        cardDeck = new List<card_data>();
        expedition_member_list = new List<expedition_member_data>();

        MonsterPool= new List<monster_data>();

        Chapter1 = new List<monster_data>();
        Chapter2 = new List<monster_data>();
        Chapter3 = new List<monster_data>();

        Chapter1_Elite = new List<monster_data>();
        Chapter2_Elite = new List<monster_data>();
        Chapter3_Elite = new List<monster_data>();

        Chapter1_Boss = new List<monster_data>();
        Chapter2_Boss = new List<monster_data>();
        Chapter3_Boss = new List<monster_data>();

}
    public int GetWatchIdFromName(string watchName)
    {
        if (watchName.StartsWith("WATCH_") && int.TryParse(watchName.Split('_')[1], out int watchId))
        {
            return watchId;
        }
        return -1;
    }
    public int GetStatusIDFromName(string statusName)
    {
        if (statusName.StartsWith("STATUS_") && int.TryParse(statusName.Split('_')[1], out int statusId))
        {
            return statusId;
        }
        return -1;
    }
    
}
