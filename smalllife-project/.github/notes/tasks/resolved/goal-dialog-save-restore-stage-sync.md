# [已完成] GoalDialog 存档恢复阶段同步修复

**状态：** ✅ 已完成  
**完成日期：** 2026-04-09

---

## 背景

在“保存后返回场景”的回归测试中，GoalDialog 出现了阶段错位：

1. 打字机首播/复播状态已恢复正常（复播秒出）。
2. 但对话内容仍错误落在 `PreAnim1`（应按当前进度进入 `PostAnim1` 或 `PostAnim2`）。

---

## 根因

`Goal.Start()` 中无条件执行：

- `currentStage = Stage.PreAnim1`

该重置在部分启动顺序下会覆盖读档恢复后的阶段值，导致视觉动画虽然正确，但对话阶段回退为 `PreAnim1`。

---

## 实施修复（最小改动）

### 1) 对话复播状态恢复（已先行落地）

文件：`Assets/Script/Manager/DialogueManager.cs`  
新增公开方法 `MarkSpriteAsPlayed(GameObject sprite)`，允许 `Goal` 在读档后把对应阶段对话标记为“已播放过”。

### 2) 读档后恢复对话已播状态

文件：`Assets/Script/Goals/Goal.cs`  
在 `ApplySavedProgress(...)` 末尾调用 `RestoreDialoguePlayedState()`：

- `step1Completed == true` 时标记 `dialogueSpritesPreAnim1`
- `step2Completed == true` 时标记 `dialogueSpritesPostAnim2`
- 仅 `step1Completed == true` 时标记 `dialogueSpritesPostAnim1`

### 3) 修复阶段被 Start 覆盖

文件：`Assets/Script/Goals/Goal.cs`  
调整 `Start()` 初始化策略：

- 仅当“无该 goal 的存档进度”时，才初始化 `currentStage = Stage.PreAnim1`
- 有存档进度时，不做硬重置，交由 `ApplySavedProgress(...)` 还原阶段

---

## 验证结果

用户回归结果：✅ 通过

1. 返回场景后，GoalDialog 不再重复打字播放。  
2. 返回场景后，对话阶段正确显示（不再错误落到 `PreAnim1`）。

---

## 影响文件

- `Assets/Script/Manager/DialogueManager.cs`
- `Assets/Script/Goals/Goal.cs`

---

## 备注

本次为 demo 前的最小风险修复：不改存档结构，不迁移 `GoalProgress`，仅修正启动顺序覆盖问题与对话复播状态同步。