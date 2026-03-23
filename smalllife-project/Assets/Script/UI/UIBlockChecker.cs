using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public static class UIBlockChecker
{
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        return raycastResults.Count > 0;
    }
}