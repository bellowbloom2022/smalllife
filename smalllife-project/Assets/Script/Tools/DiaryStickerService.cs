using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例服务：管理日记贴纸书状态（贴纸放置/查询/同步 GameData）
/// </summary>
public class DiaryStickerService : MonoBehaviour
{
    public static DiaryStickerService Instance { get; private set; }

    private List<DiaryStickerEntry> diaryStickers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // 初始化
        diaryStickers = SaveSystem.GameData.diaryStickers ?? new List<DiaryStickerEntry>();
    }

    // ================== 查询 ==================
    public bool IsPlaced(string goalKey)
    {
        var entry = diaryStickers.Find(s => s.goalKey == goalKey);
        return entry != null && entry.slotIndex >= 0;
    }

    public DiaryStickerEntry GetEntry(string goalKey)
    {
        return diaryStickers.Find(s => s.goalKey == goalKey);
    }

    // ================== 放置贴纸 ==================
    public void PlaceSticker(string goalKey, string slotId)
    {
        if (string.IsNullOrEmpty(goalKey) || string.IsNullOrEmpty(slotId)) return;

        int index = ParseSlotId(slotId); // 将 Slot0/Slot1 → 0/1

        var entry = diaryStickers.Find(e => e.goalKey == goalKey);
        if (entry == null)
        {
            entry = new DiaryStickerEntry(goalKey, index);
            diaryStickers.Add(entry);
        }
        else
        {
            entry.slotIndex = index;
        }
        entry.animationPlayed = true;
        SaveSystem.SaveGame();
    }

    // ================== 移除贴纸（可选，备用接口） ==================
    public void RemoveSticker(string goalKey)
    {
        var entry = diaryStickers.Find(e => e.goalKey == goalKey);
        if (entry != null)
        {
            diaryStickers.Remove(entry);
            SaveSystem.GameData.diaryStickers = diaryStickers;
            SaveSystem.SaveGame();
        }
    }

    // ================== 辅助函数 ==================
    private int ParseSlotId(string slotId)
    {
        if (slotId.StartsWith("Slot") && int.TryParse(slotId.Substring(4), out int index))
            return index;
        return -1;
    }

    // ================== 全量同步（可在 LoadPage 时调用） ==================
    public void SyncFromGameData()
    {
        diaryStickers = SaveSystem.GameData.diaryStickers ?? new List<DiaryStickerEntry>();
    }
}
