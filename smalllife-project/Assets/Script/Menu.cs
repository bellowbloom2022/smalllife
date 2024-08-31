using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject[] levelStars; // 每个关卡对应的星星UI数组

    private void Start()
    {
        UpdateLevelStars();
    }

    private void UpdateLevelStars()
    {
        GameData data = SaveSystem.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("No saved game data found!");
            return;
        }
        // 打印出所有存储的数据
        Debug.Log("Updating Stars: " + string.Join(", ", data.goalsFound));

        for (int i = 0; i < levelStars.Length; i++)
        {
            // 假设第一个关卡在data.goalsFound[0]
            int completedGoals = (data.goalsFound != null && data.goalsFound.ContainsKey(i)) ? data.goalsFound[i] : 0;
            int totalGoals = GetTotalGoalsForLevel(i + 1);//levelIndex从1开始

            Debug.Log("Level " + (i + 1) + ": Found " + completedGoals + " goals out of " + totalGoals);

            UpdateStarsUI(levelStars[i], completedGoals, totalGoals);
        }
    }

    private int GetTotalGoalsForLevel(int levelIndex)
    {
        int[] levelGoals = { 1, 4, 3, 4 }; // 示例：假设有四个关卡，目标数分别是1，4, 7, 12
        if (levelIndex > 0 && levelIndex <= levelGoals.Length)
        {
            return levelGoals[levelIndex - 1]; // levelIndex 是从1开始的
        }
        else
        {
            Debug.LogWarning("Invalid level index: " + levelIndex);
            return 0; // 无效关卡索引返回0
        }
    }

    private void UpdateStarsUI(GameObject starContainer, int completedGoals, int totalGoals)
    {
        int starCount = starContainer.transform.childCount;
        int filledStars = Mathf.FloorToInt((float)completedGoals / totalGoals * starCount);

        //遍历每个星星UI
        for (int j = 0; j < starCount; j++)
        {
            Transform star = starContainer.transform.GetChild(j);
            Animator animator = star.GetComponent<Animator>();

            if (j < filledStars)
            {
                // 更新为已完成的星星状态
                if (animator != null)
                {
                    animator.SetTrigger("FillStar");//播放填充动画
                }
                star.gameObject.SetActive(true); // 确保星星被激活
            }
            else
            {
                // 确保星星显示默认状态
                if (animator != null)
                {
                    animator.Play("Default", 0, 0); // 播放默认状态动画
                }
                star.gameObject.SetActive(true); // 激活未填充的星星
            }
        }
    }
}

