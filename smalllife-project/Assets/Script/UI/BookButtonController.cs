using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BookButtonController : MonoBehaviour
{
    [SerializeField] private GameObject highlightRing; // bookbutton 外圈光效
    [SerializeField] private float highlightFadeDuration = 1.0f; // 闪烁速度
    private Button button;
    private CanvasGroup ringCanvasGroup;
    private Tween ringTween;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnBookClicked);
    }

    private void Start()
    {
        if (highlightRing != null)
        {
            ringCanvasGroup = highlightRing.GetComponent<CanvasGroup>();
            if (ringCanvasGroup == null)
                ringCanvasGroup = highlightRing.AddComponent<CanvasGroup>();
        }
        // 从存档加载状态
        bool shouldHighlight = SaveSystem.HasNewDiaryContent();
        SetHighlight(shouldHighlight);
    }

    private void OnBookClicked()
    {
        if (highlightRing.activeSelf)
        {
            // 玩家点击后清除高亮
            SaveSystem.MarkNewDiaryContent(false);
            SetHighlight(false);
        }
        // 播放打开日记本的逻辑，比如切换场景
        AudioHub.Instance.PlayGlobal("click_confirm");
    }

    public void SetHighlight(bool val)
    {
        if (highlightRing == null) return;

        highlightRing.SetActive(val);

        if (val)
        {
            Debug.Log("[BookButton] Highlight ON");
            StartHighlightAnimation();
        }
        else
        {
            Debug.Log("[BookButton] Highlight OFF");
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
}
