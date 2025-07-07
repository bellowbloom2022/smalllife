using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarDisplayController : MonoBehaviour
{
    public GameObject starPrefab;     // 拖入你的星星 prefab
    public bool useLayoutGroup = true; // 是否使用 Horizontal Layout Group
    public float manualSpacing = 50f; // 如果不使用 Layout Group，就用这个手动设置间距

    private List<GameObject> currentStars = new List<GameObject>();

    /// <summary>
    /// 更新星星 UI。
    /// </summary>
    /// <param name="completed">已完成的目标数量</param>
    /// <param name="total">总目标数量</param>
    public void UpdateStars(int completed, int total)
    {
        // 清空旧星星
        foreach (GameObject star in currentStars)
        {
            Destroy(star);
        }
        currentStars.Clear();

        // 动态生成星星
        for (int i = 0; i < total; i++)
        {
            GameObject star = Instantiate(starPrefab, transform);
            star.SetActive(true);

            if (!useLayoutGroup)
            {
                // 手动排列位置（需关闭 Layout Group）
                RectTransform rt = star.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * manualSpacing, 0);
            }

            Animator animator = star.GetComponent<Animator>();

            if (i < completed)
            {
                animator?.SetTrigger("FillStar");
            }
            else
            {
                animator?.Play("Default", 0, 0);
            }

            currentStars.Add(star);
        }
    }
}
