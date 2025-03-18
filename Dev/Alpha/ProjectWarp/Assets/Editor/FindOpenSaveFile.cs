using System.IO;
using UnityEditor;
using UnityEngine;

public class FindOpenSaveFile : EditorWindow
{
    private string jsonContent = ""; // JSON 파일 내용을 저장할 변수
    private Vector2 scrollPosition;

    [MenuItem("Tools/Open Save Folder and View JSON")]
    public static void ShowWindow()
    {
        // 에디터 창 열기
        GetWindow<FindOpenSaveFile>("Save File Viewer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Persistent Data Path", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Save Folder"))
        {
            OpenPersistentDataPath();
        }

        if (GUILayout.Button("Load and Display JSON"))
        {
            ReadAndDisplayJson();
        }

        GUILayout.Label("JSON Content:", EditorStyles.boldLabel);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
        GUILayout.TextArea(jsonContent, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    private void OpenPersistentDataPath()
    {
        // 영구 저장 위치 폴더 열기
        EditorUtility.RevealInFinder(Application.persistentDataPath+ "/ProjectWarp");
    }

    private void ReadAndDisplayJson()
    {
        // JSON 파일 경로
        string filePath = Path.Combine(Application.persistentDataPath, "savefile.json");

        if (File.Exists(filePath))
        {
            // JSON 파일 읽기
            jsonContent = File.ReadAllText(filePath);
            Debug.Log("JSON file loaded successfully.");
        }
        else
        {
            jsonContent = "Save file not found.";
            Debug.LogWarning("Save file not found at: " + filePath);
        }
    }
}
