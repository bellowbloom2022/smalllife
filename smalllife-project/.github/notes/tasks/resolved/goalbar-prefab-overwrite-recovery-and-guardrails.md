# [已完成] GoalBar Prefab 覆盖事故复盘与防删守则

**状态：** ✅ 已完成  
**完成日期：** 2026-04-08

---

## 背景

在检查并修复 Level3 后，创建/拖拽 GoalBar 相关 prefab 的过程中，Level0/Level1/Level2 的 GoalBar Content 槽位内容被 prefab 状态覆盖，出现“原本关卡内配置丢失”的连带问题。

本次已通过 Git 回退恢复 Level0/1/2，并确认 Level3 单独检查后可继续使用。

---

## GoalBar 当前功能要点（实现基线）

1. GoalBar 使用手动拖拽内容位移（非 ScrollRect 惯性吸附），支持在 icon 区域直接拖动。
2. goal 获取后会先对齐焦点，再执行飞行动画，避免落点偏移到 Content 外。
3. 获取完成后可延迟清理 `mGameObjectNovel`（默认 1s），防止场景残留漂浮物。
4. icon 保持可射线命中以兼容 ShowTextOnUI，同时通过转发拖拽事件保证滚动体验。

---

## 本次失误点（必须避免）

1. 在场景实例上直接创建/应用 GoalBar prefab，导致多关卡共享结构被覆盖。
2. 未先确认其他关卡是否仍处于“prefab 丢失/断链状态”就继续应用结构性改动。
3. 对 GoalBar Content 子节点（goal_get 槽位）进行批量操作时，缺少“先备份 + 先 diff”的双保险。

---

## 已执行恢复动作

1. 已对受影响场景执行恢复：
   - `Assets/Scenes/Level0.unity`
   - `Assets/Scenes/Level1.unity`
   - `Assets/Scenes/Level2.unity`
2. 使用命令：

```bash
git restore --source=HEAD -- Assets/Scenes/Level0.unity Assets/Scenes/Level1.unity Assets/Scenes/Level2.unity
```

3. 以 `git status --short -- <scene files>` 验证回退后为 clean。

---

## 强警告（高优先级）

## ⚠️ 绝对禁止删除以下 prefab

- `GoalBar 1`
- `GoalBar`
- `GoalBar-default`

以上三个 prefab 属于关卡 GoalBar 结构恢复与引用链稳定的关键资产。删除其中任意一个，可能导致：

1. 多关卡 GoalBar Content 槽位丢失或被重置
2. 关卡内目标 icon 引用断链
3. 后续 prefab 覆盖时出现不可预期的批量污染

---

## 后续操作守则

1. 先在独立测试场景验证 GoalBar prefab 改动，再回灌到正式关卡。
2. 修改前先备份场景文件，并记录当前 `git status`。
3. 涉及 GoalBar 结构调整时，必须同步抽查 Level0/1/2/3 的 Content 子节点完整性。
4. 仅在确认引用稳定后再保存场景，避免把临时错误写回磁盘。

---

## 相关文件

- `Assets/Scenes/Level0.unity`
- `Assets/Scenes/Level1.unity`
- `Assets/Scenes/Level2.unity`
- `Assets/Scenes/Level3.unity`
- `Assets/Script/UI/GoalIconBarController.cs`
- `Assets/Script/UI/GoalIconUIController.cs`
- `Assets/Script/Goals/Goal.cs`
