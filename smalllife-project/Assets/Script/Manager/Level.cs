using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public static Level ins;
    public int currentLevelIndex;

    [Header("UI Buttons")]
    public Button mBtnNext;     // 找到部分目标后显示的按钮
    public Button mBtnNext1;    // 找到全部目标后显示的按钮
    public Button apartmentButton; // “跳转到公寓界面”按钮
    public Button proceedButton;  // “确认跳转下一关”按钮

    [Header("congrats panel prefabs")]
    public GameObject CongratulatePanel_CelebratePrefab; 
    private BasePanel celebratePanelInstance;
    private CelebratePanelController celebrateController;

    [Header("Game Logic")]
    public int TotalCount;
    public int requiredCount;
    public string NextLevelName;
    public TextMeshProUGUI goalText;
    public List<GameObject> goals;

    [Header("Animation Settings")]
    public float panelStartDelay = 1f;       // 弹窗显示后等待时间
    public float stepDelay = 0.15f;          // 每次数字间隔
    public float scaleFactor = 1.2f;         // 放大倍率

    private int mCount = 0;
    private GameData currentGameData;

    private void Awake()
    {
        ins = this;
    }

    private void Start()
    {
        // 设置当前关卡索引为 lastLevelIndex
        if (SaveSystem.GameData.lastLevelIndex != currentLevelIndex){
            SaveSystem.GameData.lastLevelIndex = currentLevelIndex;
            SaveSystem.SaveGame();
        }
        
        // 注册按钮点击事件 + 播放音效
        if (mBtnNext != null)
            mBtnNext.onClick.AddListener(OnBtnNextClicked);
        if (mBtnNext1 != null)
            mBtnNext1.onClick.AddListener(OnBtnNextClicked);
        if (proceedButton != null)
            proceedButton.onClick.AddListener(OnProceedButtonClicked);

        // 初始化目标和进度
        LoadGameData();
        LoadAllGoalStates();
        if (goalText != null) goalText.text = $"{mCount}/{TotalCount}";
        UpdateLevelGoals();
    }

    private void OnDestroy()
    {
        // 注销按钮点击事件，防止内存泄漏
        if (mBtnNext != null)
            mBtnNext.onClick.RemoveListener(OnBtnNextClicked);
        if (mBtnNext1 != null)
            mBtnNext1.onClick.RemoveListener(OnBtnNextClicked);
        if (proceedButton != null)
            proceedButton.onClick.RemoveListener(OnProceedButtonClicked);
    }

    private void LoadGameData()
    {
        SaveSystem.LoadGame(); // 仅调用加载方法
        var data = SaveSystem.GameData; // 获取数据
        Debug.Log("Game data loaded successfully.");
    }

    private void LoadAllGoalStates(){
        var data = SaveSystem.GameData;
        int foundCount = 0;

        foreach (GameObject goal in goals){
            Goal goalComponent = goal.GetComponent<Goal>();
            if (goalComponent != null){
                string key = $"{currentLevelIndex}_{goalComponent.GoalID}";
                if (data.goalProgressMap.TryGetValue(key, out var progress)){
                    goalComponent.ApplySavedProgress(progress);// 调用 Goal 脚本中的统一加载方法

                    if (progress.step1Completed && progress.step2Completed){
                        foundCount++;
                    }
                }
            }
        }
        mCount = foundCount;
    }

    public void AddCount()
    {
        ++mCount;

        if (mCount >= requiredCount)
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

            //当目标数为1且requireCount也是1时，直接显示AllGoals反馈
            if (TotalCount == 1 && requiredCount == 1)
            {
                ShowAllGoalsFoundFeedback();
            }
            else
            {
                ShowNextButton();
            }
        }

        if (mCount == TotalCount){
            ShowAllGoalsFoundFeedback();
        }
        StartCoroutine(DelaySave());
    }

    private void ShowNextButton()
    {
        if (mBtnNext != null)
            mBtnNext.gameObject.SetActive(true);
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
        if (mBtnNext != null)
            mBtnNext.gameObject.SetActive(false);
        if (mBtnNext1 != null)
            mBtnNext1.gameObject.SetActive(true);
            
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

    public void OnBtnNextClicked()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
        if (celebrateController == null)
        {
            GameObject obj = Instantiate(CongratulatePanel_CelebratePrefab, GameObject.Find("Canvas").transform);
            celebratePanelInstance = obj.GetComponent<BasePanel>();
            celebrateController = obj.GetComponent<CelebratePanelController>();
            if (celebrateController != null)
            {
                celebrateController.panelStartDelay = panelStartDelay;
                celebrateController.stepDelay = stepDelay;
                celebrateController.scaleFactor = scaleFactor;
            }
        }
        if (celebratePanelInstance != null)
        {
            celebratePanelInstance.Show();
            celebrateController.ShowAndPlay(mCount, TotalCount);
        }
    }
    public void OnProceedButtonClicked()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
        SceneManager.LoadScene(NextLevelName);
    }
    public void OnApartmentButtonClicked()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
    }
    private IEnumerator DelaySave()
    {
        yield return new WaitForEndOfFrame();
        SaveLevelData();
    }

    public void SaveLevelData()
    {
        var data = SaveSystem.GameData;

        foreach (GameObject goal in goals){
            Goal goalComponent = goal.GetComponent<Goal>();
            if (goalComponent != null && goalComponent.GoalID != -1 && goalComponent.isFound)
            {
                string key = currentLevelIndex + "_" + goalComponent.GoalID;

                //使用 goalComponent 的实际 step 状态，避免把 only-step1 的目标误标为 step2Completed
                GameDataUtils.SetGoalStep(data, currentLevelIndex, goalComponent.GoalID,
                    goalComponent.step1Completed, goalComponent.step2Completed);
            }
        }
        data.currentLevel = currentLevelIndex;
        SaveSystem.SaveGame();
    }

    public void UpdateLevelGoals()
    {
        var data = SaveSystem.GameData;
        int foundCount = 0;

        foreach (GameObject goal in goals)
        {
            Goal goalComponent = goal.GetComponent<Goal>();
            if (goalComponent != null)
            {
                string key = currentLevelIndex + "_" + goalComponent.GoalID;
                if (data.goalProgressMap.TryGetValue(key, out var progress) && progress.step1Completed && progress.step2Completed)
                {
                    UpdateGoalUI(goal);
                    foundCount++;
                }
            }
        }

        mCount = foundCount;

        if (mCount >= requiredCount)
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
