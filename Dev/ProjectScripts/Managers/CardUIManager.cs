using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUIManager : MonoBehaviour
{
    private Main main;

    public TMP_Text drawPileCount;
    public TMP_Text discardPileCount;

    private void Start()
    {
        main = FindAnyObjectByType<Main>();
    }

    private void Update()
    {
        drawPileCount.text = $"{main.CardDeck.Count}";
        discardPileCount.text = $"{main.DisCardPile.Count}";
    }
}
