using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// 挂在 goal1_get UI 根节点上。
/// 负责：Icon 透明度 + Dot Circle 圆环填充进度 + Checkmark 显示 三个视觉状态。
/// </summary>
public class GoalIconUIController : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Goal Identity")]
    [SerializeField] private string levelID;
    [SerializeField] private int goalID;
    /// <summary>true = SingleGoal（单步），false = Goal（两步）</summary>
    [SerializeField] private bool isSingleStep;



    [Header("Dot Circle Filled (Dashed Circle Background)")]
    [SerializeField] private Image dotCircleFilledImage; // 虚线圆环底图

    [Header("Icon Image")]
    [SerializeField] private Image iconImage; // Icon 主体

    [Header("Checkmark")]
    [SerializeField] private GameObject checkmark; // Checkmark 图标

    private const float FillDuration = 0.5f;
    private const float IconAlphaDuration = 0.5f; // Icon 透明度动画时长

    public string LevelID => levelID;
    public int GoalID => goalID;

    private bool? runtimeSingleStepOverride;

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
        ApplyProgressFromSave();
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
        bool singleStep = GetEffectiveSingleStepFlag();
        NormalizeLegacyProgressForDisplay(singleStep, ref step1Done, ref step2Done);
        bool isFullyDone = GoalProgressRules.IsCollected(singleStep, step1Done, step2Done);

        // Dot Circle Filled 填充量（非动画）
        if (dotCircleFilledImage != null)
        {
            if (isFullyDone)
                dotCircleFilledImage.fillAmount = 1f;
            else if (step1Done)
                dotCircleFilledImage.fillAmount = 0.5f;
            else
                dotCircleFilledImage.fillAmount = 0f;
        }

        // Icon 透明度（非动画）
        if (iconImage != null)
        {
            Color color = iconImage.color;
            if (isFullyDone)
                color.a = 1f;
            else if (step1Done)
                color.a = 0.7f;
            else
                color.a = 0.4f;
            iconImage.color = color;
        }

        // Checkmark（非动画）
        if (checkmark != null)
        {
            checkmark.SetActive(isFullyDone);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // 事件响应：有动画地更新状态
    // ──────────────────────────────────────────────────────────────────────

    private void HandleGoalCompleted(string inLevelID, int inGoalID, GoalNoteStep completedStep)
    {
        if (!GoalProgressRules.IsSameLevelID(inLevelID, levelID) || inGoalID != goalID)
            return;

        bool singleStep = GetEffectiveSingleStepFlag();
        bool isFullyDone = completedStep == GoalNoteStep.Step2 || singleStep;

        AnimateDotCircle(0, isFullyDone ? 1f : 0.5f);
        AnimateIconAlpha(isFullyDone ? 0.7f : 0.4f, isFullyDone ? 1f : 0.7f);

        if (isFullyDone && checkmark != null)
        {
            checkmark.SetActive(true);
        }
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

        if (Level.ins != null)
        {
            string currentLevelKey = Level.ins.currentLevelIndex + "_" + goalID;
            if (data.goalProgressMap.TryGetValue(currentLevelKey, out GoalProgress currentLevelProgress))
                return currentLevelProgress;
        }

        return null;
    }

    private static void NormalizeLegacyProgressForDisplay(bool singleStep, ref bool step1Done, ref bool step2Done)
    {
        // 历史存档可能出现两步目标仅记录 step2Completed=true 的情况；
        // 仅在图标展示层将其视为已完成，避免与手机面板出现读档后不一致。
        if (!singleStep && step2Done && !step1Done)
            step1Done = true;
    }

    private bool GetEffectiveSingleStepFlag()
    {
        if (runtimeSingleStepOverride.HasValue)
            return runtimeSingleStepOverride.Value;

        if (Level.ins == null || Level.ins.goals == null)
            return isSingleStep;

        for (int i = 0; i < Level.ins.goals.Count; i++)
        {
            GameObject goalObj = Level.ins.goals[i];
            if (goalObj == null)
                continue;

            Goal goal = goalObj.GetComponent<Goal>();
            if (goal == null || goal.GoalID != goalID)
                continue;

            runtimeSingleStepOverride = goal is SingleGoal;
            return runtimeSingleStepOverride.Value;
        }

        return isSingleStep;
    }

    // ──────────────────────────────────────────────────────────────────────
    // 内部动画
    // ──────────────────────────────────────────────────────────────────────

    private void AnimateDotCircle(float from, float to)
    {
        if (dotCircleFilledImage != null)
        {
            DOTween.To(() => dotCircleFilledImage.fillAmount, x => dotCircleFilledImage.fillAmount = x, to, FillDuration);
        }
    }

    private void AnimateIconAlpha(float from, float to)
    {
        if (iconImage != null)
        {
            Color color = iconImage.color;
            DOTween.To(() => color.a, x => {
                color.a = x;
                iconImage.color = color;
            }, to, IconAlphaDuration);
        }
    }
}
