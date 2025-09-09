using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlacedItem : MonoBehaviour
{
    [Header("基础数据")]
    public int goalID;                // 对应的目标ID
    public string persistId;          // 唯一ID，用于存档恢复

    [Header("绑定区域")]
    [HideInInspector] public PlacementArea currentArea; // 当前占用的放置点

    [Header("可选")]
    public Animator animator;         // 可选动画控制

    /// <summary>初始化（生成时调用）</summary>
    public void Init(int id, string savedId = null)
    {
        goalID = id;
        persistId = string.IsNullOrEmpty(savedId) ? System.Guid.NewGuid().ToString() : savedId;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    /// <summary>绑定到某个放置区域</summary>
    public void BindToArea(PlacementArea area)
    {
        if (area == null)
        {
            Debug.LogWarning("[PlacedItem] BindToArea: area为空");
            return;
        }

        // 释放旧区域
        ReleaseFromArea();

        currentArea = area;
        currentArea.Occupy(this);

        // 对齐位置
        transform.position = currentArea.GetSnapPosition();
        Debug.Log($"[PlacedItem] 绑定到区域 zoneId={area.zoneId}, goalID={goalID}");
        // ---- 可选：自动更新存档 ----
        var controller = ApartmentController.Instance;
        if (controller != null)
        {
            var data = controller.placedItems.Find(d => d.id == persistId);
            if (data != null)
            {
                data.zoneId = area.zoneId;
                data.position = area.GetSnapPosition();
                SaveSystem.GameData.apartmentPlacedItems = controller.placedItems;
                SaveSystem.SaveGame();
            }
        }
    }

    /// <summary>释放当前绑定的放置区域</summary>
    public void ReleaseFromArea()
    {
        if (currentArea != null)
        {
            Debug.Log($"[PlacedItem] 从区域 {currentArea.zoneId} 释放");
            currentArea.Release();
            currentArea = null;
        }
    }

    /// <summary>回到 Sidebar，释放区域并通知 Controller</summary>
    public void ReturnToSidebar()
    {
        ReleaseFromArea();

        if (ApartmentController.Instance != null)
        {
            ApartmentController.Instance.NotifyItemReturned(this);
        }

        Destroy(gameObject);
    }

    /// <summary>点击交互（可选）</summary>
    public void Interact()
    {
        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }
        // TODO: 播放音效 / 粒子 / UI
    }

    // 简单点击支持
    private void OnMouseDown()
    {
        Interact();
    }
}
