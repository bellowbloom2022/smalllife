using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HowToPlayPanel : MonoBehaviour
{
    public GameObject[] images; // �洢�ĸ�HowToPlay��ͼƬ����
    public Text[] texts; // �洢��Ӧ���ı�����
    public Button prevButton; // ��ť
    public Button nextButton; // �Ұ�ť
    public Button closeButton; // �رհ�ť
    public TMP_Text pageNumberText; // ��ʾҳ����ı�����
    public Button questionButton;

    private int currentPage = 0;
    private int maxPages;

    void Start()
    {
        maxPages = images.Length;

        UpdateUI();
        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);
        closeButton.onClick.AddListener(ClosePanel);
        questionButton.onClick.AddListener(OpenPanel);
    }

    void UpdateUI()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].SetActive(i == currentPage);
            texts[i].gameObject.SetActive(i == currentPage);
        }

        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < maxPages - 1;

        pageNumberText.text = $"{currentPage + 1}/{maxPages}";
    }

    public void NextPage()
    {
        if (currentPage < maxPages - 1)
        {
            currentPage++;
            UpdateUI();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false); // �ر�panel
    }

    public void OpenPanel()
    {
        currentPage = 0; // ��ʱ���õ���һҳ
        gameObject.SetActive(true);
        UpdateUI();
    }
}
