using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public static Level ins;

    [Header("Level Info")]
    public int currentLevelIndex;
    
    [Header("Goals")]
    public List<GameObject> goals;
    private List<Goal> goalComponents = new(); // ✅ 缓存组件引用

    [Header("关卡数据")]
    public LevelDataAsset levelDataAsset;

    [Header("UI")]
    public Button nextButton;    // 找到全部目标后显示的按钮
    public TextMeshProUGUI goalText;

    [Header("Count")]
    public int TotalCount;
    private int mCount = 0;

    [Header("Scene")]
    public SceneChanger sceneChanger;

    private GameData currentGameData;

    private void Awake()
    {
        ins = this;
    }

    private void Start()
    {
        // 设置当前关卡索引为 lastLevelIndex
        if (SaveSystem.GameData.lastLevelIndex != currentLevelIndex)
        {
            SaveSystem.GameData.lastLevelIndex = currentLevelIndex;
            SaveSystem.SaveGame();
        }

        // 初始化目标缓存 & 注入 levelData
        CacheGoals();
        // 初始化目标和进度
        LoadGameData();
        LoadAllGoalStates();
        UpdateGoalText();
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        UpdateLevelGoals();
    }
    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }
    
    //  缓存所有 Goal 脚本引用 & 自动注入关卡数据
    private void CacheGoals()
    {
        goalComponents.Clear();

        foreach (GameObject goalObj in goals)
        {
            if (goalObj == null) continue;

            Goal goal = goalObj.GetComponent<Goal>();
            if (goal != null)
            {
                goal.levelData = this.levelDataAsset;
                goalComponents.Add(goal);
            }
        }
    }

    private void LoadGameData()
    {
        SaveSystem.LoadGame(); // 仅调用加载方法
        var data = SaveSystem.GameData; // 获取数据
        Debug.Log("Game data loaded successfully.");
    }

    private void LoadAllGoalStates()
    {
        int found = 0;

        foreach (var goal in goalComponents)
        {
            string key = $"{currentLevelIndex}_{goal.GoalID}";

            if (SaveSystem.GameData.goalProgressMap.TryGetValue(key, out var progress))
            {
                goal.ApplySavedProgress(progress);

                if (progress.step1Completed && progress.step2Completed)
                    found++;
            }
        }

        mCount = found;
    }

    public void AddCount()
    {
        ++mCount;

        if (mCount >= TotalCount)
        {
            // 改为使用 GameData 标记关卡通关状态
            string sceneName = SceneManager.GetActiveScene().name;
            LevelDataAsset data = Resources.Load<LevelDataAsset>($"LevelDataAssets/{sceneName}");
            if (data == null)
            {
                Debug.LogError($"未找到对应的 LevelDataAsset: {sceneName}");
                return;
            }
            string levelID = data.levelID;
            Debug.Log($"[Save] Saving Completed LevelID: {levelID}");

            if (!SaveSystem.GameData.completedLevels.ContainsKey(levelID))
            {
                SaveSystem.GameData.completedLevels[levelID] = true;
                SaveSystem.SaveGame();  // 立即保存
            }
            nextButton.gameObject.SetActive(true);
        }
        SaveLevelData();
        ShowAllGoalsFoundFeedback();
    }
    private void UpdateGoalText()
    {
        if (goalText != null)
            goalText.text = $"{mCount}/{TotalCount}";
    }

    private void ShowNextButton()
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
        //通关时存入完成的关卡ID
        string sceneName = SceneManager.GetActiveScene().name;
        LevelDataAsset data = Resources.Load<LevelDataAsset>($"LevelDataAssets/{sceneName}");

        string levelID = SceneManager.GetActiveScene().name;
        SaveSystem.GameData.completedLevels[levelID] = true;
        SaveSystem.SaveGame();
        
        bool isCompleted = SaveSystem.GameData.completedLevels.ContainsKey(levelID)
            && SaveSystem.GameData.completedLevels[levelID];
    }

    private void ShowAllGoalsFoundFeedback()
    {   
        // ✅ 添加 checkmark 记录逻辑
        string levelID = SceneManager.GetActiveScene().name;
        var data = SaveSystem.GameData;

        if (!data.newlyCompletedLevelIDs.Contains(levelID))
        {
            data.newlyCompletedLevelIDs.Add(levelID);
            SaveSystem.SaveGame();
            Debug.Log($"✅ 记录该关卡 {levelID} 为新通关，用于主菜单显示勾选");
        }
    }

    public void OnNextButtonClicked()
    {
        if (sceneChanger == null)
        {
            Debug.LogError("SceneChanger not assigned in Level.");
            return;
        }
        sceneChanger.ChangeScene();
    }

    public void SaveLevelData()
    {
        var data = SaveSystem.GameData;

        foreach (var goal in goalComponents)
        {
            if (goal == null || goal.GoalID == -1 || !goal.isFound) continue;
            {
                //使用 goalComponent 的实际 step 状态，避免把 only-step1 的目标误标为 step2Completed
                GameDataUtils.SetGoalStep(
                    data,
                    currentLevelIndex,
                    goal.GoalID,
                    goal.step1Completed,
                    goal.step2Completed
                );
            }
        }
        data.currentLevel = currentLevelIndex;
        SaveSystem.SaveGame();
    }

    public void UpdateLevelGoals()
    {
        var data = SaveSystem.GameData;
        int foundCount = 0;

        foreach (var goal in goalComponents)
        {
            if (goal == null) continue;
            string key = $"{currentLevelIndex}_{goal.GoalID}";
            if (data.goalProgressMap.TryGetValue(key, out var progress) &&
                progress.step1Completed && progress.step2Completed)
            {
                UpdateGoalUI(goal.gameObject);
                foundCount++;
            }
        }
        mCount = foundCount;
        if (mCount >= TotalCount)
        {
            ShowNextButton();
        }
    }

    private void UpdateGoalUI(GameObject goal)
    {
        if (goal == null) return;

        Goal goalComponent = goal.GetComponent<Goal>();
        if (goalComponent == null) return;

        if (goalComponent.mGameObjectNovel != null)
        {
            goalComponent.mGameObjectNovel.transform.localScale = new Vector3(1, 1, goalComponent.mGameObjectNovel.transform.localScale.z);

            Animator animator = goalComponent.mGameObjectNovel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("click");
            }
        }
    }

    public bool IsGoalFound(int goalID)
    {
        string key = currentLevelIndex + "_" + goalID;
        return SaveSystem.GameData.goalProgressMap.ContainsKey(key) &&
               SaveSystem.GameData.goalProgressMap[key].step1Completed &&
               SaveSystem.GameData.goalProgressMap[key].step2Completed;
    }
    
    public void CompleteStep(string levelID, int goalID, int step)
    {
        var data = SaveSystem.GameData;
        bool step1 = step == 1;
        bool step2 = step == 2;

        string key = $"{levelID}_{goalID}";
        if (!data.goalProgressMap.ContainsKey(key))
            data.goalProgressMap[key] = new GoalProgress();

        var progress = data.goalProgressMap[key];
        GameDataUtils.SetGoalStep(data, int.Parse(levelID), goalID, progress.step1Completed || step1, progress.step2Completed || step2);

        SaveSystem.SaveGame();
    }
}
