using UnityEditor;
using Lean.Localization;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataEditorWindow : EditorWindow
{
    private List<LevelDataAsset> levelDateList = new List<LevelDataAsset>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Level Data Editor")]
    public static void OpenWindow(){
        GetWindow<LevelDataEditorWindow>("Level Data Editor");
    }

    private void OnEnable(){
        LoadAllLevelData();
    }

    private void LoadAllLevelData(){
        levelDateList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:LevelDataAsset");
        foreach (string guid in guids){
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelDataAsset asset = AssetDatabase.LoadAssetAtPath<LevelDataAsset>(path);
            if (asset != null)
                levelDateList.Add(asset);
        }
    }

    private void OnGUI(){
        if (GUILayout.Button("Creat New Level Data")){
            CreatNewLevelData();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var levelData in levelDateList){
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.ObjectField("Asset", levelData, typeof(LevelDataAsset), false);
            levelData.levelID = EditorGUILayout.TextField("Level ID", levelData.levelID);
            levelData.titleKey = EditorGUILayout.TextField("Title Key", levelData.titleKey);
            levelData.descriptionKey = EditorGUILayout.TextField("Description Key", levelData.descriptionKey);
            levelData.previewImage = (Sprite)EditorGUILayout.ObjectField("Preview", levelData.previewImage, typeof(Sprite), false);
            levelData.goalTotal = EditorGUILayout.IntField("Total Goals", levelData.goalTotal);

            if (GUI.changed)
               EditorUtility.SetDirty(levelData);
            
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    private void CreatNewLevelData(){
        string path = EditorUtility.SaveFilePanelInProject("Save New Level Data", "NewLevelData", "asset", "Choose location to save new level data asset");
        if (!string.IsNullOrEmpty(path)){
            LevelDataAsset newData = ScriptableObject.CreateInstance<LevelDataAsset>();
            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.SaveAssets();
            LoadAllLevelData();
        }
    }
}
