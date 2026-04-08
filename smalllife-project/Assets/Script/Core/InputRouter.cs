using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance;
    public static event Action<InputRouter> InstanceReady;

    public event Action<Vector3> OnDrag;   // 拖拽事件（delta）
    public event Action<Vector3> OnClick;  // 点击事件（屏幕位置）
    public static event Action OnBlankClick;
    public static event Action<int> OnBlankClickAnyButton;

    private Vector3 lastMousePos;
    private float clickCooldown = 0.2f;
    private float lastClickTime = -10f;

    private string dragMode = "right"; // "left" or "right"
    public bool InputLocked { get; private set; } = false;
    
    // 本次拖拽是否从 UI 区域开始；如果是，则不转发场景拖拽
    private bool dragStartedOverUI = false;
    [SerializeField] private bool enableInputLockDebugLogs = false;
    private readonly List<GraphicRaycaster> disabledUiRaycasters = new List<GraphicRaycaster>();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (enableInputLockDebugLogs)
                Debug.LogWarning($"[InputRouter] Duplicate instance detected. Keep={Instance.GetInstanceID()} Drop={GetInstanceID()} ({name})");

            Destroy(gameObject);
            return;
        }

        Instance = this;
        dragMode = PlayerPrefs.GetString("Control_DragMode", "right");
        DontDestroyOnLoad(gameObject);
        InstanceReady?.Invoke(this);

        if (enableInputLockDebugLogs)
            Debug.Log($"[InputRouter] Awake instance={GetInstanceID()} scene={gameObject.scene.name}");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (InputLocked)
        {
            // Prevent drag delta spikes right after unlock.
            lastMousePos = Input.mousePosition;
            dragStartedOverUI = false;
            return;
        }

        float now = Time.time;

        // === 拖拽检测 ===
        int dragBtn = dragMode == "left" ? 0 : 1;
        if (Input.GetMouseButtonDown(dragBtn))
        {
            lastMousePos = Input.mousePosition;
            dragStartedOverUI = UIBlockChecker.IsPointerOverUI() || BasePanel.IsPointerOverAnyShownPanel(Input.mousePosition);
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
        ProcessBlankClick(0, now);
        ProcessBlankClick(1, now);
    }

    private void ProcessBlankClick(int button, float now)
    {
        if (!Input.GetMouseButtonDown(button))
            return;

        if (now - lastClickTime < clickCooldown)
            return;

        lastClickTime = now;

        bool isOverUI = UIBlockChecker.IsPointerOverUI() || BasePanel.IsPointerOverAnyShownPanel(Input.mousePosition);
        if (isOverUI)
            return;

        if (button == 0)
        {
            OnBlankClick?.Invoke(); // 兼容旧逻辑：左键点击空白
            OnClick?.Invoke(Input.mousePosition);
        }

        OnBlankClickAnyButton?.Invoke(button);
    }

    public void SetDragMode(string mode)
    {
        if (string.IsNullOrEmpty(mode))
            return;

        if (dragMode == mode)
            return;

        dragMode = mode;
        PlayerPrefs.SetString("Control_DragMode", mode);
    }

    public void LockInput(string source = null)
    {
        SetInputLocked(true, source ?? "Unknown");
    }

    public void UnlockInput(string source = null)
    {
        SetInputLocked(false, source ?? "Unknown");
    }

    private void SetInputLocked(bool locked, string source)
    {
        if (InputLocked == locked)
        {
            if (enableInputLockDebugLogs)
                Debug.Log($"[InputRouter] {(locked ? "Lock" : "Unlock")} ignored (already state={InputLocked}) by {source}, instance={GetInstanceID()}");
            return;
        }

        InputLocked = locked;
        SetUiRaycastEnabled(!locked);

        if (enableInputLockDebugLogs)
            Debug.Log($"[InputRouter] {(locked ? "Locked" : "Unlocked")} by {source}, instance={GetInstanceID()}, frame={Time.frameCount}, time={Time.time:F3}");
    }

    private void SetUiRaycastEnabled(bool enabled)
    {
        if (!enabled)
        {
            disabledUiRaycasters.Clear();
            GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>(true);
            foreach (GraphicRaycaster raycaster in raycasters)
            {
                if (raycaster == null || !raycaster.enabled)
                    continue;

                raycaster.enabled = false;
                disabledUiRaycasters.Add(raycaster);
            }

            return;
        }

        foreach (GraphicRaycaster raycaster in disabledUiRaycasters)
        {
            if (raycaster != null)
                raycaster.enabled = true;
        }

        disabledUiRaycasters.Clear();
    }

    public string GetDragMode() => dragMode;
}
