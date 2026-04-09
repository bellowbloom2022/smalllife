using UnityEngine;

public class SignboardTrigger : MonoBehaviour
{
    public InfoPanelController infoPanel; // 拖入 Canvas 中的 InfoPanel
    private static int lastSignboardPointerDownFrame = -1;

    public static bool WasPointerDownOnSignboardThisFrame()
    {
        return lastSignboardPointerDownFrame == Time.frameCount;
    }

    private void OnMouseDown()
    {
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        lastSignboardPointerDownFrame = Time.frameCount;
    }

    private void OnMouseUp()
    {
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        // 点击被 UI 消费时，不触发场景路牌交互。
        if (UIBlockChecker.IsPointerOverUI() || BasePanel.IsPointerOverAnyShownPanel(Input.mousePosition))
            return;

        if (infoPanel == null)
            return;

        if (infoPanel.IsExpanded)
            infoPanel.Hide();
        else
            infoPanel.OpenFromSignboard();
    }
}
