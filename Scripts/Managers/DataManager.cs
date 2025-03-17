using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public string gamesave_dataPath;
    public string persistant_dataPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gamesave_dataPath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            persistant_dataPath = Path.Combine(Application.persistentDataPath, "persistant.json");
            persistent_data.Instance.Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadDefaultData();
        ApplyUnlocks();
        DisplayAll();
    }

    public void LoadDefaultData()
    {
        TextAsset statusTextAsset = Resources.Load<TextAsset>("Table/status_data");
        TextAsset monsterTextAsset = Resources.Load<TextAsset>("Table/monster_data");
        TextAsset cardTextAsset = Resources.Load<TextAsset>("Table/card_data");
        TextAsset playerTextAsset = Resources.Load<TextAsset>("Table/player_data");
        TextAsset watchTextAsset = Resources.Load<TextAsset>("Table/watch_data");
        TextAsset eventTextAsset = Resources.Load<TextAsset>("Table/event_data");
        TextAsset mapTextAsset = Resources.Load<TextAsset>("Table/map_data");
        TextAsset expeditionMemberTextAsset = Resources.Load<TextAsset>("Table/expedition_member_data");
        TextAsset rewardTextAsset = Resources.Load<TextAsset>("Table/reward_data");

        if (statusTextAsset != null)
        {
            var statusList = JsonConvert.DeserializeObject<List<status_data>>(statusTextAsset.text);
            statusInfo.Instance.statusDic = new Dictionary<int, status_data>();
            foreach (var item in statusList)
            {
                statusInfo.Instance.statusDic[item.id] = item;
            }
        }

        if (monsterTextAsset != null)
        {
            var monsterList = JsonConvert.DeserializeObject<List<monster_data>>(monsterTextAsset.text);
            MonsterInfo.Instance.monsterDic = new Dictionary<int, monster_data>();
            foreach (var item in monsterList)
            {
                MonsterInfo.Instance.monsterDic[item.id] = item;
            }
        }

        if (cardTextAsset != null)
        {
            var cardList = JsonConvert.DeserializeObject<List<card_data>>(cardTextAsset.text);
            CardInfo.Instance.cardDic = new Dictionary<int, card_data>();
            foreach (var item in cardList)
            {
                CardInfo.Instance.cardDic[item.id] = item;
            }
        }

        if (playerTextAsset != null)
        {
            var playerList = JsonConvert.DeserializeObject<List<player_data>>(playerTextAsset.text);
            PlayerInfo.Instance.playerDic = new Dictionary<int, player_data>();
            foreach (var item in playerList)
            {
                PlayerInfo.Instance.playerDic[item.id] = item;
            }
        }

        if (watchTextAsset != null)
        {
            var watchList = JsonConvert.DeserializeObject<List<watch_data>>(watchTextAsset.text);
            WatchInfo.Instance.watchDic = new Dictionary<int, watch_data>();
            foreach (var item in watchList)
            {
                WatchInfo.Instance.watchDic[item.id] = item;
            }
        }

        if (eventTextAsset != null)
        {
            var eventList = JsonConvert.DeserializeObject<List<event_data>>(eventTextAsset.text);
            EventInfo.Instance.eventDic = new Dictionary<int, event_data>();
            foreach (var item in eventList)
            {
 
                EventInfo.Instance.eventDic[item.id] = item;
            }
        }

        if (mapTextAsset != null)
        {
            var mapList = JsonConvert.DeserializeObject<List<map_data>>(mapTextAsset.text);
            MapInfo.Instance.mapDic = new Dictionary<int, map_data>();
            foreach (var item in mapList)
            {
                MapInfo.Instance.mapDic[item.id] = item;
            }
        }

        if (expeditionMemberTextAsset != null)
        {
            var expeditionMemberList = JsonConvert.DeserializeObject<List<expedition_member_data>>(expeditionMemberTextAsset.text);
            ExpeditionMemberInfo.Instance.expedition_memberDic = new Dictionary<int, expedition_member_data>();
            foreach (var item in expeditionMemberList)
            {
                ExpeditionMemberInfo.Instance.expedition_memberDic[item.id] = item;
            }
        }
        if(rewardTextAsset != null)
        {
            var rewardList = JsonConvert.DeserializeObject<List<reward_data>>(rewardTextAsset.text);
            RewardInfo.Instance.rewardDic = new Dictionary<int, reward_data>();
            foreach(var reward in rewardList)
            {
                RewardInfo.Instance.rewardDic[reward.ID] = reward;
            }
        }
    }

    private void ApplyUnlocks()
    {
        if (persistent_data.Instance.unlockedCardIds != null)
        {
            foreach (var card in CardInfo.Instance.cardDic.Values)
            {
                if (persistent_data.Instance.unlockedCardIds.Contains(card.id))
                {
                    card.unlock = true;
                }
            }
        }

        if (persistent_data.Instance.unlockedWatchIds != null)
        {
            foreach (var watch in WatchInfo.Instance.watchDic.Values)
            {
                if (persistent_data.Instance.unlockedWatchIds.Contains(watch.id))
                {
                    watch.unlock = true;
                }
            }
        }
    }


    public void SaveGame()
    {
        string gameDataJson = JsonConvert.SerializeObject(gamesave_data.Instance,  Formatting.Indented);
        File.WriteAllText(gamesave_dataPath, gameDataJson);
    }
    public void LoadGame()
    {
        if (!File.Exists(gamesave_dataPath))
        {
            return;
        }

        else
        {
            string gameDataJson = File.ReadAllText(gamesave_dataPath);
            gamesave_data loadedData = JsonConvert.DeserializeObject<gamesave_data>(gameDataJson);
            if (loadedData != null)
            {
                gamesave_data.Instance.playerData = loadedData.playerData;
                gamesave_data.Instance.selectedEventIds = loadedData.selectedEventIds;
                gamesave_data.Instance.expedition_member_list = loadedData.expedition_member_list;
                gamesave_data.Instance.cardDeck = loadedData.cardDeck;
                gamesave_data.Instance.monsterData = loadedData.monsterData;
                gamesave_data.Instance.cardData = loadedData.cardData;
                gamesave_data.Instance.currentSelectedEventId = loadedData.currentSelectedEventId;
                gamesave_data.Instance.mapSeed = loadedData.mapSeed;
                gamesave_data.Instance.currentSceneStatus = loadedData.currentSceneStatus;

                gamesave_data.Instance.isDead = loadedData.isDead;
                gamesave_data.Instance.isMonsterPoolReady = loadedData.isMonsterPoolReady;
                gamesave_data.Instance.isMonsterPackDownloaded = loadedData.isMonsterPackDownloaded;    

                gamesave_data.Instance.Chapter1 = loadedData.Chapter1;
                gamesave_data.Instance.Chapter2 = loadedData.Chapter2;
                gamesave_data.Instance.Chapter3 = loadedData.Chapter3;

                gamesave_data.Instance.Chapter1_Elite = loadedData.Chapter1_Elite;
                gamesave_data.Instance.Chapter2_Elite = loadedData.Chapter2_Elite;
                gamesave_data.Instance.Chapter3_Elite = loadedData.Chapter3_Elite;

                gamesave_data.Instance.Chapter1_Boss = loadedData.Chapter1_Boss;
                gamesave_data.Instance.Chapter2_Boss = loadedData.Chapter2_Boss;
                gamesave_data.Instance.Chapter3_Boss = loadedData.Chapter3_Boss;

                gamesave_data.Instance.MonsterPool = loadedData.MonsterPool;
            }
        }

    }

    public void ResetGame()
    {
        if (File.Exists(gamesave_dataPath))
        {
            File.Delete(gamesave_dataPath);
        }
    }
    public void DisplayAll()
    {
        CardInfo.Instance.DisplayAll();
        WatchInfo.Instance.DisplayAll();
        PlayerInfo.Instance.DisplayAll();
        MonsterInfo.Instance.DisplayAll();
        statusInfo.Instance.DisplayAll();
        EventInfo.Instance.DisplayAll();
        MapInfo.Instance.DisplayAll();
        ExpeditionMemberInfo.Instance.DisplayAll();
    }
}
