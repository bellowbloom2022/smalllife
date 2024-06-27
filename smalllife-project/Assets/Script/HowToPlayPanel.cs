using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HowToPlayPanel : MonoBehaviour
{
    public GameObject[] images; // 存储四个HowToPlay的图片对象
    public Text[] texts; // 存储对应的文本对象
    public Button prevButton; // 左按钮
    public Button nextButton; // 右按钮
    public Button closeButton; // 关闭按钮
    public TMP_Text pageNumberText; // 显示页码的文本对象
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
        gameObject.SetActive(false); // 关闭panel
    }

    public void OpenPanel()
    {
        currentPage = 0; // 打开时重置到第一页
        gameObject.SetActive(true);
        UpdateUI();
    }
}
