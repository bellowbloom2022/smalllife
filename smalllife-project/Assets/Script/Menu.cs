using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject[] levelStars; // ÿ���ؿ���Ӧ������UI����

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
        // ��ӡ�����д洢������
        Debug.Log("Updating Stars: " + string.Join(", ", data.goalsFound));

        for (int i = 0; i < levelStars.Length; i++)
        {
            // �����һ���ؿ���data.goalsFound[0]
            int completedGoals = (data.goalsFound != null && data.goalsFound.ContainsKey(i)) ? data.goalsFound[i] : 0;
            int totalGoals = GetTotalGoalsForLevel(i + 1);//levelIndex��1��ʼ

            Debug.Log("Level " + (i + 1) + ": Found " + completedGoals + " goals out of " + totalGoals);

            UpdateStarsUI(levelStars[i], completedGoals, totalGoals);
        }
    }

    private int GetTotalGoalsForLevel(int levelIndex)
    {
        int[] levelGoals = { 1, 4, 3, 4 }; // ʾ�����������ĸ��ؿ���Ŀ�����ֱ���1��4, 7, 12
        if (levelIndex > 0 && levelIndex <= levelGoals.Length)
        {
            return levelGoals[levelIndex - 1]; // levelIndex �Ǵ�1��ʼ��
        }
        else
        {
            Debug.LogWarning("Invalid level index: " + levelIndex);
            return 0; // ��Ч�ؿ���������0
        }
    }

    private void UpdateStarsUI(GameObject starContainer, int completedGoals, int totalGoals)
    {
        int starCount = starContainer.transform.childCount;
        int filledStars = Mathf.FloorToInt((float)completedGoals / totalGoals * starCount);

        //����ÿ������UI
        for (int j = 0; j < starCount; j++)
        {
            Transform star = starContainer.transform.GetChild(j);
            Animator animator = star.GetComponent<Animator>();

            if (j < filledStars)
            {
                // ����Ϊ����ɵ�����״̬
                if (animator != null)
                {
                    animator.SetTrigger("FillStar");//������䶯��
                }
                star.gameObject.SetActive(true); // ȷ�����Ǳ�����
            }
            else
            {
                // ȷ��������ʾĬ��״̬
                if (animator != null)
                {
                    animator.Play("Default", 0, 0); // ����Ĭ��״̬����
                }
                star.gameObject.SetActive(true); // ����δ��������
            }
        }
    }
}

