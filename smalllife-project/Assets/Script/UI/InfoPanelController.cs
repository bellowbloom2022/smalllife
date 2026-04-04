using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;
using DG.Tweening;

public class InfoPanelController : BasePanel
{
    public Image popupImage;
    public Sprite popupSprite;
    public Button closeButton;

    [Header("Folded State")]
    public Button edgePeekButton;
    public RectTransform animatedRoot;
    public float foldedPeekWidth = 56f;
    public float foldedSlideDistanceOverride = 627f;
    [Range(0.05f, 0.5f)] public float foldedVisibleRatio = 0.2f;
    public float panelSlideDuration = 0.3f;
    public Ease panelSlideEase = Ease.OutCubic;

    [Header("Completion Mode")]
    public Image checkmarkImage;
    public Text nextLevelNameText;
    public Button nextButton;
    public float completionRevealStartDelay = 0.14f;
    public float completionRevealGap = 0.16f;
    public float completionFadeDuration = 0.18f;

    private RectTransform panelRect;
    private Vector2 expandedAnchoredPosition;
    private Vector2 foldedAnchoredPosition;
    private Tween panelTween;
    private Sequence completionSequence;
    private SceneChanger pendingSceneChanger;
    private bool isCompletionMode;
    private bool isExpanded;
    private bool isInitialized;
    private Vector2 lastParentRectSize;
    private CanvasGroup nextLevelNameCanvasGroup;
    private CanvasGroup nextButtonCanvasGroup;

    private void Awake()
    {
        gameObject.SetActive(true);

        panelRect = animatedRoot != null ? animatedRoot : transform as RectTransform;
        nextLevelNameCanvasGroup = EnsureCanvasGroup(nextLevelNameText != null ? nextLevelNameText.gameObject : null);
        nextButtonCanvasGroup = EnsureCanvasGroup(nextButton != null ? nextButton.gameObject : null);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(HandleNextButtonClicked);
            nextButton.onClick.AddListener(HandleNextButtonClicked);
        }

        if (edgePeekButton != null)
        {
            edgePeekButton.onClick.RemoveListener(ExpandCurrentMode);
            edgePeekButton.onClick.AddListener(ExpandCurrentMode);
        }

        ApplyDefaultSprite();
        ResetToNormalMode();
    }

    private void Start()
    {
        RecalculateAnchoredPositions();
        FoldImmediate();
        isInitialized = true;
    }

    public override void Show()
    {
        OpenFromSignboard();
    }

    public void OpenFromSignboard()
    {
        ResetToNormalMode();
        ExpandPanel();
    }

    private void OnEnable()
    {
        InputRouter.OnBlankClick += TryHide;
    }

    protected override void OnDisable()
    {
        InputRouter.OnBlankClick -= TryHide;
        panelTween?.Kill();
        completionSequence?.Kill();

        pendingSceneChanger = null;
        base.OnDisable();
    }

    private void OnDestroy()
    {
        panelTween?.Kill();
        completionSequence?.Kill();

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(HandleNextButtonClicked);

        if (edgePeekButton != null)
            edgePeekButton.onClick.RemoveListener(ExpandCurrentMode);
    }

    public void ShowAsCompletion(LevelDataAsset nextLevelData, SceneChanger nextSceneChanger)
    {
        ResetToNormalMode();
        isCompletionMode = true;

        pendingSceneChanger = nextSceneChanger;
        if (pendingSceneChanger == null)
            Debug.LogWarning("InfoPanelController: next SceneChanger is null, next button will be hidden.");

        bool shouldShowTitle = false;
        if (nextLevelNameText != null)
        {
            bool hasNextLevelName = nextLevelData != null && !string.IsNullOrEmpty(nextLevelData.titleKey);
            string fallbackSceneName = pendingSceneChanger != null ? pendingSceneChanger.targetSceneName : string.Empty;
            shouldShowTitle = hasNextLevelName || !string.IsNullOrEmpty(fallbackSceneName);

            if (hasNextLevelName)
            {
                string localizedTitle = LeanLocalization.GetTranslationText(nextLevelData.titleKey);
                nextLevelNameText.text = string.IsNullOrEmpty(localizedTitle)
                    ? nextLevelData.titleKey
                    : localizedTitle;
            }
            else if (!string.IsNullOrEmpty(fallbackSceneName))
            {
                nextLevelNameText.text = fallbackSceneName;
            }
        }
        else
        {
            Debug.LogWarning("InfoPanelController: nextLevelNameText is not assigned in Inspector.");
        }

        bool shouldShowNextButton = false;
        if (nextButton != null)
            shouldShowNextButton = pendingSceneChanger != null;
        else
            Debug.LogWarning("InfoPanelController: nextButton is not assigned in Inspector.");

        ExpandPanel();
        PlayCompletionRevealSequence(shouldShowTitle, shouldShowNextButton);
    }

    public override void Hide()
    {
        FoldPanel();
    }

    private void ApplyDefaultSprite()
    {
        if (popupImage != null && popupSprite != null)
            popupImage.sprite = popupSprite;
    }

    private void ResetToNormalMode()
    {
        ApplyDefaultSprite();
        isCompletionMode = false;
        completionSequence?.Kill();

        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(false);
            checkmarkImage.color = WithAlpha(checkmarkImage.color, 1f);
        }

        if (nextLevelNameText != null)
        {
            nextLevelNameText.gameObject.SetActive(false);
            nextLevelNameText.text = string.Empty;
        }

        if (nextLevelNameCanvasGroup != null)
            nextLevelNameCanvasGroup.alpha = 0f;

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (nextButtonCanvasGroup != null)
            nextButtonCanvasGroup.alpha = 0f;

        pendingSceneChanger = null;
    }

    private void RecalculateAnchoredPositions()
    {
        if (panelRect == null)
            return;

        // 在初始化前记录一次展开基准，避免分辨率变化时被当前折叠位置覆盖。
        if (!isInitialized)
            expandedAnchoredPosition = panelRect.anchoredPosition;

        float foldedTargetX;
        if (foldedSlideDistanceOverride > 0f)
        {
            // 兼容旧配置：当填写覆盖值时，直接作为 folded 的绝对 anchoredPosition.x。
            foldedTargetX = foldedSlideDistanceOverride;
        }
        else
        {
            float panelWidth = GetPanelWidthInParentSpace();
            // 目标：在右侧只保留一部分可见区域。
            float visibleWidth = Mathf.Max(foldedPeekWidth, panelWidth * foldedVisibleRatio);
            foldedTargetX = Mathf.Max(0f, panelWidth - visibleWidth);
        }

        foldedAnchoredPosition = new Vector2(foldedTargetX, expandedAnchoredPosition.y);

        RectTransform parentRect = panelRect.parent as RectTransform;
        if (parentRect != null)
            lastParentRectSize = parentRect.rect.size;
    }

    private float GetPanelWidthInParentSpace()
    {
        if (panelRect == null)
            return 0f;

        RectTransform parentRect = panelRect.parent as RectTransform;
        if (parentRect == null)
            return panelRect.rect.width;

        float panelScaleX = panelRect.lossyScale.x;
        float parentScaleX = parentRect.lossyScale.x;
        if (Mathf.Approximately(parentScaleX, 0f))
            return panelRect.rect.width;

        float widthInParentSpace = panelRect.rect.width * (panelScaleX / parentScaleX);
        return Mathf.Abs(widthInParentSpace);
    }

    private void ExpandCurrentMode()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
        ExpandPanel();
    }

    private void ExpandPanel()
    {
        if (panelRect == null)
            return;

        bool wasExpanded = isExpanded;

        if (!wasExpanded)
            AudioHub.Instance.PlayGlobal("button_flip_info");

        isExpanded = true;
        edgePeekButton?.gameObject.SetActive(false);
        MovePanel(expandedAnchoredPosition, panelSlideDuration);
    }

    private void FoldPanel()
    {
        if (panelRect == null)
            return;

        isExpanded = false;
        completionSequence?.Kill();
        MovePanel(foldedAnchoredPosition, panelSlideDuration);

        if (edgePeekButton != null)
            edgePeekButton.gameObject.SetActive(true);
    }

    private void FoldImmediate()
    {
        if (panelRect == null)
            return;

        isExpanded = false;
        edgePeekButton?.gameObject.SetActive(true);
        panelRect.anchoredPosition = foldedAnchoredPosition;
    }

    private void MovePanel(Vector2 targetPos, float duration)
    {
        panelTween?.Kill();
        panelTween = panelRect
            .DOAnchorPos(targetPos, duration)
            .SetEase(panelSlideEase)
            .SetUpdate(true)
            .OnComplete(() => panelTween = null);
    }

    private void PlayCompletionRevealSequence(bool showNextLevelName, bool showNextButton)
    {
        completionSequence?.Kill();
        completionSequence = DOTween.Sequence().SetUpdate(true);

        if (completionRevealStartDelay > 0f)
            completionSequence.AppendInterval(completionRevealStartDelay);

        if (checkmarkImage != null)
        {
            completionSequence.AppendCallback(() =>
            {
                AudioHub.Instance.PlayGlobal("level_complete");
                checkmarkImage.gameObject.SetActive(true);
                checkmarkImage.color = WithAlpha(checkmarkImage.color, 0f);
            });
            completionSequence.Append(checkmarkImage.DOFade(1f, completionFadeDuration));
            completionSequence.AppendInterval(completionRevealGap);
        }

        if (showNextLevelName && nextLevelNameText != null)
        {
            completionSequence.AppendCallback(() =>
            {
                nextLevelNameText.gameObject.SetActive(true);
                if (nextLevelNameCanvasGroup != null)
                    nextLevelNameCanvasGroup.alpha = 0f;
            });
            if (nextLevelNameCanvasGroup != null)
                completionSequence.Append(nextLevelNameCanvasGroup.DOFade(1f, completionFadeDuration));
            completionSequence.AppendInterval(completionRevealGap);
        }

        if (showNextButton && nextButton != null)
        {
            completionSequence.AppendCallback(() =>
            {
                nextButton.gameObject.SetActive(true);
                if (nextButtonCanvasGroup != null)
                    nextButtonCanvasGroup.alpha = 0f;
            });
            if (nextButtonCanvasGroup != null)
                completionSequence.Append(nextButtonCanvasGroup.DOFade(1f, completionFadeDuration));
        }

        completionSequence.OnComplete(() => completionSequence = null);
    }

    private static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        if (target == null)
            return null;

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = target.AddComponent<CanvasGroup>();

        return canvasGroup;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void HandleNextButtonClicked()
    {
        if (pendingSceneChanger == null)
        {
            Debug.LogWarning("InfoPanelController: SceneChanger not assigned for next button.");
            return;
        }

        pendingSceneChanger.ChangeScene();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isInitialized || panelRect == null)
            return;

        RectTransform parentRect = panelRect.parent as RectTransform;
        if (parentRect == null)
            return;

        Vector2 currentSize = parentRect.rect.size;
        if (currentSize == lastParentRectSize)
            return;

        Vector2 currentPos = panelRect.anchoredPosition;
        RecalculateAnchoredPositions();

        float distanceToExpanded = Vector2.Distance(currentPos, expandedAnchoredPosition);
        float distanceToFolded = Vector2.Distance(currentPos, foldedAnchoredPosition);
        isExpanded = distanceToExpanded <= distanceToFolded;

        panelTween?.Kill();
        panelRect.anchoredPosition = isExpanded ? expandedAnchoredPosition : foldedAnchoredPosition;

        if (edgePeekButton != null)
            edgePeekButton.gameObject.SetActive(!isExpanded);
    }

    private void TryHide()
    {
        if (isExpanded)
            FoldPanel();
    }
}
