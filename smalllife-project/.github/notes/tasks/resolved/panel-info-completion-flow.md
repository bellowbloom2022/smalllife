# [已完成] Panel_info 完成态、折叠侧边栏与节奏优化

**状态：** ✅ 已完成  
**完成日期：** 2026-04-04

---

## 目标

在保留场景内牌子交互的前提下，扩展 `Panel_info`：

- 正常状态下作为右侧常驻折叠面板使用
- 点击牌子或折叠边缘时展开
- 当关卡全部目标完成后，自动以完成态展开
- 完成态展示缩略图勾选、下一关名称、下一步按钮
- 与 note-panel 打字节奏错开，避免 UI 同时抢焦点

最终采用方案：**保留牌子入口 + 完成后自动展开 InfoPanel**。

---

## 已落地行为

### 1. 常驻折叠态

- `InfoPanel` 不再依赖激活 / 失活切换显示
- 默认保持在屏幕右侧折叠位置
- 点击 `edgePeekButton` 可展开
- 点击关闭按钮或空白区域时折回
- 支持在分辨率变化时重新计算折叠 / 展开位置

### 2. 场景牌子入口保留

- `SignboardTrigger` 统一调用 `infoPanel.OpenFromSignboard()`
- 不再依赖旧的 `IsShown` 语义判断
- 保留关卡内“看路牌了解关卡信息”的叙事入口

### 3. 完成态展示

关卡全部目标完成后，`InfoPanel` 会切换为完成态并展开，展示：

- 缩略图上的完成勾选
- 下一关名称
- 下一步按钮

下一关名称优先来自 `LevelDataAsset.titleKey` 的本地化文本；若缺失，则回退到 `SceneChanger.targetSceneName`。

### 4. 完成态节奏

- 全目标完成后不立即弹出，而是延迟约 `1.5s`
- 先给 note-panel 打字效果留出缓冲
- 展开后按顺序显示：`Checkmark -> NextLevelName -> NextButton`

---

## 音效策略

由于 `InfoPanel` 已转为“常驻 + 折叠/展开”模型，不再适合完全依赖 `BasePanel.Show/Hide()` 的音效语义。

当前音效逻辑放在 `InfoPanelController` 内部：

- 点击折叠边缘按钮：`click_confirm`
- 面板从折叠态展开：`button_flip_info`
- 完成态勾选出现：`level_complete`

这样可以避免常驻面板与 `BasePanel` 的激活式音效产生错位。

---

## 实现位置

- `Assets/Script/UI/InfoPanelController.cs`
- `Assets/Script/UI/SignboardTrigger.cs`
- `Assets/Script/Manager/Level.cs`

---

## 相关架构说明

- 输入分发与空白点击来源：`../../architecture/input-system.md`
- Goal Step 完成、计数与完成触发链路：`../../architecture/goal-step-system.md`

如果后续再调整完成态弹出时机、空白点击折回逻辑或输入穿透问题，建议先回看上述两份架构说明，再改 `InfoPanelController` 和 `Level`。

---

## 关键注意事项

### Inspector 依赖

以下引用需要在场景 / prefab 中正确绑定：

- `InfoPanelController.infoPanel`（由 `Level` 持有）
- `Level.sceneChanger`
- `InfoPanelController.edgePeekButton`
- `InfoPanelController.animatedRoot`
- `InfoPanelController.checkmarkImage`
- `InfoPanelController.nextLevelNameText`
- `InfoPanelController.nextButton`

若 `sceneChanger` 未绑定，则完成态仍可弹出，但“下一步按钮”会隐藏或失效。

### 折叠位置

- 当前支持固定折叠位置覆盖值 `foldedSlideDistanceOverride`
- 当其小于等于 `0` 时，使用基于面板宽度和可见比例的自动计算
- 当前项目阶段以 PC 测试包为主，允许使用固定值做稳定交付

---

## 后续 Bug 修复记录

### [2026-04-06] 完成态再次打开时元素消失

**问题现象：**

1. 关卡完成后 InfoPanel 自动展开，若此时点击 InfoPanel 面板本身，面板折叠。再次打开时，NextLevelName 和 NextButton 消失（Checkmark 仍显示）。
2. 点击场景内牌子（SignboardTrigger）重开时，Checkmark / NextLevelName / NextButton 全部消失。

**根本原因：**

1. `FoldPanel` 会 `Kill` 掉 `completionSequence`，若动画尚未播完，NextLevelName / NextButton 停留在 `SetActive(false)` 的中间态，但 `isCompletionMode` 仍为 true。再次展开时无任何逻辑恢复这些元素。
2. `OpenFromSignboard` 无条件调用 `ResetToNormalMode()`，直接清除所有 completion 状态。

**修复方式（`InfoPanelController.cs`）：**

- 新增 `_completionShowTitle` / `_completionShowNextButton` 字段，在 `ShowAsCompletion` 中保存参数。
- `FoldPanel` 折叠时，若 `isCompletionMode == true`，调用新增的 `ShowCompletionElementsImmediate()`，将 Checkmark / NextLevelName / NextButton 立即置为最终可见状态（`SetActive(true)` + `alpha = 1`）。
- `OpenFromSignboard` 中新增判断：若 `isCompletionMode == true`，跳过 `ResetToNormalMode()`，直接 `ExpandPanel()` 返回。

### [2026-04-08] 带存档重进已通关关卡时未恢复完成态

**问题现象：**

- 关卡所有 goal 已完成并已存档。
- 重新进入同一关卡时，`InfoPanel` 不会自动恢复 completion mode，导致 checkmark / NextLevelName / NextButton 不显示。

**根本原因：**

- 旧逻辑只在本局运行中 `AddCount()` 达到 `TotalCount` 时调用 `ShowCompletionInfoPanel()`。
- 带存档重进时虽然 `mCount` 已在 `LoadAllGoalStates()` / `UpdateLevelGoals()` 中恢复，但没有任何启动阶段逻辑去重新切换 `InfoPanel` 到完成态。

**修复方式（`Level.cs`）：**

- `Start()` 中在恢复计数后，若 `mCount >= TotalCount`，启动 `ShowCompletionInfoPanelNextFrame()`。
- 使用“下一帧恢复”而非立即调用，避免与 `InfoPanelController.Start()` 的初始折叠/定位流程抢时序。

### [2026-04-09] 通关后不再自动弹出 InfoPanel，改为右上角快捷下一关

**背景：**

- 发行侧反馈：通关时自动弹出 `InfoPanel` 会打断玩家，且“下一步”入口不应只放在可折叠侧边栏内部。

**调整结果：**

- `Level` 通关后不再调用自动弹出 `InfoPanel` 的流程。
- 新增右上角快捷按钮（`topRightNextLevelButton`）作为通关后的主跳转入口。
- 该按钮仅在“已通关且 `sceneChanger` 有效”时显示；点击后调用 `sceneChanger.ChangeScene()`。
- 读档进入已通关关卡时，按钮可见性也会正确恢复。

**影响说明：**

- `InfoPanel` 内原有完成态内容与按钮逻辑未删除，仅从“自动打断式入口”改为“可主动查看入口”。

### [2026-04-09] 路牌触发改为开关切换，并修复重复触发/性能问题

**问题现象：**

1. 面板展开时点击路牌会出现“收起后又展开”的体感抖动。
2. 路牌与空白点击事件叠加时，可能重复触发展开动画。
3. 每次空白点击都做全场景物理检测，存在不必要开销。

**根本原因：**

1. `InputRouter.OnBlankClick` 先触发 `TryHide()`，再到 `OnMouseUp()` 执行路牌逻辑，形成同帧“先关后开”。
2. 路牌逻辑早期只做“打开”，未统一为开关语义。
3. `InfoPanelController.IsPointerOverSignboard()` 采用 `RaycastAll/OverlapPointAll`，每次点击都会分配数组并全量扫描。

**修复方式：**

- `SignboardTrigger` 统一为“点击切换”：展开时点路牌收起，收起时点路牌展开。
- 新增路牌按下帧标记（`OnMouseDown` 记录 frame），`InfoPanel.TryHide()` 优先读取该标记并直接跳过收起。
- 保留物理检测兜底，但改为 `RaycastNonAlloc` / `OverlapPointNonAlloc`，避免运行时分配。
- 在 `SignboardTrigger.OnMouseUp()` 增加 UI 遮挡判断，防止 UI 点击误触场景路牌。

**当前交互结论：**

1. 点击路牌与点击 `edgePeekButton` 都可开关面板。
2. 两种入口可互通，不会互相打架。
3. 连续点击路牌时不再重复触发切入动画。

---

## 本次 review 记录的后续优化项

以下不是当前阻塞项，留待后续统一处理：

1. `Level` 在目标完成节点附近存在多次 `SaveSystem.SaveGame()` 调用，可在后续把通关状态、goal 进度和 newly-completed 标记合并为一次落盘。
2. `InfoPanel` 目前常驻订阅 `InputRouter.OnBlankClick`，运行成本很低，但后续可改为仅在展开态订阅。
3. `InfoPanel` 当前继承 `BasePanel`，但可见性语义已偏离 `BasePanel` 的 active/inactive 模型；若后续继续扩展 UI 框架，可考虑抽离为独立侧边栏面板模型。

---

## 验证结论

- 已完成的 UI 行为本身没有发现明显高能耗逻辑
- 当前主要运行成本来自轻量级 DOTween UI 动画与少量事件监听，可接受
- 真正值得做的性能 / 资源优化主要集中在后续统一整理存档写入路径

---

## 相关文档

- `../../architecture/input-system.md`
- `../../architecture/goal-step-system.md`
- `../ongoing/goal-input-lock-bug.md`
