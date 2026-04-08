# 架构说明 — Goal Step1/Step2 系统

**主文件：** `Assets/Script/Goals/Goal.cs`

---

## 流程概览

```
玩家点击 Goal
    ↓
OnClicked()
    ↓ (step1Completed == false)
PlayStep1()
    → animator.SetTrigger("step1")
    → BeginStep1() → ExecuteStep(step1Config)
         → LockInput()          ← 输入冻结
         → ScheduleInputUnlockFallback()  ← 兜底计时
         → 镜头移动 / FocusMask
    ↓
[animation plays...]
    ↓
OnAnimEnd()   ← Animator 事件，在 animation clip 指定时间点触发
    ↓
HandleStep1AnimEnd()
    → UnlockInput()             ← 输入恢复
    → CancelInputUnlockFallback()
    → SetClickableColliders(step2Colliders)
    → step1Completed = true
    → SaveGame / Events
    ↓
玩家点击 Goal（此时 step1Completed == true）
    ↓
PlayStep2() → BeginStep2() → ExecuteStep(step2Config)
    → [同上流程]
    ↓
HandleStep2AnimEnd()
    → TriggerCollectAnimation()  ← 收集动画
    → step2Completed = true
```

### SingleGoal（单步目标）补充流程（2026-04-08）

```
玩家点击 SingleGoal
    ↓
OnClicked()
    ↓
PlayStep1()
    → animator.SetTrigger("step1")
    → BeginStep1() → ExecuteStep(step1Config)
    ↓
[single-step animation plays...]
    ↓
OnAnimEnd()
    ↓
EndStep(step1Config)
    ↓
TriggerCollectAnimation(false)
    → AddCount()
    → step1Completed = true
    → step2Completed = false
```

- `SingleGoal` 仍然复用 `step1` trigger 与 `OnAnimEnd()` 动画事件。
- 不再走第二次点击；step1 动画播完后直接进入收集。
- 计数、读档恢复、InfoPanel 完成判定都按 `step1Completed == true` 视为单步目标完成。

---

## 关键字段（StepConfig）

| 字段 | 典型值 | 说明 |
|---|---|---|
| `lockInput` | `true` | 是否在步骤期间锁输入 |
| `cameraDelay` | `0.2s` | 触发镜头移动的延迟 |
| `cameraDuration` | `0.5s` | 镜头移动时长 |
| `focusShowDuration` | `0.4s` | FocusMask 淡入时长 |

---

## Collider 切换

- `step1ClickableColliders`：Step1 前可点击区域
- `step2ClickableColliders`：Step1 完成后切换为 Step2 可点击区域
- `SetClickableColliders()` 统一管理启用/禁用

---

## 动画事件规范

`OnAnimEnd()` 必须由 Unity Animation Event 在 animation clip 中配置，**事件时间必须小于动画实际时长**。

> ⚠️ 常见错误：事件时间 > 动画时长 → 事件永远不触发 → 输入永久冻结
> 见：[goal-input-lock-bug.md](../tasks/ongoing/goal-input-lock-bug.md)

### 命名约定（2026-04-08 更新）

- 当前项目已统一回 `goal` 命名体系，`SingleGoal` 也应使用与普通 Goal 一致的 Animator 状态名：
    - `0_goal{ID}_normal`
    - `1_goal{ID}`
    - `1_goal{ID}_loop`
- 不再建议新建 `sgoal` 命名分支，否则读档恢复与通用状态回放会额外分叉。

---

## 阶段（Stage）枚举

```csharp
enum Stage { PreAnim1, PostAnim1, PostAnim2 }
```

- `PreAnim1`：未触发 Step1
- `PostAnim1`：Step1 已完成，等待 Step2
- `PostAnim2`：Step2 已完成，Goal 收集完毕

---

## Panel-note 输入与穿透策略（2026-03-25）

### 背景与目标

- 新增 `panel-note` 用于展示当前场景 goal 相关剧情描述与完成总结语句。
- 实际需求不是“面板打开后全局禁用输入”，而是“面板区域不允许点击/拖拽穿透到后方场景”。
- 面板外区域仍允许玩家继续操控场景。

### 最终策略（统一逻辑）

- 不按“阻塞/非阻塞面板”分类。
- 所有 panel 统一遵循：
  - 命中 UI 时，不向场景分发输入。
  - 非 UI 区域，照常分发场景输入。

### 关键发现

- 现有 `InputRouter` 在命中 UI 后仍会触发 `OnClick`，会导致场景物体被穿透点击。
- 现有 `BasePanel` 在 `Show/Hide` 里默认 `LockInput/UnlockInput`，导致打开面板即全局冻结。

### 推荐改动顺序

1. 改 `InputRouter` 点击分发
    - 点击命中 UI 时直接返回，不触发 `OnClick`、`OnBlankClick`。
2. 改 `InputRouter` 拖拽分发
    - 增加 `dragStartedOverUI`。
    - 若拖拽起点在 UI 上，本次不触发 `OnDrag`。
3. 调整 `BasePanel`
    - 移除 `Show/Hide` 中默认 `LockInput/UnlockInput`。
    - 保留显示与音效逻辑。
4. 稳定性优化 `UIBlockChecker`
    - 在 `IsPointerOverUI()` 内按需初始化 `PointerEventData`。
    - `EventSystem.current == null` 时直接返回 `false`。
    - 去掉每次点击命中 UI 的调试刷屏日志。

### 回归验证清单

1. 点击 panel 区域：不触发场景物体交互。
2. 点击 panel 外区域：场景点击仍正常。
3. 在 panel 上按下并拖动：镜头不移动。
4. 在 panel 外按下并拖动：镜头可正常移动。
5. 重点流程：查看描述 -> 完成 goal -> 显示总结语句 -> 继续场景交互。

### 本次验证结果（2026-03-25）

- 已在场景内验证：点击 panel-note 不再穿透到后方场景物体。
- 当前结论：输入分发与面板区域拦截策略生效。
- 后续若出现个别区域漏点，优先检查对应 UI 节点是否具备可命中的射线配置（Raycast Target / CanvasGroup.blocksRaycasts）。

---

## 与 Panel_info 完成态的衔接

- 当前关卡完成后的 `InfoPanel` 弹出时机，不放在 `Goal` 内部处理，而是由 `Level` 在总目标数满足后统一调度。
- 这样可以避免把 `panel-note` 的打字节奏、关卡完成态 UI 和 Goal 单体逻辑耦合在一起。
- 当前实现还额外保留约 `1.5s` 延迟，给 note-panel 的文字显示留出缓冲。
- 若带存档重新进入且该关卡已全完成，`Level.Start()` 也会在初始化计数后主动恢复 `InfoPanel` 完成态，不依赖再次收集触发。
- 详见 `../tasks/resolved/panel-info-completion-flow.md`。
