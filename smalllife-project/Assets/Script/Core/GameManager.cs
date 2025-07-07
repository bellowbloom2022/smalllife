using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    public static GameManager instance;
    private GameData gameData; // �������ֶ����洢��ǰ����Ϸ����

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
        // ����Ϸ����ʱ���ر��������
        LoadGameData();
    }
    private void LoadGameData()
    {
        // ���Լ������е���Ϸ���ݣ����û�����ʼ���µ�����
        gameData = SaveSystem.LoadGame() ?? new GameData();
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
        InputRouter.Instance?.LockInput();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        InputRouter.Instance?.UnlockInput();
    }
    public void ResetGame()
    {
        //ɾ�����д���ļ�ֵ������
        PlayerPrefs.DeleteAll();
        Debug.Log("ɾ����������");
    }

    public void OnClearDataButtonClicked()
    {
        ClearSavedData();
    }
    
    public void ClearSavedData()
    {
        // ʹ�����Լ��ı���ϵͳ�������
        SaveSystem.ClearData();
        //������Ϸ���ݣ���ȷ���ڴ��е�����Ҳ�����
        gameData = new GameData();
        //�����������������״̬
        PrintGameData();

        Debug.Log("All saved data cleared.");
    }

    public void PrintGameData()
    {
        Debug.Log("Current Level: " + gameData.currentLevel);
        Debug.Log("Goals Found: " + (gameData.goalsFound != null ? string.Join(",", gameData.goalsFound) : "No data"));
    }
}
