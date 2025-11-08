using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BagButtonController : MonoBehaviour
{
    public static BagButtonController Instance;

    [SerializeField] private GameObject highlightRing; // bag 外圈光效
    [SerializeField] private RectTransform sidebar;    // ScrollView-sidebar
    [SerializeField] private GameObject sideBarHint; 
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private float highlightFadeDuration = 1.0f; // 闪烁时间
    [SerializeField] private float hintDelay = 0.6f;
    [SerializeField] private float hintFadeDuration = 0.5f;

    private bool isOpen = false;
    private Vector2 sidebarHiddenPos;
    private Vector2 sidebarShownPos;
    private CanvasGroup ringCanvasGroup;
    private CanvasGroup hintCanvasGroup;
    private Tween ringTween;
    private Tween hintTween;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 假设 sidebar 是锚在右侧的，可以用 anchoredPosition 控制
        sidebarShownPos = sidebar.anchoredPosition;
        sidebarHiddenPos = sidebarShownPos + new Vector2(sidebar.rect.width, 0);
        sidebar.anchoredPosition = sidebarHiddenPos;

        if (highlightRing != null)
        {
            ringCanvasGroup = highlightRing.GetComponent<CanvasGroup>();
            if (ringCanvasGroup == null)
                ringCanvasGroup = highlightRing.AddComponent<CanvasGroup>();
        }
        
        if (sideBarHint != null)
        {
            hintCanvasGroup = sideBarHint.GetComponent<CanvasGroup>();
            if (hintCanvasGroup == null)
                hintCanvasGroup = sideBarHint.AddComponent<CanvasGroup>();
            sideBarHint.SetActive(false);
            hintCanvasGroup.alpha = 0f;
        }

        SetHighlight(false);

        GetComponent<Button>().onClick.AddListener(ToggleSidebar);
    }

    public void SetHighlight(bool val)
    {
        if (highlightRing != null)
            highlightRing.SetActive(val);

        if (val)
        {
            Debug.Log("[BagButton] Highlight ON");
            StartHighlightAnimation();
        }
        else
        {
            Debug.Log("[BagButton] Highlight OFF");
            StopHighlightAnimation();
        }
    }
    
    private void StartHighlightAnimation()
    {
        if (ringCanvasGroup == null) return;
        ringTween?.Kill();
        ringCanvasGroup.alpha = 1f;
        ringTween = ringCanvasGroup.DOFade(0.3f, highlightFadeDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopHighlightAnimation()
    {
        ringTween?.Kill();
        if (ringCanvasGroup != null)
            ringCanvasGroup.alpha = 1f;
    }

    private void ToggleSidebar()
    {
        if (isOpen)
        {
            AudioHub.Instance.PlayGlobal("click_confirm");
            sidebar.DOAnchorPos(sidebarHiddenPos, animDuration).SetEase(Ease.InOutQuad);
            HideHint();
            isOpen = false;
        }
        else
        {
            AudioHub.Instance.PlayGlobal("click_confirm");
            sidebar.DOAnchorPos(sidebarShownPos, animDuration).SetEase(Ease.InOutQuad);
            ShowHintWithDelay();
            isOpen = true;
        }
    }

    private void ShowHintWithDelay()
    {
        if (sideBarHint == null || hintCanvasGroup == null) return;
        hintTween?.Kill();

        sideBarHint.SetActive(true);
        hintCanvasGroup.alpha = 0f;
        hintTween = DOVirtual.DelayedCall(hintDelay, () =>
        {
            hintCanvasGroup.DOFade(1f, hintFadeDuration).SetEase(Ease.OutQuad);
        });
    }

    private void HideHint()
    {
        if (sideBarHint == null || hintCanvasGroup == null) return;
        hintTween?.Kill();
        hintCanvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            sideBarHint.SetActive(false);
        });
    }
}
