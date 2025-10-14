using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public class DiaryStickerPageController : MonoBehaviour
{
    [Header("UI References")]
    public List<StickerSlot> leftSlots;      // 左页槽位（最多14）
    public List<SketchSlot> rightSlots;      // 右页速写（最多14）
    public List<DiaryTextSlot> diaryTextSlots;// 右页日记文本（最多14）

    [Header("Page Title")]
    public Text pageTitleText;

    [Header("Paging")]
    public Button prevButton;
    public Button nextButton;

    [Header("Data")]
    public LevelDataAsset[] allLevels;       // 全部 Spot / LevelData

    [Header("Sticker Library")]
    public Transform stickerLibraryParent;   // 右上父物体（UI）
    public GameObject stickerDraggablePrefab; // 可拖拽 prefab (Image + CanvasGroup + StickerDraggable)

    private int currentPage = 0;
    private int maxSlots => Mathf.Min(leftSlots.Count, 14);

    void Start()
    {
        prevButton.onClick.AddListener(() => LoadPage(currentPage - 1));
        nextButton.onClick.AddListener(() => LoadPage(currentPage + 1));
        LoadPage(0);
    }

    public void LoadPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= allLevels.Length) return;
        currentPage = pageIndex;
        var levelData = allLevels[currentPage];

        // title: localized name 
        pageTitleText.text = $"{LeanLocalization.GetTranslationText(levelData.titleKey)}";

        // fill slots according to levelData.goalTotal (1..14)
        int displayCount = Mathf.Min(levelData.goalIDs.Length, maxSlots);
        for (int i = 0; i < displayCount; i++)
        {
            string goalKey = $"{levelData.levelID}_{levelData.goalIDs[i]}";

            leftSlots[i].Setup(goalKey, levelData.goalIcons[i], levelData.graySprites[i], i);
            rightSlots[i].Setup(goalKey, levelData.sketchImages[i]);
            diaryTextSlots[i].Setup(goalKey, levelData.diaryKeys[i]);
        }
        // 清空多余 slot
        for (int i = displayCount; i < maxSlots; i++)
        {
            leftSlots[i].Clear();
            rightSlots[i].Clear();
            diaryTextSlots[i].Clear();
        }
        // 👉真正开始处理存档状态
        // restore already placed stickers for this level
        LoadPlacedStickersForLevel(levelData);
        // prepare sticker library (unlocked & not placed)
        RefreshStickerLibrary(levelData);

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < allLevels.Length - 1;
    }

    // generate draggable icons for unlocked & unplaced goals
    void RefreshStickerLibrary(LevelDataAsset levelData)
    {
        Debug.Log("📖 RefreshStickerLibrary CALLED!");
        if (stickerLibraryParent == null || stickerDraggablePrefab == null) return;

        // clear
        for (int i = stickerLibraryParent.childCount - 1; i >= 0; i--)
            Destroy(stickerLibraryParent.GetChild(i).gameObject);

        int count = Mathf.Min(levelData.goalTotal, maxSlots);
        for (int i = 0; i < count; i++)
        {
            string goalKey = $"{levelData.levelID}_{levelData.goalIDs[i]}";   // 使用真实 goalID

            bool unlocked = IsGoalUnlocked(levelData.levelID, levelData.goalIDs[i]);
            // ✅ 使用 slotIndex 判断已放置
            bool placed = DiaryStickerService.Instance.GetEntry(goalKey)?.slotIndex >= 0;
            Debug.Log($"[RefreshStickerLibrary] goal {goalKey}, unlocked={unlocked}, placed={placed}");

            if (unlocked && !placed)
            {
                CreateDraggableIcon(levelData, i, goalKey);
            }
        }
    }

    void CreateDraggableIcon(LevelDataAsset levelData, int i, string goalKey)
    {
        var go = Instantiate(stickerDraggablePrefab, stickerLibraryParent);
        var draggable = go.GetComponent<StickerDraggable>();
        if (draggable == null)
        {
            Debug.LogError("[DiarySticker] stickerDraggablePrefab needs StickerDraggable");
            Destroy(go);
            return;
        }

        draggable.stickerId = goalKey; 
        draggable.levelID = levelData.levelID;
        draggable.goalID = levelData.goalIDs[i]; 

        var img = go.GetComponent<UnityEngine.UI.Image>();
        // 图像必须用 slotIndex 访问 goalIcons 数组
        if (img != null && i < levelData.goalIcons.Length) img.sprite = levelData.goalIcons[i];
    }

    bool IsGoalUnlocked(string levelID, int goalID)
    {
        var gd = SaveSystem.GameData;
        if (gd == null) return false;
        return gd.IsGoalStep2Completed(levelID, goalID);
    }

    // restore placed stickers for this level
    void LoadPlacedStickersForLevel(LevelDataAsset levelData)
    {
        var gd = SaveSystem.GameData;
        if (gd == null || gd.diaryStickers == null || gd.diaryStickers.Count == 0) return;//无贴纸数据时直接结束，避免报错

        int displayCount = Mathf.Min(levelData.goalTotal, maxSlots);
        //var service = DiaryStickerService.Instance;

        for (int i = 0; i < displayCount; i++)
        {
            string goalKey = $"{levelData.levelID}_{levelData.goalIDs[i]}";
            var entry = gd.diaryStickers.Find(e => e.goalKey == goalKey);
            Debug.Log($"[LoadPlacedStickers] goalKey={goalKey}, entry={(entry != null ? "found" : "null")}");
            // ✅ 使用 slotIndex 判断是否已放置
            if (entry != null && entry.slotIndex >= 0)
            {
                Debug.Log($"[LoadPlacedStickers] slot {i}: leftSlot placed={leftSlots[i].IsPlaced()}");
                // 标记左页彩色贴纸
                leftSlots[i].Setup(goalKey,
                    i < levelData.goalIcons.Length ? levelData.goalIcons[i] : null,
                    i < levelData.graySprites.Length ? levelData.graySprites[i] : null,
                    i);
                // 右页：解锁速写和文本
                rightSlots[i].Setup(goalKey,
                    i < levelData.sketchImages.Length ? levelData.sketchImages[i] : null);
                diaryTextSlots[i].Setup(goalKey,
                    i < levelData.diaryKeys.Length ? levelData.diaryKeys[i] : null);
                if (!entry.animationPlayed)
                {
                    // Coroutine 顺序播放动画
                    StartCoroutine(PlayPlacedAnimation(i, entry, levelData));
                }
                else
                {
                    // 直接显示
                    leftSlots[i].ForcePlaced();
                    rightSlots[i].ForceUnlocked(
                        i < levelData.sketchImages.Length ? levelData.sketchImages[i] : null);
                    diaryTextSlots[i].ShowText();
                }
            }
        }
    }
    private IEnumerator PlayPlacedAnimation(int index, DiaryStickerEntry entry, LevelDataAsset levelData)
    {
        // 1️⃣ 左页贴纸动画
        leftSlots[index].ForcePlaced();
        yield return new WaitForSeconds(1f);

        // 2️⃣ 右页 sketch 动画
        rightSlots[index].ForceUnlocked(
            index < levelData.sketchImages.Length ? levelData.sketchImages[index] : null);
        yield return new WaitForSeconds(1f);

        // 3️⃣ 日记文字动画显示
        diaryTextSlots[index].ShowText();

        // 记录动画已播放，避免翻页重复
        entry.animationPlayed = true;
    }

    // 检查某个 goal 是否已经贴上
    private bool IsGoalPlaced(string goalKey)
    {
        return DiaryStickerService.Instance.IsPlaced(goalKey);
    }

    // 贴上一个贴纸
    public void ApplySticker(string goalKey, string slotId)
    {
        // 调用 Service 处理存档
        DiaryStickerService.Instance.PlaceSticker(goalKey, slotId);

        int goalIndex = GetGoalIndexFromKey(goalKey);
        if (goalIndex >= 0 && goalIndex < rightSlots.Count)
        {
            // 左页贴纸显示彩色
            leftSlots[goalIndex].ForcePlaced();

            // 右页 sketch 显示
            rightSlots[goalIndex].ForceUnlocked();

            // 日记文字显示
            diaryTextSlots[goalIndex].gameObject.SetActive(true);
        }
    }

    // 辅助：解析 goalKey 获取 goalIndex
    private int GetGoalIndexFromKey(string goalKey)
    {
        var parts = goalKey.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[1], out int index))
            return index;
        return -1;
    }
}
