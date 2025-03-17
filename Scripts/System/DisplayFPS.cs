using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayFPS : MonoBehaviour
{
    public TMP_Text fpsText;
    private float deltaTime = 0.0f;

    private void Start()
    {
        DontDestroyOnLoad(transform.root.gameObject);  // 최상위 오브젝트를 DontDestroyOnLoad로 설정
    }

    private void Update()
    {
        // deltaTime을 축적하여 FPS 계산
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // 텍스트 UI에 FPS 업데이트
        fpsText.text = "FPS: " + Mathf.RoundToInt(fps).ToString();
    }
}