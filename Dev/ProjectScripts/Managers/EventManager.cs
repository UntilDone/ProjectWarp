using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    // 버튼 //
    public Button eventButton1;
    public Button eventButton2;
    public Button eventButton3;
    public Button eventButton4;
    public Button BackBtn;

    // 버튼 텍스트 //
    public TMP_Text button1_text;
    public TMP_Text button2_text;
    public TMP_Text button3_text;
    public TMP_Text button4_text;
    public TMP_Text Desc_text;
    public TMP_Text Back_text;
    public TMP_Text Proceed_text;

    public SpriteAtlas spriteAtlas;
    public Image eventImage;

    // 선택된 이벤트 ID 저장 리스트
    public List<int> selectedEventIds;
    public event_data selectedEvent;

    private void Awake()
    {
        gamesave_data.Instance.SaveScene();
        if (Instance == null)
        {
            Instance = this;
            EventPicker(); 
        }
        else
        {
            EventPicker();
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        eventButton1.onClick.AddListener(() =>
        {
            EventRewardCalculator.Instance.GetReward(1);
        });

        eventButton2.onClick.AddListener(() =>
        {
            EventRewardCalculator.Instance.GetReward(2);
        });

        eventButton3.onClick.AddListener(() =>
        {
            EventRewardCalculator.Instance.GetReward(3);
        });
        
        eventButton4.onClick.AddListener(() =>
        {
            EventRewardCalculator.Instance.GetReward(4);
        });

        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });

    }

    private void EventPicker()
    {
        if (gamesave_data.Instance.currentSelectedEventId != 0)
        {
            selectedEvent = EventInfo.Instance.eventDic[gamesave_data.Instance.currentSelectedEventId];
            selectedEventIds = gamesave_data.Instance.selectedEventIds;
            if (!selectedEventIds.Contains(gamesave_data.Instance.currentSelectedEventId))
            {
                selectedEventIds.Add(gamesave_data.Instance.currentSelectedEventId);
            }
        }
        else
        {
            selectedEventIds = gamesave_data.Instance.selectedEventIds;

            if (EventInfo.Instance.eventDic.Count == 0) return;
            List<int> availableKeys = new List<int>();
            foreach (var key in EventInfo.Instance.eventDic.Keys)
            {
                if (!selectedEventIds.Contains(key) && EventInfo.Instance.eventDic[key].IsNext!=true)
                {
                    availableKeys.Add(key);
                }
            }
            if (availableKeys.Count == 0)
            {
#if DEBUG_MODE
                Debug.Log("<color=red>No More Events.</color>");
#endif
                SceneManager.LoadScene("Map");
                return;
            }

            int randomKey = availableKeys[Random.Range(0, availableKeys.Count)];
            selectedEvent = EventInfo.Instance.eventDic[randomKey];
            selectedEventIds.Add(randomKey);
            gamesave_data.Instance.currentSelectedEventId = randomKey;
        }
#if DEBUG_MODE
        Debug.Log($"Selected Event Name: <color=cyan>{selectedEvent.name} </color> ");
#endif
        CheckButtonText();
        eventImage.sprite = Resources.Load<Sprite>($"Event/{selectedEvent.id}");
        //eventImage.sprite = spriteAtlas.GetSprite(selectedEvent.sprite_name);
        gamesave_data.Instance.selectedEventIds = selectedEventIds;
        DataManager.Instance.SaveGame();

    }

    public void CheckButton()
    {
        eventButton1.gameObject.SetActive(!string.IsNullOrEmpty(button1_text.text));
        eventButton2.gameObject.SetActive(!string.IsNullOrEmpty(button2_text.text));
        eventButton3.gameObject.SetActive(!string.IsNullOrEmpty(button3_text.text));
        eventButton4.gameObject.SetActive(!string.IsNullOrEmpty(button4_text.text));
    }
    public void ActiveButtons()
    {
        ButtonInteract(true);
        eventButton1.gameObject.SetActive(true);
        eventButton2.gameObject.SetActive(true);
        eventButton3.gameObject.SetActive(true);
        eventButton4.gameObject.SetActive(true);
    }
    public void ButtonInteract(bool status)
    {
        eventButton1.interactable = status;
        eventButton2.interactable = status;
        eventButton3.interactable = status;
        eventButton4.interactable = status;
    }
    public void CheckButtonText()
    {
        EventRewardCalculator.Instance.MakeRewardDic(selectedEvent.id);
        button1_text.text = $"{LocalizationManager.Instance.GetLocalizedText(EventRewardCalculator.Instance.SetButtonText(1))}";
        //button1_text.fontSize = 30; button1_text.fontStyle = FontStyles.Bold;

        button2_text.text = $"{LocalizationManager.Instance.GetLocalizedText(EventRewardCalculator.Instance.SetButtonText(2))}";
        //button2_text.fontSize = 30; button2_text.fontStyle = FontStyles.Bold;

        button3_text.text = $"{LocalizationManager.Instance.GetLocalizedText(EventRewardCalculator.Instance.SetButtonText(3))}";
        //button3_text.fontSize = 30; button3_text.fontStyle = FontStyles.Bold;

        button4_text.text = $"{LocalizationManager.Instance.GetLocalizedText(EventRewardCalculator.Instance.SetButtonText(4))}";
        //button4_text.fontSize = 30; button4_text.fontStyle = FontStyles.Bold;

        Desc_text.text = $"{LocalizationManager.Instance.GetLocalizedText(selectedEvent.desc)}";
        //Desc_text.fontSize = 30; Desc_text.fontStyle = FontStyles.Bold;

        Back_text.text = LocalizationManager.Instance.GetLocalizedText("UI_BACK");
        CheckButton();
    }
}
