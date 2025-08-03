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
    public Button continueButton; // “取消/继续当前关卡”按钮
    public Button proceedButton;  // “确认跳转下一关”按钮

    [Header("congrats panel prefabs")]
    public GameObject CongratulatePanel_ConfirmPrefab;   // Panel-congratulate1
    public GameObject CongratulatePanel_CelebratePrefab; // Panel-congratulate2
    
    private BasePanel confirmPanelInstance;
    private BasePanel celebratePanelInstance;

    [Header("Game Logic")]
    public int TotalCount;
    public int requiredCount;
    public string NextLevelName;
    public TextMeshProUGUI goalText;
    public List<GameObject> goals;

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
            mBtnNext1.onClick.AddListener(OnAllGoalsFoundBtnClicked);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (proceedButton != null)
            proceedButton.onClick.AddListener(OnProceedButtonClicked);

        // 初始化目标和进度
        LoadGameData();
        LoadAllGoalStates();
        UpdateGoalHint(mCount, TotalCount);
        UpdateLevelGoals();
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
        UpdateGoalHint(mCount, TotalCount);

        if (mCount >= requiredCount){
            PlayerPrefs.SetInt(SceneManager.GetActiveScene().name,1);

            //当目标数为1且requireCount也是1时，直接显示AllGoals反馈
            if (TotalCount == 1 && requiredCount == 1){
                ShowAllGoalsFoundFeedback();
            }
            else{
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

        if (confirmPanelInstance == null)
        {
            GameObject obj = Instantiate(CongratulatePanel_ConfirmPrefab, GameObject.Find("Canvas").transform);
            confirmPanelInstance = obj.GetComponent<BasePanel>();
        }

        confirmPanelInstance.Show();
    }

    public void OnAllGoalsFoundBtnClicked()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");

        if (celebratePanelInstance == null)
        {
            GameObject obj = Instantiate(CongratulatePanel_CelebratePrefab, GameObject.Find("Canvas").transform);
            celebratePanelInstance = obj.GetComponent<BasePanel>();
        }

        celebratePanelInstance.Show();
    }

    public void OnContinueButtonClicked()
    {
        AudioHub.Instance.PlayGlobal("back_confirm");
        if (confirmPanelInstance != null)
            confirmPanelInstance.Hide();
    }

    public void OnProceedButtonClicked()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
        SceneManager.LoadScene(NextLevelName);
    }

    private void UpdateGoalHint(int current, int total)
    {
        if (goalText != null)
        {
            goalText.text = $"{current}/{total}";
            StartCoroutine(AnimateGoalText());
        }
    }

    private IEnumerator AnimateGoalText()
    {
        Vector3 originalScale = goalText.transform.localScale;
        goalText.transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        goalText.transform.localScale = originalScale;
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
            if (goalComponent != null && goalComponent.GoalID != -1 && goalComponent.isFound){
                string key = currentLevelIndex + "_" + goalComponent.GoalID;

                GameDataUtils.SetGoalStep(data, currentLevelIndex, goalComponent.GoalID, true, true);
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
        UpdateGoalHint(mCount, TotalCount);

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
