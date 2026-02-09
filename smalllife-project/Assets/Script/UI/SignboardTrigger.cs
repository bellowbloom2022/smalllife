using UnityEngine;

public class SignboardTrigger : MonoBehaviour
{
    public HowToPlayPanel howToPlayPanel; // 拖入 Canvas 中的 howToPlayPanel

    private void OnMouseUp()
    {
        if (howToPlayPanel != null && !howToPlayPanel.IsShown)
        {
            howToPlayPanel.Show();
        }
    }
}
