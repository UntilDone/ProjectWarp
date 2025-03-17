using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfo
{
    private static CardInfo instance;
    public Dictionary<int, card_data> cardDic;
    private CardInfo()
    {
        Init();
    }
    public static CardInfo Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CardInfo();
            }
            return instance;
        }
    }
    void Init()
    {
        cardDic = new Dictionary<int, card_data>();
    }
    public void DisplayAll()
    {
        foreach (var cardData in cardDic.Values)
        {
#if DEBUG_MODE
            Debug.Log($"ID: {cardData.id}\n" +
                      $"Name: {cardData.card_name}\n" +
                      $"Sprite Name: {cardData.sprite_name}\n" +
                      $"Type: {cardData.type}\n" +
                      $"Rarity: {cardData.rarity}\n" +
                      $"Stat: {cardData.stat}\n" +
                      $"Description: {cardData.desc}\n" +
                      $"Status ID: {cardData.status_name}\n" +
                      $"Status Duration: {cardData.status_duration}\n" +
                      $"Status Target: {cardData.status_target}\n" +
                      $"-------------------------");
#endif
        }

    }

}
