using UnityEngine;
using System;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance;

    public event Action<Vector3> OnDrag;   // 拖拽事件（delta）
    public event Action<Vector3> OnClick;  // 点击事件（屏幕位置）
    public static event Action OnBlankClick;

    private Vector3 lastMousePos;
    private float clickCooldown = 0.2f;
    private float lastClickTime = -10f;

    private string dragMode = "right"; // "left" or "right"
    public bool InputLocked { get; private set; } = false;


    void Awake()
    {
        Instance = this;
        dragMode = PlayerPrefs.GetString("Control_DragMode", "right");
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (InputLocked)
            return;

        float now = Time.time;

        // === 拖拽检测 ===
        int dragBtn = dragMode == "left" ? 0 : 1;
        if (Input.GetMouseButtonDown(dragBtn))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(dragBtn))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;
            OnDrag?.Invoke(delta);
        }

        // === 点击检测 ===
        if (Input.GetMouseButtonDown(0) && now - lastClickTime >= clickCooldown)
        {
            lastClickTime = now;

            // 检查是否点在 UI 上
            bool isOverUI = UIBlockChecker.IsPointerOverUI();

            if (!isOverUI)
                OnBlankClick?.Invoke(); // 通知：点击空白区域

            OnClick?.Invoke(Input.mousePosition);

            //Debug.Log("click!");
        }
    }

    public void SetDragMode(string mode)
    {
        dragMode = mode;
        PlayerPrefs.SetString("Control_DragMode", mode);
    }

    public void LockInput() => InputLocked = true;
    public void UnlockInput() => InputLocked = false;
    public string GetDragMode() => dragMode;
}
