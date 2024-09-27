using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public PanelController pausePanelController; // ���� PausePanel �Ŀ�����
    public GameManager gameManager;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // �л���ͣ������ʾ״̬
            pausePanelController.TogglePanel();
        }
    }

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();//����gamemanager�е�resetGame����
    }
}



