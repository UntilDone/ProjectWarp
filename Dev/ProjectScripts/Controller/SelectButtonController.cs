using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rect;
    private Vector2 originalOffsetMax;
    private Vector2 originalOffsetMin;

    private void Awake()
    {
        rect = transform.GetChild(0).GetComponent<RectTransform>();
        originalOffsetMax = rect.offsetMax;
        originalOffsetMin = rect.offsetMin;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rect.offsetMax = new Vector2(rect.offsetMax.x, -4);
        rect.offsetMin = new Vector2(rect.offsetMin.x, -4);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        rect.offsetMax = new Vector2(rect.offsetMax.x, originalOffsetMax.y);
        rect.offsetMin = new Vector2(rect.offsetMin.x, originalOffsetMin.y);
    }
}
