using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    public static GameManager instance;
    private GameData gameData; // 添加这个字段来存储当前的游戏数据

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

        // 强制锁定帧率
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;// 可选：关闭垂直同步以避免影响帧率锁定

        // 在游戏启动时加载保存的数据
        LoadGameData();
    }
    
    private void LoadGameData()
    {
        SaveSystem.LoadGame(); // 仅调用加载方法
        var data = SaveSystem.GameData; // 获取数据

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
        //删除所有储存的键值对数据
        PlayerPrefs.DeleteAll();
        Debug.Log("删除所有数据");
    }

    public void OnClearDataButtonClicked()
    {
        ClearSavedData();
    }
    
    public void ClearSavedData()
    {
        // 使用你自己的保存系统清除数据
        SaveSystem.ClearData();
        //重置游戏数据，以确保内存中的数据也被清除
        gameData = new GameData();
        //调试输出清除后的数据状态
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
