using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckInfo : deck_data
{
    private static DeckInfo instance;
    private DeckInfo()
    {
        Init();
    }
    public static DeckInfo Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new DeckInfo();
            }
            return instance;
        }
    }
    void Init()
    {
        deck = new List<card_data>();
        deck_count = 0;
    }
    public void DisplayAll()
    {
        foreach (var card in deck)
        {
#if DEBUG_MODE
            Debug.Log($"<color=pink>{card.card_name}</color>");
#endif
        }
    }
}
