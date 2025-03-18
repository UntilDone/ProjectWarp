using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class LocalizationManager : MonoBehaviour
{

    //플레이스 홀더 번역 예시
    //string rawText = LocalizationManager.Instance.GetLocalizedText(card.cardData.desc,
    //card.cardData.stat,
    //card.cardData.status_duration,
    //LocalizationManager.Instance.GetLocalizedText(card.cardData.status_name),
    //card.cardData.attack_count,
    //card.cardData.draw_count);


    public static LocalizationManager Instance { get; private set; }
    private Dictionary<string, Dictionary<string, string>> localizationData;
    private string currentLocale;

    private void Awake()
    {
        currentLocale = persistent_data.Instance.currentLocale;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLocalizationData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadLocalizationData()
    {
        TextAsset localizationTextAsset = Resources.Load<TextAsset>("Table/localization_data");

        if (localizationTextAsset != null)
        {
            string jsonContent = localizationTextAsset.text;
            var rawEntries = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonContent);
            localizationData = new Dictionary<string, Dictionary<string, string>>();

            foreach (var rawEntry in rawEntries)
            {
                if (rawEntry.TryGetValue("id", out var id))
                {
                    var translations = new Dictionary<string, string>();

                    foreach (var pair in rawEntry)
                    {
                        if (pair.Key != "id")
                        {
                            translations[pair.Key] = pair.Value;
                        }
                    }

                    localizationData[id] = translations;
                }
            }
        }
    }


    public string GetLocalizedText(string id, params object[] args)
    {
        if (id == null || args == null)
        {
            return null;
        }

        if (localizationData.TryGetValue(id, out var translations))
        {
            if (translations.TryGetValue(currentLocale, out var localizedText))
            {
                bool hasPlaceholders = Regex.IsMatch(localizedText, @"\{\d+\}");
                return (args.Length == 0 || !hasPlaceholders)
                    ? localizedText
                    : string.Format(localizedText, args);
            }
        }
        return id;
    }

    public void SetLocale(string newLocale)
    {
        currentLocale = newLocale;
        persistent_data.Instance.currentLocale = newLocale;
        persistent_data.Instance.Save();
        DataManager.Instance.LoadDefaultData();
        if (System.IO.File.Exists(DataManager.Instance.gamesave_dataPath))
        {
            DataManager.Instance.LoadGame();
        }
        DataManager.Instance.SaveGame();
    }
}

[System.Serializable]
public class LocalizationEntry
{
    public string id;
    [JsonExtensionData]
    public Dictionary<string, string> translations;
}
