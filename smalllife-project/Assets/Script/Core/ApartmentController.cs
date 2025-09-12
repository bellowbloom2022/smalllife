using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

/// <summary>
/// Apartment 场景控制器：
/// - 生成 Sidebar（根据 step1 完成且 step2 未完成的 goals）
/// - 统一处理放置/回收/存档恢复
/// - 只允许每个 goal 放置一个实例；放置成功后 Sidebar 灰掉；已放置实例可在不同 PlacementArea 之间移动；
///   若拖到空白区域则回到 Sidebar（删除实例，清档）
/// </summary>
public class ApartmentController : MonoBehaviour
{
    #region === 内嵌数据类型（与 SaveSystem 对齐） ===
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
    #endregion

    #region === 单例（给 PlacedItem.ReturnToSidebar() 用） ===
    public static ApartmentController Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    #region === Inspector 配置 ===
    
    [Header("Sidebar UI")]
    [SerializeField] private Transform sidebarParent;               // ScrollView Content
    [SerializeField] private GameObject sidebarItemPrefab;          // 里头挂 DraggableGoalUI
    [HideInInspector] public List<DraggableGoalUI> sidebarItems = new List<DraggableGoalUI>();

    [Header("放置区域")]
    [SerializeField] private Transform placedItemsParent;
    public List<PlacementArea> areas = new List<PlacementArea>();

    [Header("Goal 元数据")]
    public List<GoalMeta> knownGoals = new List<GoalMeta>();
    #endregion

    #region === 运行时存档镜像 ===
    // 跟 SaveSystem.GameData.apartmentPlacedItems 同步
    public List<PlacedItemData> placedItems = new List<PlacedItemData>();
    #endregion

    private void Start()
    {
        // 1) 根据 step 状态往 Sidebar 填目标
        ShowNewStep1GoalIfAny();

        // 2) 恢复已摆放的实例
        if (SaveSystem.GameData.apartmentPlacedItems != null)
        {
            placedItems = new List<PlacedItemData>(SaveSystem.GameData.apartmentPlacedItems);
            RestorePlacedItems(placedItems);
        }
        else
        {
            SaveSystem.GameData.apartmentPlacedItems = placedItems;
            SaveSystem.SaveGame();
        }
    }

    #region === Sidebar 生成 ===
    private void ShowNewStep1GoalIfAny()
    {
        var data = SaveSystem.GameData;
        if (data == null) return;

        int levelIndex = data.currentLevel;

        // 读取 step1 completed && !step2 completed 的 goals
        var goalIds = data.goalProgressMap
            .Select(kv => new { key = kv.Key, prog = kv.Value })
            .Select(x =>
            {
                var parts = x.key.Split('_');
                if (parts.Length < 2) return (lvl: -1, gid: -1, prog: x.prog);
                if (!int.TryParse(parts[0], out int lvl)) return (lvl: -1, gid: -1, prog: x.prog);
                if (!int.TryParse(parts[1], out int gid)) return (lvl: -1, gid: -1, prog: x.prog);
                return (lvl, gid, x.prog);
            })
            .Where(x => x.lvl == levelIndex && x.prog.step1Completed && !x.prog.step2Completed)
            .Select(x => x.gid)
            .Distinct()
            .ToList();

        foreach (int gid in goalIds)
        {
            SpawnSidebarForGoal(gid);
        }
    }

    private void SpawnSidebarForGoal(int goalID)
    {
        var meta = knownGoals.Find(m => m.goalID == goalID);
        if (meta == null)
        {
            Debug.LogWarning($"[ApartmentController] missing GoalMeta for id {goalID}");
            return;
        }

        GameObject go = Instantiate(sidebarItemPrefab, sidebarParent);
        var ui = go.GetComponent<DraggableGoalUI>();
        if (ui != null)
        {
            // 如果该 goal 已经有实例在场景，Sidebar 直接置灰
            bool alreadyPlaced = placedItems.Any(p => p.goalID == goalID);
            bool isNew = !alreadyPlaced && !HasEverAppearedInSidebar(goalID);
            // 让 UI 知道 controller + 数据
            ui.SetData(goalID, meta.displayKey, meta.icon, this, isNew);
            ui.BindController(this);

            if (alreadyPlaced) ui.SetInteractable(false);

            sidebarItems.Add(ui);
            if (isNew)
            {
                BagButtonController.Instance?.SetHighlight(true);
            }
        }
    }
    // 记录一下：这个 goal 是否曾经生成过 Sidebar
    private HashSet<int> appearedSidebarGoals = new HashSet<int>();
    private bool HasEverAppearedInSidebar(int goalID)
    {
        if (appearedSidebarGoals.Contains(goalID)) return true;
        appearedSidebarGoals.Add(goalID);
        return false;
    }
    // DraggableGoalUI 通知过来的回调
    public void NotifyItemUsed(int goalID)
    {
        // 如果 Sidebar 中没有任何高亮新物品，就关闭 bag 高亮
        if (!sidebarItems.Any(x => x.IsNewItemActive))
        {
            BagButtonController.Instance?.SetHighlight(false);
        }
    }
    public PlacedItem SpawnPlacedItem(int goalID, Vector3 position, string zoneId, float rotation)
    {
        var meta = knownGoals.Find(m => m.goalID == goalID);
        if (meta == null || meta.worldPrefab == null)
        {
            Debug.LogWarning($"[ApartmentController] SpawnPlacedItem: 找不到 goalID={goalID} 的 worldPrefab");
            return null;
        }

        GameObject go = Instantiate(meta.worldPrefab, placedItemsParent);
        var placed = go.GetComponent<PlacedItem>();
        placed.Init(goalID);

        // 如果指定了 zoneId，优先直接绑定到对应区域
        if (!string.IsNullOrEmpty(zoneId))
        {
            var area = areas.FirstOrDefault(a => a.zoneId == zoneId);
            if (area != null && !area.isOccupied)
                placed.BindToArea(area);
        }
        else
        {
            // 没有 zone → 直接放到给定坐标
            go.transform.position = position;
        }

        // 存档（只在第一次生成时写入）
        placedItems.Add(new PlacedItemData
        {
            id = placed.persistId,
            goalID = placed.goalID,
            zoneId = zoneId,
            position = go.transform.position,
            rotation = 0f
        });
        SaveSystem.GameData.apartmentPlacedItems = placedItems;
        SaveSystem.SaveGame();

        // 灰掉 Sidebar（只能生成一次）
        SetSidebarInteractable(goalID, false);

        return placed;
    }

    #endregion

    #region === 放置&查询 ===
    /// <summary>屏幕坐标 → 最近可用区域（无则返回 null）</summary>
    public PlacementArea GetAreaAtScreenPos(Vector2 screenPos, float maxDistance = 1.5f)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        world.z = 0f;
        return FindNearestFreeArea(world, maxDistance);
    }

    /// <summary>世界坐标 → 最近可用区域（无则返回 null）</summary>
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

    /// <summary>尝试放置到指定区域（只允许空区域）。只负责合法性检查 + Bind，不改存档、不改 Sidebar。</summary>
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

        // 保守释放（OnBeginDrag 已 release 过也没问题）
        placedItem.ReleaseFromArea();
        // 绑定到新区域
        placedItem.BindToArea(targetArea);

        return true;
    }

    /// <summary>
    /// 兜底：根据屏幕坐标找最近空闲区域，找不到就返回 false（不允许自由落地，按你的需求：回 Sidebar）
    /// </summary>
    public bool TryPlaceAtScreenPosition(Vector2 screenPos, PlacedItem placedItem, float maxDistance = 1.5f)
    {
        var area = GetAreaAtScreenPos(screenPos, maxDistance);
        if (area == null) return false;
        return TryPlaceAtArea(area, placedItem);
    }
    #endregion

    #region === 恢复 ===
    /// <summary>进入场景时，从存档恢复已放置的物品。</summary>
    public void RestorePlacedItems(IEnumerable<PlacedItemData> saved)
    {
        foreach (var d in saved)
        {
            // 用 SpawnPlacedItem 统一生成逻辑
            var placed = SpawnPlacedItem(d.goalID, new Vector3(d.position.x, d.position.y, 0f), d.zoneId, d.rotation);
            if (placed == null) continue;

            // 优先按 zoneId 还原
            PlacementArea bindArea = null;
            if (!string.IsNullOrEmpty(d.zoneId))
            {
                bindArea = areas.FirstOrDefault(a => a.zoneId == d.zoneId);
            }

            if (bindArea != null && !bindArea.isOccupied)
            {
                placed.BindToArea(bindArea);
            }

            // 恢复后把对应 Sidebar 项置灰（限制一件）
            SetSidebarInteractable(d.goalID, false);
        }
    }
    #endregion

    #region === Sidebar 交互状态与回收 ===
    /// <summary>设置某个 goal 的 Sidebar 项是否可拖拽。</summary>
    public void SetSidebarInteractable(int goalID, bool value)
    {
        var ui = sidebarItems.FirstOrDefault(x => x != null && x.goalID == goalID);
        if (ui != null) ui.SetInteractable(value);
    }

    /// <summary>从场景回收到 Sidebar（PlacedItem.ReturnToSidebar() 会回调这里）。</summary>
    public void NotifyItemReturned(PlacedItem item)
    {
        if (item == null) return;

        // Sidebar 变回可拖
        SetSidebarInteractable(item.goalID, true);

        // 移除存档项
        int removed = placedItems.RemoveAll(d => d.id == item.persistId);
        if (removed > 0)
        {
            SaveSystem.GameData.apartmentPlacedItems = placedItems;
            SaveSystem.SaveGame();
        }

        Debug.Log($"[ApartmentController] Item returned: id={item.persistId}, goal={item.goalID}. Sidebar re-enabled.");
    }

    /// <summary>如果你还有旧的 ReturnToSidebar(ApartmentController ctrl) 流程，可调用这个来只做 UI 变亮。</summary>
    public void MarkGoalReturned(int goalID)
    {
        SetSidebarInteractable(goalID, true);
    }
    #endregion

    #region === 工具方法（调试/查询） ===
    public bool IsItemSaved(string itemId)
    {
        return placedItems.Any(d => d.id == itemId);
    }
    #endregion
}
