using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ApartmentDragHandler: 专门负责放置/拖拽逻辑（区域检测 + 放置合法性）
/// - 检查某个屏幕/世界坐标是否有可用放置区
/// - 尝试把物体放到区域，并更新存档
/// - 不负责 UI / 存档本身（调用 DataManager）
/// </summary>
public class ApartmentDragHandler : MonoBehaviour
{
    public static ApartmentDragHandler Instance { get; private set; }

    [Header("引用")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform placedItemsParent;
    [SerializeField] private PlacementArea[] areas;

    public IEnumerable<PlacementArea> GetAllAreas()
    {
        return areas;
    }

    public Transform PlacedItemsParent => placedItemsParent;

    private void Awake()
    {
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    #region === 区域检测 ===
    public PlacementArea GetAreaAtScreenPos(Vector2 screenPos, float maxDistance = 1.5f)
    {
        if (mainCamera == null) return null;

        Vector3 world = mainCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z))
        );
        world.z = 0f;
        return FindNearestFreeArea(world, maxDistance);
    }

    public PlacementArea FindNearestFreeArea(Vector3 worldPos, float maxDistance = 1.5f)
    {
        PlacementArea nearest = null;
        float min = float.MaxValue;

        foreach (var a in areas)
        {
            if (a == null || a.isOccupied) continue;
            float d = Vector3.Distance(a.transform.position, worldPos);
            if (d < min && d <= maxDistance)
            {
                min = d;
                nearest = a;
            }
        }
        return nearest;
    }
    #endregion

    #region === 放置尝试 ===
    public bool TryPlaceAtArea(PlacementArea targetArea, PlacedItem placedItem)
    {
        if (targetArea == null || placedItem == null) 
        {
            Debug.LogWarning("[TryPlaceAtArea] 参数为空");
            return false;
        }
        
        if (targetArea.isOccupied)
        {
            Debug.LogWarning($"[TryPlaceAtArea] 区域 {targetArea.zoneId} 已被占用");
            return false;
        }

        Debug.Log($"[TryPlaceAtArea] 开始放置 goalID={placedItem.goalID} 到 area={targetArea.zoneId}");

        placedItem.ReleaseFromArea();      // 释放原绑定
        placedItem.BindToArea(targetArea); // 绑定新区域

        // 🔥 更新存档
        var data = new ApartmentController.PlacedItemData
        {
            id = placedItem.persistId,
            goalID = placedItem.goalID,
            zoneId = targetArea.zoneId,
            position = placedItem.transform.position,
            rotation = placedItem.transform.rotation.eulerAngles.z
        };
        ApartmentDataManager.Instance.UpdatePlacedItem(data);

        return true;
    }

    public bool TryPlaceAtScreenPosition(Vector2 screenPos, PlacedItem placedItem, float maxDistance = 1.5f)
    {
        var area = GetAreaAtScreenPos(screenPos, maxDistance);
        if (area == null) return false;
        return TryPlaceAtArea(area, placedItem);
    }
    #endregion

    #region === 提供给外部的初始化 ===
    public void Init(PlacementArea[] placementAreas, Transform placedParent, Camera cam = null)
    {
        areas = placementAreas;
        placedItemsParent = placedParent;
        if (cam != null) mainCamera = cam;
    }
    #endregion
}
