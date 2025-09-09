using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlacedItem))]
public class PlacedItemDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PlacedItem placedItem;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;
    private PlacementArea currentPreviewArea;

    private ApartmentController controller => ApartmentController.Instance;

    private void Awake()
    {
        placedItem = GetComponent<PlacedItem>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;

        // 释放当前区域（允许重新放置）
        placedItem.ReleaseFromArea();
        Debug.Log("[PlacedItemDraggable] Begin Drag on: " + gameObject.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(eventData.position);
        pos.z = 0; // 防止 Z 轴跑偏
        transform.position = pos;

        Debug.Log("[PlacedItemDraggable] Dragging: " + gameObject.name + " → " + pos);
        transform.position = eventData.position;

        // 高亮潜在区域
        var area = controller.GetAreaAtScreenPos(eventData.position);
        if (area != currentPreviewArea)
        {
            if (currentPreviewArea != null)
                currentPreviewArea.ShowHighlight(false);

            currentPreviewArea = area;

            if (currentPreviewArea != null && !currentPreviewArea.isOccupied)
                currentPreviewArea.ShowHighlight(true);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        bool placed = false;

        if (currentPreviewArea != null && !currentPreviewArea.isOccupied)
        {
            Debug.Log($"[PlacedItemDraggable] Drop on area {currentPreviewArea.zoneId}");
            placed = controller.TryPlaceAtArea(currentPreviewArea, placedItem);
        }
        else
        {
            Debug.Log($"[PlacedItemDraggable] No valid area, reverting");
        }

        if (!placed)
        {
            // 回到原位
            transform.position = startPosition;
            if (currentPreviewArea != null)
                currentPreviewArea.ShowHighlight(false);
        }

        currentPreviewArea = null;
        Debug.Log("[PlacedItemDraggable] End Drag on: " + gameObject.name);
    }
}
