using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenu1;
    public GameObject mainMenu2;
    public GameObject mainMenu3;
    public GameObject Logo;

    public Button StartGameBtn;
    public Button LoadGameBtn;
    public Button AbandonGameBtn;
    public Button ChangeLocaleBtn;
    public Button AdminBtn;
    public Button QuitBtn;

    public TMP_Text StartText;
    public TMP_Text LoadText;
    public TMP_Text AbandonText;
    public TMP_Text QuitText;
    public TMP_Text SettingsText;
    public TMP_Text AdminText;
    public TMP_Text GotoSelectionText;
    public TMP_Text ChangeLocalizationText;

    private Vector3 mousePosition;

    private float elapsedTime;

    private void Start()
    {
        ChangeButtonText();
        CheckButtons();
        StartGameBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("ClassSelect");
        });
        LoadGameBtn.onClick.AddListener(() =>
        {
            if (File.Exists(DataManager.Instance.gamesave_dataPath))
            {
                DataManager.Instance.LoadGame();
                SceneManager.LoadScene(gamesave_data.Instance.currentSceneStatus);
            }
           
        });

        AbandonGameBtn.onClick.AddListener(() =>
        {
            gamesave_data.Instance.Init();
            DataManager.Instance.ResetGame();
            CheckButtons();
        });

        ChangeLocaleBtn.onClick.AddListener(() => 
        {
            if (persistent_data.Instance.currentLocale == "KR")
            {
                LocalizationManager.Instance.SetLocale("EN");

            }
            else if(persistent_data.Instance.currentLocale == "EN")
            {
                LocalizationManager.Instance.SetLocale("KR");
            }
            ChangeButtonText();
        });
        AdminBtn.onClick.AddListener(() =>
        {
            for (int i = 0; i < AdminBtn.transform.childCount; i++)
            {
                if (AdminBtn.transform.GetChild(i).gameObject.activeSelf)
                {
                    AdminBtn.transform.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    AdminBtn.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        });
        QuitBtn.onClick.AddListener(() => 
        {
            GameExit();
        });
    }
    private void Update()
    {
        elapsedTime += Time.deltaTime;

        mousePosition = Camera.main.ScreenToWorldPoint(GetMouseScreenPosition());

        mainMenu1.transform.position = -mousePosition * 0.02f;
        mainMenu2.transform.position = mousePosition * 0.01f;
        mainMenu3.transform.position = mousePosition * 0.01f;
        Logo.transform.position = -mousePosition * 0.01f;

        if(elapsedTime >= 1)
        {
            elapsedTime = 0;
        }
    }
    private void CheckButtons()
    {
        StartGameBtn.gameObject.SetActive(!File.Exists(DataManager.Instance.gamesave_dataPath));
        LoadGameBtn.gameObject.SetActive(File.Exists(DataManager.Instance.gamesave_dataPath));
        AbandonGameBtn.gameObject.SetActive(File.Exists(DataManager.Instance.gamesave_dataPath));
    }

    private void ChangeButtonText()
    {
        StartText.text = LocalizationManager.Instance.GetLocalizedText("UI_NEW_GAME");
        LoadText.text = LocalizationManager.Instance.GetLocalizedText("UI_LOAD_GAME");
        AbandonText.text = LocalizationManager.Instance.GetLocalizedText("UI_ABANDON");
        QuitText.text = LocalizationManager.Instance.GetLocalizedText("UI_QUIT");
        SettingsText.text = LocalizationManager.Instance.GetLocalizedText("UI_SETTING");
        AdminText.text = LocalizationManager.Instance.GetLocalizedText("UI_ADMIN");
        GotoSelectionText.text = LocalizationManager.Instance.GetLocalizedText("UI_GO_TO_SELECTION");
        ChangeLocalizationText.text = LocalizationManager.Instance.GetLocalizedText("UI_CHANGE_LOCALIZATION");
    }

    public Vector3 GetMouseScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }
    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
