using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class GameData
{
    public Dictionary<int, int> goalsFound = new Dictionary<int, int>(); // ÿ���ؿ��ҵ���Ŀ������
    public Dictionary<int, List<int>> foundGoalIDs = new Dictionary<int, List<int>>(); // ÿ���ؿ��ҵ���Ŀ�����ID�б�
    public int currentLevel;//��ǰ�ؿ�����
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
                // ʹ�� using �����ȷ���ļ�����ʹ����Ϻ��Զ��ر�
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
        // ֱ��ʹ�� savePath �����ļ�ɾ������
        if (File.Exists(savePath))
        {
            File.Delete(savePath); // ɾ�������ļ�
            Debug.Log("Save file deleted.");
        }
        else
        {
            Debug.Log("No save file found to delete.");
        }
    }
}
