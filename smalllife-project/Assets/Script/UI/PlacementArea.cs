using UnityEngine;

public class PlacementArea : MonoBehaviour
{
    [Header("Identity")]
    public string zoneId;           // 唯一 ID（存档用）
    
    [Header("对齐点（可选）")]
    public Transform snapPoint;

    [Header("状态")]
    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public PlacedItem currentItem;

    [Header("可选 UI/特效")]
    public GameObject highlight;  // 鼠标悬停或可放置提示
    public GameObject preview;    // 拖拽预览提示

    /// <summary>占用区域</summary>
    public void Occupy(PlacedItem item)
    {
        currentItem = item;
        isOccupied = true;

        // 隐藏高亮/预览
        SetPreview(false);
        ShowHighlight(false);
    }

    /// <summary>释放区域</summary>
    public void Release()
    {
        currentItem = null;
        isOccupied = false;
    }

    /// <summary>获取对齐位置</summary>
    public Vector3 GetSnapPosition()
    {
        return snapPoint != null ? snapPoint.position : transform.position;
    }

    /// <summary>显示或隐藏高亮</summary>
    public void ShowHighlight(bool show)
    {
        if (highlight != null)
            highlight.SetActive(show && !isOccupied);
    }

    /// <summary>显示或隐藏预览（拖拽时）</summary>
    public void SetPreview(bool show)
    {
        if (preview != null)
            preview.SetActive(show && !isOccupied);
    }
}
