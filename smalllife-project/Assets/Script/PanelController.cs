using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public RectTransform panel; // 设置面板的 RectTransform
    public Button closeButton;  // 关闭设置面板的按钮

    public GameObject[] backgroundUIElements; // 需要移动的背景 UI 元素们
    private bool isPanelOpen = false; // 用来记录面板是否打开

    void Start()
    {   
        // 给关闭按钮添加监听器
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        // 确保初始化时设置面板是关闭的
        panel.gameObject.SetActive(false);
    }

    // 打开/关闭设置面板并移动背景 UI
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;

        if (isPanelOpen)
        {
            // 打开面板
            panel.gameObject.SetActive(true);

            // 背景UI消失
            foreach(GameObject bgUI in backgroundUIElements)
            {
                bgUI.SetActive(false);
            }
        }
        else
        {
            // 关闭设置面板
            panel.gameObject.SetActive(false);

            foreach (GameObject bgUI in backgroundUIElements)
            {
                bgUI.SetActive(true);
            }
        }
    }

    private void OnCloseButtonClicked()
    {
        TogglePanel();
    }
}

