using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    private List<card_data> shopCardList;
    private List<expedition_member_data> shopExpeditionList;
    public Button deleteBtn;
    public Button ProceedBtn;
    public Button BackBtn;

    public TMP_Text deleteText;
    public TMP_Text ProceedText;
    public TMP_Text BackText;
    void Start()
    {
        gamesave_data.Instance.SaveScene();
        CheckButtonText();
        shopCardList = new List<card_data>();
        shopExpeditionList = new List<expedition_member_data>();

        ProceedBtn.onClick.AddListener(() =>
        {
            DataManager.Instance.SaveGame();
            SceneManager.LoadScene("Map");
        });
        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }
    public void GetDelete() 
    {

    }

    private void CheckButtonText()
    {
        deleteText.text = LocalizationManager.Instance.GetLocalizedText("UI_DELETE");
        BackText.text = LocalizationManager.Instance.GetLocalizedText("UI_BACK");
        ProceedText.text = LocalizationManager.Instance.GetLocalizedText("UI_PROCEED");
    }
}
