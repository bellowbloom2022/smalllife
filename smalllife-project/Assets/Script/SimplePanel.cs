using UnityEngine;
using UnityEngine.UI;

public class SimplePanel : BasePanel
{
    public Button openButton;
    public Button closeButton;

    private void Start()
    {
        if (openButton != null) openButton.onClick.AddListener(Show);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }
}
