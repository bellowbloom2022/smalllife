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
    public Transform starParent;       // ���ǵ������������壩
    public GameObject starPrefab;      // һ������ prefab��ͼ�꣩
    public GameObject confirmPopup;
    public Button previewImageButton;
    public Button buttonEnter;
    public Button buttonCancel;

    private List<GameObject> starPool = new List<GameObject>();
    public GameData gameData;  // �� Inspector �����뵱ǰ�� GameData ����

    private string currentSceneToLoad;

    private void Start(){
        previewImageButton.onClick.AddListener(OnPreviewImageButtonClicked);
        buttonCancel.onClick.AddListener(ClosePopup);
        buttonEnter.onClick.AddListener(LoadScene);
        confirmPopup.SetActive(false);
    }

    public void UpdateContent(LevelDataAsset data, int levelIndex)
    {
        // --- 1. ���ػ���������� ---
        titleLocalizedText.TranslationName = data.titleKey;
        descriptionLocalizedText.TranslationName = data.descriptionKey;
        // --- 2. ���ص�ǰԤ��ͼƬ ---
        previewImage.sprite = data.previewImage;
        // --- 3. ���õ�ǰ�����صĹؿ��� ---
        currentSceneToLoad = data.sceneToLoad;
        // --- 4. ȷ�� gameData ��Ϊ null������������ʾ��---
        if (gameData == null){
            gameData = SaveSystem.LoadGame();
            if (gameData == null){
                gameData = new GameData();// ����һ�������ݣ����� null
            }
        }
        

        // --- 5. ��������� ---
        foreach (var star in starPool)
        {
            Destroy(star);
        }
        starPool.Clear();

        // --- 6. ��ȡ��ǰ�ؿ������Ŀ���� ---
        int goalsFound = 0;
        if (gameData != null && gameData.goalsFound.ContainsKey(levelIndex)){
            goalsFound = gameData.goalsFound[levelIndex];
        }

        // --- 7. �����µ����ǣ�����Ŀ��������---
        for (int i = 0; i < data.goalTotal; i++)
        {
            GameObject star = Instantiate(starPrefab, starParent);
            Image starImage = star.GetComponent<Image>();
            starImage.color = i < goalsFound ? Color.yellow : Color.gray;
            starPool.Add(star);
        }
        Debug.Log($"��������: {data.levelID}, index: {levelIndex}, sceneToLoad: {data.sceneToLoad}");
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
        SceneManager.LoadScene(currentSceneToLoad); // ע������Ҫ�� LevelDataAsset �����ú� sceneToLoad
    }
}
