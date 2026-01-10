using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FocusMaskController : MonoBehaviour
{
    public static FocusMaskController Instance;

    [SerializeField] Image maskImage;
    [SerializeField] Material maskMaterial;

    Transform followTarget;
    Camera cam;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        maskImage.enabled = false;
    }

    void Update()
    {
        if (followTarget == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(followTarget.position);
        Vector2 uv = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );

        maskMaterial.SetVector("_FocusCenter", uv);
    }

    public void Show(Transform target, float radius, float duration)
    {
        followTarget = target;
        maskImage.enabled = true;

        maskMaterial.SetFloat("_Radius", 0f);

        DOTween.To(
            () => maskMaterial.GetFloat("_Radius"),
            r => maskMaterial.SetFloat("_Radius", r),
            radius,
            duration
        );
    }

    public void Hide(float duration)
    {
        DOTween.To(
            () => maskMaterial.GetFloat("_Radius"),
            r => maskMaterial.SetFloat("_Radius", r),
            0f,
            duration
        ).OnComplete(() =>
        {
            followTarget = null;
            maskImage.enabled = false;
        });
    }
}
