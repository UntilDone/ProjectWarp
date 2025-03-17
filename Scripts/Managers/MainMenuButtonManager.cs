using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Material material;
    private Color outlineColor;

    private void Start()
    {
        TextMeshProUGUI textMeshProUGUI = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        material = textMeshProUGUI.fontMaterial;
        outlineColor = new Color(107/255f, 81/255f, 74/255f, 1);
        material.SetColor("_OutlineColor", outlineColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        material.SetFloat("_OutlineWidth", 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        material.SetFloat("_OutlineWidth", 0f);
    }

}
