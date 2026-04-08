using UnityEngine;

public static class GoalProgressRules
{
    public static bool IsCollected(Goal goal, GoalProgress progress)
    {
        if (goal == null || progress == null)
            return false;

        return IsCollected(goal is SingleGoal, progress.step1Completed, progress.step2Completed);
    }

    public static bool IsCollected(Goal goal, bool step1Completed, bool step2Completed)
    {
        if (goal == null)
            return false;

        return IsCollected(goal is SingleGoal, step1Completed, step2Completed);
    }

    public static bool IsCollected(bool isSingleStep, bool step1Completed, bool step2Completed)
    {
        if (isSingleStep)
            return step1Completed;

        return step1Completed && step2Completed;
    }

    public static bool IsSameLevelID(string lhs, string rhs)
    {
        if (lhs == rhs)
            return true;

        if (TryParseLevelIndexStrict(lhs, out int lhsIndex) &&
            TryParseLevelIndexStrict(rhs, out int rhsIndex))
        {
            return lhsIndex == rhsIndex;
        }

        return false;
    }

    public static bool TryParseLevelIndexStrict(string raw, out int levelIndex)
    {
        levelIndex = -1;
        if (string.IsNullOrEmpty(raw))
            return false;

        if (int.TryParse(raw, out levelIndex))
            return true;

        const string levelPrefix = "Level";
        if (raw.Length > levelPrefix.Length &&
            raw.StartsWith(levelPrefix, System.StringComparison.OrdinalIgnoreCase))
        {
            string suffix = raw.Substring(levelPrefix.Length);
            return int.TryParse(suffix, out levelIndex);
        }

        return false;
    }
}