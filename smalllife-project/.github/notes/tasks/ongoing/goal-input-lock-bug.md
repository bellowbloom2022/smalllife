# [进行中] Goal Input Lock Bug — Level2 goal201/203/204

**状态：** ⚠️ 部分修复，待完全验证  
**首次发现：** 2026-03-25  
**对比基准：** `SmallLifev0.0.9 (20251103)` — 无任何等待感，行为即时

---

## 问题描述

Level2 的 goal201、goal203、goal204，点击 Step1 后：
- Step2 点击区域无响应
- 拖拽 / 滚轮也同样冻结
- 等待约 1.2s 后（fallback timer 触发）才恢复交互

其他 Goal 和旧版本 SmallLifev0.0.9 均正常，其他关卡 goal 无此问题。

---

## 根本原因（已确认）

## 当前结论

- 问题不是 `InputRouter` 主逻辑本身失效，而是 Goal Step 流程没有在预期时间走到 `EndStep()`
- 对用户表现为“点击 Step1 后输入冻结约 1.2s，或在极端情况下永久冻结”
- 已确认的两类触发源分别是：动画事件超出 clip 时长、Animator 过渡被 ExitTime 阻塞
- 当前修复策略分为两层：先修动画资源，再保留代码侧 fallback 作为兜底

### 1. 动画事件时间超出动画实际长度（主因）

`OnAnimEnd()` 事件由 Animator 在 animation clip 的指定时间点触发，
如果事件时间 > 动画结束时间，事件**从不触发** → `UnlockInput()` 永不调用 → 输入永久冻结。

| 文件 | Goal | 原事件时间 | 动画实际时长 | 修复后 |
|---|---|---|---|---|
| `Assets/Animations/Level2/sgoal/1_sgoal2.anim` | goal203 | 3.9s | ~2.9s | **2.8s** |
| `Assets/Animations/Level2/sgoal/1_sgoal3.anim` | goal204 | 2.2s | ~2.23s | **1.8s** |

### 2. Animator 状态机 ExitTime（次因，goal201）

`goal1.controller` 中 `1_goal201_loop → 2_goal201` 过渡设置了 `m_HasExitTime: 1`，
导致 step2 trigger 必须等 loop 动画自然走完一圈才响应。

| 文件 | 字段 | 原值 | 修复后 |
|---|---|---|---|
| `Assets/Animations/Level2/goal1.controller` | `m_HasExitTime` | `1` | **0** |

---

## 已实施的修复

### 动画资产修改
- `1_sgoal2.anim`：OnAnimEnd 事件时间 3.9 → 2.8
- `1_sgoal3.anim`：OnAnimEnd 事件时间 2.2 → 1.8
- `goal1.controller`：step2 过渡 HasExitTime 1 → 0

### `Goal.cs` 防御性代码（fallback timer）

```csharp
private int inputLockVersion;
private const float MinInputUnlockFallbackDelay = 0.4f;

// ExecuteStep() 中每次 LockInput 时调用
private void ScheduleInputUnlockFallback(StepConfig config)
{
    int lockVersion = ++inputLockVersion;
    float fallbackDelay = Mathf.Max(
        MinInputUnlockFallbackDelay,
        config.cameraDelay + config.cameraDuration + 0.5f  // = 1.2f
    );
    DOVirtual.DelayedCall(fallbackDelay, () =>
    {
        if (lockVersion != inputLockVersion) return; // 已被 Cancel 则跳过
        if (!InputRouter.Instance.InputLocked) return;
        InputRouter.Instance.UnlockInput();
    });
}

// EndStep() 中正常流程调用，取消 fallback
private void CancelInputUnlockFallback() { inputLockVersion++; }
```

- `HandleStep1AnimEnd()` / `HandleStep2AnimEnd()` 顶部额外增加了 `UnlockInput()` 保险调用
- 实际 fallback 延迟 = `Mathf.Max(0.4, 1.2) = 1.2s`
- 若动画事件正常触发 → fallback 被 Cancel → **用户感知不到任何等待**

---

## 待验证清单

- [ ] goal203 Step2 点击是否立即响应（无需等 1.2s fallback）
- [ ] goal204 Step2 点击是否立即响应
- [ ] goal201 Step2 点击是否立即响应，ExitTime 修复是否保留（Unity 重开 controller 可能覆盖）
- [ ] 上述三个 goal 完整完成后，UI 收集动画是否正常播放

---

## 注意事项

> Unity Editor 打开 `.controller` 文件后会重写，可能覆盖手动改动的 `m_HasExitTime: 0`。
> 每次打开 Unity 后建议用 `git diff` 确认 `goal1.controller` 的该字段是否仍为 `0`。

---

## 何时转入 Resolved

满足以下条件后，可将本条从 `ongoing/` 移到 `resolved/`：

- goal201、goal203、goal204 的 Step2 点击均能立即响应
- Play Mode 下不再依赖 1.2s fallback 才恢复输入
- `goal1.controller` 中 step2 过渡的 `HasExitTime` 不会被 Unity 重新覆盖
- 三个 goal 全流程完成后，收集动画与计数逻辑均正常

---

## 相关文件

- `Assets/Script/Goals/Goal.cs`
- `Assets/Script/Core/InputRouter.cs`
- `Assets/Animations/Level2/sgoal/1_sgoal2.anim`
- `Assets/Animations/Level2/sgoal/1_sgoal3.anim`
- `Assets/Animations/Level2/goal1.controller`
