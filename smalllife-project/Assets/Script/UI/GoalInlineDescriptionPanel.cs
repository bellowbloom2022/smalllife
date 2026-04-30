using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lean.Localization;

/// <summary>
/// 挂在 goal*_get.prefab 根节点上。
/// 统一管理 goal-icon hover 时的 infoText 和 descriptionPanel 显示/隐藏。
/// 实现 delayed exit + 子区域检测，允许鼠标从 icon 移动到面板时保持显示。
/// </summary>
[RequireComponent(typeof(Graphic))]
public class GoalInlineDescriptionPanel : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Goal Identity（与 GoalIconUIController 保持一致）")]
    [SerializeField] private string levelID;
    [SerializeField] private int goalID = 1;

    [Header("Info Text 控制（已有组件）")]
    [SerializeField] private ShowTextOnUI showTextOnUI;

    [Header("描述面板根节点")]
    [SerializeField] private GameObject descriptionPanelRoot;

    [Header("文本内容")]
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text summaryText;

    [Header("翻页控件")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Text pageIndicatorText;

    [Header("Hover 延迟设置")]
    [SerializeField] private float hideDelay = 0.15f;

    [Header("完成提示显示时长（秒）")]
    [SerializeField] private float completedShowDuration = 2f;

    [Header("Icon hover 检测扩展边距（像素）")]
    [SerializeField] private float iconPadding = 20f;

    // 内部状态
    private int currentPage = 0;       // 0=description, 1=summary
    private bool isStep2Done = false;
    private string descriptionKey = "";
    private string summaryKey = "";
    private Coroutine hideCoroutine;
    private Coroutine autoShowCoroutine;
    private CanvasGroup descriptionTextCG;
    private CanvasGroup summaryTextCG;
    private RectTransform cachedRootRect;
    private readonly Vector3[] worldCorners = new Vector3[4];

    private const int TotalPages = 2;

    private void Awake()
    {
        // 缓存根节点 RectTransform
        cachedRootRect = GetComponent<RectTransform>();

        // 尝试自动查找缺失的引用
        if (showTextOnUI == null)
            showTextOnUI = GetComponent<ShowTextOnUI>();
    }

    private void Start()
    {
        ResolveGoalIdentity();
        ResolveTextKeys();
        RestoreStateFromSave();
        BindButtons();

        // 初始隐藏（立即隐藏，避免首帧闪烁）
        HideAll(true);
    }

    private void OnEnable()
    {
        GoalNoteEvents.GoalCompleted += HandleGoalCompleted;
    }

    private void OnDisable()
    {
        GoalNoteEvents.GoalCompleted -= HandleGoalCompleted;
        CancelPendingHide();
        CancelAutoShow();
    }

    // ── IPointer 接口 ──
    public void OnPointerEnter(PointerEventData eventData)
    {
        CancelPendingHide();
        CancelAutoShow();
        ShowAll();

        // 播放 hover 音效
        if (AudioHub.Instance != null)
            AudioHub.Instance.PlayGlobal("goal-icon-hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 不立即启动隐藏，由 Update 持续检测鼠标位置
    }

    private void Update()
    {
        bool isShowing = IsAnyUIVisible();
        if (!isShowing) return;

        bool isInZone = IsPointerInHoverZone();

        if (isInZone)
        {
            CancelPendingHide();
        }
        else if (hideCoroutine == null && autoShowCoroutine == null)
        {
            ScheduleHide();
        }
    }

    /// <summary>
    /// 检测鼠标是否在 icon 或 panel 的 RectTransform 范围内（含间距容差）
    /// </summary>
    private bool IsPointerInHoverZone()
    {
        Vector2 mousePos = Input.mousePosition;

        // 检测 icon 根节点
        if (IsPointInRect(cachedRootRect, mousePos, iconPadding))
            return true;

        // 检测 descriptionPanel
        if (descriptionPanelRoot != null && descriptionPanelRoot.activeSelf)
        {
            if (IsPointInRect(descriptionPanelRoot.GetComponent<RectTransform>(), mousePos, 0f))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 检测点是否在 RectTransform 范围内（可向外扩展 padding）
    /// </summary>
    private bool IsPointInRect(RectTransform rt, Vector2 screenPoint, float padding)
    {
        if (rt == null) return false;

        rt.GetWorldCorners(worldCorners);

        float minX = worldCorners[0].x - padding;
        float maxX = worldCorners[2].x + padding;
        float minY = worldCorners[0].y - padding;
        float maxY = worldCorners[1].y + padding;

        return screenPoint.x >= minX && screenPoint.x <= maxX
            && screenPoint.y >= minY && screenPoint.y <= maxY;
    }

    // ── 公开方法 ──

    /// <summary>
    /// 显示 infoText 和 descriptionPanel
    /// </summary>
    public void ShowAll()
    {
        ShowInfoText();
        ShowDescriptionPanel();
    }

    /// <summary>
    /// 隐藏 infoText 和 descriptionPanel
    /// </summary>
    /// <param name="immediate">是否立即隐藏（延迟模式用）</param>
    public void HideAll(bool immediate = true)
    {
        if (immediate)
        {
            HideInfoText();
            HideDescriptionPanel();
        }
        else
        {
            ScheduleHide();
        }
    }

    // ── 内部逻辑 ──

    private void ResolveGoalIdentity()
    {
        // 优先从同级 GoalIconUIController 获取
        if (string.IsNullOrEmpty(levelID) || goalID <= 0)
        {
            var iconController = GetComponent<GoalIconUIController>();
            if (iconController != null)
            {
                if (string.IsNullOrEmpty(levelID))
                    levelID = iconController.LevelID;
                if (goalID <= 0)
                    goalID = iconController.GoalID;
            }
        }
    }

    private void ResolveTextKeys()
    {
        if (Level.ins == null || Level.ins.levelDataAsset == null)
        {
            Debug.LogWarning($"[GoalDescPanel] Level or levelDataAsset not ready. goalID={goalID}");
            return;
        }

        var data = Level.ins.levelDataAsset;
        if (data.goalIDs == null || data.goalDescriptionKeys == null || data.goalSummaryKeys == null)
        {
            Debug.LogWarning($"[GoalDescPanel] LevelDataAsset arrays are null. goalID={goalID}");
            return;
        }

        for (int i = 0; i < data.goalIDs.Length; i++)
        {
            if (data.goalIDs[i] == goalID)
            {
                descriptionKey = i < data.goalDescriptionKeys.Length ? data.goalDescriptionKeys[i] : "";
                summaryKey = i < data.goalSummaryKeys.Length ? data.goalSummaryKeys[i] : "";
                return;
            }
        }

        Debug.LogWarning($"[GoalDescPanel] GoalID {goalID} not found in LevelDataAsset.");
    }

    private void RestoreStateFromSave()
    {
        var gd = SaveSystem.GameData;
        if (gd?.goalProgressMap == null) return;

        string key = GetProgressKey();
        if (gd.goalProgressMap.TryGetValue(key, out var progress) && progress != null)
        {
            isStep2Done = progress.step2Completed;
        }

        UpdatePagingUI();
    }

    private string GetProgressKey()
    {
        // 与 GoalIconUIController 保持一致
        if (!string.IsNullOrEmpty(levelID))
            return $"{levelID}_{goalID}";
        if (Level.ins != null)
            return $"{Level.ins.currentLevelIndex}_{goalID}";
        return $"_{goalID}";
    }

    private void HandleGoalCompleted(string lvlID, int gID, GoalNoteStep step)
    {
        if (gID != goalID) return;
        if (!GoalProgressRules.IsSameLevelID(lvlID, levelID)) return;

        // Step1 显示 description 页，Step2 显示 summary 页
        currentPage = (step == GoalNoteStep.Step2) ? 1 : 0;

        if (step == GoalNoteStep.Step2)
            isStep2Done = true;

        ShowAll();
        UpdatePagingUI();
        RefreshCurrentPage();
        StartAutoHide();
    }

    /// <summary>
    /// 自动显示一段时间后隐藏（用于 step 完成提示）
    /// </summary>
    private void StartAutoHide()
    {
        CancelAutoShow();
        CancelPendingHide();
        autoShowCoroutine = StartCoroutine(AutoHideRoutine());
    }

    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSeconds(completedShowDuration);

        // 如果鼠标不在 hover 区域，则隐藏
        if (!IsPointerInHoverZone())
        {
            HideAll(true);
        }

        autoShowCoroutine = null;
    }

    private void CancelAutoShow()
    {
        if (autoShowCoroutine != null)
        {
            StopCoroutine(autoShowCoroutine);
            autoShowCoroutine = null;
        }
    }

    private void BindButtons()
    {
        if (prevButton != null)
            prevButton.onClick.AddListener(() => SwitchPage(currentPage - 1));

        if (nextButton != null)
            nextButton.onClick.AddListener(() => SwitchPage(currentPage + 1));
    }

    private void SwitchPage(int targetPage)
    {
        currentPage = Mathf.Clamp(targetPage, 0, TotalPages - 1);
        UpdatePagingUI();
        RefreshCurrentPage();
    }

    private void UpdatePagingUI()
    {
        bool canNavigate = isStep2Done;

        if (prevButton != null)
            prevButton.interactable = canNavigate && currentPage > 0;

        if (nextButton != null)
            nextButton.interactable = canNavigate && currentPage < TotalPages - 1;

        if (pageIndicatorText != null)
            pageIndicatorText.text = $"{currentPage + 1} / {TotalPages}";
    }

    private void RefreshCurrentPage()
    {
        if (currentPage == 0)
        {
            SetTextActive(descriptionText, true, descriptionKey);
            SetTextActive(summaryText, false, null);
        }
        else
        {
            SetTextActive(descriptionText, false, null);
            SetTextActive(summaryText, true, summaryKey);
        }
    }

    private void SetTextActive(Text text, bool active, string localizationKey)
    {
        if (text == null) return;

        // 通过 CanvasGroup 控制显示/隐藏，缓存避免每帧 GetComponent
        CanvasGroup cg = text == descriptionText ? descriptionTextCG
                       : text == summaryText ? summaryTextCG
                       : null;

        if (cg == null)
        {
            cg = text.gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = text.gameObject.AddComponent<CanvasGroup>();

            // 缓存
            if (text == descriptionText) descriptionTextCG = cg;
            else if (text == summaryText) summaryTextCG = cg;
        }

        cg.alpha = active ? 1f : 0f;
        cg.blocksRaycasts = active;
        cg.interactable = active;

        if (active)
        {
            string localized = LeanLocalization.GetTranslationText(localizationKey);
            text.text = string.IsNullOrEmpty(localized) ? localizationKey : localized;
        }
    }

    /// <summary>
    /// 判断当前是否有任何 UI 元素正在显示（info text 或 description panel）
    /// </summary>
    private bool IsAnyUIVisible()
    {
        if (descriptionPanelRoot != null && descriptionPanelRoot.activeSelf)
            return true;
        if (showTextOnUI != null && showTextOnUI.infoText != null && showTextOnUI.infoText.gameObject.activeSelf)
            return true;
        return false;
    }

    // ── 显示/隐藏控制 ──

    private void ShowInfoText()
    {
        if (showTextOnUI != null)
        {
            // 调用 ShowText 方法显示 info text
            showTextOnUI.ShowText();
        }
    }

    private void HideInfoText()
    {
        if (showTextOnUI != null)
        {
            showTextOnUI.HideText();
        }
    }

    private void ShowDescriptionPanel()
    {
        if (descriptionPanelRoot != null && !descriptionPanelRoot.activeSelf)
        {
            descriptionPanelRoot.SetActive(true);
            RefreshCurrentPage();
        }
    }

    private void HideDescriptionPanel()
    {
        if (descriptionPanelRoot != null && descriptionPanelRoot.activeSelf)
        {
            descriptionPanelRoot.SetActive(false);
        }
    }

    // ── Delayed Exit + 子区域检测 ──

    private void CancelPendingHide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private void ScheduleHide()
    {
        CancelPendingHide();
        hideCoroutine = StartCoroutine(DelayedHideRoutine());
    }

    private IEnumerator DelayedHideRoutine()
    {
        yield return new WaitForSeconds(hideDelay);

        // 延迟结束后用 RectTransform 范围检测判断
        if (!IsPointerInHoverZone())
        {
            HideAll(true);
        }

        hideCoroutine = null;
    }

    // ── 调试用 ──

#if UNITY_EDITOR
    [ContextMenu("Test Show All")]
    private void TestShowAll()
    {
        ShowAll();
    }

    [ContextMenu("Test Hide All")]
    private void TestHideAll()
    {
        HideAll(true);
    }
#endif
}
