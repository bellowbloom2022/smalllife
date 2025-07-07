using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PreviewImageHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Tween scaleTween;

    private void Awake(){
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        scaleTween = rectTransform.DOScale(originalScale * 1.05f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData){
        if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
        scaleTween = rectTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
    }
}
