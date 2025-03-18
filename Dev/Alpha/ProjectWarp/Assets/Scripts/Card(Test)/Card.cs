using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Card Stats
    //public int id;
    //public string card_name;
    //public string sprite_name;
    //public string type;
    //public string rarity;
    //public int cost;
    //public int stat;
    //public string desc;
    //public int attack_count;
    //public string status_name;
    //public int status_duration;
    //public string status_target;
    //public bool unlock;
    //public bool targetable;
    //public string part;
    //public int part_burden;
    #endregion

    [SerializeField]
    GameObject mainGo;

    public LineRenderer lineRenderer;

    public int savedLayerOrder;
    public Vector2 savedScale;

    public Vector2 savedPosition;
    public Vector3 savedRotation;
    public Vector3 mousePosition;

    public Vector2 hoverScale;
    public Vector2 normalScale;
    public Vector2 smallScale;

    private float pushSpeed = 0.2f;
    private float pullSpeed = 0.2f;
    private float useSpeed = 0.5f;
    private float pullVerticalSpeed = 0.2f;
    private float scaleSpeed = 0.2f;

    public bool isCentered;
    public bool isInHand;
    public bool isScaling;
    public bool isPosSaved;
    public bool isMouseIn;
    public bool isKeyDown;
    public bool isKeySelect;
    public bool isClicked;
    public bool isUsed;
    public bool isPushed;
    public bool isPushing;
    public bool isGetBack;
    public bool isPullingHorizontal;
    public bool isDiscarded;

    public Tween scaleTween;
    public GameObject zoneGo;
    public Collider2D coll2D;

    private CardTweenManager cardTweenManager;
    public AudioManager audioManager;

    public int cardIndex;
    public int cardId;

    public (int startIndex, int length)? animateInfo;
    private Coroutine shakeCoroutine;
    public card_data cardData = new card_data();

    public string animatedHexColor;

    void Start()
    {
        hoverScale = new Vector2(0.44f, 0.44f);
        normalScale = new Vector2(0.36f, 0.36f); // 1 ~ 6 
        smallScale = new Vector2(0.30f, 0.30f); // 7 ~ 10

        mainGo = FindAnyObjectByType<Main>().gameObject;
        zoneGo = FindAnyObjectByType<CenterZone>().gameObject;
        coll2D = GetComponent<Collider2D>();
        cardTweenManager = CardTweenManager.Instance;
        audioManager = AudioManager.Instance;
    }

    private void Update()
    {
        if (isUsed || !isInHand)
        {
            return;
        }

        CheckClick();
    }

    private void CheckClick()
    {
        if (cardData.playable == false)
        {
            return;
        }

        #region select card by key button
        if (Input.GetKeyDown(CheckKeyCode()))
        {
            if (cardTweenManager.draggingCard == null
                && cardTweenManager.targetingCard == null
                //&& cardTweenManager.isArranging == false
                && CardChooseManager.Instance.cardChooseList.Count == 0
                /*&& !cardTweenManager.isUsingAnyCard*/)
            {
                isMouseIn = true;

                Main main = FindAnyObjectByType<Main>();

                if (cardData.targetable)
                {
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 300;
                    transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = 301;
                    transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 302;
                    transform.GetChild(3).GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 302;
                    transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 303;
                    transform.GetChild(3).GetChild(2).GetComponent<Canvas>().sortingOrder = 303;
                    transform.GetChild(3).GetChild(2).GetComponent<Canvas>().overrideSorting = true;
                }
            }
        }
        #endregion

        #region select card by mouse left or key button
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(CheckKeyCode()))
        {
            if (!isClicked && isMouseIn
                && cardTweenManager.draggingCard == null
                && cardTweenManager.targetingCard == null)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 300;
                transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = 301;
                transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 302;
                transform.GetChild(3).GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 302;
                transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 303;
                transform.GetChild(3).GetChild(2).GetComponent<Canvas>().sortingOrder = 303;
                transform.GetChild(3).GetChild(2).GetComponent<Canvas>().overrideSorting = true;

                isClicked = true;

                if (cardData.targetable)
                {
                    ActivateTargetCursor();
                }
                else
                {
                    UnityEngine.Cursor.visible = false;
                    cardTweenManager.draggingCard = this;
                }

            }
            else if (isClicked)
            {
                GameObject targetEnemy = BattleManager.Instance.targetEnemy;
                if (cardData.type == "CARD_TYPE_ATTACK" && cardData.targetable == true && targetEnemy == null)
                {
                    ResetLayerOrder();
                    CheckPushing();
                    Unclick();
                    return;
                }

                if (cardTweenManager.draggingCard != null)
                {
                    transform.position = mousePosition;
                    cardTweenManager.draggingCard = null;
                }
                else if (cardTweenManager.targetingCard != null)
                {
                    cardTweenManager.targetingCard = null;
                }
                cardTweenManager.EmptyHand(transform);
                UseCard();

                isMouseIn = false;
            }
        }
        #endregion

        #region card already selected then space button down
        if (isClicked && Input.GetKeyDown(KeyCode.Space))
        {
            GameObject targetEnemy = BattleManager.Instance.targetEnemy;
            if (cardData.type == "CARD_TYPE_ATTACK" && cardData.targetable == true && targetEnemy == null)
            {
                ResetLayerOrder();
                CheckPushing();
                Unclick();
                return;
            }

            if (cardTweenManager.draggingCard != null)
            {
                transform.position = mousePosition;
                cardTweenManager.draggingCard = null;
            }
            else if (cardTweenManager.targetingCard != null)
            {
                cardTweenManager.targetingCard = null;
                TargetManager[] targets = BattleManager.Instance.targets;

                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i].TargetOff();
                }
            }
            cardTweenManager.EmptyHand(transform);
            UseCard();

            isMouseIn = false;
        }
        #endregion

        #region mouse right or escape button down
        if (isClicked && isMouseIn)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ResetLayerOrder();
                CheckPushing();
                Unclick();

                if (cardData.targetable)
                {
                    cardTweenManager.ArrangeHand(true, this);
                }
            }
        }
        #endregion

        mousePosition = cardTweenManager.mousePosition;

        if (isClicked && isMouseIn && !isUsed && !cardData.targetable && cardTweenManager.draggingCard != null)
        {
            mousePosition.z = -5;
            transform.position = mousePosition;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = hoverScale;
        }

        if (isMouseIn)
        {
            // text animation
            if (animateInfo != null && shakeCoroutine == null)
            {
                ApplyShakeAnimation(animateInfo.Value.startIndex, animateInfo.Value.length);
            }
        }
    }

    private void ActivateTargetCursor()
    {
        cardTweenManager.targetingCard = this;
        cardTweenManager.SetTargetCardTween(transform, 0.2f);

        MouseRenderer mouseRenderer = FindObjectOfType<MouseRenderer>(true);
        mouseRenderer.gameObject.SetActive(true);
    }

    #region KeyCode to integer
    private KeyCode CheckKeyCode()
    {
        switch (cardIndex)
        {
            case 0: return KeyCode.Alpha1;
            case 1: return KeyCode.Alpha2;
            case 2: return KeyCode.Alpha3;
            case 3: return KeyCode.Alpha4;
            case 4: return KeyCode.Alpha5;
            case 5: return KeyCode.Alpha6;
            case 6: return KeyCode.Alpha7;
            case 7: return KeyCode.Alpha8;
            case 8: return KeyCode.Alpha9;
            case 9: return KeyCode.Alpha0;
            default: break;
        }
        return KeyCode.None;
    }
    #endregion


    public void ResetScale()
    {
        Main main = FindAnyObjectByType<Main>();
        if (main.HandDeck.Count > 6)
        {
            scaleTween = transform.DOScale(smallScale, scaleSpeed);
            isScaling = false;
        }
        else if (main.HandDeck.Count < 7 && main.HandDeck.Count > 0)
        {
            scaleTween = transform.DOScale(normalScale, scaleSpeed);
            isScaling = false;
        }
    }

    private void DisableMouseRenderer()
    {
        MouseRenderer mouseRenderer = FindObjectOfType<MouseRenderer>(true);
        mouseRenderer.gameObject.SetActive(false);

        //Enemy enemy = FindAnyObjectByType<Enemy>();

        //if (enemy.reticleGo != null)
        //{
        //    Destroy(enemy.reticleGo);
        //}
    }

    private void UseCard()
    {
        Main main = FindObjectOfType<Main>();

        BattleManager battle = BattleManager.Instance;
        battle.targets = FindObjectsOfType<TargetManager>();

        TargetManager target = null;
        TargetManager[] targets = battle.targets;

        GameObject targetObject = BattleManager.Instance.targetEnemy;

        if (targetObject != null)
        {
            target = targetObject.GetComponent<TargetManager>();
            target.TargetOff();
        }

        audioManager.PlayAudioById(cardData.id, 0.5f);

        if (cardData.status_name != "")
        {
            if (cardData.targetable == true)
            {
                int statusId = target.GetStatusIdFromCard(cardData);
                target.AddStatus(statusId, cardData.status_duration);
            }
            else
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].isTargetPlayer)
                    {
                        continue;
                    }
                    int id = targets[i].GetStatusIdFromCard(cardData);
                    targets[i].AddStatus(id, cardData.status_duration);
                }
            }
        }

        if (cardData.type == "CARD_TYPE_ATTACK")
        {
            battle.player.GiveMovingEffect("right", true);

            if (cardData.targetable == true)
            {
                for (int i = 0; i < cardData.attack_count; i++)
                {
                    target.GiveMovingEffect("right", false);
                    target.GiveDamage(cardData.stat);
                    target.GiveEffect(cardData.id);
                }
            }
            else if (cardData.targetable == false)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].isTargetPlayer || targets[i].isTargetDead)
                    {
                        continue;
                    }

                    targets[i].GiveMovingEffect("right", false);
                    targets[i].GiveDamage(cardData.stat);
                }
            }
        }
        else if (cardData.type == "CARD_TYPE_SKILL")
        {
            battle.player.GiveMovingEffect("up", false);

            if (cardData.draw_count > 0)
            {
                for (int i = 0; i < cardData.draw_count; i++)
                {
                    if (main.CardDeck.Count == 0)
                    {
                        StartCoroutine(main.DrawCardAfterShuffleFinished());
                    }
                    else
                    {
                        StartCoroutine(main.DrawCardAfterDiscard());
                    }
                }
            }

            if (cardData.defend)
            {
                battle.player.GiveShield(cardData.stat);
            }
        }

        UnityEngine.Cursor.visible = true;

        isUsed = true;

        // count how many cards have used in this turn
        zoneGo.GetComponent<CenterZone>().usedCardAmount++;
        int usedCardAmount = zoneGo.GetComponent<CenterZone>().usedCardAmount;

        // rendering order
        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 100 + usedCardAmount * 10;
        transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 104 + usedCardAmount * 10;
        transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = 101 + usedCardAmount * 10;
        transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 102 + usedCardAmount * 10;
        transform.GetChild(3).GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 102 + usedCardAmount * 10;
        transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 103 + usedCardAmount * 10;
        transform.GetChild(3).GetChild(2).GetComponent<Canvas>().sortingOrder = 103 + usedCardAmount * 10;
        transform.GetChild(3).GetChild(2).GetComponent<Canvas>().overrideSorting = true;
        transform.GetChild(3).GetChild(2).GetChild(2).gameObject.SetActive(false);

        cardTweenManager.StartUsingSequence(transform, useSpeed);

        //CheckCardId();
        DisableMouseRenderer();
    }

    public void DamageEffect(int damage)
    {
        Main main = mainGo.GetComponent<Main>();
        //Enemy enemy = FindAnyObjectByType<Enemy>();
        //Player player = FindAnyObjectByType<Player>();
        GameObject damageEffectGo = Instantiate(main.damageEffectGo, main.effectCanvas.transform);
        TMP_Text text = damageEffectGo.GetComponent<TMP_Text>();
        text.text = $"{damage}";

        //player.PlayAnimation("Ironclad_Attack");
        //enemy.currentHp -= damage;
    }

    public void ResetLayerOrder()
    {
        if (isScaling) return;

        int baseOrder = cardIndex * 10;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = baseOrder + 1;
        transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = baseOrder + 5;
        transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = baseOrder + 2;
        transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = baseOrder + 3;
        transform.GetChild(3).GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = baseOrder + 3;
        transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = baseOrder + 4;
        transform.GetChild(3).GetChild(2).GetComponent<Canvas>().sortingOrder = baseOrder + 4;
        transform.GetChild(3).GetChild(2).GetComponent<Canvas>().overrideSorting = true;
        transform.GetChild(3).GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = $"{cardIndex + 1}";
    }

    public void Unclick()
    {
        UnityEngine.Cursor.visible = true;

        TargetManager[] targetManagers = FindObjectsOfType<TargetManager>();

        for (int i = 0; i < targetManagers.Length; i++)
        {
            targetManagers[i].TargetOff();
        };

        isClicked = false;
        isMouseIn = false;
        isScaling = false;

        if (cardTweenManager.draggingCard)
        {
            cardTweenManager.draggingCard = null;
        }
        else if (cardTweenManager.targetingCard)
        {
            cardTweenManager.targetingCard = null;
            TargetManager[] targets = BattleManager.Instance.targets;

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].TargetOff();
            }
        }

        Main main = FindAnyObjectByType<Main>();
        for (int i = 0; i < main.HandDeck.Count; i++)
        {
            cardTweenManager.SetPullVerticalTween(main.HandDeck[i].transform, pullVerticalSpeed);
        }

        DisableMouseRenderer();
    }

    private bool CheckDrawing()
    {
        if (cardTweenManager.isArranging)
        {
            return true;
        }
        return false;
    }

    private void CheckPushing()
    {
        List<GameObject> hand = mainGo.GetComponent<Main>().HandDeck;

        if (hand.Count == 1)
        {
            return;
        }

        isPushing = false;

        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i] == gameObject)
            {
                if (i == 0) // Left
                {
                    hand[i + 1].GetComponent<Card>().isPushed = false;
                    cardTweenManager.SetGetBackTween(hand[i + 1].transform, pullSpeed);
                }
                else if (i == hand.Count - 1) // Right
                {
                    hand[i - 1].GetComponent<Card>().isPushed = false;
                    cardTweenManager.SetGetBackTween(hand[i - 1].transform, pullSpeed);
                }
                else
                {
                    hand[i - 1].GetComponent<Card>().isPushed = false;
                    hand[i + 1].GetComponent<Card>().isPushed = false;
                    cardTweenManager.SetGetBackTween(hand[i - 1].transform, pullSpeed);
                    cardTweenManager.SetGetBackTween(hand[i + 1].transform, pullSpeed);
                }
                break;
            }
        }
    }

    public void Push()
    {
        if (!cardTweenManager.isArranging && !isClicked && isMouseIn)
        {
            cardTweenManager.SetPushTween(transform, pushSpeed);
        }
    }

    public void Hover()
    {
        if (
            cardTweenManager.draggingCard == null
            && isMouseIn && !isClicked && transform.position.y <= -3.0f)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 120;
            transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = 121;
            transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 122;
            transform.GetChild(3).GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 122;
            transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 123;
            transform.GetChild(3).GetChild(2).GetComponent<Canvas>().sortingOrder = 123;
            transform.GetChild(3).GetChild(2).GetComponent<Canvas>().overrideSorting = true;

            transform.position = new Vector3(transform.position.x, -3.0f, -5.0f);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = hoverScale;
            isScaling = true;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardTweenManager.draggingCard != null
            || cardTweenManager.targetingCard != null
            || isUsed || isClicked || !isInHand
            || transform.position.x < FindAnyObjectByType<Main>().HandDeck[0].GetComponent<Card>().savedPosition.x - 0.6f
            && FindAnyObjectByType<Main>().HandDeck.Count > 0)
        {
            return;
        }
        else
        {
            isMouseIn = true;

            Hover();
            Push();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isUsed || isClicked || !isInHand)
        {
            return;
        }
        else
        {
            isMouseIn = false;

            transform.localScale = savedScale;
            isScaling = false;

            cardTweenManager.SetGetBackTween(transform, pullSpeed);
            ResetLayerOrder();
            CheckPushing();

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("CenterZone"))
        {
            isCentered = true;
        }
    }

    #region text animations
    public void ApplyShakeAnimation(int startIndex, int length)
    {
        shakeCoroutine = StartCoroutine(ShakeCharacters(startIndex, length));
    }

    private IEnumerator ShakeCharacters(int startIndex, int length)
    {
        TMP_Text cardDescText = transform.GetChild(3).GetChild(2).GetChild(1).GetComponent<TMP_Text>();

        while (true)
        {
            cardDescText.ForceMeshUpdate();
            TMP_TextInfo textInfo = cardDescText.textInfo;

            for (int i = 0; i < length; i++)
            {
                int charIndex = startIndex + i;

                if (charIndex >= textInfo.characterCount)
                {
                    continue;
                }

                TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];

                if (!charInfo.isVisible)
                {
                    continue;
                }

                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                float offsetY = Mathf.Sin(Time.time * 5f) * 0.0020f;

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] += new Vector3(0, offsetY, 0);
                }
            }

            cardDescText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            yield return null;
        }
    }
    #endregion

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