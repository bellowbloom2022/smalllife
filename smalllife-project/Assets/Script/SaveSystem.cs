using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class GameData
{
    public Dictionary<int, int> goalsFound = new Dictionary<int, int>(); // 每个关卡找到的目标总数
    public Dictionary<int, List<int>> foundGoalIDs = new Dictionary<int, List<int>>(); // 每个关卡找到的目标对象ID列表
    public int currentLevel;//当前关卡进度
}
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private static string savePath;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.persistentDataPath + "/gamedata.save";
            Debug.Log("SaveSystem initialized and preserved across scenes.");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static void SaveGame(GameData data)
    {

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(file, data);
        file.Close();
    }

    public static GameData LoadGame()
    {

        if (File.Exists(savePath))
        {
            try
            {
                // 使用 using 语句来确保文件流在使用完毕后自动关闭
                using (FileStream file = new FileStream(savePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    GameData data = formatter.Deserialize(file) as GameData;

                    if (data != null)
                    {
                        Debug.Log("Game data loaded successfully.");
                    }
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading game data: " + ex.Message);
                return null;
            }
        }
        else
        {
            Debug.LogWarning("No save file found at path: " + savePath);
            return null;
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
    }
}
