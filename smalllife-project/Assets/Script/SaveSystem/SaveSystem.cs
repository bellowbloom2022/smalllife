using System.IO;
using UnityEngine;
using System;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private static string savePath;
    public static GameData GameData { get; private set; }


    private void Awake()
    {
        if(Instance == null)
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

    public static void SaveGame()
    {
        try{
            GameData.SerializeGoalData(); // 转换 Dictionary 为 List

            string json = JsonUtility.ToJson(GameData, true);//true: 格式化输出，便于调试
            File.WriteAllText(savePath, json);
            Debug.Log("Game saved to: " + savePath);
        }
        catch (Exception ex){
            Debug.LogError("Failed to save game: " + ex.Message);
        }
        Debug.Log(JsonUtility.ToJson(GameData)); // 打印实际写入的数据
    }

    public static void LoadGame()
    {

        if (File.Exists(savePath)){
            try {
                string json = File.ReadAllText(savePath);
                GameData = JsonUtility.FromJson<GameData>(json);
                GameData.DeserializeGoalData(); // 转换 List 为 Dictionary
                Debug.Log("Game data loaded successfully.");
            }
            catch (Exception ex){
                Debug.LogError("Error loading game data: " + ex.Message);
                GameData = new GameData();
            }
        }
        else{
            Debug.LogWarning("No save file found. Creating new game data.");
            GameData = new GameData();
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
        GameData = new GameData();
    }

    //只有目标数增加时才更新存档
    public static void UpdateLevelStar(int levelIndex, int newValue){
        if(GameData == null)
           LoadGame();

        if (GameData.levelStars.ContainsKey(levelIndex)){
            if(GameData.levelStars[levelIndex] < newValue){
                GameData.levelStars[levelIndex] = newValue;
                SaveGame();
            }
        }
        else{
            GameData.levelStars[levelIndex] = newValue;
            SaveGame();
        }
    }
}
