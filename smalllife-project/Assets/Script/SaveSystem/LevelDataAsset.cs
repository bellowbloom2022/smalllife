using UnityEngine;
using Lean.Localization;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/LevelData")]
public class LevelDataAsset : ScriptableObject
{
    public string levelID;
    public string sceneToLoad;
    public string titleKey;
    public string descriptionKey;
    public Sprite previewImage;
    public int goalTotal;
}
