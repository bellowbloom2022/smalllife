using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Sketch 显示槽：继承 BaseDiarySlot
/// Setup(key, sketchSprite)
/// 控制逻辑：
/// - unlocked 控制右页 sketch 是否显示
/// - isPlaced 控制左页贴纸动画显示（仅 SketchSlot 内部可选）
/// </summary>
public class SketchSlot : BaseDiarySlot
{
    [Header("UI")]
    public Image sketchImage;         // 贴图显示
    public GameObject lockMask;       // 锁定显示
    public CanvasGroup canvasGroup;   // 动画用

    private Sprite sketchSprite;
    private bool isPlaced = false;    // 内部标记，可用于动画或左页贴纸逻辑

    /// <summary>
    /// 初始化槽位
    /// </summary>
    public void Setup(string key, Sprite sketch)
    {
        sketchSprite = sketch;
        base.Setup(key);
    }

    /// <summary>
    /// 刷新 UI
    /// unlocked 控制 sketch 是否显示
    /// lockMask 控制锁定显示
    /// </summary>
    protected override void Refresh()
    {
        // lockMask 显示逻辑依然可以用 unlocked
        if (lockMask != null) lockMask.SetActive(!unlocked);

        // sketch 显示逻辑改为依赖 isPlaced
        if (sketchImage != null)
            sketchImage.sprite = isPlaced ? sketchSprite : null;
    }

    /// <summary>
    /// 强制解锁右页显示（贴纸贴入或翻页恢复）
    /// </summary>
    public void ForceUnlocked(Sprite s = null)
    {
        if (s != null) sketchSprite = s;
        isPlaced = true;
        unlocked = true;
        Refresh();
    }

    /// <summary>
    /// 内部标记贴纸已放置（可选，用于动画或左页贴纸逻辑）
    /// </summary>
    public void ForcePlaced()
    {
        isPlaced = true;
        // 可在这里触发左页动画或其他效果
    }

    /// <summary>
    /// 清空槽位，翻页时调用
    /// </summary>
    public void Clear()
    {
        sketchSprite = null;
        isPlaced = false;
        if (sketchImage != null) sketchImage.sprite = null;
        if (lockMask != null) lockMask.SetActive(true);
        unlocked = false;
        goalKey = null;
    }

    /// <summary>
    /// 动画显示（可选）
    /// </summary>
    public void AnimateUnlock(float duration = 0.5f)
    {
        CanvasGroup cg = canvasGroup;
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        transform.localScale = Vector3.one * 0.9f;

        cg.DOFade(1f, duration);
        transform.DOScale(1f, duration).SetEase(Ease.OutBack);
    }
}
