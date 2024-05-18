using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    public static GameManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    //公共方法用来执行GUI射线检测
    public bool CheckGuiRaycastObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())//检测当前鼠标指针是否悬停在GUI对象上
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void ResetGame()
    {
        //删除所有储存的键值对数据
        PlayerPrefs.DeleteAll();
    }
}
