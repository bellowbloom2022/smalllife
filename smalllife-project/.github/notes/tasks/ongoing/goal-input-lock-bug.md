# [进行中] Goal Input Lock Bug — Level2 goal201/203/204

**状态：** ⚠️ 输入锁问题已修复，Level2 历史动画问题仍待完全验证  
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

---

## 2026-04-08 新增结论：演出期全输入锁定优化

### 用户反馈场景

发行侧反馈要求：

- 演出期间禁止玩家一切操作
- 避免聚光灯重复触发
- 避免一个演出覆盖另一个演出
- 避免地图/画布被拖走或 UI 在演出期间被点开

### 本次确认的真实根因

1. **fallback 解锁过早**
    - 旧版代码按 `cameraDelay + cameraDuration + 0.5f` 估算 fallback 时间
    - 当动画实际时长大于该估算值时，输入会在演出尚未结束时提前解锁
    - 用户就能在演出中点击其他 goal 或打开 UI

2. **存在绕过 InputRouter 的输入入口**
    - `OnMouseDown()` / `OnMouseUp()` 这类直连输入不会自动受 `InputRouter.InputLocked` 约束
    - 代表脚本：`OnObjectClicked.cs`、`SignboardTrigger.cs`

3. **仅锁场景输入不够，还需要锁 UI 点击**
    - 仅拦截 `InputRouter.OnClick` 后，Panel/Button 仍可通过 UI 射线点击
    - 最终方案是在锁定期间禁用所有 `GraphicRaycaster`

4. **不应禁用整个 EventSystem**
    - 这样会导致演出结束后 panel hover / click 状态异常
    - 正确做法是只关 UI 射线，不关整个事件系统

5. **锁定期间需要清理拖拽状态**
    - 否则解锁瞬间可能把锁定期间积压的鼠标位移一次性结算，出现镜头突跳

---

## 2026-04-08 已实施的输入锁优化

### 1. `InputRouter` 收敛为统一输入锁入口

- 保留单例并增加重复实例保护
- 锁定期间直接停止 `OnDrag` / `OnClick` 分发
- 锁定期间重置 `lastMousePos`，避免解锁瞬间拖拽突跳
- 锁定期间禁用所有已启用的 `GraphicRaycaster`，解锁后恢复
- 调试日志开关保留，但默认关闭

### 2. Goal Step 演出统一强制锁输入

- 移除 `StepConfig.lockInput`
- 所有 Step1 / Step2 演出在 `ExecuteStep()` 时一律加锁
- 正常解锁统一收敛到 `EndStep()`
- fallback 仅作为动画事件失效时的超时保底，不再提前抢解锁
- 当前 fallback timeout 为 **8 秒**

### 3. 绕过入口补丁

以下脚本增加了 `InputRouter.InputLocked` 保护：

- `Assets/Script/Player/CameraController.cs`
- `Assets/Script/UI/PauseManager.cs`
- `Assets/Script/UI/OnObjectClicked.cs`
- `Assets/Script/UI/SignboardTrigger.cs`
- `Assets/Script/UI/BasePanel.cs`
- `Assets/Script/UI/InfoPanelController.cs`
- `Assets/Script/Goals/SingleGoal.cs`

### 4. 清理错误解锁路径

- `DialogueManager.TryHandleDialogueBlankClick()` 中移除了越权 `UnlockInput()`
- 避免对话关闭时错误解除 Goal 演出锁

---

## 当前状态（2026-04-08）

### 已验证通过

- [x] Goal 演出期间，其他 goal 不可触发
- [x] Goal dialogue 在演出期间不可点击
- [x] Esc 在演出期间被锁住
- [x] 画布拖拽/移动在演出期间被锁住
- [x] Panel / InfoPanel 在演出期间不可点击
- [x] 演出结束后 UI 可恢复点击
- [x] 解锁瞬间不再出现大位移突跳

### 仍需继续观察

- [ ] fallback 8 秒是否过长，是否需要改为更保守但更短的统一值
- [ ] Level2 的 goal201 / 203 / 204 动画资源修复后，是否已经完全不依赖 fallback
- [ ] `GraphicRaycaster` 全量禁用/恢复方案在更多 UI Canvas 场景下是否稳定

---

## 下一步建议

### P1. Level2 历史问题最终回归

- Play Mode 重点回归 goal201 / goal203 / goal204
- 确认 Step2 立即响应，不再依赖 fallback 才恢复输入
- 确认 `goal1.controller` 的 `HasExitTime` 改动未被 Unity 覆盖

### P2. 输入锁方案最终收尾

- 观察 `FallbackUnlockTimeout = 8f` 是否需要下调到更合理值
- 如果后续发现 UI Canvas 数量继续增加，可考虑把 `GraphicRaycaster` 缓存化，避免每次锁定时全场扫描

### P3. 可选重构（非当前必须）

- 若未来不止 Goal 演出需要输入锁，可把当前方案抽象成独立 `CutsceneInputBlocker`
- 若后续需要“多个系统同时申请锁”，再评估把 `InputLocked` 升级为引用计数/token 模型

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

- [ ] Level1 / Level0 常规 goal 与 single goal 在演出锁下完整回归
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
- `Assets/Script/Player/GoalManager.cs`
- `Assets/Script/Player/CameraController.cs`
- `Assets/Script/UI/PauseManager.cs`
- `Assets/Script/UI/OnObjectClicked.cs`
- `Assets/Script/UI/SignboardTrigger.cs`
- `Assets/Script/UI/BasePanel.cs`
- `Assets/Script/UI/InfoPanelController.cs`
- `Assets/Script/Manager/DialogueManager.cs`
- `Assets/Animations/Level2/sgoal/1_sgoal2.anim`
- `Assets/Animations/Level2/sgoal/1_sgoal3.anim`
- `Assets/Animations/Level2/goal1.controller`
