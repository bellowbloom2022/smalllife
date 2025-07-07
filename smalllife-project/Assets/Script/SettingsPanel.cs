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
        // ����֮ǰ���������
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // ��ֵ�� UI
        masterSlider.value = masterVol;
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        // Ӧ�õ���Ƶϵͳ
        AudioListener.volume = masterVol;
        FindObjectOfType<BGMController>()?.SetVolume(musicVol);
        AudioHub.Instance.SetSFXVolume(sfxVol);

        // ��Ӽ���
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        UpdateSoundLabels();

    }

    private void Awake()
    {
        // ���� tab ӳ��
        tabMap = new Dictionary<string, GameObject>
        {
            { "Main", mainTab },
            { "Language", languageTab },
            { "Sound", soundTab },
            { "Control", controlTab },
            { "Display", displayTab }
        };

        // Ĭ����ʾ Main ���
        ShowTab("Main");

        // ��ÿ�� tab ע�᷵�ذ�ť������ÿ�� tab �¶���һ���� button_back �İ�ť��
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
        // ������ tab�����裩
        ShowMain();
    }

    /// <summary>
    /// ��ʾָ�� Tab���������أ�
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
    /// ��������壨���˵� Tab��
    /// </summary>
    public void ShowMain()
    {
        ShowTab("Main");
        AudioHub.Instance.PlayGlobal("back_confirm");
    }

    /// <summary>
    /// ע���� tab ���ذ�ť�¼�
    /// </summary>
    private void RegisterBackButton(GameObject tab)
    {
        Button backButton = tab.GetComponentInChildren<Button>();
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowMain);
        }
    }

    // ��ѡ�����������ʱ���õ����˵�
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

        AudioHub.Instance.PlayGlobal("click_confirm"); // ��ѡ��������Ч
    }
}