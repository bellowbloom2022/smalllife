# [已完成] GoalNotePanelController 可维护性重构（行为不变）

**状态：** ✅ 已完成  
**完成日期：** 2026-03-25

---

## 目标

在不改变现有功能的前提下，优化 `GoalNotePanelController` 的结构与判断逻辑：

- 减少重复 `if` 判空与重复流程
- 降低脚本阅读成本与后续改动风险
- 保持现有已验证行为不回退（分页、summary 显示、按钮开关）

---

## 关键改动

### 1. 合并数据就绪检查

新增 `TryGetReadyLevelData`，统一处理：

- `currentLevelData` 空检查
- 关卡数组完整性与长度一致性校验

减少多个函数里重复的数据前置判断。

### 2. 建立 Goal 数据索引缓存

新增 `dataIndexByGoalId`（`goalID -> dataIndex`），在 `RebuildActiveGoals` 时一次性构建。

优化前：渲染与事件回调需反复线性查找索引。  
优化后：通过字典常量时间读取，逻辑更清晰。

### 3. 抽取统一单行渲染

新增 `UpdateRow(...)`，将以下行为集中：

- row 显示
- description 本地化赋值
- summary 按完成态显示/隐藏

让 `ApplyPage` 与 `HandleGoalCompleted` 只保留流程判断。

### 4. 统一分页状态与 UI 更新

新增：

- `SafeRowsPerPage`：统一保证每页最小 1 行
- `UpdatePagingUI`：统一上一页/下一页按钮和页码文字刷新
- `ResetPagingState`：数据无效时重置到 `1/1`

避免分页状态散落在多个分支中。

### 5. 生命周期绑定去重

移除 `Start()` 中重复绑定，仅保留 `OnEnable/OnDisable` 管理按钮监听。  
降低重复 Add/Remove listener 的维护噪音。

---

## 结果

- 代码长度变化不大，但职责边界更清楚
- `if` 分支数量与重复路径明显减少
- 可读性和可维护性提升，后续继续拆分成本更低

---

## 验证

- 目标文件编译检查通过，无错误
- 既有行为保持不变：
  - step2 后 summary 正常显示
  - 分页与行显示正常
  - close 与 notebook 打开关闭按钮正常

---

## 相关文件

- `Assets/Script/UI/GoalNotePanelController.cs`
- `.github/notes/README.md`
