using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Localization;

public class MainMenuController : MonoBehaviour
{
    public Button newGameButton;
    public Button continueButton;
    public GameObject newGameConfirmPopup;
    public string firstLevelSceneName = "Level0";

    public List<LevelDataAsset> allLevelData;
    public LeanLocalizedText continueButtonTitleText;  // 显示关卡标题，如“静安公园”
    public Text continueButtonPrefixText;              // 可选：用于显示“上次：”
    public string lastPlayedPrefixKey = "LastPlayed"; // Lean Localization Key: 如“上次：”

    public RectTransform backgroundRect;  // 白底
    public float animDuration = 0.4f;
    public float backgroundMaxWidth = 300f;  // 或用 text.preferredWidth 来动态获取
    public Text continueButtonTitleRawText;

    void Start()
    {
        bool hasSavedGame = SaveSystem.GameData != null && SaveSystem.GameData.lastLevelIndex >= 0;

        continueButton.gameObject.SetActive(hasSavedGame);
        backgroundRect.gameObject.SetActive(hasSavedGame);

        SetupNewGameButton(hasSavedGame);
        SetupContinueButton(hasSavedGame);

        if (hasSavedGame)
        {
            UpdateContinueButtonSubText(SaveSystem.GameData.lastLevelIndex);
            PlayContinueButtonSubTextAnim();
        }
    }

    void SetupNewGameButton(bool hasSavedGame)
    {
        newGameButton.onClick.RemoveAllListeners();
        if (hasSavedGame)
            newGameButton.onClick.AddListener(() => ShowNewGameConfirmPopup());
        else
            newGameButton.onClick.AddListener(() => StartNewGame());
    }

    void SetupContinueButton(bool hasSavedGame)
    {
        continueButton.onClick.RemoveAllListeners();
        if (hasSavedGame)
            continueButton.onClick.AddListener(() => ContinueGame());
    }

    void UpdateContinueButtonSubText(int lastIndex)
    {
        if (lastIndex < 0 || lastIndex >= allLevelData.Count)
        {
            continueButtonPrefixText.text = "";
            continueButtonTitleText.TranslationName = "";
            return;
        }

        string localizedPrefix = LeanLocalization.GetTranslationText(lastPlayedPrefixKey); // e.g., “上次：”
        continueButtonPrefixText.text = localizedPrefix;

        // 设置 LeanLocalizedText 的 key，让它自动刷新显示
        continueButtonTitleText.TranslationName = allLevelData[lastIndex].titleKey;

    }

    void PlayContinueButtonSubTextAnim()
    {
        float delay = 0.2f;

        backgroundRect.localScale = new Vector3(0f, 1f, 1f);
        backgroundRect.DOScaleX(1f, animDuration)
            .SetEase(Ease.OutCubic)
            .SetDelay(delay);

        continueButtonPrefixText.transform.localScale = Vector3.zero;
        continueButtonTitleText.transform.localScale = Vector3.zero;

        continueButtonPrefixText.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack).SetDelay(0.3f);
        continueButtonTitleText.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack).SetDelay(0.35f);
    }

    void ShowNewGameConfirmPopup()
    {
        newGameConfirmPopup.SetActive(true);
    }

    public void ConfirmStartNewGame()
    {
        SaveSystem.ClearData();
        SceneManager.LoadScene("IntroScene");
    }

    public void CancelStartNewGame()
    {
        newGameConfirmPopup.SetActive(false);
    }

    void StartNewGame()
    {
        SceneManager.LoadScene("IntroScene");
    }

    void ContinueGame()
    {
        int levelToLoad = SaveSystem.GameData.lastLevelIndex;
        SceneManager.LoadScene("Level" + levelToLoad);
    }
}
