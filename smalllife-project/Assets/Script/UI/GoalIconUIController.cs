using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// 挂在 goal1_get UI 根节点上。
/// 负责：右上角角标（2 / 1 / ✓）+ Icon 底部填充进度 两个视觉状态。
///
/// Unity Editor 配置步骤：
/// 1. 在 goal1_get 下新建子物体 "FillIcon"，添加 Image 组件，
///    图片与底图相同，Image Type = Filled，Fill Method = Vertical，
///    Fill Origin = Bottom，Fill Amount 默认 = 0，Color.alpha = 255。
/// 2. 在 goal1_get 下新建子物体 "Badge"，添加 Image 组件，
///    锚点设置到右上角，准备好 step2/step1/checkmark 三张 sprite。
/// 3. 将此脚本拖入 goal1_get，在 Inspector 里填写 levelID / goalID /
///    isSingleStep，以及三个引用和三张 sprite。
/// </summary>
public class GoalIconUIController : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Goal Identity")]
    [SerializeField] private string levelID;
    [SerializeField] private int goalID;
    /// <summary>true = SingleGoal（单步），false = Goal（两步）</summary>
    [SerializeField] private bool isSingleStep;

    [Header("Fill Effect (Image Type = Filled / Vertical / Bottom)")]
    [SerializeField] private Image fillImage;

    [Header("Badge")]
    [SerializeField] private Image badgeImage;
    [SerializeField] private Sprite badgeStep2Sprite;   // 显示 "2"
    [SerializeField] private Sprite badgeStep1Sprite;   // 显示 "1"
    [SerializeField] private Sprite badgeCheckSprite;   // 显示 ✓

    // DOTween 动画参数（可按需在 Inspector 调整）
    private const float FillDuration = 0.5f;
    private const float PunchDuration = 0.45f;

    public string LevelID => levelID;
    public int GoalID => goalID;

    private GoalIconBarController ResolveDragTarget()
    {
        return GoalIconBarController.Instance;
    }

    private void Awake()
    {
        // 保持 raycast 开启：用于 ShowTextOnUI 悬停/点击等 UI 事件。
    }

    // ──────────────────────────────────────────────────────────────────────
    // Unity 生命周期
    // ──────────────────────────────────────────────────────────────────────

    private void Start()
    {
        ApplyProgressFromSave();
    }

    private void OnEnable()
    {
        GoalNoteEvents.GoalCompleted += HandleGoalCompleted;
        GoalIconBarController.Instance?.RegisterIcon(this);
    }

    private void OnDisable()
    {
        GoalNoteEvents.GoalCompleted -= HandleGoalCompleted;
        GoalIconBarController.Instance?.UnregisterIcon(this);
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        GoalIconBarController target = ResolveDragTarget();
        if (target == null)
            return;

        target.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GoalIconBarController target = ResolveDragTarget();
        if (target == null)
            return;

        target.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        GoalIconBarController target = ResolveDragTarget();
        if (target == null)
            return;

        target.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GoalIconBarController target = ResolveDragTarget();
        if (target == null)
            return;

        target.OnEndDrag(eventData);
    }

    // ──────────────────────────────────────────────────────────────────────
    // 公开接口：由 Goal.ApplySavedProgress 在读档后调用，无动画直接还原状态
    // ──────────────────────────────────────────────────────────────────────

    public void ApplyProgress(bool step1Done, bool step2Done)
    {
        bool isFullyDone = GoalProgressRules.IsCollected(isSingleStep, step1Done, step2Done);

        // 填充量（非动画）
        if (fillImage != null)
        {
            if (isFullyDone)
                fillImage.fillAmount = 1f;
            else if (step1Done)
                fillImage.fillAmount = 0.5f;
            else
                fillImage.fillAmount = 0f;
        }

        // 角标 sprite（非动画）
        if (badgeImage != null)
        {
            if (isFullyDone)
                badgeImage.sprite = badgeCheckSprite;
            else if (step1Done)
                badgeImage.sprite = badgeStep1Sprite;
            else
                badgeImage.sprite = isSingleStep ? badgeStep1Sprite : badgeStep2Sprite;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // 事件响应：有动画地更新状态
    // ──────────────────────────────────────────────────────────────────────

    private void HandleGoalCompleted(string inLevelID, int inGoalID, GoalNoteStep completedStep)
    {
        if (!GoalProgressRules.IsSameLevelID(inLevelID, levelID) || inGoalID != goalID)
            return;

        // isSingleStep 时任何 Step 到达即为全部完成
        bool isFullyDone = completedStep == GoalNoteStep.Step2 || isSingleStep;

        AnimateFill(isFullyDone ? 1f : 0.5f);
        AnimateBadge(isFullyDone ? badgeCheckSprite : badgeStep1Sprite);
    }

    private void ApplyProgressFromSave()
    {
        GoalProgress progress = GetSavedProgress();
        if (progress == null)
        {
            ApplyProgress(false, false);
            return;
        }

        ApplyProgress(progress.step1Completed, progress.step2Completed);
    }

    private GoalProgress GetSavedProgress()
    {
        GameData data = SaveSystem.GameData;
        if (data == null || data.goalProgressMap == null)
            return null;

        string exactKey = levelID + "_" + goalID;
        if (!string.IsNullOrEmpty(levelID) && data.goalProgressMap.TryGetValue(exactKey, out GoalProgress exactProgress))
            return exactProgress;

        if (GoalProgressRules.TryParseLevelIndexStrict(levelID, out int levelIndex))
        {
            string numericKey = levelIndex + "_" + goalID;
            if (data.goalProgressMap.TryGetValue(numericKey, out GoalProgress numericProgress))
                return numericProgress;
        }

        return null;
    }

    // ──────────────────────────────────────────────────────────────────────
    // 内部动画
    // ──────────────────────────────────────────────────────────────────────

    private void AnimateFill(float targetFill)
    {
        if (fillImage == null) return;
        fillImage.DOKill();
        fillImage.DOFillAmount(targetFill, FillDuration).SetEase(Ease.OutQuad);
    }

    private void AnimateBadge(Sprite nextSprite)
    {
        if (badgeImage == null) return;
        badgeImage.sprite = nextSprite;
        badgeImage.transform.DOKill();
        badgeImage.transform.DOPunchScale(Vector3.one * 0.4f, PunchDuration, 2, 0.5f);
    }
}
