using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public GameManager gameManager;

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();//����gamemanager�е�resetGame����
    }
}