using UnityEngine;

public class SignboardTrigger : MonoBehaviour
{
    public InfoPanelController infoPanel; // 拖入 Canvas 中的 InfoPanel

    private void OnMouseUp()
    {
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        if (infoPanel != null)
            infoPanel.OpenFromSignboard();
    }
}
