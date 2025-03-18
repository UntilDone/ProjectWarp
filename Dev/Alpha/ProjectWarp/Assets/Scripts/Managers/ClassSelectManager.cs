using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClassSelectManager : MonoBehaviour
{
    public Button Watch1;
    public Button Watch2;
    public Button Watch3;
    public Button ProceedBtn;
    public Button BackBtn;

    public TMP_Text watch1_text;
    public TMP_Text watch2_text;
    public TMP_Text watch3_text;
    public TMP_Text proceed_text;
    public TMP_Text Back_text;
    gamesave_data gameSave = gamesave_data.Instance;
    Dictionary<int, watch_data> watchDic = WatchInfo.Instance.watchDic;
    private string selectedWatch;

    public GameObject background;

    public TMP_Text title;
    public TMP_Text desc;
    public GameObject fadeOut;

    private void Start()
    {
        UpdateButtonStates();
        WatchSelection("WATCH_100_STANDARD_NAME");

        Watch1.onClick.AddListener(() =>
        {
            WatchSelection("WATCH_100_STANDARD_NAME");
        });

        Watch2.onClick.AddListener(() =>
        {
            WatchSelection("WATCH_101_SCANNER_ENHANCED_NAME");
        });

        Watch3.onClick.AddListener(() =>
        {
            WatchSelection("WATCH_102_SIGNAL_ENHANCED_NAME");
        });

        SpriteRenderer image = fadeOut.GetComponent<SpriteRenderer>();

        ProceedBtn.onClick.AddListener(() =>
        {
            gameSave.Init();
            UpdatePlayerWatch(selectedWatch);
            gameSave.current_Chapter = 1;
            image.DOFade(1, 0.2f).SetEase(Ease.InQuad)
            .OnComplete(() =>
             {
                 DataManager.Instance.SaveGame();
                 SceneManager.LoadScene("Map");
             });
        });
        BackBtn.onClick.AddListener(() =>
        {
            image.DOFade(1, 0.2f).SetEase(Ease.InQuad)
            .OnComplete(() => SceneManager.LoadScene("MainMenu"));
        });
        SetButtonTexts();

        #region button text movement
        #endregion
    }

    private void UpdateButtonStates()
    {
        Watch1.gameObject.SetActive(IsWatchSelectable("WATCH_100_STANDARD_NAME"));
#if DEBUG_MODE
        Debug.Log($"watch1 status : {IsWatchSelectable("WATCH_100_STANDARD_NAME")}");
#endif
        Watch2.gameObject.SetActive(IsWatchSelectable("WATCH_101_SCANNER_ENHANCED_NAME"));
#if DEBUG_MODE
        Debug.Log($"watch2 status : {IsWatchSelectable("WATCH_101_STANDARD_NAME")}");
#endif
        Watch3.gameObject.SetActive(IsWatchSelectable("WATCH_102_SIGNAL_ENHANCED_NAME"));
#if DEBUG_MODE
        Debug.Log($"watch3 status : {IsWatchSelectable("WATCH_102_STANDARD_NAME")}");
#endif

        if (!IsWatchSelectable("WATCH_102_STANDARD_NAME"))
        {
            RectTransform rect0 = BackBtn.GetComponent<RectTransform>();
            Vector2 position0 = rect0.anchoredPosition;
            rect0.anchoredPosition = new Vector2(-450, position0.y);

            RectTransform rect1 = Watch1.GetComponent<RectTransform>();
            Vector2 position1 = rect1.anchoredPosition;
            rect1.anchoredPosition = new Vector2(-150, position1.y);

            RectTransform rect2 = Watch2.GetComponent<RectTransform>();
            Vector2 position2 = rect2.anchoredPosition;
            rect2.anchoredPosition = new Vector2(150, position1.y);

            RectTransform rect3 = ProceedBtn.GetComponent<RectTransform>();
            Vector2 position3 = rect3.anchoredPosition;
            rect3.anchoredPosition = new Vector2(450, position2.y);
        }
    }

    private void SetButtonTexts()
    {
        if (IsWatchSelectable("WATCH_100_STANDARD_NAME"))
        {
            watch1_text.text = LocalizationManager.Instance.GetLocalizedText("WATCH_100_STANDARD_NAME");
        }
        if (IsWatchSelectable("WATCH_101_SCANNER_ENHANCED_NAME"))
        {
            watch2_text.text = LocalizationManager.Instance.GetLocalizedText("WATCH_101_SCANNER_ENHANCED_NAME");
        }
        if (IsWatchSelectable("WATCH_102_SIGNAL_ENHANCED_NAME"))
        {
            watch3_text.text = LocalizationManager.Instance.GetLocalizedText("WATCH_102_SIGNAL_ENHANCED_NAME");
        }

        proceed_text.text = LocalizationManager.Instance.GetLocalizedText("UI_PROCEED");
        Back_text.text = LocalizationManager.Instance.GetLocalizedText("UI_BACK");
    }

    public void WatchSelection(string watchName)
    {
        selectedWatch = watchName;

        SpriteRenderer watch
            = background.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

        watch.sprite = Resources.Load<Sprite>($"Watch/{GetWatchIdFromName(watchName)}");

        title.text = $"{LocalizationManager.Instance.GetLocalizedText(watchName)}";

        string watchDesc = watchName.Replace("NAME", "DESC");
        desc.text = $"{LocalizationManager.Instance.GetLocalizedText(watchDesc)}";

#if DEBUG_MODE
        Debug.Log($"Selected Watch : {LocalizationManager.Instance.GetLocalizedText(watchName)}");
#endif
    }

    private bool IsWatchSelectable(string watchName)
    {
        var watchId = GetWatchIdFromName(watchName);

        var watchData = WatchInfo.Instance.watchDic;

        if (watchData.TryGetValue(watchId, out var watchUnlockData))
        {
            return watchUnlockData.unlock;
        }

        return false;
    }

    private void UpdatePlayerWatch(string watchValue)
    {
        if (gameSave.playerData.ContainsKey(1))
        {
            var player = gameSave.playerData[1];

            player.watch = watchValue;
            if (watchDic.TryGetValue(GetWatchIdFromName(watchValue), out var watchData))
            {
                player.max_hp = watchData.player_maxHp;
                player.current_hp = player.max_hp;
                gameSave.cardDeck.Clear();
                for (int i = 0; i < watchData.attack_amount; i++)
                {
                    if (CardInfo.Instance.cardDic.TryGetValue(watchData.attack_id, out var cardData))
                    {
                        gameSave.cardDeck.Add(cardData);
                    }
                }
                for (int i = 0; i < watchData.defend_amount; i++)
                {
                    if (CardInfo.Instance.cardDic.TryGetValue(watchData.defend_id, out var cardData))
                    {
                        gameSave.cardDeck.Add(cardData);
                    }
                }
                for (int i = 0; i < watchData.special_amount; i++)
                {
                    if (CardInfo.Instance.cardDic.TryGetValue(watchData.special_id, out var cardData))
                    {
                        gameSave.cardDeck.Add(cardData);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("PlayerData for Player ID 1 not found!");
        }
    }


    private int GetWatchIdFromName(string watchName)
    {
        if (watchName.StartsWith("WATCH_") && int.TryParse(watchName.Split('_')[1], out int watchId))
        {
            return watchId;
        }
        return -1;
    }
}
