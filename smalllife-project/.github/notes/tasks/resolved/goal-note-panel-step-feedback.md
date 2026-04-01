# [已完成] GoalNote Panel Step 联动与文本动效优化

**状态：** ✅ 已完成  
**完成日期：** 2026-03-25

---

## 目标

完成 Goal Step 与 panel-note 的联动，并优化文本反馈体验：

- Step1 / Step2 完成后，panel-note 自动切到对应页（不自动打开面板）
- 支持点击 description / summary 触发镜头聚焦与文本强调
- summary 在 step2 时逐字显示
- 逐字与强调动画加入颜色反馈和触发后字重提示

---

## 关键改动

### 1. Goal 事件扩展为带步骤参数

- `GoalNoteEvents.GoalCompleted` 从二参扩展为三参
- 新增 `GoalNoteStep`（`Step1` / `Step2`）
- `Goal.cs` 在 `HandleStep1AnimEnd()` 触发 Step1 事件，在 Step2 收集完成后触发 Step2 事件

### 2. Panel-Note 联动逻辑

- `GoalNotePanelController` 监听 Goal 完成事件
- 根据 `goalID` 计算页码并刷新对应行
- 保留“仅切页不自动 Show()”策略
- `descriptionText` 仅在 step1 完成后可点击

### 3. 控制器拆分（降耦合）

将原先的单体控制器拆分为：

- `GoalNoteCameraFocusController`：负责 goal 查找与镜头移动
- `GoalNoteTextAnimator`：负责文本强调动效（放大回弹 + 颜色 + 终态字形）
- `GoalNoteRowUpdater`：负责单行文本刷新、点击绑定、summary 逐字显示

### 4. 文本动效策略

- 颜色：黑色 -> `#CC6666` -> 黑色
- description：强调结束后默认 `Bold`
- summary：强调/逐字结束后默认 `Bold + Italic`
- 配置入口集中在 `GoalNotePanelController` Inspector（方案1），运行时下发给 RowUpdater 与 TextAnimator

### 5. 数据问题排查结论

- level2 summary key 异常并非代码链路错误，而是 `Level2.asset` 中 `goalSummaryKeys` 配置错误（多项误填为 `Goal201Description`）
- 修正资源配置后，summary 读取恢复正常

### 6. 2026-04-01 性能优化补充

本轮优化发生在手机界面自动布局参数加入之后。重点目标不是继续扩功能，而是先压低 panel-note 运行时负荷，保证 4 月 8 日 Steam 上线 demo 前的稳定性。

当前决策：

- 4 月 8 日 demo 前，冻结 `GoalNotePanelController` 的大规模重构与强拆分
- 优先保留现有流程清晰和行为稳定，不为了代码行数单独冒重构风险
- 若后续继续追加 notebook 新功能，再考虑按布局、分页、渲染职责做二次拆分

本轮已落地的优化项：

#### a. Summary 打字机布局重建降频

- `LocalizedTypewriterEffect` 新增 `RELAYOUT_FREQUENCY = 5`
- summary 逐字显示时不再每个字都强制 `ForceRelayout()`，而是每 5 个字或最后一个字才触发布局刷新
- 保留 iOS 笔记式逐字输入观感，不改打字速度与完成态样式

#### b. 面板刷新入口去重

- `GoalNotePanelController` 新增 `RefreshAllRowsDeduplicated()`
- `OnEnable()` 恢复数据初始化刷新，避免未经过 `Show()` 的路径出现 `activeGoalIds` 未构建的问题
- 同一帧内的重复全量刷新被去重，兼顾功能正确性与打开面板时的性能

#### c. Goal 完成事件只在必要时整页刷新

- `HandleGoalCompleted(...)` 改为优先定位当前 goal 的可见索引
- 若目标仍在当前页，只更新单行并播放对应强调反馈
- 只有跨页时才调用 `ApplyPage(...)` 做整页切换

#### d. 活跃 goal 索引缓存 O(1) 化

- `RebuildActiveGoals(...)` 同步构建 `activeIndexByGoalId`
- 替代事件处理中的 `activeGoalIds.IndexOf(goalID)` 线性查找
- 使 step1 / step2 事件在 goal 数量增加时仍保持稳定开销

#### e. 摄像机聚焦查询缓存

- `GoalNoteCameraFocusController` 新增 `goalById` 缓存
- 点击 description / summary 时不再每次全场景扫描 `Goal`
- 同时缓存 `Camera.main` 与 `CameraController` 组件，减少重复 `GetComponent` 和相机查找

#### f. 单行刷新去重

- `GoalNoteRowUpdater` 对文本赋值、激活状态切换、点击中继更新做去重处理
- `GoalNotePanelController` 复用按 `goalID` 缓存的 description / summary 点击回调，避免刷新时重复生成委托
- 降低翻页、刷新、goal 事件触发时的细碎 CPU 与 GC 抖动

#### g. 分页渲染差量化

- `ApplyPage(...)` 从“每次先全清空再整页重建”改为“首次全清空，后续按页差量更新”
- 仅关闭不属于目标页的行，并更新当前页需要显示的行
- 降低翻页与刷新时的无意义 `SetActive`、文本清空和重绘开销

本轮优化后的回归结论：

- Step1 后 description 正常出现并可响应点击
- Step2 后 summary 正常出现并保留逐字显示
- 翻页、镜头聚焦、分页指示与按钮逻辑保持正常
- 当前版本适合在 demo 节点前继续冻结结构，后续以稳定验证为主

---

## 相关文件

- `Assets/Script/Core/GoalNoteEvents.cs`
- `Assets/Script/Goals/Goal.cs`
- `Assets/Script/UI/GoalNotePanelController.cs`
- `Assets/Script/UI/GoalNoteCameraFocusController.cs`
- `Assets/Script/UI/GoalNoteTextAnimator.cs`
- `Assets/Script/UI/GoalNoteRowUpdater.cs`
- `Assets/Script/Localization/LocalizedTypewriterEffect.cs`
- `Assets/Resources/LevelDataAssets/Level2.asset`

---

## 验证结论

- 编译检查通过（上述改动文件无编译错误）
- 运行效果符合需求：
  - Step2 summary 可逐字显示
  - description 与 summary 均支持触发态颜色反馈
  - summary 最终样式为 Bold + Italic
  - panel-note 不自动打开，仅内部翻页
  - 面板在当前优化后回归验证正常，未发现 step1 / step2 文本缺失或翻页错乱
