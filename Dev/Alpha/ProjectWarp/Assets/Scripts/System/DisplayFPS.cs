using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayFPS : MonoBehaviour
{
    public TMP_Text fpsText;
    private float deltaTime = 0.0f;

    private void Start()
    {
        DontDestroyOnLoad(transform.root.gameObject);  // �ֻ��� ������Ʈ�� DontDestroyOnLoad�� ����
    }

    private void Update()
    {
        // deltaTime�� �����Ͽ� FPS ���
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // �ؽ�Ʈ UI�� FPS ������Ʈ
        fpsText.text = "FPS: " + Mathf.RoundToInt(fps).ToString();
    }
}