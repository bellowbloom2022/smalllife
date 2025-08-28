using UnityEngine;
using DG.Tweening;

public class Level0TutorialStarter : MonoBehaviour
{
    public HowToPlayPanel howToPlayPanel;   // 拖入场景里的那份 HowToPlayPanel
    public RectTransform cornerAnchor;      // UI 左上角的目标锚点（比如建一个空的UI对象放左上角）

    private bool hasClosed = false;         // 保证只执行一次动画

    private void Start()
    {
        Debug.Log("Level0TutorialStarter Start running!");
        if (howToPlayPanel != null)
        {
            howToPlayPanel.Show();
            // 安全调用 KillShowTween 而不是直接访问 showTween
            howToPlayPanel.closeButton.onClick.AddListener(OnCloseTutorial);
        }
    }

    private void OnCloseTutorial()
    {
        if (hasClosed) return; 
        hasClosed = true;

        // 先杀掉内部 Tween
        howToPlayPanel.KillShowTween();

        // 设置飞角落锚点
        howToPlayPanel.flyToAnchorOnClose = cornerAnchor;

        // 调用 Hide，内部会处理飞角落动画
        howToPlayPanel.Hide();
    }
}
