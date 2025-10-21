using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 放在可拖拽贴纸 prefab 上（Image + CanvasGroup）
/// 运行时：拖拽开始把对象挂到主 Canvas 顶层（SetAsLastSibling），使用主 Canvas 的坐标系进行位置计算。
/// 拖拽结束：如果没有被任何 slot 接受则回弹到原父物体位置。
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class StickerDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string stickerId;    // 等于 goalKey，例如 "0_2"
    public int goalID;          // 对应真实 GoalID（可选）
    public string levelID;      // 可选

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private Canvas mainCanvas; // 主 Canvas —— 名称为 "Canvas"
    public static bool debugLogs = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 尽量先通过场景查找主 Canvas（你告诉我名字叫 "Canvas"）
        var go = GameObject.Find("Canvas");
        if (go != null) mainCanvas = go.GetComponent<Canvas>();

        // 兜底：如果找不到，再用 GetComponentInParent
        if (mainCanvas == null)
            mainCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // save original parent & pos
        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;

        // Move into main Canvas and bring to top so it's never occluded
        if (mainCanvas != null)
        {
            rect.SetParent(mainCanvas.transform, true);
            rect.SetAsLastSibling();
        }

        // allow raycasts to hit slots under the draggable
        canvasGroup.blocksRaycasts = false;

        if (debugLogs) Debug.Log($"[Draggable] BeginDrag {stickerId} -> parent={rect.parent.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // convert screen point to mainCanvas local UI position (works with CanvasScaler)
        if (mainCanvas == null)
        {
            // fallback to basic behavior
            rect.position = Input.mousePosition;
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
            out Vector2 localPoint
        );

        // anchoredPosition is the correct one for UGUI coordinate
        rect.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // restore raycast so UI picks it up again if necessary (but usually we destroy on success)
        canvasGroup.blocksRaycasts = true;

        // if no slot accepted the drop this frame, return to original position
        if (!StickerSlot.dropSuccessThisFrame)
        {
            rect.SetParent(originalParent, false);
            rect.anchoredPosition = originalAnchoredPos;
            if (debugLogs) Debug.Log($"[Draggable] EndDrag {stickerId} -> return to original parent");
        }
        else
        {
            if (debugLogs) Debug.Log($"[Draggable] EndDrag {stickerId} -> accepted by a slot");
            // 被 slot 接受后通常 slot 会 Destroy(this.gameObject)
            // 但若 slot 没有 Destroy，我们在这里也尝试将对象销毁以避免残留
            // 注意：不要在这里盲目 Destroy，因为 slot.OnDrop 已经 Destroy 了 draggable。
        }

        // reset frame flag
        StickerSlot.dropSuccessThisFrame = false;
    }
}
