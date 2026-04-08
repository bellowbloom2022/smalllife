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
    Tween radiusTween;

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
        StopRadiusTween();
        followTarget = target;
        maskImage.enabled = true;

        maskMaterial.SetFloat("_Radius", 0f);

        radiusTween = DOTween.To(
            () => maskMaterial.GetFloat("_Radius"),
            r => maskMaterial.SetFloat("_Radius", r),
            radius,
            duration
        );
    }

    public void Hide(float duration, FocusHideMode hideMode = FocusHideMode.LegacyShrink)
    {
        StopRadiusTween();

        float targetRadius = hideMode == FocusHideMode.ExpandToFullThenHide
            ? ComputeFullScreenRadiusFromCurrentCenter()
            : 0f;

        radiusTween = DOTween.To(
            () => maskMaterial.GetFloat("_Radius"),
            r => maskMaterial.SetFloat("_Radius", r),
            targetRadius,
            duration
        ).OnComplete(() =>
        {
            followTarget = null;
            maskImage.enabled = false;
            radiusTween = null;
        });
    }

    float ComputeFullScreenRadiusFromCurrentCenter()
    {
        Vector4 center = maskMaterial.GetVector("_FocusCenter");
        Vector2 uv = new Vector2(center.x, center.y);

        float d0 = Vector2.Distance(uv, new Vector2(0f, 0f));
        float d1 = Vector2.Distance(uv, new Vector2(1f, 0f));
        float d2 = Vector2.Distance(uv, new Vector2(0f, 1f));
        float d3 = Vector2.Distance(uv, new Vector2(1f, 1f));

        return Mathf.Max(d0, d1, d2, d3) + 0.02f;
    }

    void StopRadiusTween()
    {
        if (radiusTween == null)
            return;

        if (radiusTween.IsActive())
            radiusTween.Kill();

        radiusTween = null;
    }
}
