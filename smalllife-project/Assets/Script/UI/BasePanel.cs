using UnityEngine;
using System.Collections.Generic;

public class BasePanel : MonoBehaviour
{
    public bool IsShown => gameObject.activeSelf;

    private static readonly List<BasePanel> ActivePanels = new List<BasePanel>();

    public virtual void Show()
    {
        gameObject.SetActive(true);
        AudioHub.Instance.PlayGlobal("click_confirm");

        if (!ActivePanels.Contains(this))
            ActivePanels.Add(this);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        AudioHub.Instance.PlayGlobal("back_confirm");

        ActivePanels.Remove(this);
    }

    protected virtual void OnDisable()
    {
        ActivePanels.Remove(this);
    }

    public static bool IsPointerOverAnyShownPanel(Vector2 screenPosition)
    {
        for (int i = ActivePanels.Count - 1; i >= 0; i--)
        {
            var panel = ActivePanels[i];
            if (panel == null)
            {
                ActivePanels.RemoveAt(i);
                continue;
            }

            if (!panel.isActiveAndEnabled || !panel.gameObject.activeInHierarchy)
                continue;

            RectTransform rect = panel.transform as RectTransform;
            if (rect == null)
                continue;

            Canvas canvas = panel.GetComponentInParent<Canvas>();
            Camera eventCamera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                eventCamera = canvas.worldCamera;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, eventCamera))
                return true;
        }

        return false;
    }
}