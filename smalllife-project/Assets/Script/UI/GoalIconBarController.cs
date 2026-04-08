using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GoalIconBarController : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GoalIconBarController Instance { get; private set; }
    public ScrollRect ScrollRectRef => scrollRect;

    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [Header("Focus")]
    [SerializeField] private bool autoFocusEnabled = true;
    [SerializeField] private float focusDuration = 0.28f;
    [SerializeField] private Ease focusEase = Ease.OutCubic;
    [SerializeField, Range(0f, 0.5f)] private float centerDeadZoneNormalized = 0.12f;
    [SerializeField, Min(0)] private int focusRetryFrames = 2;

    [Header("Drag Feel")]
    [SerializeField] private bool applyRecommendedDragFeel = true;
    [SerializeField] private bool lockPositionOnRelease = true;
    [SerializeField] private bool useManualDrag = true;
    [SerializeField] private float dragSensitivity = 1f;

    [Header("Scrollbar")]
    [SerializeField] private bool removeScrollbarsAtRuntime = true;

    private readonly Dictionary<string, GoalIconUIController> iconMap = new Dictionary<string, GoalIconUIController>();

    private Tween focusTween;
    private bool isUserDragging;
    private bool hasPendingFocus;
    private string pendingLevelID;
    private int pendingGoalID;

    private void Awake()
    {
        Instance = this;

        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        if (scrollRect != null)
        {
            if (viewport == null)
                viewport = scrollRect.viewport;
            if (content == null)
                content = scrollRect.content;

            // 统一运行时行为：手动拖拽模式下仅保留 Viewport/Content 组织，不使用 ScrollRect 自带拖拽。
            scrollRect.enabled = !useManualDrag;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;

            if (removeScrollbarsAtRuntime)
            {
                // 用户采用“无滚动条”方案时，运行时彻底解除引用避免 Missing 引用干扰。
                scrollRect.horizontalScrollbar = null;
                scrollRect.verticalScrollbar = null;
            }

            if (applyRecommendedDragFeel)
            {
                // Goal 栏需要“松手停住”的体验，避免强吸附和回弹。
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.elasticity = 0f;
                scrollRect.inertia = !lockPositionOnRelease;
            }
        }
    }

    private void Start()
    {
        RebuildIconCache();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        focusTween?.Kill();
        focusTween = null;
    }

    public void RegisterIcon(GoalIconUIController icon)
    {
        if (icon == null)
            return;

        iconMap[BuildKey(icon.LevelID, icon.GoalID)] = icon;
    }

    public void UnregisterIcon(GoalIconUIController icon)
    {
        if (icon == null)
            return;

        iconMap.Remove(BuildKey(icon.LevelID, icon.GoalID));
    }

    public void RebuildIconCache()
    {
        iconMap.Clear();
        GoalIconUIController[] icons = FindObjectsOfType<GoalIconUIController>(true);
        foreach (GoalIconUIController icon in icons)
        {
            RegisterIcon(icon);
        }
    }

    public bool IsGoalVisible(string levelID, int goalID)
    {
        if (!TryGetIconRect(levelID, goalID, out RectTransform iconRect))
            return false;

        return IsIconVisible(iconRect);
    }

    public bool TryFocusToGoal(string levelID, int goalID, bool animate = true)
    {
        if (!autoFocusEnabled)
            return false;

        if (isUserDragging)
        {
            hasPendingFocus = true;
            pendingLevelID = levelID;
            pendingGoalID = goalID;
            return false;
        }

        return TryFocusToGoalInternal(levelID, goalID, animate, focusRetryFrames);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isUserDragging = true;
        focusTween?.Kill();
        focusTween = null;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (!useManualDrag || scrollRect == null)
            return;

        scrollRect.StopMovement();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!useManualDrag || viewport == null || content == null)
            return;

        float scaleFactor = 1f;
        Canvas canvas = viewport.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.scaleFactor > 0f)
            scaleFactor = canvas.scaleFactor;

        Vector3 localPosition = content.localPosition;
        localPosition.x += (eventData.delta.x / scaleFactor) * dragSensitivity;
        content.localPosition = GetClampedContentPosition(localPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isUserDragging = false;

        if (scrollRect != null && lockPositionOnRelease)
            scrollRect.StopMovement();

        if (content != null)
            content.localPosition = GetClampedContentPosition(content.localPosition);

        if (!hasPendingFocus)
            return;

        hasPendingFocus = false;
        TryFocusToGoalInternal(pendingLevelID, pendingGoalID, true, focusRetryFrames);
    }

    private bool TryFocusToGoalInternal(string levelID, int goalID, bool animate, int retriesLeft)
    {
        if (scrollRect == null || viewport == null || content == null)
            return false;

        if (!TryGetIconRect(levelID, goalID, out RectTransform iconRect))
        {
            if (retriesLeft > 0)
            {
                DOVirtual.DelayedCall(0f, () => TryFocusToGoalInternal(levelID, goalID, animate, retriesLeft - 1));
            }
            return false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        if (IsNearCenter(iconRect))
            return true;

        focusTween?.Kill();

        Vector3 targetPosition = GetFocusedContentPosition(iconRect);

        if (!animate || focusDuration <= 0f)
        {
            content.localPosition = targetPosition;
            return true;
        }

        focusTween = content.DOLocalMove(targetPosition, focusDuration).SetEase(focusEase);

        return true;
    }

    private bool TryGetIconRect(string levelID, int goalID, out RectTransform iconRect)
    {
        iconRect = null;

        string key = BuildKey(levelID, goalID);
        if (!iconMap.TryGetValue(key, out GoalIconUIController icon) || icon == null)
        {
            RebuildIconCache();
            if (!iconMap.TryGetValue(key, out icon) || icon == null)
                return false;
        }

        iconRect = icon.transform as RectTransform;
        return iconRect != null;
    }

    private bool IsNearCenter(RectTransform iconRect)
    {
        Vector3 iconCenterViewport = GetIconCenterInViewport(iconRect);
        float viewportCenterX = viewport.rect.center.x;
        float centerOffset = Mathf.Abs(iconCenterViewport.x - viewportCenterX);

        return centerOffset <= viewport.rect.width * centerDeadZoneNormalized;
    }

    private bool IsIconVisible(RectTransform iconRect)
    {
        Vector3[] corners = new Vector3[4];
        iconRect.GetWorldCorners(corners);

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 local = viewport.InverseTransformPoint(corners[i]);
            minX = Mathf.Min(minX, local.x);
            maxX = Mathf.Max(maxX, local.x);
        }

        return maxX >= viewport.rect.xMin && minX <= viewport.rect.xMax;
    }

    private Vector3 GetFocusedContentPosition(RectTransform iconRect)
    {
        Vector3 iconCenterInViewport = GetIconCenterInViewport(iconRect);
        float centerOffset = viewport.rect.center.x - iconCenterInViewport.x;
        Vector3 desiredPosition = content.localPosition + new Vector3(centerOffset, 0f, 0f);
        return GetClampedContentPosition(desiredPosition);
    }

    private Vector3 GetIconCenterInViewport(RectTransform iconRect)
    {
        Vector3 iconCenterWorld = iconRect.TransformPoint(iconRect.rect.center);
        return viewport.InverseTransformPoint(iconCenterWorld);
    }

    private Vector3 GetClampedContentPosition(Vector3 desiredPosition)
    {
        if (viewport == null || content == null)
            return desiredPosition;

        Vector3 originalPosition = content.localPosition;
        content.localPosition = desiredPosition;

        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);
        Rect viewRect = viewport.rect;
        Vector3 clampedPosition = desiredPosition;

        if (bounds.size.x <= viewRect.width)
        {
            clampedPosition.x += viewRect.center.x - bounds.center.x;
        }
        else
        {
            if (bounds.min.x > viewRect.xMin)
                clampedPosition.x += viewRect.xMin - bounds.min.x;
            else if (bounds.max.x < viewRect.xMax)
                clampedPosition.x += viewRect.xMax - bounds.max.x;
        }

        content.localPosition = originalPosition;
        return clampedPosition;
    }

    private static string BuildKey(string levelID, int goalID)
    {
        return string.Concat(levelID, "#", goalID.ToString());
    }
}
