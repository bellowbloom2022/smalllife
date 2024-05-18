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


    //������������ִ��GUI���߼��
    public bool CheckGuiRaycastObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())//��⵱ǰ���ָ���Ƿ���ͣ��GUI������
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
        //ɾ�����д���ļ�ֵ������
        PlayerPrefs.DeleteAll();
    }
}
