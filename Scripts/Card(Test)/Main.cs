using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.Rendering;
public class Main : MonoBehaviour
{
    // effects
    public GameObject effectCanvas;
    public GameObject CardGo;
    public GameObject shuffleCardGo;
    public GameObject shieldEffectGo;
    public GameObject shieldBreakEffectGo;
    public GameObject bashEffectGo;
    public GameObject strikeEffectGo;
    public GameObject damageEffectGo;
    public GameObject reticleEffectGO;
    public GameObject vulnerableEffectGo;

    private GameObject CurrentCard;
    private CardTweenManager cardTweenManager;

    public List<GameObject> CardDeck;
    public List<GameObject> HandDeck;
    public List<GameObject> UsingPile;
    public List<GameObject> DisCardPile;
    public List<Animator> HandDeckAnims;

    public Button TurnBtn;

    [Range(0, 5f)] public float FirstDrawSpeed = 0.08f;
    [Range(0, 5f)] public float DrawSpeed = 0.1f;
    [Range(1, 10)] public int MaxDrawCount = 5;

    public int LessCard;

    public int idCount100;
    public int idCount101;
    public int idCount102;

    private AudioManager audioManager;
    private BattleManager battle;

    private Coroutine endTurnCoroutine;

    private void Awake()
    {
        DOTween.Init(false, true, LogBehaviour.Verbose).SetCapacity(500, 125);
    }

    void Start()
    {
        battle = BattleManager.Instance;

        cardTweenManager = FindAnyObjectByType<CardTweenManager>();

        DisCardPile = new List<GameObject>();
        CardDeck = new List<GameObject>();
        HandDeck = new List<GameObject>();
        MaxDrawCount = 5;

        MakeDeck();
        BattleManager.Instance.ChangeCardText();
        StartCoroutine(WaitForDrawingAndShuffle());

        TurnBtn.onClick.AddListener(() =>
        {
            EndTurn();
        });

        audioManager = AudioManager.Instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) /*|| Input.GetKeyDown(KeyCode.Space)*/)
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        CardTweenManager card = cardTweenManager;
        if (/*card.isBezier ||*/ card.drawTweenCount > 0 || card.isArranging || card.isUsingAnyCard || card.draggingCard != null || card.targetingCard != null
            || battle.playerTurnCount > battle.enemyTurnCount || battle.monsterList.Count == 0 || battle.isPlayerTurn == false)
        {
            return;
        }

        if (HandDeck.Count > 0)
        {
            cardTweenManager.isEndingTurn = true;
        }

        //float volume = 0.5f;
        //AudioManager.Instance.PlayAudio(AudioManager.Instance.sfxClips[2], volume);

        cardTweenManager.drawTweenCount = MaxDrawCount;
        cardTweenManager.discardTweenCount = HandDeck.Count;
        DiscardHand();

        FindAnyObjectByType<CenterZone>().usedCardAmount = 0;

        battle.playerTurnCount++;

        if (battle.playerTurnCount > battle.enemyTurnCount)
        {
            if (endTurnCoroutine != null)
            {
                StopCoroutine(endTurnCoroutine);
                endTurnCoroutine = null;
            }
            endTurnCoroutine = StartCoroutine(WaitUntilDrawingFinished());
        }
        battle.ChangeTurnButtonColor(false);
    }

    private IEnumerator WaitUntilDrawingFinished()
    {
        while (cardTweenManager.isUsingAnyCard)
        {
            yield return null;
        }
        battle.StartMonsterTurn(0);
    }

    public IEnumerator DrawCardAfterShuffleFinished()
    {
        while (cardTweenManager.isUsingAnyCard)
        {
            yield return null;
        }

        Shuffle();

        while (!cardTweenManager.isShuffleFinished)
        {
            yield return null;
        }

        DrawCard(false);
    }

    public IEnumerator DrawCardAfterDiscard()
    {
        while (cardTweenManager.isUsingAnyCard)
        {
            yield return null;
        }

        DrawCard(false);
    }

    public IEnumerator HandleDrawAndShuffle()
    {
        // 1. 남은 카드를 먼저 뽑음
        yield return StartCoroutine(DrawCardsWithInterval());

        // 2. 1초 대기
        yield return new WaitForSeconds(0.35f);

        // 3. 덱이 비었으면 셔플
        Shuffle();

        yield return new WaitUntil(() => cardTweenManager.isShuffleFinished);

        // 4. 남은 부족한 카드 수를 계산해서 다시 뽑기
        MaxDrawCount = 5;  // 다시 5장이 되도록 맞추기 위해 MaxDrawCount를 5로 설정
        yield return StartCoroutine(DrawCardsWithInterval());  // 남은 부족한 카드 수만큼 뽑기
    }

    public void MakeDeck()
    {
        int startingDeckSize = gamesave_data.Instance.cardDeck.Count;

        for (int i = 0; i < startingDeckSize; i++)
        {
            Transform cardUITransform = FindAnyObjectByType<CardUIManager>().transform;
            CurrentCard = Instantiate(CardGo, CardGo.transform.position, CardGo.transform.rotation, cardUITransform);

            #region card field
            Card card = CurrentCard.GetComponent<Card>();
            card.cardData = gamesave_data.Instance.cardDeck[i];
            #endregion

            Vector2 targetPosition = cardTweenManager.drawPileGo.transform.position;
            float random0 = Random.Range(targetPosition.x - 0.1f, targetPosition.x);
            float random1 = Random.Range(targetPosition.y + 0.1f, targetPosition.y + 0.2f);
            float random2 = Random.Range(-10f, 10f);

            CurrentCard.transform.position = new Vector3(random0, random1, 0);
            CurrentCard.transform.localScale = new Vector3(0.10f, 0.10f, 0);
            CurrentCard.transform.rotation = Quaternion.Euler(new Vector3(0, 0, random2));

            Sprite cardSprite = Resources.Load<Sprite>($"CardResources/{card.cardData.id}");

            int statusId = gamesave_data.Instance.GetStatusIDFromName(card.cardData.status_name);
            Sprite plateSprite = Resources.Load<Sprite>($"CardResources/PLATE_{statusId}");
            Sprite descSprite = Resources.Load<Sprite>($"CardResources/DESC_{statusId}");

            SpriteRenderer cardImage
                = CurrentCard.transform.GetChild(3).transform.GetChild(0).GetComponent<SpriteRenderer>();
            cardImage.sprite = cardSprite;

            if (plateSprite != null)
            {
                SpriteRenderer cardPlate = CurrentCard.transform.GetChild(2).GetComponent<SpriteRenderer>();
                SpriteRenderer cardDesc = CurrentCard.transform.GetChild(3).GetChild(1).GetComponent<SpriteRenderer>();
                SpriteRenderer cardFX = CurrentCard.transform.GetChild(0).GetComponent<SpriteRenderer>();
                TMP_Text name = CurrentCard.transform.GetChild(3).GetChild(2).GetChild(0).GetComponent<TMP_Text>();
                TMP_Text desc = CurrentCard.transform.GetChild(3).GetChild(2).GetChild(1).GetComponent<TMP_Text>();

                cardPlate.sprite = plateSprite;
                cardDesc.sprite = descSprite;

                Color fxColor = cardFX.color;
                Color modifiedColor = name.color;

                switch (statusId)
                {
                    case 1100:
                        card.animatedHexColor = "#008080";
                        modifiedColor = new Color(0, 80 / 255f, 80 / 255f);
                        fxColor = new Color(0, 150 / 255f, 150 / 255f);
                        break;

                    case 1101:
                        card.animatedHexColor = "#800000";
                        modifiedColor = new Color(80 / 255f, 0, 0);
                        fxColor = new Color(150 / 255f, 0, 0);
                        break;

                    case 1103:
                        card.animatedHexColor = "#FFFF00";
                        fxColor = new Color(1, 1, 0);
                        break;
                }

                cardFX.color = fxColor;
                name.color = modifiedColor;
                desc.color = modifiedColor;
            }

            CurrentCard.transform.GetChild(0).gameObject.SetActive(false);
            CurrentCard.transform.GetChild(1).gameObject.SetActive(false);
            CurrentCard.transform.GetChild(3).transform.GetChild(2).GetComponent<Canvas>().worldCamera = Camera.main;
            CurrentCard.transform.GetChild(3).gameObject.SetActive(false);
            CardDeck.Add(CurrentCard);
        }
    }

    public void Shuffle()
    {
        int MaxDiscard = DisCardPile.Count;

        if (MaxDiscard == 0)
        {
            for (int i = 0; i < CardDeck.Count; i++)
            {
                int random = Random.Range(0, CardDeck.Count);

                GameObject temp = CardDeck[i];
                CardDeck[i] = CardDeck[random];
                CardDeck[random] = temp;
            }
            return;
        }

        for (int i = 0; i < MaxDiscard; i++)
        {
            int ran1 = Random.Range(0, MaxDiscard);
            int ran2 = Random.Range(0, MaxDiscard);

            GameObject temp = DisCardPile[ran1];
            DisCardPile[ran1] = DisCardPile[ran2];
            DisCardPile[ran2] = temp;
        }

        if (DisCardPile.Count > 0)
        {
            // shuffle effect
            for (int i = 0; i < DisCardPile.Count; i++)
            {
                GameObject card = Instantiate(shuffleCardGo);
                card.transform.position = new Vector3(6.90f + i * 0.05f, -2.15f, 10);
                card.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 75));
                card.transform.localScale = new Vector3(0.03f, 0.03f, 0);
                card.transform.GetChild(0).gameObject.SetActive(false);

                cardTweenManager.isShuffleFinished = false;
                cardTweenManager.shuffleTweenCount = 0;

                float shuffleSpeed = (1f - (5 - i) * 0.1f) * 0.03f;
                cardTweenManager.SetShuffleTween(card.transform, shuffleSpeed);
            }
        }

        for (int i = 0; i < MaxDiscard; i++)
        {

            // reset position
            Vector2 targetPosition = cardTweenManager.drawPileGo.transform.position;

            float random0 = Random.Range(targetPosition.x - 0.1f, targetPosition.x);
            float random1 = Random.Range(targetPosition.y + 0.1f, targetPosition.y + 0.2f);
            float random2 = Random.Range(-10f, 10f);

            DisCardPile[0].transform.position = new Vector3(random0, random1, 0);
            DisCardPile[0].transform.localScale = new Vector3(0.10f, 0.10f, 0);
            DisCardPile[0].transform.rotation = Quaternion.Euler(new Vector3(0, 0, random2));

            CardDeck.Add(DisCardPile[0]);
            DisCardPile.Remove(DisCardPile[0]);

        }
    }

    private IEnumerator WaitForDrawingAndShuffle()
    {
        yield return new WaitUntil(() => !cardTweenManager.isArranging);

        Shuffle();
        MaxDrawCount = 5 - LessCard;
        StartCoroutine(DrawCardsWithInterval());
    }

    public IEnumerator DrawCardsWithInterval()
    {
        DrawCard(true);
        yield return new WaitForSeconds(FirstDrawSpeed);
        while (HandDeck.Count < MaxDrawCount)
        {
            DrawCard(true);
            yield return new WaitForSeconds(DrawSpeed);
        }
    }

    public void DrawCard(bool arrange)
    {
        if (arrange & HandDeck.Count > 0)
        {
            AudioClip cardDealSound = audioManager.sfxClips[2];
            audioManager.PlayAudioOnce(cardDealSound, 8);
        }

        if (/*HandDeck.Count < MaxDrawCount &&*/ CardDeck.Count > 0)
        {
            Card card = CardDeck[0].GetComponent<Card>();
            card.cardIndex = -1;
            card.isDiscarded = false;
            card.isInHand = true;

            HandDeckAnims.Add(CardDeck[0].transform.GetChild(0).GetComponent<Animator>());
            CardDeck[0].transform.GetChild(0).gameObject.SetActive(true);

            BattleManager battleManager = FindAnyObjectByType<BattleManager>();
            if (battleManager.isDisplayingHotkeys)
            {
                CardDeck[0].transform.GetChild(3).GetChild(2).GetChild(2).gameObject.SetActive(true);
            }

            HandDeck.Add(CardDeck[0]);
            CardDeck.Remove(CardDeck[0]);

            cardTweenManager.drawTweenCount--;
        }

        if (arrange)
        {
            cardTweenManager.ArrangeHand(false, null);
        }
        else
        {
        }
    }

    public void DiscardHand()
    {
        audioManager.PlayAudioOnce(audioManager.sfxClips[3], 0.3f);

        if (HandDeck.Count > 0)
        {
            for (int i = HandDeck.Count - 1; i < HandDeck.Count && i >= 0; i--)
            {
                float discardSpeed = 0.2f + (i / 10);
                float animSpeed = discardSpeed * 2f; // should be 20% of discardSpeed for now
                float bezierScale = Random.Range(-4f, -8f); // higher means higher curve
                bool isOnHand = true;

                cardTweenManager.SetUsedDiscardTween(HandDeck[i].transform, animSpeed, discardSpeed, bezierScale, isOnHand);
                cardTweenManager.EmptyHand(HandDeck[i].transform);
            }
        }
    }



}