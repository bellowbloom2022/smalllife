using UnityEngine;

/// <summary>
/// 抽象基类：统一管理 goalKey / unlocked 判定 & Refresh 接口
/// 子类必须实现 Refresh() 来控制自己的 UI 展示
/// </summary>
public abstract class BaseDiarySlot : MonoBehaviour
{
    [HideInInspector] public string goalKey;
    protected bool unlocked = false;

    /// <summary>
    /// 初始化槽位（传入 goalKey，例如 "0_2"）
    /// 子类可以重载 Setup(specific args...) 并调用 base.Setup(key)
    /// </summary>
    public virtual void Setup(string key)
    {
        goalKey = key;
        unlocked = IsUnlocked(goalKey);
        Refresh();
    }

    /// <summary>
    /// 强制解锁（例如从存档恢复时或测试）
    /// </summary>
    public virtual void ForceUnlock()
    {
        unlocked = true;
        Refresh();
    }

    /// <summary>
    /// 子类实现：刷新 UI（根据 unlocked 决定显示什么）
    /// </summary>
    protected abstract void Refresh();

    /// <summary>
    /// 统一的解锁判断，严格依据 SaveSystem.GameData.goalProgressMap[key].step2Completed
    /// 返回 true 表示 step2 已完成
    /// </summary>
    protected bool IsUnlocked(string key)
    {
        var gd = SaveSystem.GameData;
        if (gd == null) return false;
        if (gd.goalProgressMap == null) return false;

        if (gd.goalProgressMap.TryGetValue(key, out var progress))
        {
            // GoalProgress 强类型： step1Completed, step2Completed
            if (progress is GoalProgress gp)
            {
                return gp.step2Completed;
            }
        }
        return false;
    }
}
