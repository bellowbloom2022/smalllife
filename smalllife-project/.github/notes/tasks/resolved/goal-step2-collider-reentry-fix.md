# [已完成] Goal Step2 快速连点触发聚光灯卡死问题修复

**状态：** ✅ 已完成  
**完成日期：** 2026-04-10

---

## 问题描述

发行商测试反馈：在动画播放期间快速点击，第二步演出结束后聚光灯会卡停在屏幕上，其他所有交互失效。

**复现路径：**
1. 触发 Goal Step1 → 等待 Step1 动画结束
2. Step1 结束后 collider1 关闭、collider2 显示
3. 在 Step2 还在播放时（或刚播完）疯狂点击 collider2 位置
4. 聚光灯卡住，输入锁未释放，全场交互失效

---

## 根本原因

存在一个"可重入窗口"：

- `HandleStep2AnimEnd()` 触发 `TriggerCollectAnimation()`
- `TriggerCollectAnimation()` 是异步（DOTween 链），完成后才将 `step2Completed = true`
- 在此期间 collider2 仍处于启用状态
- 再次点击 collider2 → 再次进入 `OnClicked()` → 再次调用 `PlayStep2()` → 再次执行 `ExecuteStep()` → 再次 `LockInput()` 并触发 `FocusMaskController.Show()`
- 新的一次 LockInput 没有对应的 UnlockInput，输入永久冻结，聚光灯卡在上次的位置

---

## 修复方案（精简三防线）

**文件：** `Assets/Script/Goals/Goal.cs`

### 防线 1：PlayStep2 一触发就立刻关闭 clickable collider（主防线）

```csharp
private void PlayStep2()
{
    ...
    if (animator != null)
        animator.SetTrigger("step2");

    // Disable clickable colliders immediately after Step2 trigger.
    SetClickableColliders(null);

    BeginStep2();
}
```

这是最早的拦截点：点击动作发出后，collider 立刻禁用，疯狂点击从物理层就打不到了。

### 防线 2：step2Completed 态统一无 clickable collider（状态一致性）

```csharp
private void ApplyClickableCollidersByStepState()
{
    if (step2Completed)
    {
        SetClickableColliders(null);
        return;
    }
    ...
}
```

覆盖读档恢复 / 任何状态同步路径，保证持久化状态中 step2Completed 后也不会意外留着 collider。

### 防线 3：mIsTriggered 轻量 guard（兜底）

```csharp
public virtual void OnClicked()
{
    ...
    // Block re-entry while Step2 collect flow is already in progress.
    if (mIsTriggered)
        return;
    ...
}
```

防止非预期入口（非点击路径调用 OnClicked）或 collider 配置漏网时再次触发演出。

---

## 删除的冗余代码

- 去掉了 `HandleStep2AnimEnd()` 里的重复 `SetClickableColliders(null)`（主防线已在更早的时机关闭）
- 去掉了 `TriggerCollectAnimation()` 完成回调里的 `ApplyClickableCollidersByStepState()`（状态规则防线已覆盖）

---

## 经验教训

- Step2 收集动画是异步 DOTween 链，`mIsTriggered = true` 只代表"流程启动"，不代表"step2Completed = true"，这段窗口期如果 collider 仍可点击就会重入。
- 最有效的修复是"在行为入口关闭再进入的物理条件"，而非在逻辑末尾反复检查状态。
- 已记录到仓库记忆：`/memories/repo/goal-input-lock-bug.md`（条目 8）

---

## 相关文件

- `Assets/Script/Goals/Goal.cs`
