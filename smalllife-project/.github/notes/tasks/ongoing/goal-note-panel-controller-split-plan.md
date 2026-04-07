# GoalNotePanelController 拆分计划（Steam Demo 后执行）

**状态：** 🕒 暂缓中（已规划）  
**记录日期：** 2026-04-07  
**目标完成窗口：** 2026-04-08 Steam demo 之后

---

## 当前结论

- 当前 `GoalNotePanelController` 虽然体量较大，但运行流程稳定，近期优化后回归通过。
- 4 月 8 日 demo 前以稳定性优先，不进行大规模结构重构。
- 已完成的性能优化可以满足当前 demo 阶段运行需求。

---

## 暂时不处理拆分的理由

1. **上线窗口风险控制**：demo 截止前，重构类改动会提高回归成本和未知行为风险。
2. **收益时点不匹配**：当前主要瓶颈已通过局部优化缓解，继续拆分对短期帧耗收益有限。
3. **验证成本较高**：脚本涉及分页、自动布局、Step1/Step2 联动、镜头聚焦、文本动画，多场景回归成本高。
4. **资源改动并行较多**：当前仓库有多个场景/Prefab 并行调整，叠加结构重构不利于问题归因。

---

## 后续拆分目标（行为不变）

原则：先拆“变化频率高”的模块，保留外部接口与 Inspector 配置，避免一次性大改。

### 阶段 1：分页与渲染调度拆分（优先）

拆出 `GoalNotePagingPresenter`（或同等命名）职责：

- activeGoalIds 构建与索引映射
- page 计算、翻页、页码状态
- 当前页目标集合计算与差量渲染调度

`GoalNotePanelController` 保留：

- Unity 生命周期入口
- 按钮事件绑定
- 外部系统事件桥接（GoalCompleted、语言切换等）

### 阶段 2：自动布局拆分

拆出 `GoalNoteAutoLayoutController` 职责：

- 面板尺寸变化监听
- 行高计算与 row RectTransform 布局
- 强制布局重建触发策略（仅必要时）

`GoalNotePanelController` 改为只下发布局参数。

### 阶段 3：状态查询与可见行刷新收敛

拆出 `GoalNoteProgressQuery`（或同等命名）职责：

- step1/step2 完成态查询
- description/summary 可见性判定
- 避免同帧/同状态重复查询

并将“单行更新”与“页面更新”统一到一个小型渲染入口，减少分支重复。

---

## 拆分执行约束（必须满足）

1. **行为不变**：不改变现有交互时序（Step1/Step2、分页、镜头聚焦、summary 打字反馈）。
2. **小步可回滚**：每阶段独立提交并可单独回退。
3. **每阶段都回归**：至少覆盖以下回归项：
   - Step1 后 description 出现并可点击
   - Step2 后 summary 出现并保留逐字反馈
   - 翻页与页码按钮一致
   - 跨页完成事件可定位并刷新正确目标
4. **不触碰资源引用关系**：不改 prefab/meta 的 GUID 关系。

---

## 风险清单

1. 输入锁与 Goal 动画事件时序耦合：避免在拆分过程中改动 InputRouter/Goal 事件链。
2. 自动布局与文本动画互相影响：保持“文本变化后布局刷新”的触发边界清晰。
3. 分页差量渲染中的旧回调残留：确保行复用时点击中继器正确覆盖或清空。

---

## 触发执行条件

满足以下条件再启动拆分：

1. Steam demo 提交完成。
2. 当前 UI/场景并行改动收敛。
3. 预留至少 1 个完整回归周期（多场景 + 移动端分辨率）。

---

## 关联文档

- `.github/notes/tasks/resolved/goal-note-panel-step-feedback.md`
- `.github/notes/tasks/resolved/goal-note-panel-controller-refactor.md`
- `.github/notes/architecture/goal-step-system.md`
- `.github/notes/architecture/input-system.md`
