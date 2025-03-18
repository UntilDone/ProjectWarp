using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour
{
    public card_data data;
    public int index;

    public (int startIndex, int length)? animateInfo;

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            CardChooseManager reward = CardChooseManager.Instance;

            if (reward.isCardSelected)
            {
                return;
            }

            reward.isCardSelected = true;
            gamesave_data.Instance.cardDeck.Add(data);
            reward.cardChooseList.Remove(data);
            reward.cardObjects.Remove(gameObject);
            Destroy(gameObject);

            for (int i = 0; i < reward.cardObjects.Count; i++)
            {
                reward.cardObjects[i].GetComponent<Button>().interactable = false;

                int childCount = reward.cardObjects[i].transform.childCount;
                for (int j = 0; j < childCount; j++)
                {
                    reward.cardObjects[i].transform.GetChild(j).gameObject.SetActive(false);
                }
            }

            GameObject screen = BattleManager.Instance.FindInactiveObjectWithTag("RewardScreen").transform.parent.GetChild(2).gameObject;
            screen.SetActive(true);
            screen.GetComponent<SpriteRenderer>().DOFade(1, 0.2f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                reward.isCardSelected = false;
                reward.cardChooseList.Clear();
                reward.cardObjects.Clear();
                DOTween.PauseAll();
                BattleManager.Instance.player.SavePlayerData();
                DataManager.Instance.SaveGame();
                SceneManager.LoadScene("Map");
            });
        });

        ChangeCardImage();
        ChangeCardText();
    }

    public void ChangeCardImage()
    {
        Image image = transform.GetChild(1).GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>($"CardResources/{data.id}");
    }

    public void ChangeCardText()
    {
        TMP_Text name = transform.GetChild(2).GetComponent<TMP_Text>();

        if (name == null)
        {
#if DEBUG_MODE
            Debug.LogError("cardNameText is null at path rewardCard.transform.GetChild(2)");
#endif
        }
        name.text = LocalizationManager.Instance.GetLocalizedText(data.card_name);

        TMP_Text desc = transform.GetChild(3).GetComponent<TMP_Text>();

        string rawText = LocalizationManager.Instance.GetLocalizedText(data.desc,
                data.stat,
                data.status_duration,
                LocalizationManager.Instance.GetLocalizedText(data.status_name),
                data.attack_count,
                data.draw_count);

        string animatedText = BattleManager.Instance.ParseAnimateTag("#944431", rawText, out (int startIndex, int length)? animateInfo);
        this.animateInfo = animateInfo;
        desc.text = animatedText;
    }

    #region box gizmo
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (!box)
            return;

        var offset = box.offset;
        var extents = box.size * 0.5f;
        var verts = new Vector2[] {
                transform.TransformPoint (new Vector2 (-extents.x, -extents.y) + offset),
                transform.TransformPoint (new Vector2 (extents.x, -extents.y) + offset),
                transform.TransformPoint (new Vector2 (extents.x, extents.y) + offset),
                transform.TransformPoint (new Vector2 (-extents.x, extents.y) + offset) };

        Gizmos.color = Color.green;
        Gizmos.DrawLine(verts[0], verts[1]);
        Gizmos.DrawLine(verts[1], verts[2]);
        Gizmos.DrawLine(verts[2], verts[3]);
        Gizmos.DrawLine(verts[3], verts[0]);
    }
    #endregion
}
