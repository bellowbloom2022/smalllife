using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public static class UIBlockChecker
{
    private static PointerEventData cachedPointerData;
    private static EventSystem cachedEventSystem;
    private static readonly List<RaycastResult> cachedRaycastResults = new List<RaycastResult>(16);

    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (cachedPointerData == null || cachedEventSystem != EventSystem.current)
        {
            cachedEventSystem = EventSystem.current;
            cachedPointerData = new PointerEventData(EventSystem.current);
        }

        cachedPointerData.position = Input.mousePosition;
        cachedRaycastResults.Clear();
        EventSystem.current.RaycastAll(cachedPointerData, cachedRaycastResults);

        return cachedRaycastResults.Count > 0;
    }
}