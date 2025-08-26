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
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = false;
#endif
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ǿ������֡��
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;// ��ѡ���رմ�ֱͬ���Ա���Ӱ��֡������

        // ����Ϸ����ʱ���ر��������
        LoadGameData();
    }
    
    private void LoadGameData()
    {
        SaveSystem.LoadGame(); // �����ü��ط���
        var data = SaveSystem.GameData; // ��ȡ����

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
        Debug.Log("Current Level: " + SaveSystem.GameData.currentLevel);

        var goalMap = SaveSystem.GameData.goalProgressMap;
        if (goalMap == null || goalMap.Count == 0){
            Debug.Log("Goals Found: No data");
        }
        else{
            List<string> goalStates = new();
            foreach (var kvp in goalMap){
                string key = kvp.Key;
                var progress = kvp.Value;
                string status = $"{key}: step1={progress.step1Completed}, step2={progress.step2Completed}";
                goalStates.Add(status);
            }
            Debug.Log("Goals Found: " + string.Join(";", goalStates));
        }
    }
}
