using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ApartmentDataManager: 专门负责公寓数据的加载、保存与同步。
/// - 存储 placedItems（场景中已放置的物品数据）
/// - 存储 appearedSidebarGoals（sidebar 中曾出现过的 goalID）
/// - 提供对外接口，避免在 ApartmentController 里直接操作 SaveSystem.GameData
/// </summary>
public class ApartmentDataManager : MonoBehaviour
{
    public static ApartmentDataManager Instance { get; private set; }

    // 本地缓存，随时与 GameData 保持一致
    private List<ApartmentController.PlacedItemData> placedItems = new();
    private HashSet<int> appearedSidebarGoals = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadFromSave();
    }

    #region === 初始化与存档同步 ===
    private void LoadFromSave()
    {
        var data = SaveSystem.GameData;
        if (data == null) return;

        // 已放置物品
        if (data.apartmentPlacedItems != null)
            placedItems = new List<ApartmentController.PlacedItemData>(data.apartmentPlacedItems);
        else
            placedItems = new();

        // 已出现过的 sidebar goals
        appearedSidebarGoals = new HashSet<int>(data.apartmentSidebarAppearedGoals ?? new List<int>());
    }

    private void SaveToGameData()
    {
        var data = SaveSystem.GameData;
        if (data == null) return;

        data.apartmentPlacedItems = new List<ApartmentController.PlacedItemData>(placedItems);
        data.apartmentSidebarAppearedGoals = appearedSidebarGoals.ToList();

        SaveSystem.SaveGame();
    }
    #endregion

    #region === 对外接口：PlacedItem ===
    public List<ApartmentController.PlacedItemData> GetAllPlacedItems()
    {
        return placedItems;
    }

    public void AddPlacedItem(ApartmentController.PlacedItemData item)
    {
        if (!placedItems.Any(p => p.id == item.id))
        {
            placedItems.Add(item);
            SaveToGameData();
        }
    }

    public void UpdatePlacedItem(ApartmentController.PlacedItemData item)
    {
        var existing = placedItems.FirstOrDefault(p => p.id == item.id);
        if (existing != null)
        {
            existing.zoneId = item.zoneId;
            existing.position = item.position;
            existing.rotation = item.rotation;
        }
        else
        {
            placedItems.Add(item);
        }
        SaveToGameData();
    }

    public void RemovePlacedItem(string persistId)
    {
        int removed = placedItems.RemoveAll(p => p.id == persistId);
        if (removed > 0) SaveToGameData();
    }

    public bool IsItemSaved(string persistId)
    {
        return placedItems.Any(p => p.id == persistId);
    }
    #endregion

    #region === 对外接口：Sidebar Goals ===
    public bool HasEverAppearedInSidebar(int goalID)
    {
        return appearedSidebarGoals.Contains(goalID);
    }

    public void MarkGoalAppeared(int goalID)
    {
        if (!appearedSidebarGoals.Contains(goalID))
        {
            appearedSidebarGoals.Add(goalID);
            SaveToGameData();
        }
    }

    public void MarkGoalRemoved(int goalID)
    {
        if (appearedSidebarGoals.Contains(goalID))
        {
            appearedSidebarGoals.Remove(goalID);
            SaveToGameData();
        }
    }

    public HashSet<int> GetAppearedGoals()
    {
        return appearedSidebarGoals;
    }
    #endregion
}
