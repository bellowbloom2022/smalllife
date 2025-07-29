using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsPanel : BasePanel
{
    [Header("Tab Panels")]
    public GameObject mainTab;
    public GameObject languageTab;
    public GameObject soundTab;
    public GameObject controlTab;
    public GameObject displayTab;

    [Header("Main Tab Buttons")]
    public Button backButton;
    public Button languageButton;
    public Button soundButton;
    public Button controlButton;
    public Button displayButton;

    public Button resetSoundButton;

    [Header("Sound Controls")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public DisplaySettingsController displayController;

    private Resolution[] availableResolutions;
    private int defaultResIndex;
    
    private Dictionary<string, GameObject> tabMap;
    private bool isInitialized = false;

    private GameSettings settings;

    [SerializeField] private BGMController bgmController;

    private void Start(){

        // 赋值到 UI
        masterSlider.value = SaveSystem.GameData.settings.masterVolume;
        musicSlider.value = SaveSystem.GameData.settings.musicVolume;
        sfxSlider.value = SaveSystem.GameData.settings.sfxVolume;

        // 应用到音频系统
        AudioListener.volume = SaveSystem.GameData.settings.masterVolume;
        FindObjectOfType<BGMController>()?.SetVolume(SaveSystem.GameData.settings.musicVolume);
        AudioHub.Instance.SetSFXVolume(SaveSystem.GameData.settings.sfxVolume);

        // 添加监听
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void Awake()
    {
        // 建立 tab 映射
        tabMap = new Dictionary<string, GameObject>
        {
            { "Main", mainTab },
            { "Language", languageTab },
            { "Sound", soundTab },
            { "Control", controlTab },
            { "Display", displayTab }
        };

        // 默认显示 Main 面板
        ShowTab("Main");

        // 给每个 tab 注册返回按钮（假设每个 tab 下都有一个叫 button_back 的按钮）
        RegisterBackButton(languageTab);
        RegisterBackButton(soundTab);
        RegisterBackButton(controlTab);
        RegisterBackButton(displayTab);
    }

    private void OnEnable()
    {
        settings = SaveSystem.GameData.settings;
        GameSettingsApplier.ApplyAll(settings, bgmController);

        if (!isInitialized)
        {
            backButton.onClick.AddListener(ShowMain);
            languageButton.onClick.AddListener(() => ShowTab("Language"));
            soundButton.onClick.AddListener(() => ShowTab("Sound"));
            controlButton.onClick.AddListener(() => ShowTab("Control"));
            displayButton.onClick.AddListener(() => ShowTab("Display"));
            resetSoundButton.onClick.AddListener(ResetSoundSettings);
            isInitialized = true;
        }
        // 重置主 tab（如需）
        ShowMain();
    }

    /// <summary>
    /// 显示指定 Tab（其余隐藏）
    /// </summary>
    public void ShowTab(string tabName)
    {
        foreach (var kvp in tabMap)
        {
            kvp.Value.SetActive(kvp.Key == tabName);
        }

        AudioHub.Instance.PlayGlobal("click_confirm");
    }

    /// <summary>
    /// 返回主面板（主菜单 Tab）
    /// </summary>
    public void ShowMain()
    {
        ShowTab("Main");
        AudioHub.Instance.PlayGlobal("back_confirm");
    }

    /// <summary>
    /// 注册子 tab 返回按钮事件
    /// </summary>
    private void RegisterBackButton(GameObject tab)
    {
        Button backButton = tab.GetComponentInChildren<Button>();
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowMain);
        }
        //SaveSystem.SaveGame();
    }

    // 可选：打开设置面板时重置到主菜单
    public override void Show()
    {
        base.Show();
        ShowTab("Main");
    }
    
    public override void Hide()
    {
        Debug.Log("[SettingsPanel] Hide called, saving settings...");
        SaveSystem.SaveGame(); 
        base.Hide();           
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        SaveSystem.GameData.settings.masterVolume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        FindObjectOfType<BGMController>()?.SetVolume(value);
        SaveSystem.GameData.settings.musicVolume = value;
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioHub.Instance.SetSFXVolume(value);
        SaveSystem.GameData.settings.sfxVolume = value;
    }

    private void ResetSoundSettings()
    {
        masterSlider.value = 1f;
        musicSlider.value = 1f;
        sfxSlider.value = 1f;

        OnMasterVolumeChanged(1f);
        OnMusicVolumeChanged(1f);
        OnSFXVolumeChanged(1f);

        AudioHub.Instance.PlayGlobal("click_confirm"); 
    }
}