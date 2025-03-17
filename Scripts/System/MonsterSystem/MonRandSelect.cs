
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;

public class MonRandSelect
{
    public List<monster_data> Chapter1;
    public List<monster_data> Chapter2;
    public List<monster_data> Chapter3;

    public List<monster_data> Chapter1_Elite;
    public List<monster_data> Chapter2_Elite;
    public List<monster_data> Chapter3_Elite;


    public List<monster_data> Chapter1_Boss;
    public List<monster_data> Chapter2_Boss;
    public List<monster_data> Chapter3_Boss;

    public List<monster_data> MonsterPool;

    private static MonRandSelect instance;

    private int Chapter1_totalHp = 48;
    private int Chapter2_totalHp = 96;
    private int Chapter3_totalHp = 144;

    private int monsterPool_totalHp;
    private int monsterPool_minHp;

    private MonRandSelect()
    {
        Init();
    }
    public static MonRandSelect Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MonRandSelect();
            }
            return instance;
        }
    }

    public void Init()
    {
#if DEBUG_MODE
        Debug.Log($"초기화");
#endif
        if (gamesave_data.Instance.MonsterPool.Count == 0)
        {
            gamesave_data.Instance.isMonsterPoolReady=false;
        }
        Chapter1 = new List<monster_data>();
        Chapter2 = new List<monster_data>();
        Chapter3 = new List<monster_data>();

        Chapter1_Elite = new List<monster_data>();
        Chapter2_Elite = new List<monster_data>();
        Chapter3_Elite = new List<monster_data>();

        Chapter1_Boss = new List<monster_data>();
        Chapter2_Boss = new List<monster_data>();
        Chapter3_Boss = new List<monster_data>();

        MonsterPool = new List<monster_data>();
        if (!gamesave_data.Instance.isMonsterPoolReady && gamesave_data.Instance.isMonsterPackDownloaded == true)
        {
#if DEBUG_MODE
            Debug.Log($"몬스터 로딩");
#endif
            LoadMonsterPool();
        }
        else if (!gamesave_data.Instance.isMonsterPoolReady&&gamesave_data.Instance.isMonsterPackDownloaded==false)
        {
#if DEBUG_MODE
            Debug.Log($"몬스터 리스팅2");
#endif
            ListingMonsters();
        }
        else
        {
#if DEBUG_MODE
            Debug.Log($"몬스터 로드");
#endif
            LoadMonsterPool();
        }
    }

    void ListingMonsters()
    {
        Dictionary<int, monster_data> monsterDic = MonsterInfo.Instance.monsterDic;

        foreach (var monster in monsterDic.Values)
        {
            if (monster.chapter_1 > 0 && monster.type == "Normal")
            {
                for (int i = 0; i < monster.chapter_1; i++)
                {
                    Chapter1.Add(monster);
                }

            }
            if (monster.chapter_2 > 0 && monster.type == "Normal")
            {
                for (int i = 0; i < monster.chapter_2; i++)
                {
                    Chapter2.Add(monster);
                }
            }

            if (monster.chapter_3 > 0 && monster.type == "Normal")
            {
                for (int i = 0; i < monster.chapter_3; i++)
                {
                    Chapter3.Add(monster);
                }
            }

            if (monster.chapter_1 > 0 && monster.type == "Elite")
            {
                for (int i = 0; i < monster.chapter_1; i++)
                {
                    Chapter1_Elite.Add(monster);
                }

            }
            if (monster.chapter_2 > 0 && monster.type == "Elite")
            {
                for (int i = 0; i < monster.chapter_2; i++)
                {
                    Chapter2_Elite.Add(monster);
                }

            }
            if (monster.chapter_3 > 0 && monster.type == "Elite")
            {
                for (int i = 0; i < monster.chapter_3; i++)
                {
                    Chapter3_Elite.Add(monster);
                }

            }
            if (monster.chapter_1 > 0 && monster.type == "Boss")
            {
                for (int i = 0; i < monster.chapter_1; i++)
                {
                    Chapter1_Boss.Add(monster);
                }

            }
            if (monster.chapter_2 > 0 && monster.type == "Boss")
            {
                for (int i = 0; i < monster.chapter_2; i++)
                {
                    Chapter2_Boss.Add(monster);
                }

            }
            if (monster.chapter_3 > 0 && monster.type == "Boss")
            {
                for (int i = 0; i < monster.chapter_3; i++)
                {
                    Chapter3_Boss.Add(monster);
                }

            }
        }
    }
    public void LoadMonsterPool()
    {
        this.Chapter1 = gamesave_data.Instance.Chapter1;
        this.Chapter2 = gamesave_data.Instance.Chapter2;
        this.Chapter3 = gamesave_data.Instance.Chapter3;

        this.Chapter1_Elite = gamesave_data.Instance.Chapter1_Elite;
        this.Chapter2_Elite = gamesave_data.Instance.Chapter2_Elite;
        this.Chapter3_Elite = gamesave_data.Instance.Chapter3_Elite;

        this.Chapter1_Boss = gamesave_data.Instance.Chapter1_Boss;
        this.Chapter2_Boss = gamesave_data.Instance.Chapter2_Boss;
        this.Chapter3_Boss = gamesave_data.Instance.Chapter3_Boss;
    }
    public void PickRandomMonster()
    {
        MonsterPool.Clear();
        System.Random rng = new System.Random();
        Chapter1 = Chapter1.OrderBy(monster => rng.Next()).ToList();
        Chapter2 = Chapter2.OrderBy(monster => rng.Next()).ToList();
        Chapter3 = Chapter3.OrderBy(monster => rng.Next()).ToList();

        if (gamesave_data.Instance.current_Chapter == 1)
        {
            if (gamesave_data.Instance.MonsterPool.Count==0)
            {
#if DEBUG_MODE
                Debug.Log("피킹업");
#endif
                monsterPool_totalHp = 0;
                if (Chapter1.Count > 0)
                {
                    while (true)
                    {
                        monsterPool_minHp = Chapter1.Min(monster => monster.max_hp);
                        monsterPool_totalHp = 0;
                        int monsterRandPos = Random.Range(0, Chapter1.Count);
#if DEBUG_MODE
                        Debug.Log(monsterRandPos);
#endif
                        foreach (var monster in MonsterPool)
                        {
                            if (monster == null)
                            {
                                break;
                            }
                            else
                            {
                                monsterPool_totalHp += monster.max_hp;
                            }
                        }
                        if ((monsterPool_totalHp + Chapter1[monsterRandPos].max_hp) <= Chapter1_totalHp)
                        {
#if DEBUG_MODE
                            Debug.Log(Chapter1[monsterRandPos].name);
#endif
                            MonsterPool.Add(Chapter1[monsterRandPos]);
                            Chapter1.RemoveAt(monsterRandPos);
                        }
                        if ((Chapter1_totalHp - monsterPool_totalHp) < monsterPool_minHp || (monsterPool_totalHp + monsterPool_minHp) > Chapter1_totalHp)
                        {
                            break;
                        }


                    }
                    foreach (var monster in MonsterPool)
                    {
                        gamesave_data.Instance.MonsterPool.Add(monster);
                    }
#if DEBUG_MODE
                    Debug.Log(gamesave_data.Instance.MonsterPool.Count);
                    Debug.Log("<color=blue>랜덤피킹 시퀀스가 종료되었습니다.</color>");
#endif
                }

            }
            else
            {
#if DEBUG_MODE
                Debug.Log("이미 몬스터가 존재합니다.");
#endif
            }
#if DEBUG_MODE
            foreach(var monster in gamesave_data.Instance.MonsterPool)
            {
                Debug.Log(monster.name);
            }
#endif

        }
        else if(gamesave_data.Instance.current_Chapter ==2)
        {
            if (gamesave_data.Instance.MonsterPool.Count == 0)
            {
                monsterPool_totalHp = 0;
                if (Chapter2.Count > 0)
                {
                    while (true)
                    {
                        monsterPool_minHp = Chapter2.Min(monster => monster.max_hp);
                        monsterPool_totalHp = 0;
                        int monsterRandPos = Random.Range(0, Chapter2.Count);
#if DEBUG_MODE
                        Debug.Log(monsterRandPos);
#endif
                        foreach (var monster in MonsterPool)
                        {
                            if (monster == null)
                            {
                                break;
                            }
                            else
                            {
                                monsterPool_totalHp += monster.max_hp;
                            }
                        }
                        if ((monsterPool_totalHp + Chapter1[monsterRandPos].max_hp) <= Chapter2_totalHp)
                        {
#if DEBUG_MODE
                            Debug.Log(Chapter2[monsterRandPos].name);
#endif
                            MonsterPool.Add(Chapter2[monsterRandPos]);
                            Chapter1.RemoveAt(monsterRandPos);
                        }
                        if ((Chapter2_totalHp - monsterPool_totalHp) < monsterPool_minHp || (monsterPool_totalHp + monsterPool_minHp) > Chapter2_totalHp)
                        {
                            break;
                        }


                    }
                    foreach (var monster in MonsterPool)
                    {
                        gamesave_data.Instance.MonsterPool.Add(monster);
                    }
#if DEBUG_MODE
                    Debug.Log(gamesave_data.Instance.MonsterPool.Count);
                    Debug.Log("<color=blue>랜덤피킹 시퀀스가 종료되었습니다.</color>");
#endif
                }

            }
            else
            {
#if DEBUG_MODE
                Debug.Log("이미 몬스터가 존재합니다.");
#endif
            }
#if DEBUG_MODE
            foreach (var monster in gamesave_data.Instance.MonsterPool)
            {
                Debug.Log(monster.name);
            }
#endif
        }
        else if (gamesave_data.Instance.current_Chapter == 3)
        {
            if (gamesave_data.Instance.MonsterPool.Count == 0)
            {
                monsterPool_totalHp = 0;
                if (Chapter3.Count > 0)
                {
                    while (true)
                    {
                        monsterPool_minHp = Chapter3.Min(monster => monster.max_hp);
                        monsterPool_totalHp = 0;
                        int monsterRandPos = Random.Range(0, Chapter3.Count);
#if DEBUG_MODE
                        Debug.Log(monsterRandPos);
#endif
                        foreach (var monster in MonsterPool)
                        {
                            if (monster == null)
                            {
                                break;
                            }
                            else
                            {
                                monsterPool_totalHp += monster.max_hp;
                            }
                        }
                        if ((monsterPool_totalHp + Chapter3[monsterRandPos].max_hp) <= Chapter3_totalHp)
                        {
#if DEBUG_MODE
                            Debug.Log(Chapter3[monsterRandPos].name);
#endif
                            MonsterPool.Add(Chapter3[monsterRandPos]);
                            Chapter3.RemoveAt(monsterRandPos);
                        }
                        if ((Chapter3_totalHp - monsterPool_totalHp) < monsterPool_minHp || (monsterPool_totalHp + monsterPool_minHp) > Chapter3_totalHp)
                        {
                            break;
                        }


                    }
                    foreach (var monster in MonsterPool)
                    {
                        gamesave_data.Instance.MonsterPool.Add(monster);
                    }
#if DEBUG_MODE
                    Debug.Log(gamesave_data.Instance.MonsterPool.Count);
                    Debug.Log("<color=blue>랜덤피킹 시퀀스가 종료되었습니다.</color>");
#endif
                }

            }
            else
            {
#if DEBUG_MODE
                Debug.Log("이미 몬스터가 존재합니다.");
#endif
            }
#if DEBUG_MODE
            foreach (var monster in gamesave_data.Instance.MonsterPool)
            {
                Debug.Log(monster.name);
            }
#endif
        }
        gamesave_data.Instance.isMonsterPoolReady = true;

    }
    public void PickRandomElite()
    {
        MonsterPool.Clear();
        System.Random rng = new System.Random();
        Chapter1_Elite = Chapter1_Elite.OrderBy(monster => rng.Next()).ToList();
        Chapter2_Elite = Chapter2_Elite.OrderBy(monster => rng.Next()).ToList();
        Chapter3_Elite = Chapter3_Elite.OrderBy(monster => rng.Next()).ToList();
        
        if (gamesave_data.Instance.current_Chapter == 1)
        {
            if (Chapter1_Elite.Count != 0)
            {
                MonsterPool.Add(Chapter1_Elite[0]);
                Chapter1_Elite.RemoveAt(0);
            }
        }
        else if (gamesave_data.Instance.current_Chapter == 2)
        {
            if (Chapter2_Elite.Count != 0)
            {
                MonsterPool.Add(Chapter2_Elite[0]);
                Chapter2_Elite.RemoveAt(0);
            }
        }
        else if (gamesave_data.Instance.current_Chapter == 3)
        {
            if (Chapter3_Elite.Count != 0)
            {
                MonsterPool.Add(Chapter3_Elite[0]);
                Chapter3_Elite.RemoveAt(0);
            }
        }
        gamesave_data.Instance.isMonsterPoolReady = true;
    }

    public void PickRandomBoss()
    {
        MonsterPool.Clear();
        System.Random rng = new System.Random();
        Chapter1_Boss = Chapter1_Boss.OrderBy(monster => rng.Next()).ToList();
        Chapter2_Boss = Chapter2_Boss.OrderBy(monster => rng.Next()).ToList();
        Chapter3_Boss = Chapter3_Boss.OrderBy(monster => rng.Next()).ToList();

        if (gamesave_data.Instance.current_Chapter == 1)
        {
            if (Chapter1_Boss.Count != 0)
            {
                MonsterPool.Add(Chapter1_Boss[0]);
                Chapter1_Boss.RemoveAt(0);
            }
        }
        else if (gamesave_data.Instance.current_Chapter == 2)
        {
            if (Chapter2_Boss.Count != 0)
            {
                MonsterPool.Add(Chapter2_Boss[0]);
                Chapter2_Boss.RemoveAt(0);
            }
        }
        else if (gamesave_data.Instance.current_Chapter == 3)
        {
            if (Chapter3_Boss.Count != 0)
            {
                MonsterPool.Add(Chapter3_Boss[0]);
                Chapter3_Boss.RemoveAt(0);
            }
        }
        gamesave_data.Instance.isMonsterPoolReady = true;
    }

    public void SendCurrentMonsterList()
    {
        gamesave_data.Instance.Chapter1.Clear();
        gamesave_data.Instance.Chapter2.Clear();
        gamesave_data.Instance.Chapter3.Clear();

        gamesave_data.Instance.Chapter1_Elite.Clear();
        gamesave_data.Instance.Chapter2_Elite.Clear();
        gamesave_data.Instance.Chapter3_Elite.Clear();

        gamesave_data.Instance.Chapter1_Boss.Clear();
        gamesave_data.Instance.Chapter2_Boss.Clear();
        gamesave_data.Instance.Chapter3_Boss.Clear();


        // NORMAL //
        foreach (var monster in Chapter1)
        {
            gamesave_data.Instance.Chapter1.Add(monster);
        }
        foreach (var monster in Chapter2)
        {
            gamesave_data.Instance.Chapter2.Add(monster);
        }
        foreach (var monster in Chapter3)
        {
            gamesave_data.Instance.Chapter3.Add(monster);
        }

        // ELITE //
        foreach (var monster in Chapter1_Elite)
        {
            gamesave_data.Instance.Chapter1_Elite.Add(monster);
        }
        foreach (var monster in Chapter2_Elite)
        {
            gamesave_data.Instance.Chapter2_Elite.Add(monster);
        }
        foreach (var monster in Chapter3_Elite)
        {
            gamesave_data.Instance.Chapter3_Elite.Add(monster);
        }

        // BOSS //
        foreach(var monster in Chapter1_Boss)
        {
            gamesave_data.Instance.Chapter1_Boss.Add(monster);
        }
        foreach (var monster in Chapter2_Boss)
        {
            gamesave_data.Instance.Chapter3_Boss.Add(monster);
        }
        foreach (var monster in Chapter3_Boss)
        {
            gamesave_data.Instance.Chapter3_Boss.Add(monster);
        }
        gamesave_data.Instance.isMonsterPackDownloaded = true;
    }
}

