using UnityEngine;
using UnityEngine.UI;

public class PopupHandler : MonoBehaviour
{
    public GameObject Popup; // 弹窗对象
    public Button openButton;  // 打开弹窗的按钮
    public Button closeButton; // 关闭弹窗的按钮

    private void Start()
    {
        // 设置按钮的点击事件处理程序
        openButton.onClick.AddListener(OpenPopup);
        closeButton.onClick.AddListener(ClosePopup);
        Popup.SetActive(false); // 默认隐藏弹窗
    }

    private void OpenPopup()
    {
        // 显示弹窗
        Popup.SetActive(true);
    }

    private void ClosePopup()
    {
        // 隐藏弹窗
        Popup.SetActive(false);
    }
}
