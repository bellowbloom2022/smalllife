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
