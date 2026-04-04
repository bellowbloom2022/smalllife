using UnityEngine;
using UnityEngine.UI;

public class OverlayOrderLocker : MonoBehaviour
{
    private Canvas overlayCanvas;
    private int baseSortingOrder;
    private string whiteFadePanelName;

    private Canvas cachedWhiteFadeCanvas;
    private float nextSearchTime;

    public void Configure(Canvas canvas, int baseOrder, string whiteFadeName)
    {
        overlayCanvas = canvas;
        baseSortingOrder = baseOrder;
        whiteFadePanelName = whiteFadeName;
        nextSearchTime = 0f;
        ApplyOrder();
    }

    private void LateUpdate()
    {
        ApplyOrder();
    }

    private void ApplyOrder()
    {
        if (overlayCanvas == null)
            overlayCanvas = GetComponent<Canvas>();

        if (overlayCanvas == null)
            return;

        if (Time.unscaledTime >= nextSearchTime)
        {
            nextSearchTime = Time.unscaledTime + 0.5f;
            GameObject whiteFade = GameObject.Find(whiteFadePanelName);
            cachedWhiteFadeCanvas = whiteFade != null ? whiteFade.GetComponentInParent<Canvas>() : null;
        }

        int targetOrder = baseSortingOrder;
        if (cachedWhiteFadeCanvas != null)
        {
            int whiteOrder = cachedWhiteFadeCanvas.overrideSorting ? cachedWhiteFadeCanvas.sortingOrder : 0;
            targetOrder = Mathf.Max(targetOrder, whiteOrder + 1);
        }

        if (!overlayCanvas.overrideSorting)
            overlayCanvas.overrideSorting = true;

        if (overlayCanvas.sortingOrder != targetOrder)
            overlayCanvas.sortingOrder = targetOrder;

        transform.SetAsLastSibling();
    }
}
