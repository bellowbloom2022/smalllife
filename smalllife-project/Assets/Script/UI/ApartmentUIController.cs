using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ApartmentUIController : MonoBehaviour
{
    public static ApartmentUIController Instance { get; private set; }

    [Header("Sidebar UI")]
    [SerializeField] private Transform sidebarParent;       // ScrollView Content
    [SerializeField] private GameObject sidebarItemPrefab;  // 里头挂 DraggableGoalUI
    private List<DraggableGoalUI> sidebarItems = new();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>根据当前关卡进度，刷新 Sidebar</summary>
    public void RefreshSidebar(List<ApartmentController.GoalMeta> knownGoals)
    {
        ClearSidebar();

        var data = SaveSystem.GameData;
        if (data == null) return;

        int levelIndex = data.currentLevel;

        // step1 完成 && step2 未完成
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
            SpawnSidebarForGoal(gid, knownGoals);
        }
    }

    private void SpawnSidebarForGoal(int goalID, List<ApartmentController.GoalMeta> knownGoals)
    {
        var meta = knownGoals.Find(m => m.goalID == goalID);
        if (meta == null) return;

        GameObject go = Instantiate(sidebarItemPrefab, sidebarParent);
        var ui = go.GetComponent<DraggableGoalUI>();
        if (ui != null)
        {
            bool alreadyPlaced = ApartmentDataManager.Instance
                .GetAllPlacedItems()
                .Any(p => p.goalID == goalID);

            bool isNew = !alreadyPlaced
                         && !ApartmentDataManager.Instance.HasEverAppearedInSidebar(goalID);

            ui.SetData(goalID, meta.displayKey, meta.icon, ApartmentController.Instance, isNew);
            ui.BindController(ApartmentController.Instance);

            if (alreadyPlaced) ui.SetInteractable(false);

            sidebarItems.Add(ui);

            if (isNew)
            {
                BagButtonController.Instance?.SetHighlight(true);
            }
        }
    }

    public void SetSidebarInteractable(int goalID, bool value)
    {
        var ui = sidebarItems.FirstOrDefault(x => x != null && x.goalID == goalID);
        if (ui != null) ui.SetInteractable(value);
    }

    public void NotifyItemUsed(int goalID)
    {
        var ui = sidebarItems.FirstOrDefault(x => x.goalID == goalID);
        if (ui != null) ui.SetNewItem(false);

        ApartmentDataManager.Instance.MarkGoalRemoved(goalID);

        bool hasNew = sidebarItems.Any(x => x.IsNewItem);
        BagButtonController.Instance?.SetHighlight(hasNew);
    }

    public void MarkGoalReturned(int goalID)
    {
        SetSidebarInteractable(goalID, true);
    }

    private void ClearSidebar()
    {
        foreach (var item in sidebarItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        sidebarItems.Clear();
    }
}
