using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public PanelController pausePanelController; // 关联 PausePanel 的控制器
    public GameManager gameManager;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 切换暂停面板的显示状态
            pausePanelController.TogglePanel();
        }
    }

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();//调用gamemanager中的resetGame方法
    }
}



