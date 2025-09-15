using UnityEngine;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    [Header("Buttons")]
    public Button resumeButton;
    public Button settingButton;
    public Button howToPlayButton;
    public Button apartmentButton;
    public Button backToMenuButton;
    public Button backToTitleButton;

    [Header("Panels")]
    public HowToPlayPanel howToPlayPanel;
    public SettingsPanel settingsPanel;

    [Header("Scene Changer")]
    public SceneChanger backToMenuChanger;
    public SceneChanger backToTitleChanger;

    void Start()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        settingButton.onClick.AddListener(OnSettingClicked);
        howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        backToTitleButton.onClick.AddListener(OnBackToTitleClicked);
    }

    private void OnResumeClicked()
    {
        SaveSystem.SaveGame(); 
        Hide();
    }

    private void OnSettingClicked()
    {
        settingsPanel.Show();
    }

    private void OnHowToPlayClicked()
    {
        howToPlayPanel.Show();
    }

    private void OnBackToMenuClicked()
    {
        Time.timeScale = 1f;  
        backToMenuChanger.OnConfirmClick(); 
    }

    private void OnBackToTitleClicked()
    {
        Time.timeScale = 1f;  
        backToTitleChanger.OnConfirmClick();
    }

    public override void Show()
    {
        base.Show();
        Time.timeScale = 0f; 
    }

    public override void Hide()
    {
        base.Hide();
        Time.timeScale = 1f;

        if (howToPlayPanel != null) howToPlayPanel.Hide();
        if (settingsPanel != null) settingsPanel.Hide();
    }
} 
