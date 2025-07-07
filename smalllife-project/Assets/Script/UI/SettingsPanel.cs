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
    public Text masterLabel;
    public Slider musicSlider;
    public Text musicLabel;
    public Slider sfxSlider;
    public Text sfxLabel;

    public DisplaySettingsController displayController;

    private Resolution[] availableResolutions;
    private int defaultResIndex;
    
    private Dictionary<string, GameObject> tabMap;
    private bool isInitialized = false;

    private void Start(){
        // 加载之前保存的设置
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 赋值到 UI
        masterSlider.value = masterVol;
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        // 应用到音频系统
        AudioListener.volume = masterVol;
        FindObjectOfType<BGMController>()?.SetVolume(musicVol);
        AudioHub.Instance.SetSFXVolume(sfxVol);

        // 添加监听
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        UpdateSoundLabels();

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
    }

    // 可选：打开设置面板时重置到主菜单
    public override void Show()
    {
        base.Show();
        ShowTab("Main");
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        UpdateSoundLabels();
    }

    private void OnMusicVolumeChanged(float value)
    {
        FindObjectOfType<BGMController>()?.SetVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        UpdateSoundLabels();
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioHub.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        UpdateSoundLabels();
    }

    private void UpdateSoundLabels()
    {
        masterLabel.text = Mathf.RoundToInt(masterSlider.value * 100) + "%";
        musicLabel.text = Mathf.RoundToInt(musicSlider.value * 100) + "%";
        sfxLabel.text = Mathf.RoundToInt(sfxSlider.value * 100) + "%";
    }

    private void ResetSoundSettings()
    {
        masterSlider.value = 1f;
        musicSlider.value = 1f;
        sfxSlider.value = 1f;

        OnMasterVolumeChanged(1f);
        OnMusicVolumeChanged(1f);
        OnSFXVolumeChanged(1f);

        PlayerPrefs.Save();

        AudioHub.Instance.PlayGlobal("click_confirm"); // 可选：播放音效
    }
}