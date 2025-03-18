using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeadManager : MonoBehaviour
{
    public Button proceedBtn;
    public TMP_Text proceedText;
    public TMP_Text DeadText;

    private void Awake()
    {
        System.GC.Collect();
        DOTween.KillAll();
    }
    void Start()
    {
        DataManager.Instance.ResetGame();
        proceedBtn.onClick.AddListener(() => 
        {
            SceneManager.LoadScene("MainMenu");
        });
    }
    public void CheckText()
    {
        proceedText.text = LocalizationManager.Instance.GetLocalizedText("UI_PROCEED");
        DeadText.text = LocalizationManager.Instance.GetLocalizedText("UI_DEAD_TEXT");

    }
}
