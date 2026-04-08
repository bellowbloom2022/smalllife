# [已完成] Goal 存档恢复可视状态与 Icon 同步修复

**状态：** ✅ 已完成  
**完成日期：** 2026-04-09

---

## 背景

Level3 读档进入场景时出现两类异常：

1. 已完成目标的场景对象 `mGameObjectNovel` 仍可见。  
2. 顶部 GoalBar 部分 icon 未按存档状态更新（例如 `goal302`）。

---

## 现象与根因

### 问题 1：`mGameObjectNovel` 仍显示

根因分两层：

1. 读档流程里仅恢复了 step 状态与动画，没有同步“已收集后可视状态”（隐藏/销毁）。
2. 首次修复仅按 `step2Completed` 判定已收集，遗漏 `SingleGoal`（单步目标常见状态是 `step1=true, step2=false`）。

### 问题 2：部分 icon 不更新（如 `goal302`）

根因分两层：

1. `GoalIconUIController.Start()` 会执行 `ApplyProgress(false, false)`，导致刚恢复的存档状态被启动顺序覆盖。  
2. 存档 key 使用数字关卡索引（如 `3_302`），UI/事件侧使用字符串 `levelID`，在匹配逻辑不稳时会漏命中。

---

## 已实施修复

### 1) Goal 读档后强制应用收集态可视效果

文件：`Assets/Script/Goals/Goal.cs`

1. 在 `ApplySavedProgress(...)` 中补充读档可视状态恢复调用。  
2. 读档时若目标已收集，立即对 `mGameObjectNovel` 执行隐藏/销毁。  
3. 已收集判定兼容：
   - 双步 Goal：`step1 && step2`
   - SingleGoal：`step1`

### 2) Icon 启动不再重置为未完成

文件：`Assets/Script/UI/GoalIconUIController.cs`

1. 移除 `Start()` 中固定 `ApplyProgress(false, false)` 的重置行为。  
2. 改为 `Start()` 从 `SaveSystem.GameData` 读取并应用对应进度。

### 3) 统一规则并收紧 levelID 解析

新增文件：`Assets/Script/Tools/GoalProgressRules.cs`

1. 新增 `IsCollected(...)` 统一“是否已收集”判定。  
2. 新增 `IsSameLevelID(...)` 统一关卡匹配。  
3. 新增 `TryParseLevelIndexStrict(...)` 严格解析：仅接受
   - 纯数字（如 `3`）
   - `Level<number>`（如 `Level3`）

并在以下文件替换散落逻辑：

- `Assets/Script/Goals/Goal.cs`
- `Assets/Script/Manager/Level.cs`
- `Assets/Script/UI/GoalIconUIController.cs`

---

## 验证结果（本次 session）

用户回归结果：✅ 通过

1. `mGameObjectNovel` 读档后不再显示。  
2. GoalBar icon 状态与存档一致（包含 `goal302`）。  
3. 回归检查项 1/2/3 均无异常。

---

## 影响文件清单

- `Assets/Script/Goals/Goal.cs`
- `Assets/Script/UI/GoalIconUIController.cs`
- `Assets/Script/Manager/Level.cs`
- `Assets/Script/Tools/GoalProgressRules.cs`

---

## 后续优化建议（简版）

1. 在 `GoalIconUIController` 增加仅编辑器生效的诊断日志开关（命中 key 来源：exact/numeric），用于快速定位 Inspector 配置问题。  
2. 在加载关卡后增加一次 lightweight 一致性检查（scene goal 进度 vs icon 显示），仅在开发构建启用。  
3. 逐步清理并统一 Inspector 中 `levelID` 命名规范，建议统一为纯数字或 `Level<number>` 二选一。  
4. 为 `GoalProgressRules` 增加编辑器测试（SingleGoal/双步 Goal/levelID 解析边界）。

---

## 备注

本次重构为“规则收敛 + 行为稳定化”，核心目标是避免读档时 UI 与场景状态分叉，已验证达成。