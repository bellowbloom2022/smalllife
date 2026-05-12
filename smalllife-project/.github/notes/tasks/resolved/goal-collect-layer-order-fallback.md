# [已完成] Goal 收集飞行动画层级兜底

**状态：** ✅ 已完成  
**完成日期：** 2026-05-12

---

## 背景

`Goal.cs` / `Goal.Collect.cs` 中的：

- `mGameObjectNovel`：场景中飞向 GoalBar 的收集物 UI，一般放在 `Canvas/goal_item_object` 下。
- `mNovelPos`：GoalBar 内对应 goal icon 的落点引用，一般位于 `GoalBar` / `GoalBar-levelX` prefab 内。

运行时需求是：当双步 Goal 或 SingleGoal 完成并触发收集动画时，`mGameObjectNovel` 必须在飞向 GoalBar 的过程中显示在 GoalBar 上方，避免被 GoalBar 遮挡。

---

## 问题现象

手动调整 `Canvas` 下 `goal_item_object` 与 `GoalBar-level0` 的层级后，仍可能在某些时刻看起来“复位”。排查 `Goal`、`GoalBar` 相关脚本后，没有发现项目代码会直接把 `goal_item_object` 或 `GoalBar` 执行 `SetParent`、`SetSiblingIndex`、`SetAsLastSibling`。

场景序列化中，`GoalBar-level0` 本身位于 `goal_item_object` 后面，因此在同一个 Canvas 下会默认渲染在 `goal_item_object` 上方。运行时如果只依赖手动 hierarchy 顺序，容易再次出现飞行动画被盖住的问题。

---

## 已实施修复

文件：`Assets/Script/Goals/Goal.Collect.cs`

### 1) 收集动画开始前抬高承载层

在 `TriggerCollectAnimation()` 开始时：

1. 获取 `mGameObjectNovel` 的 `RectTransform`。
2. 取其父节点作为收集动画承载层，通常是 `goal_item_object`。
3. 根据 `iconController.transform` 或 `mNovelPos.transform` 找到目标 GoalBar 所在的同级顶层 UI 节点。
4. 将收集动画承载层移动到目标 UI 节点之后，确保渲染在 GoalBar 上方。

### 2) GoalBar 聚焦后再次校正

`GoalIconBarController.TryFocusToGoal(...)` 会在飞行前调整 GoalBar `Content.localPosition`。为避免刷新/聚焦期间 UI 顺序产生变化，`Canvas.ForceUpdateCanvases()` 后会再次调用层级兜底逻辑。

---

## 实现原则

1. 不直接把 `mGameObjectNovel` 设为最后 sibling，而是调整它的父层 `goal_item_object`，保证整个收集物承载层一起位于 GoalBar 上方。
2. 优先只放到目标 GoalBar 同级节点之后，而不是无条件移到整个 Canvas 最顶层，减少盖住其他全屏 UI / Fade / Overlay 的副作用。
3. 如果目标层级无法可靠解析，则退化为 `collectLayer.SetAsLastSibling()`，保证收集动画优先可见。

---

## 关键方法

新增：`ElevateCollectLayerAboveTarget(Transform collectLayer, Transform target)`

行为：

- `collectLayer` 为空：直接返回。
- `target` 为空或 `collectLayer.parent` 为空：`collectLayer.SetAsLastSibling()`。
- 向上查找 `target`，直到找到与 `collectLayer` 同父级的顶层节点。
- 若找到同父级目标，则在必要时执行 `collectLayer.SetSiblingIndex(targetIndex)`，让收集层位于目标 GoalBar 之后。

---

## 验证

- 已对 `Assets/Script/Goals/Goal.Collect.cs` 读取 linter diagnostics：无报错。
- 需要在 Unity 中回归验证：完成普通双步 Goal / SingleGoal 时，收集物飞向 GoalBar icon 的全过程应始终显示在 GoalBar 上方。

---

## 相关文件

- `Assets/Script/Goals/Goal.cs`
- `Assets/Script/Goals/Goal.Collect.cs`
- `Assets/Script/UI/GoalIconBarController.cs`
- `Assets/Prefabs/UI/GoalBar.prefab`
- `Assets/Prefabs/UI/GoalBar-default.prefab`
