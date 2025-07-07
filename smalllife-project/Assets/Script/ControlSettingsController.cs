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
        string mode = PlayerPrefs.GetString(KEY_DRAG_MODE, "right"); // 默认右键
        // 初始化 Toggle 状态
        leftDragToggle.isOn = (mode == "left");
        rightDragToggle.isOn = (mode == "right");
    }

    private void SetDragMode(string mode)
    {
        PlayerPrefs.SetString(KEY_DRAG_MODE, mode);
        Debug.Log($"[ControlSettings] current drag mode：{mode}");
        PlayerPrefs.Save();
        InputRouter.Instance?.SetDragMode(mode);

        AudioHub.Instance.PlayGlobal("click_confirm");
    }

    public void ResetToDefault()
    {
        // 默认是右键拖拽
        leftDragToggle.isOn = false;
        rightDragToggle.isOn = true;
        SetDragMode("right");
        LoadSettings();
        AudioHub.Instance?.PlayGlobal("back_confirm");
    }
}
