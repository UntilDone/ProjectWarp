using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutpostManager : MonoBehaviour
{

    public Button RestBtn;
    public Button ProceedBtn;
    public Button BackBtn;

    public TMP_Text RestText;
    public TMP_Text ProceedText;
    public TMP_Text BackText;

    public int HealRate;
    void Start()
    {
        gamesave_data.Instance.SaveScene();
        CheckButtonText();
        HealRate = 0;

        ProceedBtn.onClick.AddListener(() => 
        {
            DataManager.Instance.SaveGame();
            SceneManager.LoadScene("Map");
        });
        RestBtn.onClick.AddListener(() => { GetRest(); });
        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }
    public void GetRest()
    {
        gamesave_data.Instance.HealPlayer(15+HealRate);
#if DEBUG_MODE
        Debug.Log($"현재 체력은 : {gamesave_data.Instance.playerData[1].current_hp}");
#endif
        DataManager.Instance.SaveGame();
        SceneManager.LoadScene("Map");
    }

    private void CheckButtonText()
    {
        RestText.text = LocalizationManager.Instance.GetLocalizedText("UI_REST");
        ProceedText.text = LocalizationManager.Instance.GetLocalizedText("UI_PROCEED");
        BackText.text = LocalizationManager.Instance.GetLocalizedText("UI_BACK");
    }
}
