using UnityEngine;

public class SingleGoal : Goal
{
    private enum SingleGoalStage
    {
        Normal,     // 初始状态，物件还没被找到
        Collected   // 已经收集完成
    }
    private SingleGoalStage singleStage;
    private bool isCollecting;

    protected override void Start()
    {
        base.Start();
        singleStage = step1Completed ? SingleGoalStage.Collected : SingleGoalStage.Normal;
    }

    public override void ApplySavedProgress(GoalProgress progress)
    {
        base.ApplySavedProgress(progress);
        isFound = step1Completed;
        singleStage = step1Completed ? SingleGoalStage.Collected : SingleGoalStage.Normal;
    }

    public override void OnClicked()
    {
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        if (isCollecting || step1Completed || singleStage == SingleGoalStage.Collected)
            return;

        isCollecting = true;
        PlayStep1(); // 播放 step1 动画；收集动作在 OnAnimEnd 内触发
    }

    public override void OnAnimEnd()
    {
        // 单步 goal：step1 动画播完后直接收集，不等待 step2 点击
        if (!step1Completed)
        {
            EndStep(step1Config);
            singleStage = SingleGoalStage.Collected;
            TriggerCollectAnimation(false);
        }
    }
}
