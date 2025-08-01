using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public string version = "0.0.6"; // 存档对应的游戏版本号
    public int currentLevel = 0;
    public int lastLevelIndex = -1; // -1 表示尚未开始任何游戏

    // 设置项存储
    public GameSettings settings = new();
    // 游戏目标进度（序列化支持）: "levelIndex_goalID"
    [System.NonSerialized]
    public Dictionary<string, GoalProgress> goalProgressMap = new();

    [SerializeField]
    private List<SerializableGoalEntry> serializedGoalList = new();

    // 每关星星数统计（可用于菜单页显示）
    [System.NonSerialized]
    public Dictionary<int, int> levelStars = new();

    [SerializeField]
    private List<SerializableStarEntry> serializedStarList = new();

    //保存前调用（将 Dictionary 转为 List）
    public void SerializeGoalData(){
        serializedGoalList.Clear();
        foreach (var kvp in goalProgressMap){
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
    }
}

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

// --------- 为 Dictionary 提供序列化包装 -----------

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
