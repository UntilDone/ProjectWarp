using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private int index = -1;

    private BattleManager battle;
    private TargetManager player;

    public TargetManager target;
    public monster_data data;

    private void Awake()
    {
        target = transform.parent.parent.parent.GetChild(1).GetComponent<TargetManager>();
    }

    private void OnEnable()
    {
        battle = BattleManager.Instance;
        player = battle.player;

        if (index == -1)
        {
            index = battle.monsterIndex;
            battle.monsterIndex++;
        }

        //Debug.Log($"<color=white>[index:{index}/{battle.monsterList.Count - 1}] Explorer's Turn</color>");

        if (target.actions.Count == 0)
        {
            MakeActions();
        }

        StartCoroutine(Action());
    }

    public IEnumerator Action()
    {
        AudioManager audio = AudioManager.Instance;

        yield return new WaitForSeconds(1);

        switch (target.actions[0])
        {
            case 1000: // normal attack
#if DEBUG_MODE
                Debug.Log("Monster Attacks!");
#endif
                target.GiveMovingEffect("left", true);
                player.GiveMovingEffect("left", false);
                player.GiveDamage(data.damage);
                player.GiveEffect(500);
                audio.PlayAudioById(500, 0.5f);
                break;

            case 1001: // normal defend
#if DEBUG_MODE
                Debug.Log("Monster Defends!");
#endif
                target.GiveMovingEffect("up", false);
                target.GiveShield((int)(data.damage / 2));
                break;

            case 1103:
                Debug.Log("Monster Stunned!");
                target.GiveMovingEffect("right", true);
                break;

            case >= 1100:
#if DEBUG_MODE
                Debug.Log($"Monster Giving Status {target.actions[0]}");
#endif
                target.GiveMovingEffect("left", false);
                player.AddStatus(target.actions[0], 1);
                break;
        }

        target.ToggleActionIcon(false);
        target.actions.Remove(target.actions[0]);

        yield return new WaitForSeconds(1);

        EndTurn();
    }

    public void MakeActions()
    {
#if DEBUG_MODE
        Debug.Log("Monster Making Actions...");
#endif

        int attackCount = Random.Range(1, 4); // 1 ~ 3
        int defendCount = Random.Range(1, 3); // 1 ~ 2
        int statusCount = 0;

        if (data.action != 0) statusCount = Random.Range(0, 3); // 0 ~ 2

        int totalCount = attackCount + defendCount + statusCount;
#if DEBUG_MODE
        Debug.Log($"Total Action Count: {totalCount}");
#endif
        for (int i = 0; i < totalCount; i++)
        {
            if (attackCount > 0)
            {
                target.actions.Add(1000);
                attackCount--;
                continue;
            }
            else if (defendCount > 0)
            {
                target.actions.Add(1001);
                defendCount--;
                continue;
            }
            else if (statusCount > 0)
            {
                target.actions.Add(data.action);
                statusCount--;
                continue;
            }
        }

        int count = target.actions.Count;

        for (int i = 0; i < count; i++)
        {
            int random = Random.Range(i, count);
            int temp = target.actions[i];
            target.actions[i] = target.actions[random];
            target.actions[random] = temp;
        }
    }

    public void DelayActions()
    {
        target.actions.Insert(0, 1103);
        Debug.Log($"actions count: {target.actions.Count}");
        target.RefreshStatus();
        target.ToggleActionIcon(true);
    }

    private void EndTurn()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (battle.monsterList.Count > index + 1)
        {
            battle.StartMonsterTurn(index + 1);
        }
        else
        {
            battle.monsterIndex = 0;
            battle.StartPlayerTurn();
        }
    }
}
