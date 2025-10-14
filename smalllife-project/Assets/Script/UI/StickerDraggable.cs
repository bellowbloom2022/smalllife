using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 放在可拖拽贴纸 prefab 上（Image + CanvasGroup）
/// draggable.stickerId 应该等于 goalKey（"0_2"）
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class StickerDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string stickerId;    // 等于 goalKey，例如 "0_2"
    public int goalID; // 对应真实 GoalID
    public string levelID;

    RectTransform rect;
    CanvasGroup canvasGroup;
    Canvas rootCanvas;
    Transform originalParent;
    Vector2 originalAnchoredPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;
        rect.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform, eventData.position, rootCanvas.worldCamera, out pos);
        rect.localPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // if no slot accepted the drop this frame, return
        if (!StickerSlot.dropSuccessThisFrame)
        {
            rect.SetParent(originalParent, false);
            rect.anchoredPosition = originalAnchoredPos;
        }

        StickerSlot.dropSuccessThisFrame = false;
    }
}
