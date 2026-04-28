# [进行中] Goal Icon 环形进度动画改造

**状态：** ✅ 代码完成，字段已修正  
**首次发现：** 2026-04-24  
**最后更新：** 2026-04-25  
**对比基准：** 原 goal1_get 动画（fill amount 直接切换 + badge 切换）

---

## 任务描述

将 goal icon 的获得动画从"直接切换 fill amount/badge"改为"环形进度填充动画+最终高亮+checkmark"效果：

- **初始状态**：icon 底部有虚线圆圈，icon 透明度 40%
- **Step1 触发后**：圆圈填充 0→180度（fill amount 0→0.5），icon 透明度 40%→70%
- **Step2 触发后**：圆圈填充 180→360度（fill amount 0.5→1），icon 透明度 70%→100%，显示 checkmark

---

## 当前进度

### ✅ 已完成

1. **代码更新** (`GoalIconUIController.cs`)
   - 新增字段：`dotCircleFilledImage`（填充圆环）、`iconImage`（icon 主体）
   - 新增方法：`AnimateDotCircle()`、`AnimateIconAlpha()`
   - 修改 `HandleGoalCompleted()`：加入环形填充和透明度动画逻辑
   - 修改 `ApplyProgress()`：非动画状态设置（读档时）
   - 清理旧代码：移除 `fillImage`、`badgeImage` 及相关方法

2. **Prefab 结构更新** (`goal1_get.prefab`)
   - 添加子对象：`goal-dot-circle`、`goal-circle-filled`、`goal-icon`、`goal-checkmark`

3. **字段命名修正**
   - `dotCircleImage` → `dotCircleFilledImage`（避免与虚线圆环图片名称混淆）

4. **Novel 消失动画优化** (`Goal.cs`)
   - 第二段飞行动画（Mid → Icon）同时执行 scale 从 1 → 0 的消失效果
   - 使用 `Ease.InBack` 增强"吸入"感
   - 避免 novel 遮挡 goal-circle-filled 填充动画

### ⏳ 待验证

1. 在 Unity Editor 中配置 `goal1_get` Prefab：
   - 绑定 `dotCircleFilledImage` 字段到 `goal-circle-filled` 图片
   - 设置 Image Type = Filled，Fill Method = Radial 360

2. 测试动画效果：
   - Step1 触发后圆圈填充到 50%，icon 透明度增加到 70%
   - Step2 触发后圆圈填充到 100%，icon 透明度增加到 100%，显示 checkmark
   - Novel 消失动画是否流畅，不遮挡填充效果

3. 状态重置测试：
   - 验证 `ResetIcon()` 方法能正确重置状态

---

## 调整点

1. **资源准备**：需要准备虚线圆环底图、填充圆环、icon 高亮版、checkmark 图标
2. **Prefab 配置**：确保 Image 组件的 Fill Method 设置为 Radial 360
3. **动画参数**：可在 Inspector 中调整 `FillDuration` 和 `IconAlphaDuration`

---

## 相关文件

- `Assets/Script/UI/GoalIconUIController.cs` — 控制器脚本
- `Assets/Script/Goals/Goal.cs` — Novel 消失动画优化
- `Assets/Prefabs/all_goal_item_shadow/goal1_get.prefab` — Goal Icon Prefab
- `Assets/Textures/UI/` — 资源图片

---

## 后续步骤

1. 在 Unity Editor 中打开 `goal1_get` Prefab，配置 `dotCircleFilledImage` 引用
2. 测试 step1 和 step2 的动画效果
3. 如有需要，调整动画参数或资源
