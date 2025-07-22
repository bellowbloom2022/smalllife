using System.Collections.Generic;

public static class GameDataUtils
{
    /// 获取某关卡所有目标的完成信息（含 step1/step2 状态）
    public static List<(int goalID, GoalProgress progress)> GetGoalsForLevel(GameData data, int levelIndex)
    {
        var result = new List<(int, GoalProgress)>();
        if (data == null || data.goalProgressMap == null) return result;

        foreach (var kvp in data.goalProgressMap)
        {
            string[] parts = kvp.Key.Split('_');
            if (parts.Length != 2) continue;

            if (int.TryParse(parts[0], out int level) && int.TryParse(parts[1], out int goalID))
            {
                if (level == levelIndex)
                    result.Add((goalID, kvp.Value));
            }
        }
        return result;
    }

    /// 获取某关卡中完成了 step1 + step2 的目标数量
    public static int GetCompletedGoalCount(GameData data, int levelIndex)
    {
        int count = 0;
        foreach (var (_, progress) in GetGoalsForLevel(data, levelIndex))
        {
            if (progress.step1Completed && progress.step2Completed)
                count++;
        }
        return count;
    }

    /// 获取某关卡中是否完成某个 goal 的 step1/step2
    public static bool IsGoalCompleted(GameData data, int levelIndex, int goalID)
    {
        string key = $"{levelIndex}_{goalID}";
        if (data.goalProgressMap.TryGetValue(key, out var progress))
        {
            return progress.step1Completed && progress.step2Completed;
        }
        return false;
    }

    /// 设置某个目标的 step 状态
    public static void SetGoalStep(GameData data, int levelIndex, int goalID, bool step1, bool step2)
    {
        string key = $"{levelIndex}_{goalID}";
        if (!data.goalProgressMap.ContainsKey(key))
        {
            data.goalProgressMap[key] = new GoalProgress();
        }
        data.goalProgressMap[key].step1Completed = step1;
        data.goalProgressMap[key].step2Completed = step2;
    }

    public static GoalProgress GetGoalStep(GameData data, int levelIndex, int goalID){
        string key = $"{levelIndex}_{goalID}";
        if (data.goalProgressMap.TryGetValue(key, out GoalProgress progress)){
            return progress;
        }

        //如果没有记录，返回默认值（都为 false）
        return new GoalProgress { step1Completed = false, step2Completed = false };
    }
} 
