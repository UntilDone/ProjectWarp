using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[System.Serializable]
public class persistent_data
{
    private static persistent_data _instance;

    private readonly string filePath = Path.Combine(Application.persistentDataPath, "persistent_data.json");

    public string currentLocale = "EN";
    public HashSet<int> unlockedCardIds = new HashSet<int>();
    public HashSet<int> unlockedWatchIds = new HashSet<int>();

    private persistent_data() { }

    public static persistent_data Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new persistent_data();
            }
            return _instance;
        }
    }

    public void Save()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public void Load()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            _instance = JsonConvert.DeserializeObject<persistent_data>(json);
        }
        else
        {
            _instance = new persistent_data();
            Save();
        }
    }
}
