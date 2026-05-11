using UnityEngine;

public enum GoalHintStage
{
    PreAnim1,
    PostAnim1,
    PostAnim2
}

public partial class Goal : MonoBehaviour
{
    public GoalHintStage GetHintStage()
    {
        if (GoalProgressRules.IsCollected(this, step1Completed, step2Completed))
            return GoalHintStage.PostAnim2;

        if (step1Completed)
            return GoalHintStage.PostAnim1;

        return GoalHintStage.PreAnim1;
    }

    public string GetHintLevelID()
    {
        if (levelData != null && !string.IsNullOrEmpty(levelData.levelID))
            return levelData.levelID;

        if (Level.ins != null && Level.ins.levelDataAsset != null && !string.IsNullOrEmpty(Level.ins.levelDataAsset.levelID))
            return Level.ins.levelDataAsset.levelID;

        if (Level.ins != null)
            return Level.ins.currentLevelIndex.ToString();

        return string.Empty;
    }

    public bool IsHintCompleted()
    {
        return GoalProgressRules.IsCollected(this, step1Completed, step2Completed);
    }
}
