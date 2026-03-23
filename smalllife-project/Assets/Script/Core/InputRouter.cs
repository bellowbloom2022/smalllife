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
    
    // 本次拖拽是否从 UI 区域开始；如果是，则不转发场景拖拽
    private bool dragStartedOverUI = false;


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
        {
            lastMousePos = Input.mousePosition;
            dragStartedOverUI = UIBlockChecker.IsPointerOverUI();
        }

        if (Input.GetMouseButton(dragBtn))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            // 只有拖拽起点不在 UI 上，才允许驱动场景拖拽
            if (!dragStartedOverUI)
            OnDrag?.Invoke(delta);
        }
        
        if (Input.GetMouseButtonUp(dragBtn))
        {
            dragStartedOverUI = false;
        }

        // === 点击检测 ===
        if (Input.GetMouseButtonDown(0) && now - lastClickTime >= clickCooldown)
        {
            lastClickTime = now;

            // 点在 UI 上：不向场景派发点击，避免穿透
            bool isOverUI = UIBlockChecker.IsPointerOverUI();
            if (isOverUI)
                return;

            OnBlankClick?.Invoke(); // 点击空白区域（非 UI）
            OnClick?.Invoke(Input.mousePosition);
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
