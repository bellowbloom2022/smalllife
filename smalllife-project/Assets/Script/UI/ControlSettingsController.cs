using UnityEngine;
using UnityEngine.UI;

public class ControlSettingsController : MonoBehaviour
{
    [Header("Toggle References")]
    public Toggle leftDragToggle;
    public Toggle rightDragToggle;

    [Header("Buttons")]
    public Button resetButton;

    private const string KEY_DRAG_MODE = "Control_DragMode"; // 保存为 "left" 或 "right"
    private CameraController cameraController;
    private bool isInitialized = false;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        Initialize();
    }

    private void OnEnable()
    {
        // 添加监听
        if (!isInitialized)
        {
            leftDragToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn) SetDragMode("left");
            });

            rightDragToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn) SetDragMode("right");
            });

            resetButton.onClick.AddListener(ResetToDefault);
            isInitialized = true;
        }
        // 每次打开面板时刷新状态（防止热切换时丢失）
        LoadSettings();
    }

    private void Initialize()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        string mode = SaveSystem.GameData.settings.dragMode; 
        if (string.IsNullOrEmpty(mode)) mode = "right"; // 默认值保险

        // 初始化 Toggle 状态
        leftDragToggle.isOn = (mode == "left");
        rightDragToggle.isOn = (mode == "right");
    }

    private void SetDragMode(string mode)
    {
        SaveSystem.GameData.settings.dragMode = mode;
        Debug.Log($"[ControlSettings] current drag mode:{mode}");
        InputRouter.Instance?.SetDragMode(mode);

        AudioHub.Instance.PlayGlobal("click_confirm");
    }

    public void ResetToDefault()
    {
        SetDragMode("right");
        LoadSettings();
        AudioHub.Instance?.PlayGlobal("back_confirm");
    }
}
