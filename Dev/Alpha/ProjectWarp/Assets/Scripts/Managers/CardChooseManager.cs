using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


// 레어도 가중치에 대한 비율 계산:
// Common    = 12 / (12 + 5 + 2 + 1) = 0.6 (60%)
// Uncommon  =  5 / (12 + 5 + 2 + 1) = 0.25 (25%)
// Rare      =  2 / (12 + 5 + 2 + 1) = 0.1 (10%)
// Legendary =  1 / (12 + 5 + 2 + 1) = 0.05 (5%)
// 총합이 1(100%)이 되도록 설정하여 전체 확률 분포가 명확하고 균형 잡히도록 보장.

public class CardChooseManager
{
    private static CardChooseManager _instance;

    public List<card_data> cardChooseList;
    public List<card_data> cardDefaultList;
    public List<GameObject> cardObjects;

    public bool isCardSelected;

    private Dictionary<string, float> rarityWeights = new Dictionary<string, float>
    {
        { "CARD_RARITY_COMMON", 0.6f },
        { "CARD_RARITY_UNCOMMON", 0.25f },
        { "CARD_RARITY_RARE", 0.1f },
        { "CARD_RARITY_LEGENDARY", 0.05f }
    };

    public static CardChooseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CardChooseManager();
            }
            return _instance;
        }
    }

    private CardChooseManager()
    {
        Init();
    }

    private void Init()
    {
        cardDefaultList = new List<card_data>();
        cardChooseList = new List<card_data>();
        cardObjects = new List<GameObject>();

        foreach (var card in CardInfo.Instance.cardDic)
        {
            if (card.Value.unlock && (card.Value.rarity != "CARD_RARITY_BASIC" && card.Value.rarity != "CARD_RARITY_SPECIAL"))
            {
                cardDefaultList.Add(card.Value);
            }
        }
    }

    private card_data GetRandomCardByRarity()
    {
        float totalWeight = rarityWeights.Values.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        float rarityThreshold = 0;
        System.Random rnd = new System.Random();
        cardDefaultList = cardDefaultList.OrderBy(card => Random.Range(0f, 1f)).ToList();

        foreach (var rarity in rarityWeights)
        {
            rarityThreshold += rarity.Value;
            if (randomValue <= rarityThreshold)
            {
                var rarityCards = cardDefaultList.Where(card => card.rarity == rarity.Key).ToList();
                if (rarityCards.Count > 0)
                {
                    return rarityCards[Random.Range(0, rarityCards.Count)];
                }
            }
        }

        return cardDefaultList[0];
    }
    public void PickRandomCardsWithRarity(int num)
    {
        cardChooseList.Clear();
        Init();

        for (int i = 0; i < num; i++)
        {
            card_data card = null;

            // 카드 선택 반복
            while (card == null)
            {
                card = GetRandomCardByRarity();
            }

            cardChooseList.Add(card);
            cardDefaultList.Remove(card);
        }
    }

    public void DisplayCards()
    {
        GameObject screen = BattleManager.Instance.FindInactiveObjectWithTag("RewardScreen");
        GameObject cardPrefab = Resources.Load<GameObject>("Prefabs/Card/RewardCard");

        if (screen != null)
        {
            GameObject skipButton = screen.transform.parent.GetChild(1).gameObject;

            skipButton.SetActive(true);
            screen.SetActive(true);

            for(int i = 0; i < cardChooseList.Count; i++)
            {
                GameObject card = GameObject.Instantiate(cardPrefab, screen.transform);
                cardObjects.Add(card);

                RewardCard reward = card.GetComponent<RewardCard>();
                reward.data = cardChooseList[i];
            }
        }
    }
}
