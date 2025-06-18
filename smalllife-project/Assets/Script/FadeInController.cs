using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeInController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Start(){
        fadeImage.color = new Color(1,1,1,1);

        fadeImage.DOFade(0, fadeDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => fadeImage.gameObject.SetActive(false));
    }
}
