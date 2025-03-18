using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("MainMenu");
        Application.runInBackground = true;
        Application.targetFrameRate = 120;
        DontDestroyOnLoad(gameObject);
        
    }
    public void Update()
    {
        CheckDead();
    }
    public void CheckDead()
    {
        gamesave_data.Instance.CheckPlayerHp();
        if (gamesave_data.Instance.isDead)
        {
            SceneManager.LoadScene("Dead");
        }
    }

}
