using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HowToPlayPanel : BasePanel
{
    public GameObject[] images; // 四个引导图
    public Text[] texts; // 对应文字
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;
    public TMP_Text pageNumberText;

    [Header("Level0 Tutorial Special")]
    public RectTransform flyToAnchorOnClose; // 可选，Level0开场特效用

    private int currentPage = 0;
    private int maxPages;
    private Tween showTween; // 记录动画，避免重复播放

    private void OnEnable()
    {
        // 安全绑定
        prevButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);
        closeButton.onClick.AddListener(Hide);

        currentPage = 0;
        UpdateUI();
    }

    public override void Show()
    {
        currentPage = 0;
        UpdateUI();
        AudioHub.Instance.PlayGlobal("click_confirm");
        // 播放呼吸感弹出动画
        PlayPopupAnimation();
        base.Show(); // 播放 BasePanel 的动画
    }

    private void PlayPopupAnimation()
    {
        // 先确保之前的动画被干净地清理
        showTween?.Kill();
        transform.localScale = Vector3.one * 0.8f;
        // 创建一个新的序列动画
        showTween = DOTween.Sequence()
            .Append(transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack))
            .SetUpdate(true) // 即使 TimeScale=0 或 GameObject inactive 也照常跑
            .OnComplete(() => showTween = null); // 动画跑完后把引用清掉
    }

    private void UpdateUI()
    {
        maxPages = images.Length;

        for (int i = 0; i < images.Length; i++)
        {
            images[i].SetActive(i == currentPage);
            texts[i].gameObject.SetActive(i == currentPage);
        }

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < maxPages - 1;

        pageNumberText.text = $"{currentPage + 1}/{maxPages}";
    }

    private void NextPage()
    {
        if (currentPage < maxPages - 1)
        {
            AudioHub.Instance.PlayGlobal("howtoplay-bookflip");
            currentPage++;
            UpdateUI();
        }
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            AudioHub.Instance.PlayGlobal("howtoplay-bookflip");
            currentPage--;
            UpdateUI();
        }
    }

    public override void Hide()
    {
        showTween?.Kill();

        if (flyToAnchorOnClose != null)
        {
            // 如果有特殊锚点，播放飞角落动画
            RectTransform panelRect = GetComponent<RectTransform>();
            Sequence seq = DOTween.Sequence();
            seq.Append(panelRect.DOMove(flyToAnchorOnClose.position, 0.3f).SetEase(Ease.InBack));
            seq.Join(panelRect.DOScale(0.1f, 0.3f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                // 飞完角落后销毁面板
                Destroy(gameObject);
            });
        }
        else
        {
            // 普通 PausePanel 直接隐藏
            base.Hide();
        }
    }

    // 封装 Tween 杀掉方法，外部调用即可
    public void KillShowTween()
    {
        showTween?.Kill();
        showTween = null;
    }
}
