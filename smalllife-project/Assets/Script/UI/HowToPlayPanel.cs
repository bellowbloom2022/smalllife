using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HowToPlayPanel : BasePanel
{
    public GameObject[] images; // �ĸ�����ͼ
    public Text[] texts; // ��Ӧ����
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;
    public TMP_Text pageNumberText;

    [Header("Level0 Tutorial Special")]
    public RectTransform flyToAnchorOnClose; // ��ѡ��Level0������Ч��

    private int currentPage = 0;
    private int maxPages;
    private Tween showTween; // ��¼�����������ظ�����

    private void OnEnable()
    {
        // ��ȫ��
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
        // ���ź����е�������
        PlayPopupAnimation();
        base.Show(); // ���� BasePanel �Ķ���
    }

    private void PlayPopupAnimation()
    {
        // ��ȷ��֮ǰ�Ķ������ɾ�������
        showTween?.Kill();
        transform.localScale = Vector3.one * 0.8f;
        // ����һ���µ����ж���
        showTween = DOTween.Sequence()
            .Append(transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack))
            .SetUpdate(true) // ��ʹ TimeScale=0 �� GameObject inactive Ҳ�ճ���
            .OnComplete(() => showTween = null); // �����������������
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
            // ���������ê�㣬���ŷɽ��䶯��
            RectTransform panelRect = GetComponent<RectTransform>();
            Sequence seq = DOTween.Sequence();
            seq.Append(panelRect.DOMove(flyToAnchorOnClose.position, 0.3f).SetEase(Ease.InBack));
            seq.Join(panelRect.DOScale(0.1f, 0.3f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                // ���������������
                Destroy(gameObject);
            });
        }
        else
        {
            // ��ͨ PausePanel ֱ������
            base.Hide();
        }
    }

    // ��װ Tween ɱ���������ⲿ���ü���
    public void KillShowTween()
    {
        showTween?.Kill();
        showTween = null;
    }
}
