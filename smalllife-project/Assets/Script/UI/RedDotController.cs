using UnityEngine;
using DG.Tweening;

public class RedDotController : MonoBehaviour
{
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private bool isVisible = false;
    private Transform dotTransform;

    void Awake()
    {
        dotTransform = transform;
        dotTransform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        if (isVisible) return;
        isVisible = true;

        gameObject.SetActive(true);
        dotTransform.localScale = Vector3.zero;
        dotTransform.DOScale(1f, scaleDuration).SetEase(showEase);
    }

    public void Hide()
    {
        if (!isVisible) return;
        isVisible = false;

        dotTransform.DOScale(0f, scaleDuration)
            .SetEase(hideEase)
            .OnComplete(() => gameObject.SetActive(false));
    }

    public void SetVisible(bool visible)
    {
        if (visible) Show();
        else Hide();
    }
}
