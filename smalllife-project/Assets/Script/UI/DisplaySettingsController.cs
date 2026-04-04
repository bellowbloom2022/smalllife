using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public partial class DisplaySettingsController : MonoBehaviour
{
    public Dropdown resolutionDropdown;
    public Dropdown modeDropdown;
    public Button resetButton;

    [Header("Overlay Color Settings")]
    public Image colorOverlayImage;

    public Toggle toggleWhite;
    public Toggle toggleBeige;
    public Toggle toggleGreen;
    public Toggle toggleBrown;

    [Header("Overlay Runtime Fallback")]
    [SerializeField] private string runtimeOverlayCanvasName = "__OverlayColorRuntimeCanvas";
    [SerializeField] private string runtimeOverlayImageName = "__OverlayColorRuntimeImage";
    [SerializeField] private int overlayFallbackSortingOrder = 50;

    private List<Vector2Int> commonResolutions = new List<Vector2Int>()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1728, 1117),
        new Vector2Int(1920, 1080), // 可选但非默认
        new Vector2Int(2336, 1460),
        new Vector2Int(2560, 1600),
        new Vector2Int(2992, 1870),
        new Vector2Int(3456, 2160),
    };
    private int defaultResolutionIndex = 0;
    private int defaultModeIndex = 0; // 0: fullscreen, 1: windowed

    private int lastAppliedResIndex = -1;
    private int lastAppliedModeIndex = -1;
    private bool colorListenersBound = false;
    private bool suppressOverlayToggleCallbacks = false;
    private Canvas runtimeOverlayCanvas;
    private Image runtimeOverlayImage;
    private Material runtimeOverlayMaterial;
    private static Sprite sharedRuntimeOverlaySprite;
    private static Texture2D sharedRuntimeOverlayTexture;

    void Awake()
    {
        resetButton.onClick.AddListener(ResetToDefault);
        PopulateResolutionDropdown();
        EnsureOverlayToggleListeners();
        EnsureRuntimeOverlayCanvas();
    }

    void OnEnable()
    {
        LoadSavedSettings();
        LoadSavedOverlayColor();
    }

    void PopulateResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < commonResolutions.Count; i++)
        {
            var res = commonResolutions[i];
            string option = res.x + " x " + res.y;
            options.Add(option);

            if (res.x == Screen.currentResolution.width && res.y == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        // 保存默认分辨率索引
        defaultResolutionIndex = currentResIndex;

        resolutionDropdown.onValueChanged.AddListener(_ => ApplyDisplaySettings());
        modeDropdown.onValueChanged.AddListener(_ => ApplyDisplaySettings());
    }

    public void ApplyDisplaySettings()
    {
        int resIndex = resolutionDropdown.value;
        int modeIndex = modeDropdown.value;

        if (resIndex == lastAppliedResIndex && modeIndex == lastAppliedModeIndex)
        {
            UpdateResolutionUIState();
            return;
        }

        FullScreenMode screenMode = (modeIndex == 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Vector2Int selected;

        // ? 全屏模式 → 使用屏幕原生分辨率
        if (screenMode == FullScreenMode.FullScreenWindow)
        {
            selected = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
        }
        else
        {
            selected = commonResolutions[resIndex];
        }

        bool resolutionOrModeChanged =
            Screen.width != selected.x ||
            Screen.height != selected.y ||
            Screen.fullScreenMode != screenMode;

        if (resolutionOrModeChanged)
        {
            Screen.SetResolution(selected.x, selected.y, screenMode);
        }

        lastAppliedResIndex = resIndex;
        lastAppliedModeIndex = modeIndex;

        // UI状态刷新
        UpdateResolutionUIState();

        // 保存设置
        bool settingsChanged = SaveSystem.GameData.settings.resolutionIndex != resIndex ||
                               SaveSystem.GameData.settings.displayModeIndex != modeIndex;

        SaveSystem.GameData.settings.resolutionIndex = resIndex;
        SaveSystem.GameData.settings.displayModeIndex = modeIndex;

        if (settingsChanged)
        {
            SaveSystem.SaveGame();
        }
    }

    void UpdateResolutionUIState()
    {
        bool isFullscreen = (modeDropdown.value == 0);

        resolutionDropdown.interactable = !isFullscreen;

        if (resolutionDropdown.captionText != null)
        {
            resolutionDropdown.captionText.color = isFullscreen
                ? new Color(0.8f, 0.8f, 0.8f) // 灰色
                : Color.black;
        }
    }

    public void LoadSavedSettings()
    {
        int savedRes = SaveSystem.GameData.settings.resolutionIndex;
        int savedMode = SaveSystem.GameData.settings.displayModeIndex;

        resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(savedRes, 0, commonResolutions.Count - 1));
        modeDropdown.SetValueWithoutNotify(savedMode);

        resolutionDropdown.RefreshShownValue();
        modeDropdown.RefreshShownValue();

        ApplyDisplaySettings();
        UpdateResolutionUIState(); // 确保初始化时UI也变灰
    }

    public void ResetToDefault()
    {
        AudioHub.Instance?.PlayGlobal("back_confirm");

        resolutionDropdown.SetValueWithoutNotify(defaultResolutionIndex);
        modeDropdown.SetValueWithoutNotify(defaultModeIndex);

        resolutionDropdown.RefreshShownValue();
        modeDropdown.RefreshShownValue();

        toggleWhite.isOn = true;
        SetOverlayColor(toggleWhite);

        ApplyDisplaySettings();
        AudioHub.Instance?.PlayGlobal("back_confirm");
    }
}
