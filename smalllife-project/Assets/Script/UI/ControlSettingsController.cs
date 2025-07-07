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
        string mode = PlayerPrefs.GetString(KEY_DRAG_MODE, "right"); // Ĭ���Ҽ�
        // ��ʼ�� Toggle ״̬
        leftDragToggle.isOn = (mode == "left");
        rightDragToggle.isOn = (mode == "right");
    }

    private void SetDragMode(string mode)
    {
        PlayerPrefs.SetString(KEY_DRAG_MODE, mode);
        Debug.Log($"[ControlSettings] current drag mode��{mode}");
        PlayerPrefs.Save();
        InputRouter.Instance?.SetDragMode(mode);

        AudioHub.Instance.PlayGlobal("click_confirm");
    }

    public void ResetToDefault()
    {
        // Ĭ�����Ҽ���ק
        leftDragToggle.isOn = false;
        rightDragToggle.isOn = true;
        SetDragMode("right");
        LoadSettings();
        AudioHub.Instance?.PlayGlobal("back_confirm");
    }
}
