using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HowToPlayPanel : BasePanel
{
    public GameObject[] images; // �ĸ�����ͼ
    public Text[] texts; // ��Ӧ����
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;
    public TMP_Text pageNumberText;

    private int currentPage = 0;
    private int maxPages;
    
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
        base.Show(); // ���� BasePanel �Ķ���
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
        base.Hide();
    }
}
