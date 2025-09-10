using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;       

[RequireComponent(typeof(PlacedItem))]
public class PlacedItemDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PlacedItem placedItem;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;
    private Transform dragRoot;
    private PlacementArea currentPreviewArea;
    private ApartmentController controller => ApartmentController.Instance;

    private void Awake()
    {
        placedItem = GetComponent<PlacedItem>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[PlacedItemDraggable] OnBeginDrag alpha=0.7, name={gameObject.name}");

        startPosition = transform.position;
        startParent = transform.parent;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;

        // 释放当前区域（允许重新放置）
        placedItem.ReleaseFromArea();

        // 显示所有可用区域高亮
        foreach (var area in controller.areas)
            area.ShowHighlight(!area.isOccupied);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 跟随鼠标
        Vector3 pos = Camera.main.ScreenToWorldPoint(eventData.position);
        pos.z = 0; // 防止 Z 轴跑偏
        transform.position = pos;

        //transform.position = eventData.position;

        Debug.Log($"[PlacedItemDraggable] {gameObject.name} dragging at {pos}, alpha={canvasGroup.alpha}");

        // 计算最近可用区域（带 Preview）
        var nearest = controller.FindNearestFreeArea(pos, maxDistance: 1.5f);
        if (nearest != currentPreviewArea)
        {
            if (currentPreviewArea != null) currentPreviewArea.SetPreview(false);
            currentPreviewArea = nearest;
            if (currentPreviewArea != null) currentPreviewArea.SetPreview(true);
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
        }
        // 清理所有高亮/预览
        foreach (var area in controller.areas)
        {
            area.ShowHighlight(false);
            area.SetPreview(false);
        }
        // 结束时恢复到原始 parent
        if (startParent != null)
            transform.SetParent(startParent, true);

        currentPreviewArea = null;
        Debug.Log($"[PlacedItemDraggable] End Drag on: {gameObject.name}, parent restored={transform.parent.name}");
    }
}
