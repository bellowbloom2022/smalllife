using UnityEngine;
using UnityEngine.UI;

public class CongratulatePanel : BasePanel
{
    public Button closeButton; // 可选：关闭按钮

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                AudioHub.Instance.PlayGlobal("back_confirm");
                Hide(); // 调用BasePanel的隐藏
            });
        }
    }

    public override void Show()
    {
        base.Show(); // 默认启用面板
    }

    public override void Hide()
    {
        base.Hide(); // 默认隐藏面板
    }
}
