using UnityEngine;

public class SingleGoal : Goal
{
    private enum SingleGoalStage
    {
        Normal,     // 初始状态，物件还没被找到
        Collected   // 已经收集完成
    }
    private SingleGoalStage singleStage;

    protected override void Start()
    {
        base.Start();
        singleStage = SingleGoalStage.Normal;
    }

    public void OnClick()
    {
        if (singleStage == SingleGoalStage.Normal)
        {
            TriggerCollectAnimation(false); // 单目标只记 step1，不触发 step2
            singleStage = SingleGoalStage.Collected;
        }
    }
}
