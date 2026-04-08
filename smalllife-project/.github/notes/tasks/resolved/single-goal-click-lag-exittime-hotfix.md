# [已完成] SingleGoal 点击链路、读档恢复与统一命名修复

**状态：** ✅ 已完成  
**首次记录：** 2026-04-07  
**最终完成：** 2026-04-08

---

## 现象

Level0 中 `GoalID=2` 的单步目标（`SGoal1`）点击后，获得动画体感延迟约 1-2s，玩家感知为“点击迟钝”。

---

## 已确认原因

该问题不是输入锁导致，场景中该目标配置为：
- `step1Config.lockInput: 0`
- `step2Config.lockInput: 0`

参考：
- [Assets/Scenes/Level0.unity](../../../Assets/Scenes/Level0.unity)

初始问题与后续衍生问题一共分为三类：

1. 点击入口调用的是 `Goal.OnClicked()`，不是直接调用 `SingleGoal.OnClick()`
2. `SGoal1.controller` 的 step1 过渡启用 `HasExitTime`，会引入额外等待
3. `SingleGoal` 若不重写 `OnClicked()`，会误走双步 Goal 主链路
4. 读档恢复时，基类默认按 `goal` 命名（如 `1_goal{ID}_loop`）回放状态；若单步目标使用 `sgoal` 命名，则会恢复失败并落回默认态
5. 关卡启动后的完成计数若仍按 `step1 && step2` 统计，会漏算单步目标，导致已通关关卡重进时 `InfoPanel` 不进入完成态

---

## 2026-04-07 临时修复（已执行）

在 Unity Editor 中去掉 `SGoal1` 相关 step1 过渡的 ExitTime（取消 `HasExitTime`），实测点击后反馈明显即时，当前体验可接受。

涉及资源：
- `Assets/Animations/Level0/SGoal1.controller`

---

## 2026-04-08 代码层根治（已执行）

### 1. SingleGoal 正式接入 GoalManager 点击主链路

- `Goal.OnClicked()` 改为可重写。
- `SingleGoal` 重写 `OnClicked()`，点击时走 `PlayStep1()`。
- `SingleGoal.OnAnimEnd()` 在 step1 动画结束后调用 `TriggerCollectAnimation(false)`。

### 2. 单步目标完成判定统一按 `step1Completed`

- `Level.LoadAllGoalStates()` / `UpdateLevelGoals()` / `IsGoalFound()` 已补充 `SingleGoal` 特判。
- 带存档重进时，单步目标会正确计入 `mCount`。

### 3. Animator 状态命名统一回 `goal` 体系

- 最终采用统一命名：
	- `0_goal{ID}_normal`
	- `1_goal{ID}`
	- `1_goal{ID}_loop`
- 不再保留 `sgoal` 分支命名，避免读档恢复时额外走专用代码。

### 4. 带存档重进已通关关卡时恢复 InfoPanel 完成态

- `Level.Start()` 在初始化计数后，若 `mCount >= TotalCount`，会主动恢复 `InfoPanel` completion mode。
- 完成态元素包括 checkmark、下一关名称、next button。

---

## 单步目标制作建议（新增）

1. `SingleGoal` 继续使用 `step1` trigger 和 `OnAnimEnd()` 动画事件，不需要另开一套事件函数。
2. 新建单步目标时，Animator 状态名统一使用 `goal` 命名，不再使用 `sgoal` 命名。
3. 若使用动画事件：事件时间必须小于 clip 实际时长。
4. 关键过渡尽量避免 `HasExitTime` 阻塞，尤其是点击后期望立即反馈的链路。
5. 回归时至少检查三条路径：首次点击、带存档重进、全目标完成后 InfoPanel 完成态。
