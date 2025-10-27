using UnityEngine;
using Lean.Localization;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/LevelData")]
public class LevelDataAsset : ScriptableObject
{
    [Header("基本信息")]
    public string levelID;// 和 Level.currentLevelIndex 保持一致
    public string sceneToLoad;// 关卡场景名
    public string titleKey;// 本地化标题 key (Spot01, Spot02...)
    public string descriptionKey;// 本地化描述 key
    public Sprite previewImage;// Menupage 用的预览图
    [Header("目标/贴纸")]
    public int goalTotal;// 本关目标总数
    public int[] goalIDs; // 手动填入每个 Goal 的真实 ID（比如 1, 3, 5）
    public Sprite[] goalIcons; // 每个目标的贴纸图标（原始icon，系统加阴影）
    public Sprite[] graySprites;  // 对应灰色槽位图

    [Header("速写 & 日记")]
    public Sprite[] sketchImages; // 每个 goal 对应的速写图
    public string[] diaryKeys;          // 每个目标解锁一句日记，对应 LeanLocalization key

    [Header("相册 / Album")]
    public Sprite[] photoImages;        // 每个 goal 对应的相册照片
    public string[] photoMonologueKeys; // 每张照片对应的角色碎碎念 (Lean Key)
}
