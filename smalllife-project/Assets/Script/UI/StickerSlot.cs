using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 继承 BaseDiarySlot：负责显示灰/彩贴纸，并作为 Drop 目标
/// 使用 Setup(goalKey, iconSprite, graySprite, slotIndex)
/// </summary>
[RequireComponent(typeof(Image))]
public class StickerSlot : BaseDiarySlot, IDropHandler
{
    [Header("UI")]
    public Image slotImage;    // Inspector assign (也可通过 GetComponent<Image>())

    // 内容
    private Sprite iconSprite;
    private Sprite graySprite;
    public int slotIndex = -1;      // 对应这个槽位 index
    public string slotId; // 保留给存档用 "Slot0" / "Slot1"…
    public DiaryStickerPageController controller;
    private bool isPlaced = false;  // UI 内部状态
    public bool IsPlaced() => isPlaced; // 加一个 getter

    // static frame-level flag used by draggable to know if any slot accepted drop
    public static bool dropSuccessThisFrame = false; 

    public void Setup(string key, Sprite icon, Sprite gray, int uislotIndex)
    {
        Debug.Log($"[Setup Slot] key={key}, index={uislotIndex}");
        this.goalKey = key; 
        iconSprite = icon;
        graySprite = gray;
        slotIndex = uislotIndex;
        slotId = "Slot" + uislotIndex;// 保存给 DiaryStickerEntry 使用
        isPlaced = false;
        base.Setup(key);// 会调用 IsUnlocked(key) → unlocked
    }

    public void Clear()
    {
        goalKey = null;
        iconSprite = null;
        graySprite = null;
        isPlaced = false;
        slotIndex = -1;
        if (slotImage != null) slotImage.sprite = null;
    }

    // Called when restoring from save to force a placed state
    public void ForcePlaced()
    {
        isPlaced = true;
        unlocked = true;
        Debug.Log($"[StickerSlot] ForcePlaced called, goalKey={goalKey}");
        Refresh();
    }
    public override void ForceUnlock()
    {
        unlocked = true;
        Refresh();
    }

    protected override void Refresh()
    {
        if (slotImage == null) slotImage = GetComponent<Image>();
        if (slotImage == null) return;

        if (isPlaced)
        {
            // 已贴上 → 彩色
            slotImage.sprite = iconSprite;
        }
        else if (unlocked)
        {
            // 已解锁未贴 → 灰色
            slotImage.sprite = graySprite;
        }
        else
        {
            // 未解锁 → 灰色
            slotImage.sprite = graySprite;
        }
    }

    // Drop handling: accept StickerDraggable if goalKey matches
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[OnDrop] Triggered on slot {slotId}, goalKey={goalKey}");
        var drag = eventData.pointerDrag?.GetComponent<StickerDraggable>();
        if (drag == null)
        {
            Debug.Log("[OnDrop] drag=null !!");
            return;
        }

        string draggedGoalKey = drag.stickerId;   // 拖入的贴纸
        Debug.Log($"[OnDrop] drag.goalKey={draggedGoalKey}, slot.goalKey={goalKey}");

        // 比较 goalKey
        if (string.IsNullOrEmpty(goalKey))
        {
            Debug.LogWarning("[OnDrop] slot.goalKey is empty — make sure slot was Setup()");
        }

        if (draggedGoalKey == goalKey)
        {
            Debug.Log("[OnDrop] Goal match! Placing sticker");
            // UI 状态更新
            isPlaced = true;
            unlocked = true;
            Refresh();

            // 标记成功，避免draggable 回弹
            dropSuccessThisFrame = true;

            // 通知 Controller 保存 slotIndex
            var ctrl = FindObjectOfType<DiaryStickerPageController>();
            ctrl?.ApplySticker(draggedGoalKey, slotId);// slotId = "Slot" + slotIndex
            // 最后销毁 draggable（slot 已经处理显示）
            // 注意：先销毁 draggable，再保存，会保证 next frame draggable 不再阻挡事件
            if (drag.gameObject != null)
                Destroy(drag.gameObject);
        }
        else
        {
            Debug.Log("[OnDrop] GoalKey mismatch, ignoring");
        }
    }
    public void AnimatePlaced()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        transform.localScale = Vector3.one * 0.8f;

        cg.DOFade(1f, 0.4f);
        transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }
}
