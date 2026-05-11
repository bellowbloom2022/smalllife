using System.Collections;
using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;

public class WhisperHintManager : MonoBehaviour
{
    public static WhisperHintManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform hintRectTransform;
    [SerializeField] private Text hintText;
    [SerializeField] private LeanLocalizedText localizedText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas targetCanvas;

    [Header("显示位置")]
    [SerializeField] private Transform globalHintAnchor;
    [SerializeField] private Vector2 worldAnchorScreenOffset = new Vector2(0f, 32f);
    [SerializeField] private Vector2 canvasPadding = new Vector2(24f, 24f);
    [SerializeField] private float anchorScreenPadding = 24f;

    [Header("时间层级")]
    [SerializeField] private float comfortHintTime = 30f;
    [SerializeField] private float observationHintTime = 60f;
    [SerializeField] private float stageHintTime = 90f;
    [SerializeField] private float initialGraceTime = 3f;

    [Header("显示/关闭")]
    [SerializeField] private float minVisibleDuration = 3f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private bool closeOnHintClick = true;
    [SerializeField] private bool dismissOnPlayerAction = true;
    [SerializeField] private float playerActionDismissDelay = 1f;
    [SerializeField] private float scrollDismissThreshold = 0.01f;

    [Header("Goal-specific 置信度")]
    [SerializeField] private float stageHintMinAttentionScore = 1.4f;

    [Header("30 秒：安心提示 Key")]
    public string[] comfortHintKeys =
    {
        "whisper_comfort_01",
        "whisper_comfort_02",
        "whisper_comfort_03"
    };

    [Header("60 秒：观察方式提示 Key")]
    public string[] observationHintKeys =
    {
        "whisper_observation_01",
        "whisper_observation_02",
        "whisper_observation_03"
    };

    [Header("90 秒：没有明确关注对象时的关卡提示 Key")]
    public string[] stageFallbackHintKeys =
    {
        "whisper_stage_fallback_01",
        "whisper_stage_fallback_02",
        "whisper_stage_fallback_03"
    };

    private bool comfortShown;
    private bool observationShown;
    private bool stageShown;
    private bool hintVisible;
    private float lastProgressTime;
    private float hintShownTime;
    private int hintVariantSeed;
    private Coroutine fadeCoroutine;
    private Coroutine dismissCoroutine;
    private RectTransform canvasRectTransform;
    private InputRouter subscribedRouter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (hintText == null && panelRoot != null)
            hintText = panelRoot.GetComponentInChildren<Text>(true);
        if (hintRectTransform == null && panelRoot != null)
            hintRectTransform = panelRoot.GetComponent<RectTransform>();
        if (hintRectTransform == null && hintText != null)
            hintRectTransform = hintText.GetComponentInParent<RectTransform>();
        if (localizedText == null && hintText != null)
            localizedText = hintText.GetComponent<LeanLocalizedText>();
        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null && hintText != null)
            canvasGroup = hintText.GetComponentInParent<CanvasGroup>();
        if (targetCanvas == null && hintRectTransform != null)
            targetCanvas = hintRectTransform.GetComponentInParent<Canvas>();
        if (targetCanvas != null)
            canvasRectTransform = targetCanvas.transform as RectTransform;
    }

    private void Start()
    {
        ResetIdleTimer();
        SetPanelVisible(false, true);
    }

    private void OnEnable()
    {
        GoalNoteEvents.GoalCompleted += HandleGoalCompleted;
        InputRouter.InstanceReady += HandleInputRouterReady;
        TrySubscribeToRouter(InputRouter.Instance);
    }

    private void OnDisable()
    {
        GoalNoteEvents.GoalCompleted -= HandleGoalCompleted;
        InputRouter.InstanceReady -= HandleInputRouterReady;
        UnsubscribeFromRouter();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        HandleHintDismissInput();

        if (hintVisible)
            return;

        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        float elapsed = Time.time - lastProgressTime;

        if (!comfortShown && elapsed >= comfortHintTime)
        {
            comfortShown = true;
            ShowHint(PickHintKey(comfortHintKeys), globalHintAnchor);
            return;
        }

        if (!observationShown && elapsed >= observationHintTime)
        {
            observationShown = true;
            ShowHint(PickHintKey(observationHintKeys), globalHintAnchor);
            return;
        }

        if (!stageShown && elapsed >= stageHintTime)
        {
            stageShown = true;
            ShowStageHint();
        }
    }

    public void ResetIdleTimer()
    {
        lastProgressTime = Time.time + Mathf.Max(0f, initialGraceTime);
        comfortShown = false;
        observationShown = false;
        stageShown = false;
    }

    public void DismissCurrentHint()
    {
        RequestDismiss(0f);
    }

    private void HandleInputRouterReady(InputRouter router)
    {
        TrySubscribeToRouter(router);
    }

    private void TrySubscribeToRouter(InputRouter router)
    {
        if (router == null || router == subscribedRouter)
            return;

        UnsubscribeFromRouter();
        router.OnClick += HandleSceneClick;
        router.OnDrag += HandleSceneDrag;
        subscribedRouter = router;
    }

    private void UnsubscribeFromRouter()
    {
        if (subscribedRouter == null)
            return;

        subscribedRouter.OnClick -= HandleSceneClick;
        subscribedRouter.OnDrag -= HandleSceneDrag;
        subscribedRouter = null;
    }

    private void HandleSceneClick(Vector3 screenPosition)
    {
        RequestDismissFromPlayerAction();
    }

    private void HandleSceneDrag(Vector3 delta)
    {
        if (delta.sqrMagnitude > 0.01f)
            RequestDismissFromPlayerAction();
    }

    private void HandleGoalCompleted(string levelID, int goalID, GoalNoteStep completedStep)
    {
        if (!IsCurrentLevel(levelID))
            return;

        ResetIdleTimer();
        RequestDismiss(0f);
    }

    private void ShowStageHint()
    {
        GoalAttentionTracker tracker = GoalAttentionTracker.Instance;
        if (tracker != null && tracker.TryGetBestGoal(stageHintMinAttentionScore, out Goal goal, out GoalHintStage stage, out _))
        {
            GoalWhisperHintConfig config = goal.GetComponent<GoalWhisperHintConfig>();
            if (config != null)
            {
                string hintKey = config.GetHintKey(stage, ++hintVariantSeed);
                if (!string.IsNullOrWhiteSpace(hintKey))
                {
                    ShowHint(hintKey, config.GetHintAnchor(stage));
                    return;
                }
            }
        }

        ShowHint(PickHintKey(stageFallbackHintKeys), globalHintAnchor);
    }

    private string PickHintKey(string[] hintKeys)
    {
        if (hintKeys == null || hintKeys.Length == 0)
            return string.Empty;

        int validCount = 0;
        for (int i = 0; i < hintKeys.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(hintKeys[i]))
                validCount++;
        }

        if (validCount == 0)
            return string.Empty;

        int target = Mathf.Abs(++hintVariantSeed) % validCount;
        for (int i = 0; i < hintKeys.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(hintKeys[i]))
                continue;

            if (target == 0)
                return hintKeys[i];

            target--;
        }

        return string.Empty;
    }

    private void ShowHint(string localizationKey, Transform anchor)
    {
        if (string.IsNullOrWhiteSpace(localizationKey) || hintText == null)
            return;

        StopDismissCoroutine();
        StopFadeCoroutine();

        ApplyHintPosition(ResolveDisplayAnchor(anchor));
        ApplyLocalizedText(localizationKey);
        hintShownTime = Time.time;
        hintVisible = true;
        SetPanelVisible(true, false);
        fadeCoroutine = StartCoroutine(FadeTo(1f));
    }

    private Transform ResolveDisplayAnchor(Transform requestedAnchor)
    {
        if (requestedAnchor == null)
            return globalHintAnchor;

        RectTransform anchorRect = requestedAnchor as RectTransform;
        if (anchorRect != null && anchorRect.GetComponentInParent<Canvas>() == targetCanvas)
            return requestedAnchor;

        return IsWorldAnchorVisible(requestedAnchor) ? requestedAnchor : globalHintAnchor;
    }

    private bool IsWorldAnchorVisible(Transform anchor)
    {
        if (anchor == null)
            return false;

        Camera camera = Camera.main;
        if (camera == null)
            return false;

        Vector3 screenPoint = camera.WorldToScreenPoint(anchor.position);
        if (screenPoint.z <= 0f)
            return false;

        float padding = Mathf.Max(0f, anchorScreenPadding);
        return screenPoint.x >= padding && screenPoint.x <= Screen.width - padding
            && screenPoint.y >= padding && screenPoint.y <= Screen.height - padding;
    }

    private void ApplyLocalizedText(string localizationKey)
    {
        if (localizedText != null)
        {
            localizedText.FallbackText = localizationKey;
            localizedText.TranslationName = localizationKey;
            localizedText.UpdateLocalization();
            return;
        }

        string localized = LeanLocalization.GetTranslationText(localizationKey);
        hintText.text = string.IsNullOrEmpty(localized) ? localizationKey : localized;
    }

    private void ApplyHintPosition(Transform anchor)
    {
        if (hintRectTransform == null)
            return;

        Transform targetAnchor = anchor != null ? anchor : globalHintAnchor;
        if (targetAnchor == null)
            return;

        RectTransform anchorRect = targetAnchor as RectTransform;
        if (anchorRect != null && anchorRect.GetComponentInParent<Canvas>() == targetCanvas)
        {
            hintRectTransform.position = anchorRect.position;
            return;
        }

        if (targetCanvas == null || canvasRectTransform == null)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 screenPoint = camera.WorldToScreenPoint(targetAnchor.position);
        if (screenPoint.z < 0f)
            return;

        screenPoint.x += worldAnchorScreenOffset.x;
        screenPoint.y += worldAnchorScreenOffset.y;

        Camera uiCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera;
        if (uiCamera == null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = camera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, uiCamera, out Vector2 localPoint))
        {
            Rect rect = canvasRectTransform.rect;
            localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin + canvasPadding.x, rect.xMax - canvasPadding.x);
            localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin + canvasPadding.y, rect.yMax - canvasPadding.y);
            hintRectTransform.anchoredPosition = localPoint;
        }
    }

    private void HandleHintDismissInput()
    {
        if (!hintVisible)
            return;

        if (closeOnHintClick && Input.GetMouseButtonDown(0) && IsPointerInsideHint())
        {
            RequestDismiss(0f);
            return;
        }

        if (!dismissOnPlayerAction)
            return;

        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > scrollDismissThreshold)
            RequestDismissFromPlayerAction();
    }

    private bool IsPointerInsideHint()
    {
        if (hintRectTransform == null)
            return false;

        Camera uiCamera = null;
        if (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = targetCanvas.worldCamera != null ? targetCanvas.worldCamera : Camera.main;

        return RectTransformUtility.RectangleContainsScreenPoint(hintRectTransform, Input.mousePosition, uiCamera);
    }

    private void RequestDismissFromPlayerAction()
    {
        if (!dismissOnPlayerAction)
            return;

        RequestDismiss(playerActionDismissDelay);
    }

    private void RequestDismiss(float delay)
    {
        if (!hintVisible || dismissCoroutine != null)
            return;

        float remainingMinTime = Mathf.Max(0f, hintShownTime + Mathf.Max(0f, minVisibleDuration) - Time.time);
        float finalDelay = Mathf.Max(remainingMinTime, Mathf.Max(0f, delay));
        dismissCoroutine = StartCoroutine(DismissRoutine(finalDelay));
    }

    private IEnumerator DismissRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        StopFadeCoroutine();
        fadeCoroutine = StartCoroutine(FadeTo(0f));
        yield return fadeCoroutine;
        fadeCoroutine = null;
        SetPanelVisible(false, true);
        hintVisible = false;
        dismissCoroutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (canvasGroup == null || fadeDuration <= 0f)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = targetAlpha;
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(t));
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void StopFadeCoroutine()
    {
        if (fadeCoroutine == null)
            return;

        StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;
    }

    private void StopDismissCoroutine()
    {
        if (dismissCoroutine == null)
            return;

        StopCoroutine(dismissCoroutine);
        dismissCoroutine = null;
    }

    private void SetPanelVisible(bool visible, bool applyAlpha)
    {
        GameObject root = panelRoot != null ? panelRoot : hintText != null ? hintText.gameObject : null;
        if (root != null)
            root.SetActive(visible);

        if (canvasGroup != null)
        {
            if (applyAlpha && !visible)
                canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }

    private bool IsCurrentLevel(string levelID)
    {
        if (Level.ins == null || Level.ins.levelDataAsset == null || string.IsNullOrEmpty(levelID))
            return true;

        return GoalProgressRules.IsSameLevelID(levelID, Level.ins.levelDataAsset.levelID);
    }
}
