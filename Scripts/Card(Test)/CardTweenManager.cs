using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.XR;
using TMPro;
using System.Threading.Tasks;

public class CardTweenManager : MonoBehaviour
{
    private static CardTweenManager instance;

    public static CardTweenManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CardTweenManager>();
            }
            return instance;
        }
    }

    private Main main;

    // drawPile Animation
    public GameObject drawPileGo;
    private Tween drawPileSizingTween;

    // discardPile Animation
    public GameObject discardPileGo;
    private Tween discardPileSizingTween;

    // ArrangeHand from Main
    [Range(0, 5f)] public float RotSpeed = 0.1f;
    [Range(0, 5f)] public float MoveSpeed = 0.05f;
    [Range(0, 5f)] public float LeftMoveSpeed = 0.04f;
    [Range(0, 5f)] public float ScaleSpeed = 0.3f;
    [Range(0, 5f)] public float LeftScaleSpeed = 0.2f;

    [Range(0, 5f)] public float FadeSpeed = 0.04f;
    [Range(0, 5f)] public float EffectFadeSpeed = 0.12f;

    private Vector3 defaultCardPosition = new Vector3(-6.5f, -4.5f, 0);
    private Vector2 finalScale = new Vector2(0.36f, 0.36f);

    private float leftCard;
    private float rightCard;
    private float Count;
    private float posY;

    public bool isArranging;
    public bool isBezier;
    public bool isUsingAnyCard;
    public bool isEndingTurn;

    public Vector3 mousePosition;
    public RaycastHit2D hit;

    // Card Tweens that exists before
    public Sequence fadeSequence;
    public Sequence EvenLeftHandSequence;
    public Sequence EvenRightHandSequence;
    public Sequence OddLeftHandSequence;
    public Sequence OddCenterHandSequence;
    public Sequence OddRightHandSequence;

    public Sequence hoverSequence;
    public Sequence pushSequence;
    public Sequence pullSequence;
    public Sequence pullPosSequence;
    public Sequence usingSequence;
    public Sequence usedDiscardSequence;
    public Sequence shuffleSequence;
    public Sequence bezierSequence;

    public GameObject[] pushCards = new GameObject[2];
    public Card draggingCard;
    public Card targetingCard;
    public Tween pullVerticalTween;

    public int discardTweenCount;
    public int drawTweenCount;
    public int shuffleTweenCount;

    public bool isShuffleFinished;

    private AudioManager audioManager;

    private void Start()
    {
        main = FindObjectOfType<Main>();
        audioManager = AudioManager.Instance;
    }

    private void Update()
    {
        if (CardChooseManager.Instance.cardChooseList.Count > 0)
        {
            return;
        }

        #region prevent double collide
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hit = Physics2D.Raycast(mousePosition, Vector3.zero, 0.1f);

        if (hit)
        {
            if (hit.collider.GetComponent<Card>())
            {
                Card card = hit.collider.GetComponent<Card>();
                Vector3 position = card.transform.position;
                card.transform.position = new Vector3(position.x, position.y, -5.0f);
            }
            else if (hit.collider.GetComponent<TargetManager>())
            {
                TargetManager target = hit.collider.GetComponent<TargetManager>();

                if (targetingCard != null)
                {
                    target.TargetOn();
                }
                //else
                //{
                //    target.TargetOff();
                //}
            }
        }
        #endregion
    }

    public void SetTargetCardTween(Transform trans, float speed)
    {
        Card card = trans.GetComponent<Card>();

        //Vector3 pos = new Vector3(trans.position.x, -3.0f, -5.0f);
        Vector3 pos = new Vector3(0, -3.0f, -5.0f);
        trans.rotation = Quaternion.Euler(0, 0, 0);

        trans.DOMove(pos, speed);
        trans.DORotate(new Vector3(0, 0, 0), speed);
        trans.DOScale(card.hoverScale, speed)
            .OnUpdate(() =>
            {
            });
    }

    public void StartUsingSequence(Transform trans, float speed)
    {
        Card card = trans.GetComponent<Card>();
        Vector2 pos = card.zoneGo.transform.position + Camera.main.ScreenToWorldPoint(Input.mousePosition) * 0.1f;

        usingSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                BattleManager.Instance.ChangeTurnButtonColor(false);
            });

        usingSequence.Append(trans.DOMove(pos, speed).SetEase(Ease.OutCubic))
            .Join(trans.DOScale(card.savedScale, speed).SetEase(Ease.InCubic))
            .OnUpdate(() =>
            {
                isUsingAnyCard = true;
                trans.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

                if (card.isCentered)
                {
                    usingSequence.Kill();
                    usingSequence = null;
                }
            })
            .OnKill(() =>
            {
                Sequence hoveringSequence = DOTween.Sequence();

                hoveringSequence.Join(trans.DOMove(card.zoneGo.transform.position, speed).SetEase(Ease.OutCubic))
                .Join(trans.DOScale(card.savedScale, speed).SetEase(Ease.InCubic))
                .OnUpdate(() =>
                {
                    //trans.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                })
                .OnComplete(() =>
                {
                    bool isOnHand = false;
                    card.isCentered = false;
                    card.isDiscarded = false;

                    float discardSpeed = 0.2f;
                    float speed = discardSpeed * 2f;
                    float bezierScale = Random.Range(1f, 2f); // higher means higher curve

                    // Before Arrange, card in hand must be removed
                    SetUsedDiscardTween(trans, speed, discardSpeed, bezierScale, isOnHand);

                    BattleManager battle = BattleManager.Instance;
                    battle.ChangeTurnButtonColor(true);
                });
                card.isDiscarded = true;
                //draggingCard = null;
            });
    }

    public void SetUsedDiscardTween(Transform trans, float speed, float discardSpeed, float bezierScale, bool isOnHand)
    {
        Card card = trans.GetComponent<Card>();

        if (!card.isUsed)
        {
            card.isUsed = true;
        }

        int childIndex = card.transform.childCount - 2;
        card.lineRenderer = card.transform.GetChild(childIndex).gameObject.GetComponent<LineRenderer>();
        card.lineRenderer.positionCount = 0;


        // line renderer
        Vector3[] checkPoint = new Vector3[50]; // length of array = the number of segments
        //int checkPointCount = 0;

        // pos
        Vector2 startPos = trans.position;
        Vector2 discardPilePos = discardPileGo.transform.position;
        discardPilePos.x -= 0.25f;
        discardPilePos.y += 0.20f;

        bool removeFirstCurve = isOnHand;
        float secondCurveAdjustmentX = isOnHand ? -0.25f : 1;
        float secondCurveAdjustmentY = isOnHand ? -0.45f : 1;

        // bezier first curve
        Vector3 path0 = startPos;
        path0.y += 0.05f;
        Vector3 path1 = startPos;
        path1.x += 0.1f;
        path1.y += 0.2f;

        // layover
        Vector3 waypoint0 = startPos;
        waypoint0.x += 0.7f;
        waypoint0.y += 0.3f;

        if (removeFirstCurve)
        {
            path0 = startPos;
            path1 = startPos;
            waypoint0 = startPos;
        }

        // bezier second curve
        Vector3 path2 = path1;
        path2.x += 1.0f * bezierScale * secondCurveAdjustmentX;
        Vector3 path3 = path2;
        path3.x = 1.0f * bezierScale * secondCurveAdjustmentX;
        path3.y = -1.0f * bezierScale * secondCurveAdjustmentY;
        Vector3 waypoint1 = discardPilePos;

        Vector3[] pathWaypoints = new[] { waypoint0, path0, path1, waypoint1, path2, path3 };

        // rot
        Vector3 dir = (startPos - discardPilePos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle + 90, Vector3.forward);

        bezierSequence = DOTween.Sequence().PrependInterval(2f);

        //Transform lineTrans = card.lineRenderer.gameObject.transform;

        bezierSequence.OnStart(() =>
        {
            card.transform.GetChild(0).gameObject.SetActive(false);

            card.isMouseIn = false;
            isUsingAnyCard = false;

            // to update every frame
            bezierSequence.SetUpdate(true);
            //lineTrans.position = Vector3.zero;
            //lineTrans.localPosition = Vector3.zero;
        })
            //.Join(lineTrans.DOPath(pathWaypoints, discardSpeed, PathType.CubicBezier, PathMode.Sidescroller2D, 5, Color.yellow))
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() =>
            {
                isBezier = true;

                //lineTrans.position = trans.position; // 로컬 포지션을 (0, 0, 0)으로 설정
                //lineTrans.localPosition = Vector3.zero; // 로컬 포지션을 (0, 0, 0)으로 설정

                //if (checkPointCount == 0 || Vector3.Distance(checkPoint[checkPointCount - 1], lineTrans.position) > 0.05f)
                //{
                //    if (checkPointCount < checkPoint.Length)
                //    {
                //        checkPoint[checkPointCount] = lineTrans.position;
                //        checkPointCount++;
                //    }
                //    else
                //    {
                //        for (int i = 0; i < checkPoint.Length - 1; i++)
                //        {
                //            checkPoint[i] = checkPoint[i + 1];
                //        }
                //        checkPoint[checkPoint.Length - 1] = lineTrans.position;
                //    }

                //    card.lineRenderer.positionCount = checkPointCount;

                //    for (int i = 0; i < checkPointCount; i++)
                //    {
                //        checkPoint[i].z = -1f;
                //        card.lineRenderer.SetPosition(i, checkPoint[i]);
                //    }

                //    //lineTrans.localPosition = checkPoint[checkPointCount - 1];
                //}
            })
            .OnComplete(() =>
            {
                isBezier = false;
                //checkPointCount = 0;
                //card.lineRenderer.positionCount = 0;
            });

        usedDiscardSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                card.transform.GetChild(3).GetChild(2).GetChild(2).gameObject.SetActive(false);
            });

        usedDiscardSequence.Join(trans.DOPath(pathWaypoints, speed, PathType.CubicBezier, PathMode.Sidescroller2D, 5, Color.green))
            .Join(trans.DORotate(rot.eulerAngles, speed, RotateMode.FastBeyond360).SetEase(Ease.OutExpo))
            .Join(trans.DOScale(new Vector2(0.04f, 0.04f), speed).SetEase(Ease.OutExpo))
            .OnComplete(() =>
            {
                Discard(trans);

                Main main = FindAnyObjectByType<Main>();

                trans.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 0;
                trans.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 3;
                trans.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = 1;
                trans.GetChild(3).transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 2;
                trans.GetChild(3).transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 2;
                trans.GetChild(3).transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 3;
                trans.GetChild(3).transform.GetChild(2).GetComponent<Canvas>().sortingOrder = 2;

                if (discardPileSizingTween != null)
                {
                    discardPileSizingTween.Kill();
                }

                discardPileSizingTween = discardPileGo.transform.DOScale(new Vector2(0.5f, 0.5f), 0.05f);
                discardPileSizingTween.OnComplete(() =>
                {
                    discardPileSizingTween = discardPileGo.transform.DOScale(new Vector2(0.4f, 0.4f), 0.05f);
                });

                discardTweenCount--;

                if (!card.isDiscarded && trans.gameObject != null)
                {
                    card.isInHand = false;
                    ResetCard(trans);
                }

                int count = trans.transform.childCount;
                for (int j = 0; j < count; j++)
                {
                    if (j == count - 2)
                    {
                        continue;
                    }
                    trans.transform.GetChild(j).gameObject.SetActive(false);
                }

                if (discardTweenCount == 0)
                {
                    //RedrawCards();
                    isEndingTurn = false;
                }

                //isUsingAnyCard = false;

                if (!isEndingTurn)
                {
                    ArrangeHand(false, null);
                }
            });

        usedDiscardSequence.Join(trans.GetChild(0).GetComponent<SpriteRenderer>().DOFade(0, speed).SetEase(Ease.OutQuad))
            .Join(trans.GetChild(1).GetComponent<SpriteRenderer>().DOFade(1, speed).SetEase(Ease.OutQuad));
    }

    public void EmptyHand(Transform trans)
    {
        main.HandDeck.Remove(trans.gameObject);
        main.HandDeckAnims.Remove(trans.GetChild(0).GetComponent<Animator>());
        main.UsingPile.Add(trans.gameObject);
    }

    public void Discard(Transform trans)
    {
        main.UsingPile.Remove(trans.gameObject);
        main.DisCardPile.Add(trans.gameObject);
    }

    public void SetShuffleTween(Transform trans, float speed)
    {
        speed *= Random.Range(5f, 10f);
        trans.GetChild(3).gameObject.SetActive(false);

        // pos
        Vector2 pos = trans.position;
        Vector2 drawPilePosition = drawPileGo.transform.position;
        drawPilePosition.y += 0.20f;

        // rot
        Vector2 dir = (pos - drawPilePosition).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle + 90, Vector3.forward);

        shuffleSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                AudioClip cardShuffleSound = audioManager.sfxClips[0];
                audioManager.PlayAudioOnce(cardShuffleSound, 0.1f);
            });

        shuffleSequence.Join(trans.DORotate(rot.eulerAngles, speed, RotateMode.FastBeyond360).SetEase(Ease.OutQuint))
            .Join(trans.DOMove(drawPilePosition, speed).SetEase(Ease.InSine))
            .Join(trans.DOScale(new Vector2(0.10f, 0.10f), speed).SetEase(Ease.InSine))
            .OnComplete(() =>
            {
                Destroy(trans.gameObject);

                if (drawPileSizingTween != null)
                {
                    drawPileSizingTween.Kill();
                }

                drawPileSizingTween = drawPileGo.transform.DOScale(new Vector2(0.9f, 0.9f), 0.05f);
                drawPileSizingTween.OnComplete(() =>
                {
                    drawPileSizingTween = drawPileGo.transform.DOScale(new Vector2(0.8f, 0.8f), 0.05f);
                });

                main.CardDeck[shuffleTweenCount].transform.GetChild(2).gameObject.SetActive(true);

                shuffleTweenCount++;

                if (shuffleTweenCount == main.CardDeck.Count)
                {
                    isShuffleFinished = true;
                }
            });

        trans.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, speed).SetEase(Ease.InExpo);
    }

    public void RedrawCards()
    {
        Main main = FindAnyObjectByType<Main>();

        if (BattleManager.Instance.playerTurnCount > BattleManager.Instance.enemyTurnCount)
        {
            return;
        }

        if (main.CardDeck.Count < 5)
        {
            main.LessCard = main.CardDeck.Count; // 남은 카드 수 저장
            main.MaxDrawCount = main.LessCard;   // 남은 카드만큼 뽑기 위해 MaxDrawCount를 조정
            StartCoroutine(main.HandleDrawAndShuffle());
        }
        else
        {
            main.MaxDrawCount = 5;  // 카드가 5장 이상일 때는 MaxDrawCount를 5로 설정
            StartCoroutine(main.DrawCardsWithInterval());
        }
    }

    public void ResetCard(Transform trans)
    {
        Card card = trans.GetComponent<Card>();

        card.scaleTween.Kill();
        card.scaleTween = null;

        card.isCentered = false;
        card.isInHand = false;
        card.isPosSaved = false;
        card.isMouseIn = false;
        card.isClicked = false;
        card.isUsed = false;
        card.isPushed = false;
        card.isGetBack = false;
        card.isPullingHorizontal = false;
        card.isDiscarded = false;
        card.isScaling = false;

        trans.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void SetGetBackTween(Transform trans, float speed)
    {
        Card card = trans.GetComponent<Card>();

        for (int i = 0; i < main.HandDeck.Count; i++)
        {
            if (main.HandDeck[i].GetComponent<Card>().isClicked)
            {
                return;
            }
        }

        pullSequence = DOTween.Sequence().SetEase(Ease.OutSine)
            .OnUpdate(() =>
            {
                card.isGetBack = true;
                if (card.isMouseIn)
                {
                    trans.position = new Vector3(trans.position.x, -3.0f, -5.0f);
                    trans.rotation = Quaternion.Euler(0, 0, 0);
                }
            })
            .OnComplete(() => { card.isGetBack = false; });

        pullSequence
            .Append(trans.DOMove(new Vector2(card.savedPosition.x, card.savedPosition.y), speed))
            .Join(trans.DORotate(card.savedRotation, speed));
    }

    public void SetPullHorizontalTween(Transform trans, float speed)
    {
        Card card = trans.GetComponent<Card>();
        if (card.isPullingHorizontal) return;

        pullPosSequence = DOTween.Sequence()
            .SetEase(Ease.OutSine)
            .OnUpdate(() =>
            {
                card.isPullingHorizontal = true;
                card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y, -1.0f);
            })
            .OnComplete(() => { card.isPullingHorizontal = false; });

        pullPosSequence
            .Join(trans.DOMoveY(-3, speed))
            .Join(trans.DOMoveX(card.savedPosition.x, speed))
            .Join(trans.DORotate(card.savedRotation, speed));
    }

    public void SetPullVerticalTween(Transform trans, float speed)
    {
        Card card = trans.GetComponent<Card>();

        trans.DORotate(card.savedRotation, speed);
        trans.DOScale(card.savedScale, speed);

        pullVerticalTween = trans.DOMove(card.savedPosition, speed).SetEase(Ease.OutSine)
            .OnUpdate(() =>
            {
                if (card.isScaling)
                {
                    card.Hover();
                }
            });
    }

    public void SetPushTween(Transform trans, float speed)
    {
        Card card = trans.GetComponent<Card>();
        if (card.isPushed) return;

        card.isPushing = true;

        List<GameObject> hand = main.HandDeck;

        for (int i = 0; i < hand.Count; i++)
        {
            if (trans.gameObject == hand[i])
            {
                if (i == 0) // Left
                {
                    pushCards[0] = null;
                    if (hand.Count != 1)
                    {
                        pushCards[1] = hand[i + 1];
                    }
                }
                else if (i == hand.Count - 1) // Right
                {
                    pushCards[0] = hand[i - 1];
                    pushCards[1] = null;
                }
                else
                {
                    pushCards[0] = hand[i - 1];
                    pushCards[1] = hand[i + 1];
                }
                break;
            }
        }

        pushSequence = DOTween.Sequence().SetEase(Ease.OutSine);

        float pushStrength = 0.584f;

        if (hand.Count == 1)
        {
            return;
        }

        if (pushCards[0] == null) // Left
        {
            Transform right = pushCards[1].transform;
            Card rightCard = pushCards[1].GetComponent<Card>();
            pushSequence.Append(right.DOMove(new Vector2(rightCard.savedPosition.x + pushStrength, rightCard.savedPosition.y), speed))
                .Join(right.DORotate(rightCard.savedRotation, speed))
                .OnStart(() => { rightCard.isPushed = true; });
        }
        else if (pushCards[1] == null) // Right
        {
            Transform left = pushCards[0].transform;
            Card leftCard = pushCards[0].GetComponent<Card>();
            pushSequence.Append(left.DOMove(new Vector2(leftCard.savedPosition.x - pushStrength, leftCard.savedPosition.y), speed))
                .Join(left.DORotate(leftCard.savedRotation, speed))
                .OnStart(() => { leftCard.isPushed = true; });
        }
        else
        {
            Transform left = pushCards[0].transform;
            Transform right = pushCards[1].transform;
            Card leftCard = pushCards[0].GetComponent<Card>();
            Card rightCard = pushCards[1].GetComponent<Card>();
            pushSequence.Append(left.DOMove(new Vector2(leftCard.savedPosition.x - pushStrength, leftCard.savedPosition.y), speed))
                .Join(right.DOMove(new Vector2(rightCard.savedPosition.x + pushStrength, rightCard.savedPosition.y), speed))
                .Join(left.DORotate(leftCard.savedRotation, speed))
                .Join(right.DORotate(rightCard.savedRotation, speed))
                .OnStart(() =>
                {
                    leftCard.isPushed = true;
                    rightCard.isPushed = true;
                });
        }
    }

    private void SavePos()
    {
        for (int i = 0; i < main.HandDeck.Count; i++)
        {
            Card card = main.HandDeck[i].GetComponent<Card>();
            //card.savedPos = main.HandDeck[i].transform.position;
            //card.savedRot = main.HandDeck[i].transform.rotation.eulerAngles;
            card.savedScale = finalScale;
        }
    }

    private void AnimReset()
    {
        for (int i = 0; i < main.HandDeckAnims.Count; i++)
        {
            main.HandDeck[i].transform.GetChild(0).gameObject.SetActive(true);
            main.HandDeckAnims[i].Rebind();
            main.HandDeckAnims[i].enabled = false;
            main.HandDeckAnims[i].enabled = true;
        }
    }

    public void ArrangeHand(bool fastMode, Card targetCard)
    {
        main = FindAnyObjectByType<Main>();
        leftCard = main.HandDeck.Count / 2;
        rightCard = main.HandDeck.Count / 2;

        Sequence handSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                for (int i = 0; i < main.HandDeck.Count; i++)
                {
                    int textCount = main.HandDeck[i].transform.childCount;
                    for (int j = 1; j < textCount; j++)
                    {
                        main.HandDeck[i].transform.GetChild(j).gameObject.SetActive(true);
                    }
                }
            })
            .OnUpdate(() =>
            {
                isArranging = true;
            });

        List<GameObject> handDeck = main.HandDeck;

        if (handDeck.Count < 7)
        {
            finalScale = new Vector2(0.36f, 0.36f);
        }

        for (int i = 0; i < handDeck.Count; i++)
        {
            Card card = handDeck[i].GetComponent<Card>();
            card.cardIndex = i;

            if (fastMode == false && card == targetingCard)
            {
                continue;
            }
            else if (fastMode && card != targetCard)
            {
                continue;
            }

            card.isPushed = false;
            card.ResetLayerOrder();

            fadeSequence = DOTween.Sequence();

            fadeSequence.Append(
            handDeck[i].transform.GetChild(0).GetComponent<SpriteRenderer>().DOFade(1, EffectFadeSpeed).SetEase(Ease.InOutQuint))
                .Join(handDeck[i].transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, FadeSpeed).SetEase(Ease.InQuint));

            handSequence.Join(fadeSequence);

            // difference in position Y
            float posYscale = 0.15f;
            float posYscaleUnder6 = 0.4f;

            if (handDeck.Count % 2 == 0)
            {
                EvenLeftHandSequence = DOTween.Sequence();

                if (i <= (handDeck.Count / 2 - 1))
                {
                    for (int j = i; j < (leftCard); j++)
                    {
                        if (leftCard < 4)
                        {
                            if (i == 0 && handDeck.Count == 6)
                            {
                                posY = 1.0f;
                            }
                            else
                            {
                                posY += (posYscaleUnder6 * Count);
                            }
                        }
                        else
                        {
                            posY += (posYscale * Count);
                        }
                        Count++;
                    }

                    card.savedRotation = new Vector3(0, 0, (leftCard - (i)) * 5);
                    EvenLeftHandSequence.Append(
                    handDeck[i].transform.DORotate(card.savedRotation, RotSpeed).SetEase(Ease.OutQuad));

                    if (leftCard < 4)
                    {
                        card.savedPosition = new Vector2(((-(1.6f * (leftCard - (i + 1)))) - 0.64f), -4.4f - posY);
                        EvenLeftHandSequence.Join(
                        handDeck[i].transform.DOMove(card.savedPosition, LeftMoveSpeed).SetEase(Ease.OutQuad));
                    }
                    else
                    {
                        if (handDeck.Count == 10)
                        {
                            if (i == 0)
                            {
                                card.savedPosition = new Vector2(-5.132f, -5.2f - 0.3f);
                                EvenLeftHandSequence.Join(
                                handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                            }
                            else if (i == 1)
                            {
                                card.savedPosition = new Vector2(-4.102f, -4.85f - 0.3f);
                                EvenLeftHandSequence.Join(
                                handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                            }
                            else
                            {
                                card.savedPosition = new Vector2(((-(1.6f * (leftCard - (i + 1)))) - 0.64f) * 0.8f, -4.1f - posY - 0.3f);
                                EvenLeftHandSequence.Join(
                                handDeck[i].transform.DOMove(card.savedPosition, LeftMoveSpeed).SetEase(Ease.OutQuad));
                            }
                        }
                        else
                        {
                            card.savedPosition = new Vector2(((-(1.6f * (leftCard - (i + 1)))) - 0.64f) * 0.8f, -4.1f - posY - 0.3f);
                            EvenLeftHandSequence.Join(
                            handDeck[i].transform.DOMove(card.savedPosition, LeftMoveSpeed).SetEase(Ease.OutQuad));
                        }
                        finalScale = card.smallScale;
                    }
                    handSequence.Join(EvenLeftHandSequence);
                    posY = 0;
                    Count = 0;
                }
                else
                {
                    EvenRightHandSequence = DOTween.Sequence();

                    for (int j = i; j > (rightCard - 1); j--)
                    {
                        if (rightCard < 4)
                        {
                            if (i == 5 && handDeck.Count == 6)
                            {
                                posY = 1.0f;
                            }
                            else
                            {
                                posY += (posYscaleUnder6 * Count);
                            }
                        }
                        else
                        {
                            posY += (posYscale * Count);
                        }
                        Count++;
                    }

                    card.savedRotation = new Vector3(0, 0, ((i - rightCard) + 1) * -5);
                    EvenRightHandSequence.Append(
                    handDeck[i].transform.DORotate(card.savedRotation, RotSpeed).SetEase(Ease.OutQuad));

                    if (rightCard < 4)
                    {
                        card.savedPosition = new Vector2((1.6f * ((i - rightCard) + 1)) - 0.80f, -4.4f - posY);
                        EvenRightHandSequence.Join(
                        handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                    }
                    else
                    {
                        if (i == 8)
                        {
                            card.savedPosition = new Vector2(4.358f, -4.76f - 0.3f);
                            EvenRightHandSequence.Join(
                            handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                        }
                        else if (i == 9)
                        {
                            card.savedPosition = new Vector2(5.388f, -5.1f - 0.3f);
                            EvenRightHandSequence.Join(
                            handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                        }
                        else
                        {
                            card.savedPosition = new Vector2(((1.6f * ((i - rightCard) + 1)) - 0.64f) * 0.8f, -4.1f - posY - 0.3f);
                            EvenRightHandSequence.Join(
                            handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                        }
                        finalScale = card.smallScale;
                    }
                    handSequence.Join(EvenRightHandSequence);
                    posY = 0;
                    Count = 0;
                }
            }
            else
            {
                OddLeftHandSequence = DOTween.Sequence();

                OddCenterHandSequence = DOTween.Sequence();

                OddRightHandSequence = DOTween.Sequence();

                if (i <= (handDeck.Count / 2 - 1))
                {
                    for (int j = i; j < (leftCard); j++)
                    {
                        if (leftCard < 2.5f)
                        {
                            posY += (posYscaleUnder6 * Count);
                        }
                        else
                        {
                            posY += (posYscale * Count);
                        }
                        Count++;
                    }

                    card.savedRotation = new Vector3(0, 0, (leftCard - (i)) * 5);
                    OddLeftHandSequence.Append(handDeck[i].transform.DORotate(card.savedRotation, RotSpeed).SetEase(Ease.OutQuad));

                    if (leftCard < 2.5f)
                    {
                        card.savedPosition = new Vector2((-(1.6f * (leftCard - (i)))), -4.4f - (posY + 0.1f));
                        OddLeftHandSequence.Join(handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                    }
                    else
                    {
                        card.savedPosition = new Vector2((-(1.6f * (leftCard - (i)))) * 0.8f, -4.1f - (posY + 0.1f) - 0.3f);
                        finalScale = card.smallScale;
                        OddLeftHandSequence.Join(handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                    }
                    posY = 0;
                    Count = 0;
                }

                else if (i == handDeck.Count / 2)
                {
                    card.savedRotation = Vector3.zero;
                    card.savedPosition = new Vector2(0, -4.3f);
                    OddCenterHandSequence.Append(
                    handDeck[i].transform.DORotate(card.savedRotation, RotSpeed).SetEase(Ease.OutQuad))
                        .Join(handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));

                    if (leftCard < 2.5f && rightCard < 2.5f)
                    {

                    }
                    else
                    {
                        card.savedPosition = new Vector2(0, -4.1f - 0.3f);
                        OddCenterHandSequence.Join(
                        handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                    }
                }

                else if (i > handDeck.Count / 2)
                {
                    for (int j = i; j > (rightCard); j--)
                    {
                        if (rightCard < 2.5f)
                        {
                            posY += (posYscaleUnder6 * Count);
                        }
                        else
                        {
                            posY += (posYscale * Count);
                        }
                        Count++;
                    }

                    card.savedRotation = new Vector3(0, 0, (i - (handDeck.Count / 2)) * -5);
                    OddRightHandSequence.Append(handDeck[i].transform.DORotate(card.savedRotation, RotSpeed).SetEase(Ease.OutQuad));

                    if (rightCard < 2.5f)
                    {
                        card.savedPosition = new Vector2((1.6f * ((i - (handDeck.Count / 2)))), -4.4f - (posY + 0.1f));
                        OddRightHandSequence.Join(
                        handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                    }
                    else
                    {
                        card.savedPosition = new Vector2((1.6f * ((i - (handDeck.Count / 2)))) * 0.8f, -4.1f - (posY + 0.1f) - 0.3f);
                        finalScale = card.smallScale;
                        OddRightHandSequence.Join(
                        handDeck[i].transform.DOMove(card.savedPosition, MoveSpeed).SetEase(Ease.OutQuad));
                    }
                    handSequence.Join(OddLeftHandSequence);
                    handSequence.Join(OddCenterHandSequence);
                    handSequence.Join(OddRightHandSequence);
                    posY = 0;
                    Count = 0;
                }
            }

            if (card.isScaling == false && card.isClicked == false)
            {
                card.ResetScale();
            }

            if (fastMode && card == targetCard)
            {
                break;
            }
        }

        handSequence.OnComplete(() =>
        {
            isArranging = false;
            SavePos();

            for (int i = 0; i < handDeck.Count; i++)
            {
                Card card = handDeck[i].GetComponent<Card>();
                if (!card.isClicked) { card.ResetLayerOrder(); }
            }

            if (BattleManager.Instance.isPlayerTurn)
            {
                BattleManager.Instance.ChangeTurnButtonColor(true);
            }
        });

        AnimReset();
    }
}
