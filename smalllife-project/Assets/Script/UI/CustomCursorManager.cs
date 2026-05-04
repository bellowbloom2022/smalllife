using UnityEngine;

/// <summary>
/// 全局自定义鼠标光标管理器。
/// 支持 normal / click / drag 三种状态，使用硬件光标 + DPI 自适应缩放。
/// 
/// 状态优先级：drag > click > normal
/// - normal: 默认箭头
/// - click: 按下鼠标时
/// - drag: 拖拽画布时（由 InputRouter.OnDrag 触发）
/// </summary>
public class CustomCursorManager : MonoBehaviour
{
    [Header("光标图片（建议 128×128，描边 3~4px）")]
    [Tooltip("默认状态（normal）的光标图片")]
    public Texture2D cursorNormal;
    
    [Tooltip("点击状态（click）的光标图片")]
    public Texture2D cursorClick;

    [Tooltip("拖拽状态（drag）的光标图片")]
    public Texture2D cursorDrag;
    
    [Header("热点偏移")]
    [Tooltip("光标点击点相对于图片左上角的偏移，(0,0)=左上角")]
    public Vector2 hotspot = Vector2.zero;

    [Header("DPI 适配")]
    [Tooltip("光标的逻辑大小（pt），标准系统光标约 32pt")]
    public int cursorLogicalSize = 32;

    // 缓存当前状态
    private bool isClicking = false;
    private bool isDragging = false;
    private bool initialized = false;

    // 拖拽判定：累计移动距离，超过阈值才视为拖拽（区分 click 和 drag）
    private float dragAccumulatedDist = 0f;
    private const float DRAG_THRESHOLD = 4f; // 像素，超过此距离才算拖拽

    // 缓存缩放后的纹理
    private Texture2D scaledNormal;
    private Texture2D scaledClick;
    private Texture2D scaledDrag;
    private Vector2 scaledHotspot;

    /// <summary>
    /// 根据当前屏幕计算光标纹理的目标像素尺寸。
    /// macOS: 通过物理分辨率判断 Retina → 固定 2x，不用 Screen.dpi（它返回物理PPI）
    /// Windows: 用 Screen.dpi / 96 计算缩放比
    /// </summary>
    private int GetScaledSize()
    {
        bool isMacOS = Application.platform == RuntimePlatform.OSXPlayer
                     || Application.platform == RuntimePlatform.OSXEditor;
        float scaleFactor;

        if (isMacOS)
        {
            int screenWidth = Screen.currentResolution.width;
            scaleFactor = screenWidth >= 3000 ? 2f : 1f;
        }
        else
        {
            float dpi = Screen.dpi;
            if (dpi <= 0)
            {
                int screenWidth = Screen.currentResolution.width;
                if (screenWidth >= 2400)
                    scaleFactor = 1.5f;
                else if (screenWidth >= 1600)
                    scaleFactor = 1.25f;
                else
                    scaleFactor = 1f;
            }
            else
            {
                scaleFactor = dpi / 96f;
            }
        }

        int size = Mathf.RoundToInt(cursorLogicalSize * scaleFactor);

        // macOS 硬件光标绝对上限 64px，留 2px 余量
        int maxSize = isMacOS ? 62 : 256;
        return Mathf.Clamp(size, 16, maxSize);
    }

    private Texture2D ScaleTexture(Texture2D source, int targetSize)
    {
        if (source == null) return null;

        Color[] sourcePixels = source.GetPixels();
        int sourceW = source.width;
        int sourceH = source.height;

        Texture2D scaled = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
        scaled.filterMode = FilterMode.Bilinear;

        Color[] destPixels = new Color[targetSize * targetSize];

        for (int y = 0; y < targetSize; y++)
        {
            for (int x = 0; x < targetSize; x++)
            {
                float u = (float)x / targetSize * sourceW;
                float v = (float)y / targetSize * sourceH;

                int x0 = Mathf.FloorToInt(u);
                int y0 = Mathf.FloorToInt(v);
                int x1 = Mathf.Min(x0 + 1, sourceW - 1);
                int y1 = Mathf.Min(y0 + 1, sourceH - 1);

                float fx = u - x0;
                float fy = v - y0;

                Color c00 = sourcePixels[y0 * sourceW + x0];
                Color c10 = sourcePixels[y0 * sourceW + x1];
                Color c01 = sourcePixels[y1 * sourceW + x0];
                Color c11 = sourcePixels[y1 * sourceW + x1];

                Color top = Color.Lerp(c00, c10, fx);
                Color bottom = Color.Lerp(c01, c11, fx);
                destPixels[y * targetSize + x] = Color.Lerp(top, bottom, fy);
            }
        }

        scaled.SetPixels(destPixels);
        scaled.Apply();
        return scaled;
    }

    private Vector2 ScaleHotspot(Vector2 originalHotspot, Texture2D source, int targetSize)
    {
        float ratio = (float)targetSize / source.width;
        return new Vector2(Mathf.Round(originalHotspot.x * ratio), Mathf.Round(originalHotspot.y * ratio));
    }

    private void Start()
    {
        if (cursorNormal == null)
        {
            Debug.LogWarning("[CustomCursor] cursorNormal 未设置！请将 arrow-normal.png 拖入 Inspector。");
            return;
        }

        InitializeScaledCursors();
        SetNormalCursor();
        initialized = true;
        DontDestroyOnLoad(gameObject);

        // 订阅 InputRouter 的拖拽事件
        if (InputRouter.Instance != null)
        {
            InputRouter.Instance.OnDrag += OnDragDelta;
        }
    }

    private void OnEnable()
    {
        if (initialized)
            SetNormalCursor();

        // 重新订阅（InputRouter 可能在此之后才 Awake）
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnDrag += OnDragDelta;
    }

    private void OnDisable()
    {
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnDrag -= OnDragDelta;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDestroy()
    {
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnDrag -= OnDragDelta;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (scaledNormal != null) Destroy(scaledNormal);
        if (scaledClick != null) Destroy(scaledClick);
        if (scaledDrag != null) Destroy(scaledDrag);
    }

    private void InitializeScaledCursors()
    {
        int size = GetScaledSize();

        Debug.Log($"[CustomCursor] Platform={Application.platform}, DPI={Screen.dpi}, " +
                  $"缩放后光标尺寸={size}px, 逻辑大小={cursorLogicalSize}pt, " +
                  $"源图={cursorNormal.width}x{cursorNormal.height}");

        scaledNormal = ScaleTexture(cursorNormal, size);
        scaledHotspot = ScaleHotspot(hotspot, cursorNormal, size);

        if (cursorClick != null)
            scaledClick = ScaleTexture(cursorClick, size);

        if (cursorDrag != null)
            scaledDrag = ScaleTexture(cursorDrag, size);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && initialized)
            SetNormalCursor();
    }

    private void LateUpdate()
    {
        if (!initialized) return;

        int dragBtn = GetDragButton();

        // === 拖拽中：检测松开 → 回到 normal ===
        if (isDragging)
        {
            if (!Input.GetMouseButton(dragBtn))
            {
                isDragging = false;
                isClicking = false;
                dragAccumulatedDist = 0f;
                SetNormalCursor();
            }
            return;
        }

        // === 拖拽按钮按住时：累计移动距离，超过阈值切换为 drag ===
        if (Input.GetMouseButton(dragBtn))
        {
            if (dragAccumulatedDist >= DRAG_THRESHOLD && scaledDrag != null)
            {
                isDragging = true;
                isClicking = false;
                SetDragCursor();
                return;
            }
        }

        // === 拖拽按钮松开时：重置累计距离 ===
        if (Input.GetMouseButtonUp(dragBtn))
        {
            dragAccumulatedDist = 0f;
        }

        // === click 检测（左键按下时显示 click 光标） ===
        bool mouseDown = Input.GetMouseButton(0);

        if (mouseDown && !isClicking)
        {
            SetClickCursor();
            isClicking = true;
        }
        else if (!mouseDown && isClicking)
        {
            SetNormalCursor();
            isClicking = false;
        }
    }

    /// <summary>
    /// 获取当前拖拽按钮索引（0=左键, 1=右键）
    /// </summary>
    private int GetDragButton()
    {
        if (InputRouter.Instance != null)
            return InputRouter.Instance.GetDragMode() == "left" ? 0 : 1;
        return 1; // 默认右键拖拽
    }

    /// <summary>
    /// InputRouter.OnDrag 回调：累计移动距离，超过阈值后切换到 drag 光标。
    /// 这样在 left-drag 模式下，短按不会误触发 drag 光标。
    /// </summary>
    private void OnDragDelta(Vector3 delta)
    {
        if (!initialized) return;
        if (scaledDrag == null) return;

        // 累计移动距离
        dragAccumulatedDist += delta.magnitude;
    }

    private void SetNormalCursor()
    {
        Cursor.SetCursor(scaledNormal, scaledHotspot, CursorMode.Auto);
    }

    private void SetClickCursor()
    {
        if (scaledClick != null)
            Cursor.SetCursor(scaledClick, scaledHotspot, CursorMode.Auto);
        else
            SetNormalCursor();
    }

    private void SetDragCursor()
    {
        if (scaledDrag != null)
            Cursor.SetCursor(scaledDrag, scaledHotspot, CursorMode.Auto);
        else
            SetClickCursor();
    }
}
