using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisplaySettingsController : MonoBehaviour
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

    private Dictionary<Toggle, Color> colorMap;
    private const string KEY_COLOR_INDEX = "Display_ColorIndex";
    private List<Vector2Int> commonResolutions = new List<Vector2Int>()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080), // 可选但非默认
    };
    private int defaultResolutionIndex = 0;
    private int defaultModeIndex = 0; // 0: fullscreen, 1: windowed

    private const string PREF_RES_INDEX = "Display_ResIndex";
    private const string PREF_MODE_INDEX = "Display_ModeIndex";

    void Awake()
    {
        resetButton.onClick.AddListener(ResetToDefault);
        PopulateResolutionDropdown();

        colorMap = new Dictionary<Toggle, Color>
        {
            { toggleWhite, new Color(1f, 1f, 1f, 0f) },
            { toggleBeige, new Color(1f, 1f, 0.9f, 0.3f) },
            { toggleGreen, new Color(0.85f, 1f, 0.85f, 0.3f) },
            { toggleBrown, new Color(0.9f, 0.8f, 0.7f, 0.3f) }
        };
        // ?? toggle ??
        foreach (var kvp in colorMap)
        {
            kvp.Key.onValueChanged.AddListener(isOn =>
            {
                if (isOn) SetOverlayColor(kvp.Key);
            });
        }
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

        Screen.SetResolution(selected.x, selected.y, screenMode);

        // UI状态刷新
        UpdateResolutionUIState();

        // 保存设置
        SaveSystem.GameData.settings.resolutionIndex = resIndex;
        SaveSystem.GameData.settings.displayModeIndex = modeIndex;
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

        resolutionDropdown.value = Mathf.Clamp(savedRes, 0, commonResolutions.Count - 1);
        modeDropdown.value = savedMode;

        resolutionDropdown.RefreshShownValue();
        modeDropdown.RefreshShownValue();

        ApplyDisplaySettings();
        UpdateResolutionUIState(); // 确保初始化时UI也变灰
    }
    
    private void LoadSavedOverlayColor()
    {
        int savedIndex = SaveSystem.GameData.settings.overlayColorIndex;
        Toggle selectedToggle = GetToggleByIndex(savedIndex);
        if (selectedToggle != null)
        {
            selectedToggle.isOn = true;
            colorOverlayImage.color = colorMap[selectedToggle];
        }
    }
    
    public void SetOverlayColor(Toggle toggle)
    {
        if (!colorMap.ContainsKey(toggle)) return;

        Color selectedColor = colorMap[toggle];

        // UI Image  Unity Inspector 
        colorOverlayImage.color = selectedColor;

        // Shader  _Color 
        if (colorOverlayImage.material != null)
        {
            colorOverlayImage.material.color = selectedColor;
        }

        int index = GetToggleIndex(toggle);
        SaveSystem.GameData.settings.overlayColorIndex = index;
    }

    public void ResetToDefault()
    {
        AudioHub.Instance?.PlayGlobal("back_confirm");

        resolutionDropdown.value = defaultResolutionIndex;
        modeDropdown.value = defaultModeIndex;

        resolutionDropdown.RefreshShownValue();
        modeDropdown.RefreshShownValue();

        toggleWhite.isOn = true;
        SetOverlayColor(toggleWhite);

        ApplyDisplaySettings();
        AudioHub.Instance?.PlayGlobal("back_confirm");
    }
    
    private int GetToggleIndex(Toggle toggle)
    {
        if (toggle == toggleWhite) return 0;
        if (toggle == toggleBeige) return 1;
        if (toggle == toggleGreen) return 2;
        if (toggle == toggleBrown) return 3;
        return 0;
    }
    
    public Toggle GetToggleByIndex(int index)
    {
        switch (index)
        {
            case 0: return toggleWhite;
            case 1: return toggleBeige;
            case 2: return toggleGreen;
            case 3: return toggleBrown;
            default: return toggleWhite;
        }
    }
}
