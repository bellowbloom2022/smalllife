using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Lean.Localization;
/// <summary>
/// Apartment 场景控制器：生成 Sidebar（根据 step1 完成且 step2 未完成的 goals），统一处理放置/回收/存档恢复。
/// 只允许每个 goal 放置一个实例；放置成功后 Sidebar 灰掉；已放置实例可在不同 PlacementArea 之间移动；若拖到空白区域则回到 Sidebar（删除实例，清档）
/// </summary>
public class ApartmentController : MonoBehaviour
{
    [System.Serializable]
    public class GoalMeta
    {
        public int goalID;
        public string displayKey;     // LeanLocalization key
        public Sprite icon;           // Sidebar 显示图标
        public GameObject worldPrefab;// 放到场景的 Prefab（内含 PlacedItem）
    }

    [System.Serializable]
    public class PlacedItemData
    {
        public string id;             // 唯一实例 id（persistId）
        public int goalID;
        public string zoneId;         // 占用的放置区 ID（PlacementArea.zoneId）
        public Vector2 position;      // 兜底恢复坐标（优先按 zoneId 恢复）
        public float rotation;        // 暂不使用，兼容字段
    }

    public static ApartmentController Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Header("Goal 元数据")]
    public List<GoalMeta> knownGoals = new List<GoalMeta>();

    private void Start()
    {
        // 根据 step 状态往 Sidebar 填目标
        ApartmentUIController.Instance.RefreshSidebar(knownGoals);
        // 从 DataManager 拿数据并恢复
        var saved = ApartmentDataManager.Instance.GetAllPlacedItems();
        RestorePlacedItems(saved);
    }

    public PlacedItem SpawnPlacedItem(int goalID, string persistId, string zoneId = null,
        Vector3? position = null, Quaternion? rotation = null, bool isRestore = false)
    {
        var meta = knownGoals.FirstOrDefault(m => m.goalID == goalID);
        if (meta == null) return null;

        //实例化
        GameObject go = Instantiate(meta.worldPrefab, ApartmentDragHandler.Instance.PlacedItemsParent);
        //获取PlacedItem组件
        var placed = go.GetComponent<PlacedItem>();
        if (placed == null)
        {
            Debug.LogError($"WorldPrefab {meta.worldPrefab.name} 缺少 PlacedItem 组件");
            return null;
        }
        //初始化
        placed.Init(goalID, persistId);

        // 如果指定了 zoneId，优先直接绑定到对应区域
        if (!string.IsNullOrEmpty(zoneId))
        {
            var bindArea = ApartmentDragHandler.Instance.GetAllAreas()
                .FirstOrDefault(a => a.zoneId == zoneId);

            if (bindArea != null && !bindArea.isOccupied)
                placed.BindToArea(bindArea);
        }
        else if (position.HasValue) // 没有 zoneId，用位置/旋转还原
        {
            // 没有 zone → 直接放到给定坐标
            go.transform.position = position.Value;
            go.transform.rotation = rotation ?? Quaternion.identity;
        }

        if (!isRestore) // ✅ 新放置时才写入存档
        {
            var itemData = new PlacedItemData
            {
                id = placed.persistId,
                goalID = placed.goalID,
                zoneId = zoneId,
                position = go.transform.position,
                rotation = go.transform.eulerAngles.z
            };
            ApartmentDataManager.Instance.AddPlacedItem(itemData);
            ApartmentDataManager.Instance.MarkGoalAppeared(goalID);
        }

        // 灰掉 Sidebar（只能生成一次）
        ApartmentUIController.Instance.SetSidebarInteractable(goalID, false);

        return placed;
    }

    /// <summary>
    /// 获取最近生成的 PlacedItem 实例
    /// 支持同一个 goal 多实例，通过 persistId 匹配
    /// </summary>
    public PlacedItem GetLastPlacedItem(string persistId)
    {
        if (string.IsNullOrEmpty(persistId)) return null;

        // 从父物体下的所有 PlacedItem 中寻找匹配 persistId
        return ApartmentDragHandler.Instance.PlacedItemsParent
            .GetComponentsInChildren<PlacedItem>(true)
            .FirstOrDefault(p => p.persistId == persistId);
    }
    
    /// <summary>进入场景时，从存档恢复已放置的物品。</summary>
    public void RestorePlacedItems(IEnumerable<PlacedItemData> saved)
    {
        foreach (var d in saved)
        {
            // 用 SpawnPlacedItem 统一生成逻辑
            var placed = SpawnPlacedItem(
                d.goalID,
                d.id,              // persistId
                d.zoneId,          // zoneId
                new Vector3(d.position.x, d.position.y, 0f), // position
                Quaternion.Euler(0, 0, d.rotation),          // rotation (由 float 转 Quaternion)
                true               // isRestore
            );
            if (placed == null) continue;

            // 优先按 zoneId 还原
            PlacementArea bindArea = null;
            if (!string.IsNullOrEmpty(d.zoneId))
            {
                bindArea = ApartmentDragHandler.Instance.GetAllAreas()
                    .FirstOrDefault(a => a.zoneId == d.zoneId);
            }

            if (bindArea != null && !bindArea.isOccupied)
            {
                placed.BindToArea(bindArea);
            }

            // 恢复后把对应 Sidebar 项置灰（限制一件）
            ApartmentUIController.Instance.SetSidebarInteractable(d.goalID, false);
        }
    }
    public void UpdatePlacedItem(PlacedItem item)
    {
        if (item == null) return;

        var data = new PlacedItemData
        {
            id = item.persistId,
            goalID = item.goalID,
            zoneId = item.currentArea != null ? item.currentArea.zoneId : "",
            position = item.transform.position,
            rotation = item.transform.eulerAngles.z
        };
        ApartmentDataManager.Instance.UpdatePlacedItem(data);

        Debug.Log($"[ApartmentController] UpdatePlacedItem: persistId={item.persistId}, zoneId={data.zoneId}, pos={data.position}");
    }

    /// <summary>从场景回收到 Sidebar（PlacedItem.ReturnToSidebar() 会回调这里）。</summary>
    public void NotifyItemReturned(PlacedItem item)
    {
        if (item == null) return;

        // Sidebar 变回可拖
        ApartmentUIController.Instance.SetSidebarInteractable(item.goalID, true);

        // 移除存档项
        ApartmentDataManager.Instance.RemovePlacedItem(item.persistId);

        Debug.Log($"[ApartmentController] Item returned: id={item.persistId}, goal={item.goalID}. Sidebar re-enabled.");
    }

    public bool IsItemSaved(string itemId)
    {
        return ApartmentDataManager.Instance.IsItemSaved(itemId);
    }
}
