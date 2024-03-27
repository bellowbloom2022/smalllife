using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public GameObject panel;
    public GameManager gameManager;

    private bool isPaused = false;

    void Update()
    {
        //当按下ESC键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                gameManager.PauseGame();//暂停游戏
                OpenPanel();//打开panel
            }
            else
            {
                gameManager.ResumeGame();
                ClosePanel();
            }
        }
    }
    public void OpenPanel()
    {
        if (panel != null)
        {
           panel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();//调用gamemanager中的resetGame方法
    }
}



