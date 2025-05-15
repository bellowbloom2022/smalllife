using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Level : MonoBehaviour
{
    public static Level ins;
    public int currentLevelIndex;

    public GameObject mBtnNext;
    public GameObject mBtnNext1;
    public GameObject popup;
    public GameObject confirmationPanel;
    public GameObject continueButton;
    public GameObject proceedButton;
    public int TotalCount;
    public int requiredCount;
    public string NextLevelName;

    public int mCount = 0;
    public bool usePopup = true;
    public TextMeshProUGUI goalText;

    public List<GameObject> goals;

    private GameData currentGameData; // 内存中的数据副本

    private void Awake()
    {
        ins = this;
    }

    private void Start()
    {
        // 加载数据并初始化
        LoadGameData();
        UpdateGoalHint(mCount, TotalCount);
        UpdateLevelGoals();

        if (popup != null) popup.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }

    private void LoadGameData()
    {
        // 尝试加载现有的游戏数据，如果不存在则创建新数据
        currentGameData = SaveSystem.LoadGame() ?? new GameData();
        Debug.Log("Game data loaded successfully.");
    }

    public void AddCount()
    {
        ++this.mCount;
        UpdateGoalHint(this.mCount, this.TotalCount);

        if (this.mCount >= requiredCount)
        {
            ShowNextButton();
            PlayerPrefs.SetInt(SceneManager.GetActiveScene().name, 1);
        }

        if (this.mCount == this.TotalCount)
        {
            ShowAllGoalsFoundFeedback();
        }
        // 延迟保存数据，确保状态更新完毕
        StartCoroutine(DelaySave());
    }
    private void ShowNextButton()
    {
        if (mBtnNext != null)
        {
            mBtnNext.SetActive(true);
        }
    }
    public void ClosePopup()
    {
        if (popup != null)
        {
            popup.SetActive(false); // 关闭 popup 弹窗
        }
    }
    private IEnumerator DelaySave()
    {
        yield return new WaitForEndOfFrame(); // 等待当前帧结束，确保所有状态更新完毕
        SaveLevelData();
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

    private void ShowAllGoalsFoundFeedback()
    {
        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    public void onBtnNextClicked()
    {
        if (usePopup && confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
        else if (!usePopup)
        {
            if (mBtnNext1 != null)
            {
                mBtnNext1.SetActive(true);
            }
        }
    }

    public void onProceedButtonClicked()
    {
        SceneManager.LoadScene(NextLevelName);
    }

    public void onContinueButtonClicked()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void SaveLevelData()
    {
        // 确保目标对象ID列表已经初始化
        if (currentGameData.foundGoalIDs == null)
        {
            currentGameData.foundGoalIDs = new Dictionary<int, List<int>>();
        }

        int levelIndex = currentLevelIndex;
        Debug.Log("Current level index: " + levelIndex);

        currentGameData.goalsFound[levelIndex] = this.mCount;
        Debug.Log("Goals found in current level: " + this.mCount);

        List<int> foundGoalIDs = new List<int>();
        foreach (GameObject goal in goals)
        {
            Goal goalComponent = goal.GetComponent<Goal>();
            if (goalComponent != null)
            {
                Debug.Log($"Checking goal with ID: {goalComponent.goalID}");

                if (goalComponent.goalID != -1 && goalComponent.isFound)
                {
                    Debug.Log($"Adding goal ID: {goalComponent.goalID} to found goals.");
                    foundGoalIDs.Add(goalComponent.goalID);
                }
            }
            else
            {
                Debug.LogWarning($"No Goal component found on: {goal.name}");
            }
        }
        currentGameData.foundGoalIDs[levelIndex] = foundGoalIDs;
        currentGameData.currentLevel = levelIndex;

        // 只在必要时保存到文件
        SaveSystem.SaveGame(currentGameData);
        Debug.Log("Saved goals found: " + currentGameData.goalsFound[levelIndex] + " for level: " + levelIndex);
    }

    public bool IsGoalFound(int goalID)
    {
        return currentGameData.foundGoalIDs.ContainsKey(currentLevelIndex) &&
               currentGameData.foundGoalIDs[currentLevelIndex].Contains(goalID);
    }

    public void UpdateLevelGoals()
    {
        if (currentGameData != null && currentGameData.goalsFound != null &&
            currentGameData.goalsFound.ContainsKey(currentLevelIndex))
        {
            int foundCount = currentGameData.goalsFound[currentLevelIndex];
            List<int> foundGoalIDs = currentGameData.foundGoalIDs.ContainsKey(currentLevelIndex)
                ? currentGameData.foundGoalIDs[currentLevelIndex]
                : null;

            Debug.Log("Loading goals for level:" + currentLevelIndex);
            Debug.Log("Total goals found: " + foundCount);
            Debug.Log("Found goal IDs: " + string.Join(",", foundGoalIDs));

            if (foundGoalIDs != null)
            {
                foreach (GameObject goal in goals)
                {
                    Goal goalComponent = goal.GetComponent<Goal>();
                    if (goalComponent != null && foundGoalIDs.Contains(goalComponent.goalID))
                    {
                        UpdateGoalUI(goal);
                    }
                }
            }

            mCount = foundCount;
            UpdateGoalHint(mCount, TotalCount);

            if (mCount >= requiredCount)
            {
                ShowNextButton(); // 再次检查并显示 mBtnNext
            }
        }
    }

    private void UpdateGoalUI(GameObject goal)
    {
        if (goal != null)
        {
            Goal goalComponent = goal.GetComponent<Goal>();
            if (goalComponent != null)
            {
                if(goalComponent.mGameObjectNovel != null)
                {
                    goalComponent.mGameObjectNovel.transform.localScale = new Vector3(1, 1, goalComponent.mGameObjectNovel.transform.localScale.z);

                    Animator animator = goalComponent.mGameObjectNovel.GetComponent<Animator>();
                    if(animator != null)
                    {
                        animator.SetTrigger("click");

                        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    }
                }
            }
        }
    }
}
