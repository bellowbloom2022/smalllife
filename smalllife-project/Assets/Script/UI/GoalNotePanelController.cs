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

    [Header("Text Trigger Visual")]
    [SerializeField] private Color triggerNormalColor = Color.black;
    [SerializeField] private Color triggerAccentColor = new Color32(0xCC, 0x66, 0x66, 0xFF);
    [SerializeField] private bool resetColorOnFinish = true;
    [SerializeField] private bool descriptionFinalBold = true;
    [SerializeField] private bool descriptionFinalItalic = false;
    [SerializeField] private bool summaryFinalBold = true;
    [SerializeField] private bool summaryFinalItalic = true;

    private readonly Dictionary<int, GoalNoteRowBinding> rowByGoalId = new Dictionary<int, GoalNoteRowBinding>();
    private readonly Dictionary<int, int> dataIndexByGoalId = new Dictionary<int, int>();
    private readonly List<int> activeGoalIds = new List<int>();

    private int currentPage = 0;
    private int pageCount = 1;

    // 分离出来的组件
    private GoalNoteTextAnimator textAnimator;
    private GoalNoteRowUpdater rowUpdater;
    private GoalNoteCameraFocusController cameraFocusController;

    private int SafeRowsPerPage => Mathf.Max(1, rowsPerPage);

    private void Awake()
    {
        BuildRowMap();
        InitializeComponents();
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
        }
    }

    private void OnEnable()
    {
        GoalNoteEvents.GoalCompleted += HandleGoalCompleted;
        ApplyVisualConfig();
        BindPagingButtons();
        RefreshAllRows();
    }

    private void OnDisable()
    {
        GoalNoteEvents.GoalCompleted -= HandleGoalCompleted;
        UnbindPagingButtons();
        if (textAnimator != null)
            textAnimator.KillAllEmphasis();
    }

    public override void Show()
    {
        base.Show();
        RefreshAllRows();
    }

    public void RefreshNow()
    {
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
            HideAllRows();
            UpdatePagingUI();
            return;
        }

        RebuildActiveGoals(levelData);
        ApplyPage(currentPage, levelData);
    }

    private void HideAllRows()
    {
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

        for (int i = 0; i < data.goalIDs.Length; i++)
        {
            int goalId = data.goalIDs[i];

            if (!dataIndexByGoalId.ContainsKey(goalId))
                dataIndexByGoalId.Add(goalId, i);

            if (rowByGoalId.ContainsKey(goalId))
                activeGoalIds.Add(goalId);
        }

        pageCount = Mathf.Max(1, Mathf.CeilToInt(activeGoalIds.Count / (float)SafeRowsPerPage));
        currentPage = Mathf.Clamp(currentPage, 0, pageCount - 1);
    }

    private void ApplyPage(int page, LevelDataAsset data)
    {
        HideAllRows();

        int start = page * SafeRowsPerPage;
        int end = Mathf.Min(start + SafeRowsPerPage, activeGoalIds.Count);

        for (int i = start; i < end; i++)
        {
            int goalId = activeGoalIds[i];
            if (!rowByGoalId.TryGetValue(goalId, out var row) || row == null)
                continue;

            if (!dataIndexByGoalId.TryGetValue(goalId, out int dataIndex))
                continue;

            UpdateRow(row, data, dataIndex, IsGoalCompleted(data.levelID, goalId));
        }

        UpdatePagingUI();
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

        int visibleIndex = activeGoalIds.IndexOf(goalID);
        if (visibleIndex < 0)
            return;

        currentPage = Mathf.Clamp(visibleIndex / SafeRowsPerPage, 0, pageCount - 1);
        ApplyPage(currentPage, levelData);

        bool showSummary = completedStep == GoalNoteStep.Step2 || IsGoalCompleted(levelID, goalID);
        bool playSummaryTypewriter = completedStep == GoalNoteStep.Step2;
        UpdateRow(row, levelData, dataIndex, showSummary, completedStep, playSummaryTypewriter);
    }

    private void UpdateRow(
        GoalNoteRowBinding row,
        LevelDataAsset data,
        int dataIndex,
        bool showSummary,
        GoalNoteStep? emphasizedStep = null,
        bool playSummaryTypewriter = false)
    {
        if (row.rowRoot != null)
            row.rowRoot.SetActive(true);

        bool isStep1Done = IsStep1Completed(data.levelID, row.goalID);
        Action onDescriptionClick = () => OnDescriptionTextClicked(row.goalID);
        Action onSummaryClick = () => OnSummaryTextClicked(row.goalID);

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

        if (emphasizedStep == GoalNoteStep.Step1)
            textAnimator.PlayTextEmphasis(row.descriptionText, GoalNoteTextAnimator.TextRole.Description);
        else if (emphasizedStep == GoalNoteStep.Step2)
            textAnimator.PlayTextEmphasis(row.summaryText, GoalNoteTextAnimator.TextRole.Summary);
    }

    private void ResetPagingState()
    {
        currentPage = 0;
        pageCount = 1;
    }

    private void UpdatePagingUI()
    {
        if (prevButton != null)
            prevButton.interactable = currentPage > 0;

        if (nextButton != null)
            nextButton.interactable = currentPage < pageCount - 1;

        if (pageText != null)
            pageText.text = (currentPage + 1) + "/" + pageCount;
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


}
