# [已完成] GoalManager 改为 RaycastAll 多命中版本

**状态：** ✅ 已完成  
**完成日期：** 2026-03-25

---

## 背景

原 `GoalManager.cs` 使用 `Physics.Raycast`（单次命中），点击时只取第一个被射线打中的 Collider。
当多个 Goal Collider 重叠时，可能漏选。

---

## 修改内容

**文件：** `Assets/Script/Player/GoalManager.cs`

- 移除了局部 `clickCoolDown` 冷却逻辑
- 改用 `Physics.RaycastAll` 取所有命中
- 新增 `FindNearestGoalFromHits(RaycastHit[])` 方法，按距离取最近的 Goal
- 2D 部分同步改用 `Physics2D.OverlapPointAll`

```csharp
RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
Goal goal = FindNearestGoalFromHits(hits);
```

---

## 注意事项

- 初次修改后 Unity Play Mode 崩溃，排查为 PackageManager Server `write after end` 编辑器状态问题，与代码无关
- 回滚后重新应用本修改，运行正常
