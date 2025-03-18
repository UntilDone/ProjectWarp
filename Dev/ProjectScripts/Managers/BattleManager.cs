using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public Button BackBtn;
    public Button LocaleBtn;
    public Button HotkeyBtn;
    public Button ProceedBtn;

    public TMP_Text turnText;
    public TMP_Text backText;
    public TMP_Text LocaleText;
    public TMP_Text cardDescText;
    public TMP_Text ProceedText;

    public GameObject Hotkeys;
    public bool isDisplayingHotkeys;

    public GameObject enemyPosition;
    public List<monster_data> monsterPool = new List<monster_data>();

    public GameObject targetEnemy;
    public List<GameObject> monsterList = new List<GameObject>();
    public int monsterIndex;

    GameObject firstMonsterPrefab;
    GameObject SecondMonsterPrefab;
    GameObject firstMonsterObject;
    GameObject secondMonsterObject;

    Transform firstPosition;
    Transform secondPosition;

    TargetManager firstTarget;
    TargetManager secondTarget;

    public int playerTurnCount;
    public int enemyTurnCount;

    public Coroutine shakeCoroutine;

    public TargetManager player;
    public TargetManager[] targets;
    public GameObject watch;

    private Color originalButtonColor;
    private Color originalTextColor;

    public bool isPlayerTurn = true;

    public static BattleManager Instance { get; private set; }
    public enum battle_result
    {
        None,
        Triumph,
        Lost
    }
    public battle_result result;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        gamesave_data.Instance.SaveScene();
        ChangeButtonText();
        ChangeCardText();
        LoadMonsterPool();
        result = battle_result.None;

        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });

        LocaleBtn.onClick.AddListener(() =>
        {
            if (persistent_data.Instance.currentLocale == "KR")
            {
                LocalizationManager.Instance.SetLocale("EN");

            }
            else if (persistent_data.Instance.currentLocale == "EN")
            {
                LocalizationManager.Instance.SetLocale("KR");
            }
            ChangeButtonText();
            ChangeCardText();
        });

        HotkeyBtn.onClick.AddListener(() =>
        {
            Main main = FindAnyObjectByType<Main>();

            if (isDisplayingHotkeys)
            {
                for (int i = 0; i < main.HandDeck.Count; i++)
                {
                    main.HandDeck[i].transform.GetChild(3).GetChild(2).GetChild(2).gameObject.SetActive(false);
                }
                Hotkeys.SetActive(false);
                isDisplayingHotkeys = false;
            }
            else
            {
                for (int i = 0; i < main.HandDeck.Count; i++)
                {
                    main.HandDeck[i].transform.GetChild(3).GetChild(2).GetChild(2).gameObject.SetActive(true);
                }

                Hotkeys.SetActive(true);
                isDisplayingHotkeys = true;
            }
        });

        ProceedBtn.onClick.AddListener(() =>
        {
            DOTween.PauseAll();
            BattleManager.Instance.player.SavePlayerData();
            DataManager.Instance.SaveGame();
            SceneManager.LoadScene("Map");
        });

        switch (monsterPool.Count)
        {
            case 1:
                enemyPosition.transform.GetChild(0).gameObject.SetActive(true);

                firstMonsterPrefab = Resources.Load<GameObject>($"Prefabs/Monster/MONSTER_{monsterPool[0].id}");

                firstPosition = enemyPosition.transform.GetChild(0).GetChild(2);

                firstTarget = firstPosition.parent.GetChild(1).GetComponent<TargetManager>();

                firstMonsterObject = Instantiate(firstMonsterPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero), firstPosition);
                monsterList.Add(firstMonsterObject);
                firstMonsterObject.transform.localPosition = Vector3.zero;

                Monster soleMonster = firstMonsterObject.transform.GetChild(0).GetComponent<Monster>();
                soleMonster.data = monsterPool[0];

                firstTarget.monsterData = monsterPool[0];
                firstTarget.monster = soleMonster;
                firstTarget.RefreshHealthPoint();

                break;

            case 2:
                enemyPosition.transform.GetChild(1).gameObject.SetActive(true);

                firstMonsterPrefab = Resources.Load<GameObject>($"Prefabs/Monster/MONSTER_{monsterPool[0].id}");
                SecondMonsterPrefab = Resources.Load<GameObject>($"Prefabs/Monster/MONSTER_{monsterPool[1].id}");

                firstPosition = enemyPosition.transform.GetChild(1).GetChild(0).GetChild(2);
                secondPosition = enemyPosition.transform.GetChild(1).GetChild(1).GetChild(2);

                firstTarget = firstPosition.parent.GetChild(1).GetComponent<TargetManager>();
                secondTarget = secondPosition.parent.GetChild(1).GetComponent<TargetManager>();

                firstTarget.monsterData = monsterPool[0];
                secondTarget.monsterData = monsterPool[1];

                firstMonsterObject = Instantiate(firstMonsterPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero), firstPosition);
                monsterList.Add(firstMonsterObject);
                firstMonsterObject.transform.localPosition = Vector3.zero;

                secondMonsterObject = Instantiate(SecondMonsterPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero), secondPosition);
                monsterList.Add(secondMonsterObject);
                secondMonsterObject.transform.localPosition = Vector3.zero;

                Monster firstMonster = firstMonsterObject.transform.GetChild(0).GetComponent<Monster>();
                firstMonster.data = monsterPool[0];

                Monster secondMonster = secondMonsterObject.transform.GetChild(0).GetComponent<Monster>();
                secondMonster.data = monsterPool[1];

                firstTarget.monster = firstMonster;
                firstTarget.RefreshHealthPoint();

                secondTarget.monster = secondMonster;
                secondTarget.RefreshHealthPoint();

                break;
        }

        TargetManager[] targets = FindObjectsOfType<TargetManager>();
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i].isTargetPlayer)
            {
                player = targets[i];
                break;
            }
        }

        SpriteRenderer watch = this.watch.GetComponent<SpriteRenderer>();
        string watchName = gamesave_data.Instance.playerData[1].watch;
        int id = gamesave_data.Instance.GetWatchIdFromName(watchName);
        watch.sprite = Resources.Load<Sprite>($"Watch/{id}");

        originalButtonColor = turnText.transform.parent.GetComponent<Image>().color;
        originalTextColor = turnText.GetComponent<TMP_Text>().color;
    }
    public void ChangeButtonText()
    {
        turnText.text = LocalizationManager.Instance.GetLocalizedText("UI_TURN_BUTTON");
        backText.text = LocalizationManager.Instance.GetLocalizedText("UI_BACK");
        LocaleText.text = LocalizationManager.Instance.GetLocalizedText("UI_LOCALE");
        ProceedText.text = LocalizationManager.Instance.GetLocalizedText("UI_PROCEED");
    }
    public void ChangeCardText()
    {
        Card[] cardList = FindObjectsByType<Card>(FindObjectsSortMode.None);
        foreach (Card card in cardList)
        {
            TMP_Text cardNameText = card.transform.GetChild(3).transform.GetChild(2).transform.GetChild(0).GetComponent<TMP_Text>();
            if (cardNameText == null)
            {
                Debug.LogError("cardNameText is null at path card.transform.GetChild(3).transform.GetChild(2).transform.GetChild(0)");
            }
            cardNameText.text = LocalizationManager.Instance.GetLocalizedText(card.cardData.card_name);

            cardDescText = card.transform.GetChild(3).transform.GetChild(2).transform.GetChild(1).GetComponent<TMP_Text>();

            string rawText = LocalizationManager.Instance.GetLocalizedText(card.cardData.desc,
                card.cardData.stat,
                card.cardData.status_duration,
                LocalizationManager.Instance.GetLocalizedText(card.cardData.status_name),
                card.cardData.attack_count,
                card.cardData.draw_count);
            string animatedText = ParseAnimateTag(card.animatedHexColor, rawText, out (int startIndex, int length)? animateInfo);
            card.animateInfo = animateInfo;
            cardDescText.text = animatedText;
        }
    }

    public string ParseAnimateTag(string animatedTextColor, string rawText, out (int startIndex, int length)? animateInfo)
    {
        var match = Regex.Match(rawText, @"<animate>(.+?)<\/animate>");
        if (match.Success)
        {
            string targetText = match.Groups[1].Value;
            int startIndex = match.Index;
            int targetLength = targetText.Length;

            if (animatedTextColor == "")
            {
                animatedTextColor = "#944431";
            }

            targetText = $"<color={animatedTextColor}>{targetText}</color>";

            rawText = rawText.Replace(match.Value, targetText);
            animateInfo = (startIndex, targetLength);
        }
        else
        {
            animateInfo = null;
        }

        return rawText;
    }

    void LoadMonsterPool()
    {
        if (this.monsterPool.Count == 0)
        {

            foreach (var monster in gamesave_data.Instance.MonsterPool)
            {
                this.monsterPool.Add(monster);
            }
        }
#if DEBUG_MODE
        Debug.Log($"<color=lime>현재 몬스터 풀에 있는 몬스터</color>");
        foreach (var monster in this.monsterPool)
        {
            Debug.Log($"<color=red>{monster.name}</color>");
        }
#endif
    }

    public GameObject FindInactiveObjectWithTag(string tag)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag(tag))
            {
                return obj;
            }
        }
        return null;
    }

    public void ChangeTurnButtonColor(bool active)
    {
        if (active)
        {
            turnText.transform.parent.GetComponent<Image>().color = originalButtonColor;
            turnText.GetComponent<TMP_Text>().color = originalTextColor;
        }
        else
        {
            turnText.transform.parent.GetComponent<Image>().color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
            turnText.GetComponent<TMP_Text>().color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
        }
    }

    public void StartMonsterTurn(int index)
    {
        isPlayerTurn = false;
        monsterList[index].transform.GetChild(0).gameObject.SetActive(true);

        if (index == 0)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].isTargetPlayer) continue;
                targets[i].RemoveShield();
            }
            player.DecreaseStatusCount();
            player.RefreshStatus();
        }
        ChangeTurnButtonColor(false);
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        targets = FindObjectsOfType<TargetManager>();

        player.RemoveShield();

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i].isTargetPlayer) continue;
            targets[i].DecreaseStatusCount();
            targets[i].RefreshStatus();
            targets[i].ToggleActionIcon(true);
        }

        if (playerTurnCount > enemyTurnCount)
        {
            enemyTurnCount = playerTurnCount;
            CardTweenManager.Instance.RedrawCards();
        }
        ChangeTurnButtonColor(true);
    }

    public IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera is not assigned!");
            yield break;
        }

        Transform cameraTransform = Camera.main.transform;
        Vector3 originalPosition = new Vector3(0, 0, -30);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition = new Vector3(
                originalPosition.x + offsetX,
                originalPosition.y + offsetY,
                originalPosition.z
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }

}
