using UnityEngine;

public static class UIUtils
{
    /// <summary>
    /// 根据完成目标数更新星星动画状态。
    /// </summary>
    public static void UpdateStarsUI(GameObject starContainer, int previousGoals, int completedGoals, int totalGoals)
    {
        if (totalGoals <= 0 || starContainer == null) return;

        int starCount = starContainer.transform.childCount;
        int currentFilledStars = Mathf.FloorToInt((float)completedGoals / totalGoals * starCount);
        int previousFilledStars = Mathf.FloorToInt((float)previousGoals / totalGoals * starCount);

        for (int i = 0; i < starCount; i++)
        {
            var starTransform = starContainer.transform.GetChild(i);
            var controller = starTransform.GetComponent<UIStarController>();

            if (controller == null) continue;

            if (i < currentFilledStars)
            {
                if (i >= previousFilledStars)
                {
                    // 这是新增的部分目标，播放 fill 动画
                    controller.PlayFillAnimation();
                }
                else
                {
                    // 已完成的部分目标，直接显示为已填充状态
                    controller.SetFilled();
                }
            }
            else
            {
                // 尚未完成的部分
                controller.SetEmpty();
            }
        }
    }
}
