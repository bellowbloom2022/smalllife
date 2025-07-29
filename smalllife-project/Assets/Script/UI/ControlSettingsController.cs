using UnityEngine;
using UnityEngine.UI;

public class ControlSettingsController : MonoBehaviour
{
    [Header("Toggle References")]
    public Toggle leftDragToggle;
    public Toggle rightDragToggle;

    [Header("Buttons")]
    public Button resetButton;

    private const string KEY_DRAG_MODE = "Control_DragMode"; // ����Ϊ "left" �� "right"
    private CameraController cameraController;
    private bool isInitialized = false;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        Initialize();
    }

    private void OnEnable()
    {
        // ��Ӽ���
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
        // ÿ�δ����ʱˢ��״̬����ֹ���л�ʱ��ʧ��
        LoadSettings();
    }

    private void Initialize()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        string mode = SaveSystem.GameData.settings.dragMode; 
        if (string.IsNullOrEmpty(mode)) mode = "right"; // Ĭ��ֵ����

        // ��ʼ�� Toggle ״̬
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
