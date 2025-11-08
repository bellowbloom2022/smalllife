using System.IO;
using UnityEngine;
using System;

public class SaveSystem : MonoBehaviour
{
    public const string CURRENT_SAVE_VERSION = "0.0.7";
    public static SaveSystem Instance;
    private static string savePath;
    public static GameData gameData { get; private set; }
    
    // 属性暴露外部访问(GameData 依赖其他系统初始化顺序,用懒加载（Lazy Init）模式确保调用前初始化)
    public static GameData GameData
    {
        get
        {
            EnsureSavePath();
            if (gameData == null)
            {
                LoadGame(); // 尝试加载
                if (gameData == null)
                    gameData = new GameData(); // 还是空就新建一个
            }

            return gameData;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
            Debug.Log("SaveSystem initialized and preserved across scenes.");
            LoadGame(); // 自动加载
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //安全初始化 savePath（tool异常操作测试工具需要）
    private static void EnsureSavePath()
    {
        if (string.IsNullOrEmpty(savePath))
            savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
    }

    public static void SaveGame()
    {
        EnsureSavePath(); 
        try
        {
            GameData.SerializeGoalData(); // 转换 Dictionary 为 List
            GameData.version = CURRENT_SAVE_VERSION;
            string json = JsonUtility.ToJson(GameData, true);//true: 格式化输出，便于调试
            File.WriteAllText(savePath, json);
            Debug.Log("Game saved to: " + savePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save game: " + ex.Message);
        }
        Debug.Log(JsonUtility.ToJson(GameData)); // 打印实际写入的数据
    }

    public static void LoadGame()
    {
        EnsureSavePath();
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                gameData = JsonUtility.FromJson<GameData>(json);
                gameData = SaveDataUpdater.UpdateSaveData(gameData);
                gameData.DeserializeGoalData(); // 转换 List 为 Dictionary
                Debug.Log("Game data loaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading game data: " + ex.Message);
                gameData = new GameData();
            }
        }
        else
        {
            Debug.LogWarning("No save file found. Creating new game data.");
            gameData = new GameData();
            SaveGame();
        }
    }

    public static void ClearData()
    {
        // 直接使用 savePath 进行文件删除操作
        if (File.Exists(savePath))
        {
            File.Delete(savePath); // 删除保存文件
            Debug.Log("Save file deleted.");
        }
        else
        {
            Debug.Log("No save file found to delete.");
        }
        gameData = new GameData();
    }

    //只有目标数增加时才更新存档
    public static void UpdateLevelStar(int levelIndex, int newValue)
    {
        if (gameData == null)
            LoadGame();

        if (gameData.levelStars.ContainsKey(levelIndex))
        {
            if (gameData.levelStars[levelIndex] < newValue)
            {
                gameData.levelStars[levelIndex] = newValue;
                SaveGame();
            }
        }
        else
        {
            gameData.levelStars[levelIndex] = newValue;
            SaveGame();
        }
    }
    public static void MarkNewDiaryContent(bool val)
    {
        if (GameData == null)
            LoadGame();

        GameData.hasNewDiaryContent = val;
        Debug.Log($"[SaveSystem] Diary highlight set to: {val}");
        SaveGame();
    }

    public static bool HasNewDiaryContent()
    {
        if (GameData == null)
            LoadGame();

        return GameData.hasNewDiaryContent;
    }
}
