using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class EventRewardCalculator
{
    private static EventRewardCalculator instance;
    public Dictionary<int, reward_data> ButtonRewardDic;
    public List<card_data> CardPool;
    private bool ActiveNextEvent;

    public EventRewardCalculator()
    {
        Init();
        ActiveNextEvent = false;
    }
    public static EventRewardCalculator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EventRewardCalculator();
            }
            return instance;
        }
    }
    public void Init()
    {
        ButtonRewardDic = new Dictionary<int, reward_data>();
        CardPool = new List<card_data>();
    }

    public void MakeRewardDic(int ID)
    {
        if (ButtonRewardDic.Count != 0)
        {
            ButtonRewardDic.Clear();
        }
        foreach (var reward in RewardInfo.Instance.rewardDic)
        {
            if (reward.Value.event_ID == ID)
            {
                ButtonRewardDic[reward.Value.ID] = reward.Value;
            }
        }
    }
    public string SetButtonText(int buttonID)
    {

        var result = ButtonRewardDic
            .FirstOrDefault(x => x.Value.button_ID == buttonID).Value;

        return result != null ? result.desc : null;
    }

    public void GetReward(int buttonID)
    {
        EventManager.Instance.selectedEvent = null;
        foreach (var entry in ButtonRewardDic.Where(x => x.Value.button_ID == buttonID))
        {
            switch (entry.Value.type)
            {
                case "Recruit":
                    foreach (var member in ExpeditionMemberInfo.Instance.expedition_memberDic)
                    {
                        if (member.Key == entry.Value.reward)
                        {
                            gamesave_data.Instance.expedition_member_list.Add(member.Value);
                        }
                    }
                    break;

                case "Damage":
                    gamesave_data.Instance.playerData[1].current_hp += entry.Value.reward;
                    if (gamesave_data.Instance.playerData[1].current_hp <= 0)
                    {
                        gamesave_data.Instance.playerData[1].current_hp = 0;
                        gamesave_data.Instance.isDead = true;
                    }
                    else if (gamesave_data.Instance.playerData[1].current_hp > gamesave_data.Instance.playerData[1].max_hp)
                    {
                        gamesave_data.Instance.playerData[1].current_hp = gamesave_data.Instance.playerData[1].max_hp;
                    }
                    break;

                case "Parts":
                    if (entry.Value.config == "PART_LEG")
                    {
                        gamesave_data.Instance.playerData[1].leg_max += entry.Value.reward;
                    }
                    else if (entry.Value.config == "PART_ARM")
                    {
                        gamesave_data.Instance.playerData[1].arm_max += entry.Value.reward;
                    }
                    break;
                case "Battle":
                    foreach (var monster in MonsterInfo.Instance.monsterDic)
                    {
                        if (monster.Key == entry.Value.reward)
                        {
                            gamesave_data.Instance.currentSelectedEventId = 0;
                            gamesave_data.Instance.MonsterPool.Clear();
                            gamesave_data.Instance.MonsterPool.Add(monster.Value);
                        }
                    }
                    SceneManager.LoadScene("Battle");
                    break;

                case "PickCard":
                    CardPool.Clear();
                    if (entry.Value.config == "DISTORT")
                    {
                        foreach (var card in CardInfo.Instance.cardDic)
                        {
                            if (card.Value.type == "CARD_TYPE_DISTORT")
                            {
                                CardPool.Add(card.Value);
                            }
                        }
                    }
                    else if (entry.Value.config == "LEGEND")
                    {
                        foreach (var card in CardInfo.Instance.cardDic)
                        {
                            if (card.Value.type == "CARD_TYPE_LEGEND")
                            {
                                CardPool.Add(card.Value);
                            }
                        }
                    }
                    else if (entry.Value.config == "COMMON")
                    {
                        foreach (var card in CardInfo.Instance.cardDic)
                        {
                            if (card.Value.type == "CARD_TYPE_COMMON")
                            {
                                CardPool.Add(card.Value);
                            }
                        }
                    }
                    else if (entry.Value.config == "UNCOMMON")
                    {
                        foreach (var card in CardInfo.Instance.cardDic)
                        {
                            if (card.Value.type == "CARD_TYPE_UNCOMMON")
                            {
                                CardPool.Add(card.Value);
                            }
                        }
                    }
                    else if (entry.Value.config == "RARE")
                    {
                        foreach (var card in CardInfo.Instance.cardDic)
                        {
                            if (card.Value.type == "CARD_TYPE_RARE")
                            {
                                CardPool.Add(card.Value);
                            }
                        }
                    }

                    for(int i=0; i<entry.Value.reward; i++)
                    {
                        System.Random rng = new System.Random();
                        CardPool = CardPool.OrderBy(card => rng.Next()).ToList();
                        gamesave_data.Instance.cardDeck.Add(CardPool[0]);
                    }

                    break;

                case "Proceed":
                    DataManager.Instance.SaveGame();
                    Init();
                    gamesave_data.Instance.currentSelectedEventId = 0;
                    SceneManager.LoadScene("Map");
                    break;
                case "None":
                    break;

                case "Chance":
                    System.Random random = new System.Random();
                    int chance = random.Next(100);
                    if (chance < entry.Value.reward)
                    {
                        if (entry.Value.Chance_type == "PickCard")
                        {
                            CardPool.Clear();
                            foreach (var card in CardInfo.Instance.cardDic)
                            {
                                if (card.Value.rarity == entry.Value.Chance_reward)
                                {
                                    CardPool.Add(card.Value);
                                }
                            }
                            CardPool = CardPool.OrderBy(card => random.Next()).ToList();
                            gamesave_data.Instance.cardDeck.Add(CardPool[0]);
                        }

                    }
                    else
                    {
                        if (entry.Value.hiddenEvent_ID != 0)
                        {
                            random = new System.Random();
                            chance = random.Next(100);
                            if (chance < entry.Value.hiddenEvent_percentage)
                            {
                                EventManager.Instance.selectedEvent = EventInfo.Instance.eventDic[entry.Value.hiddenEvent_ID];
                                ActiveNextEvent = true;
                            }
                            else
                            {
                                int failNextEventID = int.Parse(entry.Value.config);
                                if (EventInfo.Instance.eventDic.ContainsKey(failNextEventID))
                                {
                                    EventManager.Instance.selectedEvent = EventInfo.Instance.eventDic[failNextEventID];
                                    ActiveNextEvent = true;
                                }
                            }
                        }
                        else
                        {
                            int failNextEventID = int.Parse(entry.Value.config);
                            if (EventInfo.Instance.eventDic.ContainsKey(failNextEventID))
                            {
                                EventManager.Instance.selectedEvent = EventInfo.Instance.eventDic[failNextEventID];
                                ActiveNextEvent = true;
                            }
                        }

                    }
                    break;

                default:
#if DEBUG_MODE
                    Debug.LogWarning($"알 수 없는 타입: {entry.Value.type}");
#endif
                    break;
            }
        }
        foreach (var entry in ButtonRewardDic.Where(x => x.Value.button_ID == buttonID))
        {
            if (EventManager.Instance.selectedEvent == null)
            {
                if (entry.Value.nextEvent_ID != 0)
                {
                    EventManager.Instance.selectedEvent = EventInfo.Instance.eventDic[entry.Value.nextEvent_ID];
                    ActiveNextEvent = true;
                }
            }

        }
        if (ActiveNextEvent)
        {
            EventManager.Instance.ActiveButtons();
            MakeRewardDic(EventManager.Instance.selectedEvent.id);
            EventManager.Instance.CheckButtonText();
            ActiveNextEvent = false;
        }
        else
        {
            EventManager.Instance.ButtonInteract(false);
        }


    }
}
