using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lean.Localization;

/// <summary>
/// DraggableGoalUI: template 在侧栏，拖拽时在场景里生成 PlacedItem（或拾取已有）
/// </summary>
public class DraggableGoalUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image icon;
    public Text label;
    [SerializeField] private GameObject highlightNew;

    [HideInInspector] public bool interactable = true; // 是否可拖拽
    private bool isNewItem = false;
    private ApartmentController controller;
    [HideInInspector] public int goalID;

    private RectTransform rect;
    private Canvas canvas;

    private PlacedItem draggingItem; // 正在拖拽的世界物件实例
    private Vector3 originalPosition;     // 若放置失败，回退用
    private PlacementArea currentPreviewArea; // 当前吸附的区域
    private Transform dragRoot;          // 拖拽 UI 的全局根节点
    private Image dragPreviewUI;         // 临时预览 UI

    public bool IsNewItemActive => isNewItem;


    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        // 自动寻找 DragRoot
        if (dragRoot == null)
        {
            var dragRootObj = GameObject.Find("DragRoot");
            if (dragRootObj != null)
            {
                dragRoot = dragRootObj.transform;
            }
            else
            {
                Debug.LogError("[DraggableGoalUI] 场景里缺少 DragRoot 节点！");
            }
        }
    }

    // isTemplate: true 表示这是侧栏里保留的 template（不会被移动/销毁）
    public void SetData(int id, string displayKey, Sprite sprite, ApartmentController ctrl, bool isNew = false)
    {
        goalID = id;
        controller = ctrl;

        if (icon != null) icon.sprite = sprite;
        if (label != null) label.text = displayKey;

        SetNewItem(isNew);
    }
    public void SetNewItem(bool val)
    {
        isNewItem = val;
        if (highlightNew != null) highlightNew.SetActive(val);
    }
    // 绑定 controller
    public void BindController(ApartmentController ctrl)
    {
        controller = ctrl;
    }

    public void SetInteractable(bool value)
    {
        interactable = value;

        // 改透明度
        if (icon != null)
        {
            var c = icon.color;
            c.a = value ? 1f : 0.4f;   // 不可交互时半透明
            icon.color = c;
        }

        if (label != null)
        {
            var c = label.color;
            c.a = value ? 1f : 0.4f;
            label.color = c;
        }

        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = value;
            cg.interactable = value;
            cg.alpha = value ? 1f : 0.4f;  // 整体半透明
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (controller == null || !interactable) return;
        // 如果是新获得 → 第一次拖拽就清除提示
        if (isNewItem)
        {
            SetNewItem(false);
            controller.NotifyItemUsed(goalID); // 通知控制器
        }
        // 在 DragRoot 下创建一个临时 UI Image
        GameObject previewGO = new GameObject("DragPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        previewGO.transform.SetParent(dragRoot, false);

        dragPreviewUI = previewGO.GetComponent<Image>();
        dragPreviewUI.sprite = icon.sprite;              // 使用 sidebar icon
        dragPreviewUI.raycastTarget = false;             // 避免挡住事件
        //dragPreviewUI.color = new Color(1, 1, 1, 0.7f);  // 半透明
        dragPreviewUI.rectTransform.sizeDelta = icon.rectTransform.sizeDelta;

        UpdatePreviewPosition(eventData);

        // 第一次拖拽时生成 PlacedItem
        if (draggingItem == null)
        {
            var meta = controller.knownGoals.Find(m => m.goalID == goalID);
            if (meta == null || meta.worldPrefab == null) return;

            draggingItem = controller.SpawnPlacedItem(goalID, Vector3.zero, null, 0f);
        }
        else
        {
            // --- 拖拽已放置的实例，先释放原区域 ---
            draggingItem.ReleaseFromArea();
        }

        // 显示所有放置区域高亮（只显示可放置）
        foreach (var area in controller.areas)
            area.ShowHighlight(!area.isOccupied);

        //InputRouter.Instance?.LockInput();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingItem == null) return;
        if (dragPreviewUI != null)
            UpdatePreviewPosition(eventData);
        Camera cam = Camera.main;
        if (cam == null) return;

        float z = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        draggingItem.transform.position = worldPos;

        // 计算最近可用区域并预览
        var nearest = controller.FindNearestFreeArea(worldPos, maxDistance: 1.5f);
        if (nearest != currentPreviewArea)
        {
            if (currentPreviewArea != null) currentPreviewArea.SetPreview(false);
            currentPreviewArea = nearest;
            if (currentPreviewArea != null) currentPreviewArea.SetPreview(true);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingItem == null)
        {
            //InputRouter.Instance?.UnlockInput();
            return;
        }
        if (dragPreviewUI != null)
        {
            Destroy(dragPreviewUI.gameObject); // 拖完销毁
            dragPreviewUI = null;
        }

        bool placed = false;

        // 优先使用当前 preview area（我们已经找到最近的）
        if (currentPreviewArea != null && !currentPreviewArea.isOccupied)
        {
            Debug.Log($"[DraggableGoalUI] 尝试放置到区域 {currentPreviewArea.zoneId}");
            placed = controller.TryPlaceAtArea(currentPreviewArea, draggingItem);
        }
        else
        {
            // 兜底：根据鼠标位置再尝试一次（可能用户放在区域边缘）
            Debug.Log("[DraggableGoalUI] 没有预览区域，尝试按屏幕坐标放置");
            placed = controller.TryPlaceAtScreenPosition(eventData.position, draggingItem);
        }

        //InputRouter.Instance?.UnlockInput();

        if (!placed)
        {
            // 放置失败 → 回到 Sidebar
            Debug.Log("[DraggableGoalUI] 放置失败，回到 Sidebar");
            draggingItem.ReturnToSidebar();
            AudioHub.Instance.PlayGlobal("cancel");
        }
        else
        {
            Debug.Log("[DraggableGoalUI] 放置成功，调用 SetInteractable(false)");
            SetInteractable(false);
            AudioHub.Instance.PlayGlobal("click_confirm");
        }

        // 清理所有高亮/预览
        foreach (var area in controller.areas)
        {
            area.ShowHighlight(false);
            area.SetPreview(false);
        }
        currentPreviewArea = null;
        draggingItem = null;
    }
    private void UpdatePreviewPosition(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragRoot as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );
        dragPreviewUI.rectTransform.anchoredPosition = pos;
    }
}
