using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization.Configuration;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour, IPointerExitHandler
{
    public monster_data monsterData = new monster_data();
    public player_data playerData = new player_data();
    public Monster monster;

    public float currentHp;
    public float maxHp;
    public float hpMultiplier = 1;
    public float shield;
    public Dictionary<int, int> status;
    public Dictionary<int, status_data> statusDic;

    public bool isTargetPlayer;
    public bool isTargetDead;

    public Sequence effectSequence;

    public List<int> actions;
    public Vector3 startPosition = Vector3.zero;

    public GameObject actionCanvas;

    private int movingRepeatTime;
    private Vector3 fixedPosition;

    private void Start()
    {
        if (transform.parent.parent.name == "PlayerPosition")
        {
            isTargetPlayer = true;
            playerData = gamesave_data.Instance.playerData[1];
            RefreshHealthPoint();
        }

        status = new Dictionary<int, int>();
        statusDic = statusInfo.Instance.statusDic;
        actions = new List<int>();

        if (!isTargetPlayer)
        {
            actionCanvas = transform.parent.GetChild(0).GetChild(4).gameObject;
            actionCanvas.transform.GetChild(0).position = monster.transform.parent.GetChild(1).position;

            monster.target = this;
            monster.MakeActions();
            ToggleActionIcon(true);
        }

        int childCount = transform.parent.GetChild(2).GetChild(0).childCount;
        fixedPosition = transform.parent.GetChild(2).GetChild(0).GetChild(childCount - 1).transform.position;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardTweenManager.Instance.targetingCard != null)
        {
            TargetOff();
        }
    }

    public void TargetOn()
    {
        CardTweenManager card = CardTweenManager.Instance;

        Card draggingCard
            = card.draggingCard == null ? null : card.draggingCard;

        Card targetingCard
            = card.targetingCard == null ? null : card.targetingCard;

        if (targetingCard == null)
            return;

        if (!isTargetPlayer)
        {
            // target arrow
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).position = transform.parent.GetChild(2).GetChild(0).GetChild(1).position;
            BattleManager.Instance.targetEnemy = gameObject;

            ToggleActionIcon(false);
        }
    }

    public void TargetOff()
    {
        if (isTargetPlayer) return;

        transform.GetChild(0).gameObject.SetActive(false);
        BattleManager.Instance.targetEnemy = null;
        ToggleActionIcon(true);
    }

    public void GiveDamage(float stat)
    {
        float multiplier = 1;

        foreach (KeyValuePair<int, int> pair in status)
        {
            foreach (KeyValuePair<int, status_data> dicPair in statusDic)
            {
                if (dicPair.Value.id == pair.Key)
                {
                    if (dicPair.Value.effect_type == "damageMultiplier")
                    {
                        multiplier += dicPair.Value.multiplier;
                    }
                    break;
                }
            }
        }

        int damage = (int)(stat * multiplier);
        int finalDamage = damage;

        if (shield > 0)
        {
            finalDamage -= (int)shield;
            shield -= damage;

            if (shield <= 0)
            {
                shield *= -1;
                finalDamage = (int)shield;
                RemoveShield();
            }

            if (finalDamage <= 0)
            {
                finalDamage = 0;
            }
        }

        currentHp -= finalDamage;

        RefreshHealthPoint();
    }

    public void GiveEffect(int id)
    {
        GameObject effect = Resources.Load<GameObject>($"Prefabs/Effect/{id}");

        if (effect != null)
        {
            Vector2 position = Vector2.zero;

            if (isTargetPlayer)
            {
                position = transform.position;
            }
            else
            {
                position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            Instantiate(effect, position, Quaternion.Euler(Vector3.zero), transform);
        }
    }

    public int GetStatusIdFromCard(card_data cardData)
    {
        string name = cardData.status_name;
        int id;

        string digits = new string(name.Where(char.IsDigit).ToArray());
        if (!int.TryParse(digits, out id))
        {
            id = 0;
        }

        return id;
    }

    public void AddStatus(int id, int duration)
    {
        bool isAlreadyAdded = false;

        foreach (KeyValuePair<int, int> pair in status)
        {
            if (pair.Key == id)
            {
                isAlreadyAdded = true;
                break;
            }
        }

        if (isAlreadyAdded)
        {
            status[id] += duration;
        }
        else
        {
            status.Add(id, duration);

        }

        if (id == 1103 && isTargetPlayer == false)
        {
            monster.DelayActions();
        }

        RefreshStatus();
    }

    public void RefreshStatus()
    {
        GameObject statusPlate = transform.parent.GetChild(0).GetChild(3).GetChild(0).gameObject;

        if (status.Count > 0) // status icons
        {
            statusPlate.transform.parent.gameObject.SetActive(true);
            statusPlate.SetActive(true);

            int childCount = statusPlate.transform.childCount;

            List<int> ids = new List<int>();

            foreach (KeyValuePair<int, int> pair in status)
            {
                ids.Add(pair.Key);
            }

            for (int i = 0; i < status.Count; i++)
            {
                statusPlate.transform.GetChild(i).gameObject.SetActive(false);

                GameObject icon = statusPlate.transform.GetChild(i).gameObject;
                Image image = icon.GetComponent<Image>();
                TMP_Text count = icon.transform.GetChild(0).GetComponent<TMP_Text>();

                Color imageColor = image.color;
                imageColor.a = 1;

                Color countColor = count.color;
                countColor.a = 1;

                image.color = imageColor;
                count.color = countColor;

                if (status[ids[i]] == 0)
                {
                    image.DOFade(0, 1).SetEase(Ease.OutQuad);
                    count.DOFade(0, 1).SetEase(Ease.OutQuad);
                }

                icon.SetActive(true);
                image.sprite = Resources.Load<Sprite>($"Battle/Icons/{ids[i]}");

                count.text = $"{status[ids[i]]}";
            }

            for (int i = 0; i < status.Count; i++)
            {
                if (status[ids[i]] == 0)
                {
                    status.Remove(ids[i]);
                }
            }

        }
        else if (status.Count == 0)
        {
            statusPlate.transform.parent.gameObject.SetActive(false);
            statusPlate.SetActive(false);
        }
    }

    public void DecreaseStatusCount()
    {
        List<int> ids = new List<int>();

        foreach (KeyValuePair<int, int> pair in status)
        {
            ids.Add(pair.Key);
        }

        for (int i = 0; i < ids.Count; i++)
        {
            status[ids[i]]--;
        }
    }

    public void RefreshHealthPoint()
    {
        Slider slider;
        TMP_Text hpCount;

        if (shield <= 0)
        {
            slider = transform.parent.GetChild(0).GetChild(0).GetChild(0).GetComponent<Slider>();
            hpCount = slider.transform.GetChild(3).GetComponent<TMP_Text>();
        }
        else
        {
            slider = transform.parent.GetChild(0).GetChild(1).GetChild(0).GetComponent<Slider>();
            hpCount = slider.transform.GetChild(3).GetComponent<TMP_Text>();

            TMP_Text shieldCount = transform.parent.GetChild(0).GetChild(2).GetChild(1).GetComponent<TMP_Text>();
            shieldCount.text = $"{shield}";
        }

        if (maxHp == 0)
        {
            if (isTargetPlayer)
            {
                maxHp = gamesave_data.Instance.playerData[1].max_hp;
                currentHp = gamesave_data.Instance.playerData[1].current_hp;
            }
            else
            {
                maxHp = monsterData.max_hp;
                currentHp = maxHp;
            }
        }
        slider.value = currentHp / maxHp;
        hpCount.text = $"{currentHp}/{maxHp}";

        if (isTargetPlayer)
        {
            gamesave_data.Instance.playerData[1].current_hp = (int)currentHp;
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        BattleManager battle = BattleManager.Instance;
        battle.monsterPool.Remove(monsterData);
        battle.targetEnemy = null;

        if (battle.monsterPool.Count == 0)
        {
            CardChooseManager card = CardChooseManager.Instance;
            gamesave_data.Instance.MonsterPool.Clear();
            card.PickRandomCardsWithRarity(3);
            card.DisplayCards();
        }

        GameObject monsterObject = transform.parent.GetChild(2).GetChild(0).gameObject;
        battle.monsterList.Remove(monsterObject);
        Destroy(monsterObject);

        transform.parent.gameObject.SetActive(false);
        isTargetDead = true;
    }

    public void GiveMovingEffect(string direction, bool isCameraShaking)
    {
        if (isCameraShaking)
        {
            BattleManager battle = BattleManager.Instance;
            battle.shakeCoroutine = battle.StartCoroutine(battle.ShakeCoroutine(0.1f, 0.5f));
        }

        if (startPosition == Vector3.zero)
        {
            startPosition = transform.parent.GetChild(2).position;
        }

        Vector3 endPosition = startPosition;
        float scale = 0.5f;
        movingRepeatTime = 1;
        bool isShadowFixed = false;

        switch (direction)
        {
            case "left":
                scale *= -1;
                endPosition.x += scale;
                break;

            case "right":
                endPosition.x += scale;
                break;

            case "up":
                scale *= 0.5f;
                movingRepeatTime = 2;
                endPosition.y += scale;
                isShadowFixed = true;
                break;
        }

        AddTween(startPosition, endPosition, isShadowFixed);
    }

    private void AddTween(Vector3 startPosition, Vector3 endPosition, bool isShadowFixed)
    {
        int childCount = transform.parent.GetChild(2).GetChild(0).childCount;
        effectSequence = DOTween.Sequence();
        effectSequence
              .Prepend(transform.parent.GetChild(2).DOMove(endPosition, 0.15f).SetEase(Ease.OutQuad))
              .Append(transform.parent.GetChild(2).DOMove(startPosition, 0.10f))
              .OnUpdate(() =>
              {
                  if (isShadowFixed)
                  {
                      transform.parent.GetChild(2).GetChild(0).GetChild(childCount - 1).transform.position = fixedPosition;
                  }
              })
              .OnComplete(() =>
              {
                  movingRepeatTime--;
                  if (movingRepeatTime > 0)
                  {
                      AddTween(startPosition, endPosition, isShadowFixed);
                  }
              });
    }

    public void GiveShield(int stat)
    {
        transform.parent.GetChild(0).GetChild(0).gameObject.SetActive(false); // hp bar
        transform.parent.GetChild(0).GetChild(1).gameObject.SetActive(true); // shield bar
        transform.parent.GetChild(0).GetChild(2).gameObject.SetActive(true); // shield icon

        Slider slider = transform.parent.GetChild(0).GetChild(1).GetChild(0).GetComponent<Slider>(); // shield bar
        TMP_Text hpCount = slider.transform.GetChild(3).GetComponent<TMP_Text>();

        slider.value = currentHp / maxHp;
        hpCount.text = $"{currentHp}/{maxHp}";

        float multiplier = 1;

        foreach (KeyValuePair<int, int> pair in status)
        {
            foreach (KeyValuePair<int, status_data> dicPair in statusDic)
            {
                if (dicPair.Value.id == pair.Key)
                {
                    if (dicPair.Value.effect_type == "shieldReduction")
                    {
                        multiplier -= dicPair.Value.multiplier;
                    }
                    break;
                }
            }
        }

        int scale = (int)(stat * multiplier);

        shield += scale;

        TMP_Text shieldCount = transform.parent.GetChild(0).GetChild(2).GetChild(1).GetComponent<TMP_Text>();
        shieldCount.text = $"{shield}";
    }

    public void RemoveShield()
    {
        transform.parent.GetChild(0).GetChild(0).gameObject.SetActive(true); // hp bar
        transform.parent.GetChild(0).GetChild(1).gameObject.SetActive(false); // shield bar
        transform.parent.GetChild(0).GetChild(2).gameObject.SetActive(false); // shield icon

        shield = 0;
    }

    public void SavePlayerData()
    {
        playerData.current_hp = (int)currentHp;
        playerData.max_hp = (int)maxHp;
        gamesave_data.Instance.playerData[1] = playerData;
    }

    public void ToggleActionIcon(bool turnOn)
    {
        if (isTargetPlayer) return;
        actionCanvas.SetActive(turnOn);

        if (actions.Count > 0)
        {
            Image image = actionCanvas.transform.GetChild(0).GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>($"Battle/Icons/{actions[0]}");

            TMP_Text count = image.GetComponentInChildren<TMP_Text>();

            if (actions[0] >= 1100)
            {
                count.text = $"";
            }
            else
            {
                count.text = actions[0] == 1001 ? $"{(int)(monsterData.damage / 2)}" : $"{monsterData.damage}";
            }
        }
    }
}
