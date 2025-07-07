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
        // --- 4. 确保 gameData 不为 null（用于星星显示）---
        if (gameData == null){
            gameData = SaveSystem.LoadGame();
            if (gameData == null){
                gameData = new GameData();// 创建一个空数据，避免 null
            }
        }
        

        // --- 5. 清除旧星星 ---
        foreach (var star in starPool)
        {
            Destroy(star);
        }
        starPool.Clear();

        // --- 6. 获取当前关卡已完成目标数 ---
        int goalsFound = 0;
        if (gameData != null && gameData.goalsFound.ContainsKey(levelIndex)){
            goalsFound = gameData.goalsFound[levelIndex];
        }

        // --- 7. 生成新的星星（根据目标总数）---
        for (int i = 0; i < data.goalTotal; i++)
        {
            GameObject star = Instantiate(starPrefab, starParent);
            Image starImage = star.GetComponent<Image>();
            starImage.color = i < goalsFound ? Color.yellow : Color.gray;
            starPool.Add(star);
        }
        Debug.Log($"更新内容: {data.levelID}, index: {levelIndex}, sceneToLoad: {data.sceneToLoad}");
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
