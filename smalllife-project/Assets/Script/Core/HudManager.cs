using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public GameManager gameManager;

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();//调用gamemanager中的resetGame方法
    }
}