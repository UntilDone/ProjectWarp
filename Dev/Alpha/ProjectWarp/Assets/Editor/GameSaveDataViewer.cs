using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameSaveDataViewer : EditorWindow
{
    [MenuItem("Tools/GameSave Data Viewer")]
    public static void ShowWindow()
    {
        GetWindow<GameSaveDataViewer>("GameSave Data Viewer");
    }

    private Vector2 scrollPosition;
    private string gameSaveDataJson;
    private string persistentDataJson;

    private Dictionary<string, bool> gameSaveDataFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, bool> persistentDataFoldouts = new Dictionary<string, bool>();

    private bool showGameSaveData = true;
    private bool showPersistentData = false;

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

        EditorGUILayout.Space();
        if (GUILayout.Button("Show GameSave Data"))
        {
            LoadGameSaveData();
            showGameSaveData = true;
            showPersistentData = false;
        }

        if (GUILayout.Button("Show Persistent Data"))
        {
            LoadPersistentData();
            showPersistentData = true;
            showGameSaveData = false;
        }

        EditorGUILayout.Space();

        if (showGameSaveData && !string.IsNullOrEmpty(gameSaveDataJson))
        {
            EditorGUILayout.LabelField("GameSave Data (JSON)", EditorStyles.boldLabel);
            RenderJsonWithFoldout(gameSaveDataJson, ref gameSaveDataFoldouts);
        }

        if (showPersistentData && !string.IsNullOrEmpty(persistentDataJson))
        {
            EditorGUILayout.LabelField("Persistent Data (JSON)", EditorStyles.boldLabel);
            RenderJsonWithFoldout(persistentDataJson, ref persistentDataFoldouts);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RenderJsonWithFoldout(string json, ref Dictionary<string, bool> foldouts)
    {
        try
        {
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            foreach (var key in jsonData.Keys)
            {
                if (!foldouts.ContainsKey(key))
                {
                    foldouts[key] = true; // Default to expanded
                }

                foldouts[key] = EditorGUILayout.Foldout(foldouts[key], key, true);

                if (foldouts[key])
                {
                    EditorGUILayout.LabelField(
                        jsonData[key]?.ToString() ?? "null",
                        EditorStyles.wordWrappedLabel
                    );
                }
            }
        }
        catch
        {
            GUILayout.TextArea("Error parsing JSON. Ensure it is formatted correctly.", GUILayout.ExpandHeight(true));
        }
    }

    private void LoadGameSaveData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "gamesave.json");

        if (File.Exists(filePath))
        {
            gameSaveDataJson = File.ReadAllText(filePath);
        }
        else
        {
            gameSaveDataJson = "No savefile.json file found!";
        }
    }

    private void LoadPersistentData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "persistent_data.json");

        if (File.Exists(filePath))
        {
            persistentDataJson = File.ReadAllText(filePath);
        }
        else
        {
            persistentDataJson = "No persistent_data.json file found!";
        }
    }

    private void Update()
    {
        Repaint();
    }
}
