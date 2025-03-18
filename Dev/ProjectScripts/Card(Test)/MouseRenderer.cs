using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseRenderer : MonoBehaviour
{
    public GameObject arrowHeadPrefab;
    public GameObject arrowNodePrefab;

    public int arrowNodeNum;
    public float scaleFactor = 0.5f;

    public Transform origin;
    public List<Transform> arrowNodes = new List<Transform>();
    public List<Vector2> controlPoints = new List<Vector2>();
    public readonly List<Vector2> controlPointFactors = new List<Vector2> { new Vector2(0.01f, 0.01f), new Vector2(0.35f, 2.5f) };

    private void Awake()
    {
        // 먼저 targetingCard의 위치를 확인
        if (CardTweenManager.Instance.targetingCard == null)
        {
            Debug.LogError("No targeting card found!");
            return;
        }

        Vector3 startPosition = CardTweenManager.Instance.targetingCard.transform.position;

        arrowNodeNum = 20;
        for (int i = 0; i < arrowNodeNum; ++i)
        {
            // startPosition을 기준으로 노드 생성
            Transform node = Instantiate(arrowNodePrefab, startPosition, Quaternion.identity, transform).GetComponent<Transform>();
            arrowNodes.Add(node);

            node.GetComponent<SpriteRenderer>().sortingOrder
                = CardTweenManager.Instance.targetingCard.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder + 10;
        }

        // 화살촉도 동일하게 처리
        Transform arrowHead = Instantiate(arrowHeadPrefab, startPosition, Quaternion.identity, transform).GetComponent<Transform>();
        arrowNodes.Add(arrowHead);

        arrowNodes[arrowNodes.Count - 1].GetComponent<SpriteRenderer>().sortingOrder
            = CardTweenManager.Instance.targetingCard.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder + 11;

        for (int i = 0; i < 4; ++i)
        {
            controlPoints.Add(Vector2.zero);
        }
    }

    private void LateUpdate()
    {
        if (CardTweenManager.Instance.targetingCard == null)
        {
            return;
        }

        origin = CardTweenManager.Instance.targetingCard.transform;

        Renderer renderer = origin.GetChild(0).GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 centerPosition = renderer.bounds.center;
            controlPoints[0] = new Vector2(centerPosition.x - 1, centerPosition.y + 1.8f);
        }
        else
        {
            // Renderer가 없다면 Transform의 position 사용
            controlPoints[0] = new Vector2(origin.position.x, origin.position.y);
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        controlPoints[3] = new Vector2(mousePos.x, mousePos.y);

        controlPoints[1] = controlPoints[0] + (controlPoints[3] - controlPoints[0]) * controlPointFactors[0];
        controlPoints[2] = controlPoints[0] + (controlPoints[3] - controlPoints[0]) * controlPointFactors[1];

        for (int i = 0; i < arrowNodes.Count; ++i)
        {
            var t = Mathf.Pow(1f * i / (arrowNodes.Count - 1), 0.5f);  // 제곱근을 사용해 초반 간격 늘림

            arrowNodes[i].position =
                Mathf.Pow(1 - t, 3) * this.controlPoints[0] +
                3 * Mathf.Pow(1 - t, 2) * t * this.controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * this.controlPoints[2] +
                Mathf.Pow(t, 3) * controlPoints[3];

            if (i > 0)
            {
                var euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, arrowNodes[i].position - arrowNodes[i - 1].position));
                arrowNodes[i].rotation = Quaternion.Euler(euler);
            }

            var scale = Mathf.Lerp(0.3f, 0.8f, 1f * i / (arrowNodes.Count - 1));
            arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }

        arrowNodes[0].transform.rotation = arrowNodes[1].transform.rotation;

    }

    public void SetCursorRed()
    {
        for (int i = 0; i < arrowNodes.Count; i++)
        {
            arrowNodes[i].GetComponent<SpriteRenderer>().color = new Color(220 / 255f, 0, 0, 1);
        }
    }

    public void SetCursorGray()
    {
        for (int i = 0; i < arrowNodes.Count; i++)
        {
            arrowNodes[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        }
    }

    private void OnEnable()
    {
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }
}

