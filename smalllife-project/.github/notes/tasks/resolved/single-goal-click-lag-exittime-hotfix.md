# [已记录] SingleGoal 点击迟滞与 ExitTime 临时修复（Level0 GoalID=2）

**状态：** ✅ 已记录（临时修复已执行）  
**记录日期：** 2026-04-07  
**后续状态：** ⚠️ 代码层根治方案未执行，待后续推进

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

延迟主要来自动画链路等待叠加：

1. 点击入口调用的是 `Goal.OnClicked()`，不是直接调用 `SingleGoal.OnClick()`
2. `SGoal1.controller` 的 step1 过渡启用 `HasExitTime`，会引入额外等待
3. `SingleGoal.OnClick()` 由 `1_sgoal1.anim` 动画事件触发（事件时间约 0.55s）
4. 收集流程内部还有两段串行 tween，最后才触发 get 动画

---

## 本次临时修复（已执行）

在 Unity Editor 中去掉 `SGoal1` 相关 step1 过渡的 ExitTime（取消 `HasExitTime`），实测点击后反馈明显即时，当前体验可接受。

涉及资源：
- `Assets/Animations/Level0/SGoal1.controller`

---

## 未执行但建议的后续根治方案

### 方案 1（推荐）

代码上让单步目标直接在 `OnClicked()` 中触发收集，避免依赖 step1 动画事件回调 `OnClick()`：

- 目标：点击当帧进入单步收集逻辑
- 收益：降低对动画事件时序、ExitTime 配置的依赖
- 风险：需确认不影响现有演出与对话时机

当前状态：**未执行**。

---

## 单步目标制作建议（新增）

1. 单步目标优先“代码直达收集”，避免把关键状态推进绑在动画事件上。
2. 若必须使用动画事件：事件时间必须小于 clip 实际时长。
3. 关键过渡尽量避免 `HasExitTime` 阻塞，尤其是点击后期望立即反馈的链路。
4. 每次修改 controller/clip 后，Play Mode 回归“首击响应时间”。
