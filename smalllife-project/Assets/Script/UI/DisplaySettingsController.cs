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

    private Resolution[] resolutions;
    private int defaultResolutionIndex = 0;
    private int defaultModeIndex = 1; // 0: windowed, 1: fullscreen

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
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
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

        Resolution selectedRes = resolutions[resIndex];
        FullScreenMode screenMode = (modeIndex == 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Screen.SetResolution(selectedRes.width, selectedRes.height, screenMode);

        // 保存设置
        SaveSystem.GameData.settings.resolutionIndex = resIndex;
        SaveSystem.GameData.settings.displayModeIndex = modeIndex;
    }

    public void LoadSavedSettings()
    {
        int savedRes = SaveSystem.GameData.settings.resolutionIndex;
        int savedMode = SaveSystem.GameData.settings.displayModeIndex;

        resolutionDropdown.value = Mathf.Clamp(savedRes, 0, resolutions.Length - 1);
        modeDropdown.value = savedMode;

        resolutionDropdown.RefreshShownValue();
        modeDropdown.RefreshShownValue();

        ApplyDisplaySettings();
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
