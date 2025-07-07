using UnityEngine;
using UnityEngine.UI;

public class InfoPanelController : BasePanel
{
    public Image popupImage;
    public Sprite popupSprite;
    public Button closeButton;

    private void Start()
    {
        popupImage.sprite = popupSprite;
        closeButton.onClick.AddListener(Hide);
    }

    private void OnEnable()
    {
        InputRouter.OnBlankClick += TryHide;
    }

    private void OnDisable()
    {
        InputRouter.OnBlankClick -= TryHide;
    }

    private void TryHide()
    {
        if (IsShown)
        {
            Hide();
        }
    }
}
