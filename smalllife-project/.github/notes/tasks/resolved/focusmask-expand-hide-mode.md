# [已完成] FocusMask 收尾改为可选“扩张回全画面后隐藏”

**状态：** ✅ 已完成  
**完成日期：** 2026-04-08

---

## 背景

原有 Step 收尾阶段中，FocusMask 统一使用“收口缩小到 0 再隐藏”的演出。

本次需求是：保留开场聚焦逻辑不变，仅将 Step 结束时的收尾方向改为可选模式。
目标效果为“从当前聚焦孔向外扩张，回到全画面后隐藏”。

---

## 结论

- 不涉及镜头 Zoom In/Out，仅调整 FocusMask 遮罩半径动画方向。
- 采用 StepConfig 可选配置，默认保持旧行为，避免影响历史关卡。
- 输入锁、动画事件、fallback 解锁链路不变，规避引入 Goal 交互回归风险。

---

## 最终修改

### 1) StepConfig 新增收尾模式

- [Assets/Script/Player/StepConfig.cs](Assets/Script/Player/StepConfig.cs)
- 新增枚举：`FocusHideMode`
  - `LegacyShrink`（默认）：旧行为，收口到 0 后隐藏
  - `ExpandToFullThenHide`：新行为，向外扩张到全屏后隐藏
- 在 `StepConfig` 增加字段：`focusHideMode`（默认 `LegacyShrink`）

### 2) FocusMaskController 支持两种 Hide 方向

- [Assets/Script/Player/FocusMaskController.cs](Assets/Script/Player/FocusMaskController.cs)
- `Hide(float duration)` 扩展为 `Hide(float duration, FocusHideMode hideMode = FocusHideMode.LegacyShrink)`
- 新增“扩张到全屏”半径计算：按当前 `_FocusCenter` 到四角最远距离推导覆盖半径
- 新增 Tween 并发保护：开始新 Show/Hide 前先终止旧半径 Tween，避免闪烁与状态竞争

### 3) Goal.EndStep 接入配置

- [Assets/Script/Goals/Goal.cs](Assets/Script/Goals/Goal.cs)
- `EndStep(config)` 中调用改为：
  - `FocusMaskController.Instance.Hide(config.focusHideDuration, config.focusHideMode)`

---

## 使用方式

在目标 Goal 的 `step1Config`（或 `step2Config`）中：

1. 保持 `useFocus = true`
2. 将 `focusHideMode` 设为 `ExpandToFullThenHide`
3. 用 `focusHideDuration` 调整收尾时长手感

未改配置的 Goal 会继续使用默认 `LegacyShrink`，行为不变。

---

## 验证结果

- 脚本静态检查无新增错误（StepConfig / FocusMaskController / Goal）。
- 用户 Play Mode 验收反馈：测试无问题。

---

## 备注

- 本次未引入新的“等待参数”；若需额外停顿，可继续通过现有动画节奏与 duration 配置协调。
- 如需进一步调手感，可后续增加 easing 配置（不影响本次功能正确性）。
