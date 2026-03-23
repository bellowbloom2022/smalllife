using System;
using UnityEngine;

public enum GoalNoteStep
{
    Step1 = 1,
    Step2 = 2
}

public static class GoalNoteEvents
{
    // Params: levelID, goalID, completedStep
    public static event Action<string, int, GoalNoteStep> GoalCompleted;

    public static void RaiseGoalCompleted(string levelID, int goalID, GoalNoteStep completedStep)
    {
        if (string.IsNullOrEmpty(levelID))
        {
            Debug.LogWarning("[GoalNoteEvents] levelID is empty.");
            return;
        }

        GoalCompleted?.Invoke(levelID, goalID, completedStep);
    }
}
