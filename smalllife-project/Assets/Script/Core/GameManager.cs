using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Lean.Localization;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    public static GameManager instance;
    private GameData gameData; // 添加这个字段来存储当前的游戏数据
    [SerializeField] private string defaultLanguage = "Chinese";

    [Header("Frame Pacing")]
    [SerializeField] private int foregroundTargetFrameRate = 60;
    [SerializeField] private int backgroundTargetFrameRate = 15;
    [SerializeField] private int highRefreshThreshold = 90;
    [SerializeField] private int normalRefreshVSyncCount = 1;
    [SerializeField] private int highRefreshVSyncCount = 2;

    private bool appInBackground;

    private void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = false;
#endif
        // 初始化本地化系统
        var localization = FindObjectOfType<LeanLocalization>();

        if (localization != null)
        {
            if (string.IsNullOrEmpty(localization.CurrentLanguage))
            {
                localization.SetCurrentLanguage(defaultLanguage);
                Debug.Log($"[GameManager] 语言未设置，强制设为 {defaultLanguage}");
            }
            else
            {
                Debug.Log($"[GameManager] 当前语言: {localization.CurrentLanguage}");
            }
        }
        else
        {
            Debug.LogError("[GameManager] 没找到 LeanLocalization 组件，请确认场景里挂了这个脚本。");
        }
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

        // 根据刷新率选择 VSync 档位：高刷屏默认使用 vSync=2 以降低发热。
        ApplyForegroundFramePacing();

        // 在游戏启动时加载保存的数据
        LoadGameData();
    }
    
    private void LoadGameData()
    {
        SaveSystem.LoadGame(); // 仅调用加载方法
        var data = SaveSystem.GameData; // 获取数据

    }

    private void OnApplicationFocus(bool hasFocus)
    {
        appInBackground = !hasFocus;
        ApplyCurrentFramePacing();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        appInBackground = pauseStatus;
        ApplyCurrentFramePacing();
    }

    private void ApplyCurrentFramePacing()
    {
        if (appInBackground)
            ApplyBackgroundFramePacing();
        else
            ApplyForegroundFramePacing();
    }

    private void ApplyForegroundFramePacing()
    {
        int refreshRate = Mathf.Max(1, Screen.currentResolution.refreshRate);
        bool isHighRefresh = refreshRate > highRefreshThreshold;
        QualitySettings.vSyncCount = isHighRefresh ? highRefreshVSyncCount : normalRefreshVSyncCount;
        Application.targetFrameRate = Mathf.Max(30, foregroundTargetFrameRate);
    }

    private void ApplyBackgroundFramePacing()
    {
        // 后台时关闭 VSync 并降帧，减少无感知发热。
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Max(5, backgroundTargetFrameRate);
    }

    //公共方法用来执行GUI射线检测
    public bool CheckGuiRaycastObject()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();//检测当前鼠标指针是否悬停在GUI对象上
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        InputRouter.Instance?.LockInput("GameManager.PauseGame");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        InputRouter.Instance?.UnlockInput("GameManager.ResumeGame");
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
