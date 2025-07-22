using UnityEngine;
using Lean.Localization;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RightPanelUI : MonoBehaviour
{
    public Image previewImage;
    public LeanLocalizedText descriptionLocalizedText;
    public LeanLocalizedText titleLocalizedText;
    public Transform starParent;       // 星星的容器（空物体）
    public GameObject starPrefab;      // 一个星星 prefab（图标）
    public GameObject confirmPopup;
    public Button previewImageButton;
    public Button buttonEnter;
    public Button buttonCancel;

    private List<GameObject> starPool = new List<GameObject>();
    public GameData gameData;  // 在 Inspector 中拖入当前的 GameData 引用

    private string currentSceneToLoad;

    private void Start(){
        previewImageButton.onClick.AddListener(OnPreviewImageButtonClicked);
        buttonCancel.onClick.AddListener(ClosePopup);
        buttonEnter.onClick.AddListener(LoadScene);
        confirmPopup.SetActive(false);
    }

    public void UpdateContent(LevelDataAsset data, int levelIndex)
    {
        // --- 1. 本地化标题和描述 ---
        titleLocalizedText.TranslationName = data.titleKey;
        descriptionLocalizedText.TranslationName = data.descriptionKey;
        // --- 2. 加载当前预览图片 ---
        previewImage.sprite = data.previewImage;
        // --- 3. 设置当前待加载的关卡名 ---
        currentSceneToLoad = data.sceneToLoad;
        // --- 4. 先调用加载然后获取数据
        SaveSystem.LoadGame(); 
        gameData = SaveSystem.GameData; 
        // --- 5. 清除旧星星 ---
        foreach (var star in starPool)
        {
            Destroy(star);
        }
        starPool.Clear();
        // --- 6. 读取上一次记录的星星数（无记录则为0） ---
        int previousGoalsFound = 0;
        if (gameData.levelStars.TryGetValue(levelIndex, out int savedCount))
            previousGoalsFound = savedCount;
        // --- 7. 当前存档中统计关卡已完成目标数 ---
        int currentGoalsFound = GameDataUtils.GetCompletedGoalCount(gameData, levelIndex);
        Debug.Log($"[UI检查] 关卡 {levelIndex} 已完成目标数: {currentGoalsFound}/{data.goalTotal}");
        // --- 8. 生成星星 UI ---
        for (int i = 0; i < data.goalTotal; i++)
        {
            GameObject star = Instantiate(starPrefab, starParent);
            starPool.Add(star);
        }
        // --- 9. 更新星星状态，只有新完成的才播放 FillStar 动画 ---
        UIUtils.UpdateStarsUI(starParent.gameObject, previousGoalsFound, currentGoalsFound, data.goalTotal);
        Debug.Log($"[UI检查] 调用了 UpdateStarsUI，更新动画：{previousGoalsFound} → {currentGoalsFound}");
    }
        
    private void OnPreviewImageButtonClicked()
    {
        confirmPopup.SetActive(true);
    }

    private void ClosePopup()
    {
        confirmPopup.SetActive(false);
    }

    private void LoadScene()
    {
        confirmPopup.SetActive(false);
        SceneManager.LoadScene(currentSceneToLoad); // 注意你需要在 LevelDataAsset 中设置好 sceneToLoad
    }
}
