using UnityEngine;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour
{
    public Text nameText;
    public Text costText;
    public Text descText;

    private card_data cardData;
    public card_data CardData
    {
        get { return cardData; }
    }
    public void SetCardData(card_data card)
    {
        cardData = card;
        UpdateCardUI();
    }
    private void UpdateCardUI()
    {
        if (cardData != null)
        {
            nameText.text = cardData.card_name;
            descText.text = cardData.desc;
        }
    }
}
