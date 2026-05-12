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
    public TextMeshProUGUI goalText;

    [Header("Quick Next Button")]
    [SerializeField] private Button topRightNextLevelButton;
    [SerializeField] private NextButtonEffectController nextButtonEffect;

    [Header("Count")]
    public int TotalCount;
    private int mCount = 0;

    [Header("Scene")]
    public SceneChanger sceneChanger;

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

        UpdateLevelGoals();

        SetupTopRightNextLevelButton();
        RefreshTopRightNextLevelButtonState();
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
        // SaveSystem is persistent and already initialized before level start.
        // Re-loading from disk here can overwrite in-memory settings changed in UI
        // moments earlier (e.g. overlayColorIndex selected on Title/Pause settings).
        var data = SaveSystem.GameData;
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

                if (GoalProgressRules.IsCollected(goal, progress))
                    found++;
            }
        }

        mCount = found;
    }

    public void AddCount()
    {
        ++mCount;
        UpdateGoalText();

        if (mCount >= TotalCount)
        {
            MarkCurrentLevelCompleted();
            // 通关后不自动弹出 InfoPanel，避免打断玩家。
            RefreshTopRightNextLevelButtonState();
        }

        SaveLevelData();
        ShowAllGoalsFoundFeedback();
    }

    private void UpdateGoalText()
    {
        if (goalText != null)
            goalText.text = $"{mCount}/{TotalCount}";
    }

    private void MarkCurrentLevelCompleted()
    {
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
            SaveSystem.SaveGame();
        }
    }

    private void SetupTopRightNextLevelButton()
    {
        if (topRightNextLevelButton == null)
            return;

        topRightNextLevelButton.onClick.RemoveListener(OnTopRightNextLevelButtonClicked);
        topRightNextLevelButton.onClick.AddListener(OnTopRightNextLevelButtonClicked);
    }

    private void RefreshTopRightNextLevelButtonState()
    {
        if (topRightNextLevelButton == null)
            return;

        bool canGoNext = mCount >= TotalCount && sceneChanger != null;
        bool wasActive = topRightNextLevelButton.gameObject.activeSelf;
        topRightNextLevelButton.gameObject.SetActive(canGoNext);

        // 当按钮从隐藏变为显示时，播放出现动画
        if (canGoNext && !wasActive && nextButtonEffect != null)
        {
            nextButtonEffect.PlayAppearEffect();
        }
    }

    private void OnTopRightNextLevelButtonClicked()
    {
        if (sceneChanger == null)
        {
            Debug.LogWarning("Level: SceneChanger not assigned for top-right next level button.");
            return;
        }

        // 禁用按钮防止重复点击
        topRightNextLevelButton.interactable = false;

        // 获取淡出时长，默认 0.5f
        float fadeDuration = sceneChanger.fadeOutDuration;

        // 播放消失动画，动画完成后切换场景
        if (nextButtonEffect != null)
        {
            nextButtonEffect.PlayDisappearEffect(fadeDuration, () =>
            {
                sceneChanger.ChangeScene();
            });
        }
        else
        {
            sceneChanger.ChangeScene();
        }
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
                GoalProgressRules.IsCollected(goal, progress))
            {
                UpdateGoalUI(goal.gameObject);
                foundCount++;
            }
        }
        mCount = foundCount;
        UpdateGoalText();
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
        if (!SaveSystem.GameData.goalProgressMap.TryGetValue(key, out var progress))
            return false;

        Goal goal = goalComponents.Find(g => g != null && g.GoalID == goalID);
        return GoalProgressRules.IsCollected(goal, progress);
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
