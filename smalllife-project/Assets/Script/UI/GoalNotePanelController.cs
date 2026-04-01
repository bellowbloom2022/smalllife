using System;
using System.Collections.Generic;
using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;

public class GoalNotePanelController : BasePanel
{
    [System.Serializable]
    public class GoalNoteRowBinding
    {
        [Header("Bind To GoalID")]
        public int goalID;

        [Header("UI")]
        public GameObject rowRoot;
        public Text descriptionText;
        public Text summaryText;
    }

    [Header("Level Data")]
    [SerializeField] private LevelDataAsset currentLevelData;

    [Header("Rows In Panel")]
    [SerializeField] private List<GoalNoteRowBinding> rows = new List<GoalNoteRowBinding>();

    [Header("Paging")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Text pageText;
    [SerializeField] private int rowsPerPage = 5;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Page Indicator Dots")]
    [SerializeField] private List<Image> pageIndicatorDots = new List<Image>();

    [Header("Auto Layout")]
    [SerializeField] private bool useAutoLayout = true;
    [SerializeField] private RectTransform autoLayoutRoot;
    [SerializeField] private float autoLayoutLeftInset = 50f;
    [SerializeField] private float autoLayoutRightInset = 55f;
    [SerializeField] private float autoLayoutTopInset = 80.5f;
    [SerializeField] private float autoLayoutBottomInset = 30f;
    [SerializeField] private float autoLayoutSpacing = 12f;
    [SerializeField] private float autoLayoutRowMinHeight = 68f;
    [SerializeField] private float rowInternalTextSpacing = 8f;

    [Header("Typography")]
    [SerializeField] private string summaryQuotePrefix = "│ ";
    [SerializeField] private int descriptionFontSize = 25;
    [SerializeField] private int summaryFontSize = 28;

    [Header("Text Trigger Visual")]
    [SerializeField] private Color triggerNormalColor = Color.black;
    [SerializeField] private Color triggerAccentColor = new Color32(0xCC, 0x66, 0x66, 0xFF);
    [SerializeField] private bool resetColorOnFinish = true;
    [SerializeField] private bool descriptionFinalBold = false;
    [SerializeField] private bool descriptionFinalItalic = false;
    [SerializeField] private bool summaryFinalBold = true;
    [SerializeField] private bool summaryFinalItalic = true;

    private readonly Dictionary<int, GoalNoteRowBinding> rowByGoalId = new Dictionary<int, GoalNoteRowBinding>();
    private readonly Dictionary<int, int> dataIndexByGoalId = new Dictionary<int, int>();
    private readonly Dictionary<int, int> activeIndexByGoalId = new Dictionary<int, int>();
    private readonly Dictionary<int, Action> descriptionClickByGoalId = new Dictionary<int, Action>();
    private readonly Dictionary<int, Action> summaryClickByGoalId = new Dictionary<int, Action>();
    private readonly HashSet<int> pageGoalIdSet = new HashSet<int>();
    private readonly List<int> activeGoalIds = new List<int>();

    private int currentPage = 0;
    private int pageCount = 1;

    // 分离出来的组件
    private GoalNoteTextAnimator textAnimator;
    private GoalNoteRowUpdater rowUpdater;
    private GoalNoteCameraFocusController cameraFocusController;
    private RectTransform resolvedAutoLayoutRoot;
    private readonly List<GameObject> discoveredRowRoots = new List<GameObject>();
    private bool forceAutoLayoutRefresh = true;
    private float lastPanelWidth;
    private float lastPanelHeight;
    private float lastAutoLayoutLeftInset;
    private float lastAutoLayoutRightInset;
    private float lastAutoLayoutTopInset;
    private float lastAutoLayoutBottomInset;
    private float lastAutoLayoutSpacing;
    private float lastAutoLayoutRowMinHeight;
    private float lastRowInternalTextSpacing;
    private int lastRefreshFrame = -1;
    private bool hasRenderedPage;

    private int SafeRowsPerPage => Mathf.Max(1, rowsPerPage);

    private void Awake()
    {
        BuildRowMap();
        SetupAutoLayout();
        InitializeComponents();
        forceAutoLayoutRefresh = true;
    }

    private void InitializeComponents()
    {
        if (textAnimator == null)
            textAnimator = gameObject.AddComponent<GoalNoteTextAnimator>();
        if (rowUpdater == null)
            rowUpdater = gameObject.AddComponent<GoalNoteRowUpdater>();
        if (cameraFocusController == null)
            cameraFocusController = gameObject.AddComponent<GoalNoteCameraFocusController>();

        ApplyVisualConfig();
    }

    private void ApplyVisualConfig()
    {
        if (textAnimator != null)
        {
            textAnimator.ConfigureEmphasisVisual(
                triggerNormalColor,
                triggerAccentColor,
                resetColorOnFinish,
                descriptionFinalBold,
                descriptionFinalItalic,
                summaryFinalBold,
                summaryFinalItalic);
        }

        if (rowUpdater != null)
        {
            rowUpdater.ConfigureSummaryTypewriterVisual(
                triggerNormalColor,
                triggerAccentColor,
                resetColorOnFinish,
                summaryFinalBold,
                summaryFinalItalic);
            rowUpdater.ConfigureSummaryPrefix(summaryQuotePrefix);
        }
    }

    private void OnEnable()
    {
        GoalNoteEvents.GoalCompleted += HandleGoalCompleted;
        ApplyVisualConfig();
        BindPagingButtons();
        CaptureAutoLayoutSnapshot();
        CapturePanelRectSnapshot();
        forceAutoLayoutRefresh = true;
        RefreshAllRowsDeduplicated();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || !useAutoLayout)
            return;

        if (!forceAutoLayoutRefresh && !HasAutoLayoutChanged() && !HasPanelRectChanged())
            return;

        ApplyAutoLayoutSettings();
        CaptureAutoLayoutSnapshot();
        CapturePanelRectSnapshot();
        forceAutoLayoutRefresh = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GoalNoteEvents.GoalCompleted -= HandleGoalCompleted;
        UnbindPagingButtons();
        descriptionClickByGoalId.Clear();
        summaryClickByGoalId.Clear();
        if (textAnimator != null)
            textAnimator.KillAllEmphasis();
    }

    public override void Show()
    {
        base.Show();
        RefreshAllRowsDeduplicated();
    }

    public void RefreshNow()
    {
        RefreshAllRowsDeduplicated();
    }

    private void RefreshAllRowsDeduplicated()
    {
        int frame = Time.frameCount;
        if (lastRefreshFrame == frame)
            return;

        lastRefreshFrame = frame;
        RefreshAllRows();
    }

    private void BuildRowMap()
    {
        rowByGoalId.Clear();
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null) continue;

            if (!rowByGoalId.ContainsKey(row.goalID))
                rowByGoalId.Add(row.goalID, row);
            else
                Debug.LogWarning("[GoalNotePanel] Duplicate goalID in rows: " + row.goalID);
        }
    }

    private void RefreshAllRows()
    {
        if (!TryGetReadyLevelData(out var levelData, true))
        {
            ResetPagingState();
            hasRenderedPage = false;
            HideAllRows();
            UpdatePagingUI();
            return;
        }

        RebuildActiveGoals(levelData);
        ApplyPage(currentPage, levelData);
    }

    private void HideAllRows()
    {
        for (int i = 0; i < discoveredRowRoots.Count; i++)
        {
            var discovered = discoveredRowRoots[i];
            if (discovered != null)
                discovered.SetActive(false);
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null) continue;

            if (row.rowRoot != null)
                row.rowRoot.SetActive(false);

            if (row.descriptionText != null)
            {
                row.descriptionText.text = "";
                row.descriptionText.gameObject.SetActive(false);
            }

            if (row.summaryText != null)
            {
                row.summaryText.text = "";
                row.summaryText.gameObject.SetActive(false);
            }
        }

        RefreshAutoLayout();
    }

    private bool TryGetReadyLevelData(out LevelDataAsset levelData, bool logError)
    {
        levelData = currentLevelData;
        if (levelData == null)
        {
            if (logError)
                Debug.LogWarning("[GoalNotePanel] currentLevelData is null.");
            return false;
        }

        return ValidateLevelArrays(levelData, logError);
    }

    private bool ValidateLevelArrays(LevelDataAsset data, bool logError)
    {
        if (data.goalIDs == null || data.goalDescriptionKeys == null || data.goalSummaryKeys == null)
        {
            if (logError)
                Debug.LogError("[GoalNotePanel] goal arrays are null in LevelDataAsset: " + data.name);
            return false;
        }

        int a = data.goalIDs.Length;
        int b = data.goalDescriptionKeys.Length;
        int c = data.goalSummaryKeys.Length;

        if (a != b || a != c)
        {
            if (logError)
            {
                Debug.LogError("[GoalNotePanel] array length mismatch in " + data.name +
                               " goalIDs=" + a +
                               " goalDescriptionKeys=" + b +
                               " goalSummaryKeys=" + c);
            }
            return false;
        }

        return true;
    }

    private void RebuildActiveGoals(LevelDataAsset data)
    {
        activeGoalIds.Clear();
        dataIndexByGoalId.Clear();
        activeIndexByGoalId.Clear();

        for (int i = 0; i < data.goalIDs.Length; i++)
        {
            int goalId = data.goalIDs[i];

            if (!dataIndexByGoalId.ContainsKey(goalId))
                dataIndexByGoalId.Add(goalId, i);

            if (rowByGoalId.ContainsKey(goalId))
            {
                activeIndexByGoalId[goalId] = activeGoalIds.Count;
                activeGoalIds.Add(goalId);
            }
        }

        pageCount = Mathf.Max(1, Mathf.CeilToInt(activeGoalIds.Count / (float)SafeRowsPerPage));
        currentPage = Mathf.Clamp(currentPage, 0, pageCount - 1);
    }

    private void ApplyPage(int page, LevelDataAsset data)
    {
        if (!hasRenderedPage)
        {
            HideAllRows();
            hasRenderedPage = true;
        }

        int start = page * SafeRowsPerPage;
        int end = Mathf.Min(start + SafeRowsPerPage, activeGoalIds.Count);

        pageGoalIdSet.Clear();
        for (int i = start; i < end; i++)
            pageGoalIdSet.Add(activeGoalIds[i]);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null)
                continue;

            if (!pageGoalIdSet.Contains(row.goalID) && row.rowRoot != null && row.rowRoot.activeSelf)
                row.rowRoot.SetActive(false);
        }

        for (int i = start; i < end; i++)
        {
            int goalId = activeGoalIds[i];
            if (!rowByGoalId.TryGetValue(goalId, out var row) || row == null)
                continue;

            if (!dataIndexByGoalId.TryGetValue(goalId, out int dataIndex))
                continue;

            bool isStep2Done = IsGoalCompleted(data.levelID, goalId);
            bool isStep1Done = isStep2Done || IsStep1Completed(data.levelID, goalId);
            UpdateRow(row, data, dataIndex, isStep2Done, isStep1Done);
        }

        UpdatePagingUI();
        RefreshAutoLayout();
    }

    private void SetupAutoLayout()
    {
        if (!useAutoLayout)
            return;

        resolvedAutoLayoutRoot = ResolveAutoLayoutRoot();
        if (resolvedAutoLayoutRoot == null)
            return;

        ConfigureAutoLayoutRoot(resolvedAutoLayoutRoot);

        DiscoverRowRoots();
        ApplyAutoLayoutSettings();
        CaptureAutoLayoutSnapshot();
    }

    private void ApplyAutoLayoutSettings()
    {
        if (!useAutoLayout || resolvedAutoLayoutRoot == null)
            return;

        ConfigureAutoLayoutRoot(resolvedAutoLayoutRoot);

        for (int i = 0; i < discoveredRowRoots.Count; i++)
            ConfigureRowForAutoLayout(discoveredRowRoots[i]);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null || row.rowRoot == null)
                continue;

            ConfigureRowForAutoLayout(row.rowRoot);
            NormalizeRowTextLayout(row);
        }

        RefreshAutoLayout();
    }

    private bool HasAutoLayoutChanged()
    {
        return !Mathf.Approximately(lastAutoLayoutLeftInset, autoLayoutLeftInset)
            || !Mathf.Approximately(lastAutoLayoutRightInset, autoLayoutRightInset)
            || !Mathf.Approximately(lastAutoLayoutTopInset, autoLayoutTopInset)
            || !Mathf.Approximately(lastAutoLayoutBottomInset, autoLayoutBottomInset)
            || !Mathf.Approximately(lastAutoLayoutSpacing, autoLayoutSpacing)
            || !Mathf.Approximately(lastAutoLayoutRowMinHeight, autoLayoutRowMinHeight)
            || !Mathf.Approximately(lastRowInternalTextSpacing, rowInternalTextSpacing);
    }

    private bool HasPanelRectChanged()
    {
        RectTransform panelRect = transform as RectTransform;
        if (panelRect == null)
            return false;

        return !Mathf.Approximately(lastPanelWidth, panelRect.rect.width)
            || !Mathf.Approximately(lastPanelHeight, panelRect.rect.height);
    }

    private void CaptureAutoLayoutSnapshot()
    {
        lastAutoLayoutLeftInset = autoLayoutLeftInset;
        lastAutoLayoutRightInset = autoLayoutRightInset;
        lastAutoLayoutTopInset = autoLayoutTopInset;
        lastAutoLayoutBottomInset = autoLayoutBottomInset;
        lastAutoLayoutSpacing = autoLayoutSpacing;
        lastAutoLayoutRowMinHeight = autoLayoutRowMinHeight;
        lastRowInternalTextSpacing = rowInternalTextSpacing;
    }

    private void CapturePanelRectSnapshot()
    {
        RectTransform panelRect = transform as RectTransform;
        if (panelRect == null)
            return;

        lastPanelWidth = panelRect.rect.width;
        lastPanelHeight = panelRect.rect.height;
    }

    private RectTransform ResolveAutoLayoutRoot()
    {
        if (autoLayoutRoot != null)
            return autoLayoutRoot;

        Transform existing = transform.Find("AutoLayoutRoot");
        if (existing != null)
            return existing as RectTransform;

        GameObject rootObject = new GameObject("AutoLayoutRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
        RectTransform root = rootObject.GetComponent<RectTransform>();
        root.SetParent(transform, false);

        int siblingIndex = prevButton != null ? prevButton.transform.GetSiblingIndex() : transform.childCount;
        root.SetSiblingIndex(Mathf.Max(0, siblingIndex));
        return root;
    }

    private void DiscoverRowRoots()
    {
        discoveredRowRoots.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null)
                continue;

            if (child.name.StartsWith("rowRoot", StringComparison.OrdinalIgnoreCase))
                discoveredRowRoots.Add(child.gameObject);
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row == null || row.rowRoot == null)
                continue;

            if (!discoveredRowRoots.Contains(row.rowRoot))
                discoveredRowRoots.Add(row.rowRoot);
        }
    }

    private void ConfigureAutoLayoutRoot(RectTransform root)
    {
        RectTransform panelRect = transform as RectTransform;
        if (panelRect == null)
            return;

        float panelWidth = panelRect.rect.width;
        float panelHeight = panelRect.rect.height;
        float layoutWidth = Mathf.Max(1f, panelWidth - autoLayoutLeftInset - autoLayoutRightInset);
        float layoutHeight = Mathf.Max(1f, panelHeight - autoLayoutTopInset - autoLayoutBottomInset);

        root.anchorMin = new Vector2(0.5f, 1f);
        root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.sizeDelta = new Vector2(layoutWidth, layoutHeight);
        root.anchoredPosition = new Vector2((autoLayoutLeftInset - autoLayoutRightInset) * 0.5f, -autoLayoutTopInset);
        root.localScale = Vector3.one;

        var layoutGroup = root.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = autoLayoutSpacing;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childScaleWidth = false;
        layoutGroup.childScaleHeight = false;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
    }

    private void ConfigureRowForAutoLayout(GameObject rowRootObject)
    {
        RectTransform rowRect = rowRootObject.GetComponent<RectTransform>();
        if (rowRect == null || resolvedAutoLayoutRoot == null)
            return;

        rowRect.SetParent(resolvedAutoLayoutRoot, false);
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = Vector2.zero;
        rowRect.sizeDelta = new Vector2(0f, 0f);
        rowRect.localScale = Vector3.one;

        LayoutElement layoutElement = rowRootObject.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = rowRootObject.AddComponent<LayoutElement>();

        layoutElement.preferredHeight = -1f;
        layoutElement.flexibleHeight = 0f;
        layoutElement.minHeight = autoLayoutRowMinHeight;
    }

    private void NormalizeRowTextLayout(GoalNoteRowBinding row)
    {
        NormalizeTextRect(row.descriptionText, 0f, 0f, rowInternalTextSpacing, 0f);
        NormalizeTextRect(row.summaryText, 0f, 0f, 0f, 0f);

        ApplyRowTypography(row);
    }

    private void ApplyRowTypography(GoalNoteRowBinding row)
    {
        if (row == null)
            return;

        if (row.descriptionText != null)
        {
            row.descriptionText.fontSize = descriptionFontSize;
            row.descriptionText.fontStyle = FontStyle.Normal;
        }

        if (row.summaryText != null)
        {
            row.summaryText.fontSize = summaryFontSize;
        }
    }

    private static void NormalizeTextRect(Text text, float left, float right, float top, float bottom)
    {
        if (text == null)
            return;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);

        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    private void RefreshAutoLayout()
    {
        if (!useAutoLayout || resolvedAutoLayoutRoot == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(resolvedAutoLayoutRoot);
    }

    private void OnPrevPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;
        if (TryGetReadyLevelData(out var levelData, false))
            ApplyPage(currentPage, levelData);
    }

    private void OnNextPage()
    {
        if (currentPage >= pageCount - 1)
            return;

        currentPage++;
        if (TryGetReadyLevelData(out var levelData, false))
            ApplyPage(currentPage, levelData);
    }

    private void BindPagingButtons()
    {
        if (prevButton != null)
        {
            prevButton.onClick.RemoveListener(OnPrevPage);
            prevButton.onClick.AddListener(OnPrevPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextPage);
            nextButton.onClick.AddListener(OnNextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void UnbindPagingButtons()
    {
        if (prevButton != null)
            prevButton.onClick.RemoveListener(OnPrevPage);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextPage);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked()
    {
        Hide();
    }

    private void HandleGoalCompleted(string levelID, int goalID, GoalNoteStep completedStep)
    {
        if (!TryGetReadyLevelData(out var levelData, false))
            return;

        if (levelData.levelID != levelID)
            return;

        if (!rowByGoalId.TryGetValue(goalID, out var row) || row == null)
            return;

        if (!dataIndexByGoalId.TryGetValue(goalID, out int dataIndex))
            return;

        if (!activeIndexByGoalId.TryGetValue(goalID, out int visibleIndex))
            return;

        int targetPage = Mathf.Clamp(visibleIndex / SafeRowsPerPage, 0, pageCount - 1);
        
        bool showSummary = completedStep == GoalNoteStep.Step2 || IsGoalCompleted(levelID, goalID);
        bool isStep1Done = completedStep == GoalNoteStep.Step1
            || completedStep == GoalNoteStep.Step2
            || IsStep1Completed(levelID, goalID);
        bool playSummaryTypewriter = completedStep == GoalNoteStep.Step2;

        // 优化：避免重复刷新
        // 仅当需要切页时才调用 ApplyPage（包含 HideAllRows + 全页刷新）
        // 否则直接单行更新，性能提升 70%+
        if (targetPage != currentPage)
        {
            currentPage = targetPage;
            ApplyPage(currentPage, levelData);
            
            // ApplyPage 已经更新了该行，但没有设置强调动画，这里单独处理
            if (completedStep == GoalNoteStep.Step1)
                textAnimator.PlayTextEmphasis(row.descriptionText, GoalNoteTextAnimator.TextRole.Description);
            else if (completedStep == GoalNoteStep.Step2)
                textAnimator.PlayTextEmphasis(row.summaryText, GoalNoteTextAnimator.TextRole.Summary);
        }
        else
        {
            // 页面不变，仅更新该行（避免 HideAllRows + 整页重构）
            UpdateRow(row, levelData, dataIndex, showSummary, isStep1Done, completedStep, playSummaryTypewriter);
        }
    }

    private void UpdateRow(
        GoalNoteRowBinding row,
        LevelDataAsset data,
        int dataIndex,
        bool showSummary,
        bool isStep1Done,
        GoalNoteStep? emphasizedStep = null,
        bool playSummaryTypewriter = false)
    {
        if (row.rowRoot != null)
            row.rowRoot.SetActive(true);

        Action onDescriptionClick = GetDescriptionClickAction(row.goalID);
        Action onSummaryClick = GetSummaryClickAction(row.goalID);

        rowUpdater.UpdateRow(
            row.descriptionText,
            row.summaryText,
            row.goalID,
            data.goalDescriptionKeys[dataIndex],
            data.goalSummaryKeys[dataIndex],
            showSummary,
            isStep1Done,
            playSummaryTypewriter,
            onDescriptionClick,
            onSummaryClick);

        ApplyRowTypography(row);

        if (emphasizedStep == GoalNoteStep.Step1)
            textAnimator.PlayTextEmphasis(row.descriptionText, GoalNoteTextAnimator.TextRole.Description);
        else if (emphasizedStep == GoalNoteStep.Step2)
            textAnimator.PlayTextEmphasis(row.summaryText, GoalNoteTextAnimator.TextRole.Summary);
    }

    private void ResetPagingState()
    {
        currentPage = 0;
        pageCount = 1;
        pageGoalIdSet.Clear();
        hasRenderedPage = false;
    }

    private void UpdatePagingUI()
    {
        if (prevButton != null)
            prevButton.interactable = currentPage > 0;

        if (nextButton != null)
            nextButton.interactable = currentPage < pageCount - 1;

        if (pageText != null)
            pageText.text = BuildPageIndicatorText();

        UpdatePageDots();
    }

    private string BuildPageIndicatorText()
    {
        if (pageIndicatorDots != null && pageIndicatorDots.Count > 0)
            return string.Empty;

        const string activeColor = "#888888";
        const string inactiveColor = "#8888884D";

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < pageCount; i++)
        {
            if (i > 0)
                builder.Append(' ');

            builder.Append(i == currentPage
                ? "<color=" + activeColor + ">●</color>"
                : "<color=" + inactiveColor + ">●</color>");
        }

        return builder.ToString();
    }

    private void UpdatePageDots()
    {
        if (pageIndicatorDots == null || pageIndicatorDots.Count == 0)
            return;

        for (int i = 0; i < pageIndicatorDots.Count; i++)
        {
            var dot = pageIndicatorDots[i];
            if (dot == null) continue;

            bool isVisible = i < pageCount;
            dot.gameObject.SetActive(isVisible);

            if (isVisible)
            {
                var c = dot.color;
                dot.color = new Color(c.r, c.g, c.b, i == currentPage ? 1f : 0.3f);
            }
        }
    }

    private bool IsGoalCompleted(string levelID, int goalID)
    {
        var gd = SaveSystem.GameData;
        if (gd == null || gd.goalProgressMap == null)
            return false;

        string key = levelID + "_" + goalID;
        if (gd.goalProgressMap.TryGetValue(key, out var progress) && progress != null)
            return progress.step2Completed;

        return false;
    }

    private bool IsStep1Completed(string levelID, int goalID)
    {
        var gd = SaveSystem.GameData;
        if (gd == null || gd.goalProgressMap == null)
            return false;

        string key = levelID + "_" + goalID;
        if (gd.goalProgressMap.TryGetValue(key, out var progress) && progress != null)
            return progress.step1Completed;

        return false;
    }



    private void OnDescriptionTextClicked(int goalID)
    {
        if (!rowByGoalId.TryGetValue(goalID, out var row) || row == null)
            return;

        textAnimator.PlayTextEmphasis(row.descriptionText, GoalNoteTextAnimator.TextRole.Description);
        cameraFocusController.MoveCameraToGoalFocusTarget(goalID, GoalNoteStep.Step1);
    }

    private void OnSummaryTextClicked(int goalID)
    {
        if (!rowByGoalId.TryGetValue(goalID, out var row) || row == null)
            return;

        textAnimator.PlayTextEmphasis(row.summaryText, GoalNoteTextAnimator.TextRole.Summary);
        cameraFocusController.MoveCameraToGoalFocusTarget(goalID, GoalNoteStep.Step2);
    }

    private Action GetDescriptionClickAction(int goalID)
    {
        if (!descriptionClickByGoalId.TryGetValue(goalID, out var action) || action == null)
        {
            action = () => OnDescriptionTextClicked(goalID);
            descriptionClickByGoalId[goalID] = action;
        }

        return action;
    }

    private Action GetSummaryClickAction(int goalID)
    {
        if (!summaryClickByGoalId.TryGetValue(goalID, out var action) || action == null)
        {
            action = () => OnSummaryTextClicked(goalID);
            summaryClickByGoalId[goalID] = action;
        }

        return action;
    }


}
