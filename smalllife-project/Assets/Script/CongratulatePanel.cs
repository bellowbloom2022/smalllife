using UnityEngine;
using UnityEngine.UI;

public class CongratulatePanel : BasePanel
{
    public Button closeButton; // ��ѡ���رհ�ť

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                AudioHub.Instance.PlayGlobal("back_confirm");
                Hide(); // ����BasePanel������
            });
        }
    }

    public override void Show()
    {
        base.Show(); // Ĭ���������
    }

    public override void Hide()
    {
        base.Hide(); // Ĭ���������
    }
}
