using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // 存档版本号（用于未来升级）
    public string version = "0.0.8"; 

    // 基础游戏进度字段
    public int currentLevel = 0;
    public int lastLevelIndex = -1; // -1 表示尚未开始任何游戏
    public List<string> newlyCompletedLevelIDs = new List<string>();

    // 设置项（子结构）
    public GameSettings settings = new();

    // ========== 可扩展结构：目标进度 ==========
    [System.NonSerialized]
    public Dictionary<string, GoalProgress> goalProgressMap = new();

    [SerializeField]
    private List<SerializableGoalEntry> serializedGoalList = new();

    // ========== 可扩展结构：每关星数统计 ==========
    [System.NonSerialized]
    public Dictionary<int, int> levelStars = new();

    [SerializeField]
    private List<SerializableStarEntry> serializedStarList = new();

    // ========== 可扩展结构：通关状态 ==========
    [System.NonSerialized]
    public Dictionary<string, bool> completedLevels = new();

    [SerializeField]
    private List<SerializableCompletedLevelEntry> serializedCompletedLevels = new();

    // ========== 可扩展结构：未来预留字段 ==========
    //首次触发提示或首次操作记录
    [System.NonSerialized]
    public HashSet<string> seenHints = new();
    [SerializeField]
    private List<string> serializedSeenHints = new();
    // 玩家见过的最大关卡ID（用于动态解锁、避免重复提示）
    [SerializeField]
    public int maxLevelIDSeen = 0;
    // 已完成目标（例如剧情任务、可选任务）
    [System.NonSerialized]
    public HashSet<string> completedGoals = new();
    [SerializeField]
    private List<string> serializedCompletedGoals = new();
    // 收藏夹（如收藏角色、彩蛋）
    public List<string> favoriteIDs = new();
    // 已阅读文本记录（剧情防重复播放）
    [System.NonSerialized]
    public HashSet<string> viewedDialogIDs = new();
    [SerializeField]
    private List<string> serializedViewedDialogIDs = new();

    // ========== 序列化函数 ==========保存前调用（将 Dictionary 转为 List）

    public void SerializeAll()
    {
        // ✅ 完成目标列表
        serializedCompletedGoals.Clear();
        foreach (var id in completedGoals)
        {
            serializedCompletedGoals.Add(id);
        }

        // ✅ 已阅对白ID
        serializedViewedDialogIDs.Clear();
        foreach (var id in viewedDialogIDs)
        {
            serializedViewedDialogIDs.Add(id);
        }

        // ✅ 已显示提示
        serializedSeenHints.Clear();
        foreach (var id in seenHints)
        {
            serializedSeenHints.Add(id);
        }

        // ✅ 其余已有数据结构也调用（如目标进度、星星数等）
        SerializeGoalData(); // 你现有的老函数
    }

    public void DeserializeAll()
    {
        // ✅ 完成目标列表
        completedGoals.Clear();
        foreach (var id in serializedCompletedGoals)
        {
            completedGoals.Add(id);
        }

        // ✅ 已阅对白ID
        viewedDialogIDs.Clear();
        foreach (var id in serializedViewedDialogIDs)
        {
            viewedDialogIDs.Add(id);
        }

        // ✅ 已显示提示
        seenHints.Clear();
        foreach (var id in serializedSeenHints)
        {
            seenHints.Add(id);
        }

        // ✅ 其余已有数据结构也调用（如目标进度、星星数等）
        DeserializeGoalData(); // 你现有的老函数
    }

    public void SerializeGoalData()
    {
        serializedGoalList.Clear();
        foreach (var kvp in goalProgressMap)
        {
            serializedGoalList.Add(new SerializableGoalEntry
            {
                key = kvp.Key,
                value = kvp.Value
            });
        }

        serializedStarList.Clear();
        foreach (var kvp in levelStars)
        {
            serializedStarList.Add(new SerializableStarEntry
            {
                levelIndex = kvp.Key,
                starCount = kvp.Value
            });
        }

        serializedCompletedLevels.Clear();
        foreach (var kvp in completedLevels)
        {
            serializedCompletedLevels.Add(new SerializableCompletedLevelEntry
            {
                levelID = kvp.Key,
                isCompleted = kvp.Value
            });
        }
    }

    // 读取后调用（将 List 转回 Dictionary）
    public void DeserializeGoalData()
    {
        goalProgressMap.Clear();
        foreach (var entry in serializedGoalList)
        {
            goalProgressMap[entry.key] = entry.value;
        }

        levelStars.Clear();
        foreach (var entry in serializedStarList)
        {
            levelStars[entry.levelIndex] = entry.starCount;
        }

        completedLevels.Clear();
        foreach (var entry in serializedCompletedLevels)
        {
            completedLevels[entry.levelID] = entry.isCompleted;
        }
    }
}

// ========== 子结构定义 ==========
[System.Serializable]
public class GoalProgress
{
    public bool step1Completed = false;
    public bool step2Completed = false;
}

[System.Serializable]
public class GameSettings
{
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public string dragMode = "RightClick"; // 举例：LeftClick, RightClick
    public int displayModeIndex = 0;
    public int resolutionIndex = 0;
    public int overlayColorIndex = 0; // 色调选项
    public string language = "Chinese";
}

// ========== 字典包装结构 ==========

[System.Serializable]
public class SerializableGoalEntry
{
    public string key;
    public GoalProgress value;
}

[System.Serializable]
public class SerializableStarEntry
{
    public int levelIndex;
    public int starCount;
}

[System.Serializable]
public class SerializableCompletedLevelEntry
{
    public string levelID;
    public bool isCompleted;
}
