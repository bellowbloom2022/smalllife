using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public static class UIBlockChecker
{
    static PointerEventData pointerData = new PointerEventData(EventSystem.current);
    static List<RaycastResult> raycastResults = new List<RaycastResult>();

    public static bool IsPointerOverUI()
    {
        pointerData.position = Input.mousePosition;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            Debug.Log($"UI Hit: {result.gameObject.name}");
        }

        return raycastResults.Count > 0;
    }
}
