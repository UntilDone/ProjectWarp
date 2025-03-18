using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public int seed;
    public Button ProceedBtn;
    public Button EventBtn;
    public Button OutpostBtn;
    public Button BackBtn;

    public TMP_Text ProceedText;
    public TMP_Text OutpostText;
    public TMP_Text BackText;
    public TMP_Text EventText;


    private void Start()
    {
        gamesave_data.Instance.SaveScene();
        CheckButtonText();
        ProceedBtn.onClick.AddListener(() =>
        {
#if DEBUG_MODE
            Debug.Log("테스트를 위해 다음 맵으로 이동합니다.");
            Debug.Log($"<color=lime>현재 1챕 몬스터 수 : {gamesave_data.Instance.Chapter1.Count}</color>");
#endif      
            MonRandSelect.Instance.PickRandomMonster();
            MonRandSelect.Instance.SendCurrentMonsterList();
#if DEBUG_MODE  
            Debug.Log($"<color=lime>현재 1챕 몬스터 수 : {gamesave_data.Instance.Chapter1.Count}</color>");
            Debug.Log($"<color=lime>현재 몬스터 수 : {gamesave_data.Instance.MonsterPool.Count}</color>");
#endif
            SceneManager.LoadScene("Battle");
        });
        EventBtn.onClick.AddListener(() =>
        {
#if DEBUG_MODE
            Debug.Log("테스트를 위해 이벤트로 이동합니다.");
#endif
            DataManager.Instance.SaveGame();
            SceneManager.LoadScene("Event");
        });
        BackBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
        OutpostBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Outpost");
        });
    }
    void GenerateMap()
    {

    }
    private void CheckButtonText()
    {
        ProceedText.text= LocalizationManager.Instance.GetLocalizedText("UI_PROCEED");
        OutpostText.text = LocalizationManager.Instance.GetLocalizedText("UI_OUTPOST");
        BackText.text = LocalizationManager.Instance.GetLocalizedText("UI_BACK");
        EventText.text = LocalizationManager.Instance.GetLocalizedText("UI_EVENT");
    }
}