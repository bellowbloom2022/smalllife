# [进行中] Goal 顶部名称黑方块本地化自适应批量化

**状态：** ⚠️ 方案已确认，待工程化落地  
**优先级：** 中  
**记录日期：** 2026-04-07

---

## 背景

当前每个 Level 的 Goal 顶部黑方块白字（例如 The second）需要适配中日英文本长度。

已验证的手工做法可用：
- 将 goal_name 放入对应 Image 下，令背景可随文本尺寸变化
- 通过布局组件让文本过长时自动换行

但逐关逐目标手工配置成本高，容易漏改。

---

## 当前结论

1. 目标节点主要是场景内对象，不是统一 Prefab 实例，无法通过“改一次 Prefab 全覆盖”。
2. 推荐路线为 Editor 批处理工具：一次扫描全部 Level 场景，自动补齐组件与层级。
3. 你当前手工验证的结构方向正确，可作为批处理模板基准。

---

## 已完成（本阶段）

- [x] 明确需求：黑方块宽度自适应 + 文本超长换行。
- [x] 确认实施路线：Editor 批处理优先，运行时仅做兜底告警。
- [x] 形成实施与验证清单（场景扫描、组件补齐、父子修正、多语言抽检）。

---

## 待实施

- [ ] 在 Assets/Editor 新增批处理工具（示例菜单：Tools/Goal/Apply Name Layout To All Levels）。
- [ ] 扫描 Assets/Scenes/Level*.unity，定位 all_goal_item_shadow 下的 sgoal*_name 节点。
- [ ] 自动执行“有则改、无则加”：
  - Vertical Layout Group
  - Content Size Fitter
  - Layout Element
  - Text 换行与溢出策略
  - RectTransform 锚点与边距参数
- [ ] 自动修正父子关系：保证 goal_name 挂在对应 Image 下。
- [ ] 输出每个场景的修改日志并保存。

---

## 验证清单

- [ ] 中/日/英切换下，短文本显示正常。
- [ ] 临界长度文本不挤压、不遮挡。
- [ ] 超长文本在限定宽度后自动换行。
- [ ] 黑方块背景尺寸随文本和换行结果稳定变化。
- [ ] 无明显 Layout 循环警告。

---

## 相关文件

- Assets/Script/UI/ShowTextOnUI.cs
- Assets/Script/UI/GoalNotePanelController.cs
- Assets/Scenes/Level0.unity
- Assets/Scenes/Level1.unity
- Assets/Scenes/Level2.unity
- Assets/Scenes/Level3.unity
- Assets/Scenes/Level4.unity
