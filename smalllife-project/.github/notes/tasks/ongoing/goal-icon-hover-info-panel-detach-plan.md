# [进行中] Goal Icon Hover 信息框脱离 GoalBar Viewport 方案

**状态：** 🕒 已规划，待后续结构性整理  
**优先级：** 中低（方案 B 已验证可用，Steam demo 后可处理）  
**记录日期：** 2026-04-29

---

## 背景

当前正在实现 goal-icon 鼠标 hover 后显示 goal 信息框的功能。

已验证临时方案 B 可用：扩大 `GoalBar` / `Viewport` 高度，并关闭透明背景区域不必要的 `Raycast Target`，可以避免信息框被裁剪，同时不明显影响游戏场景点击输入。

但该方案仍依赖较大的 UI 命中区域与手动 raycast 配置，长期维护上不如结构性方案稳定。

---

## 后续目标：方案 A

将 goal hover 信息框从 `Viewport/Content` 裁剪层级中移出，挂到 `GoalBar` 根节点外侧或更高层 Canvas UI 容器下。

目标效果：

1. 信息框不再受 `Viewport` 的 `Mask` / `RectMask2D` 裁剪。
2. `GoalBar` 本体高度可以保持较小，不需要为了 tooltip 显示扩大到 700/800。
3. 避免透明 UI 区域扩大后拦截游戏场景点击。
4. 保持 goal-icon 横向滚动、拖拽、hover 显示行为不变。

---

## 推荐实现方向

1. 新增或指定一个 tooltip 容器：
   - 可放在 `GoalBar` 根节点同级/子级但不在 `Viewport` 内。
   - 或放到 Canvas 下专门的全局 UI overlay 容器。

2. hover 时由 goal-icon 提供屏幕/Canvas 坐标：
   - 使用 icon 的 `RectTransform` 计算 tooltip 锚点位置。
   - 将位置转换到 tooltip 容器所在 Canvas 坐标系。

3. 信息框只负责显示内容与跟随位置：
   - 不参与 `ScrollRect.content` 布局。
   - 不被 `Viewport` mask 裁剪。
   - 默认关闭不必要的 `Raycast Target`，除非信息框内部有可点击内容。

4. 保留当前方案 B 作为短期 fallback：
   - 若方案 A 回归风险较高，demo 阶段继续使用已验证可用的高度扩大 + raycast 关闭配置。

---

## 回归检查项

- [ ] goal-icon hover 后信息框完整显示，不被 GoalBar Viewport 裁剪。
- [ ] 信息框位置跟随当前 icon，横向滚动后仍能正确定位。
- [ ] GoalBar 拖拽滚动不受影响。
- [ ] 透明 UI 区域不拦截游戏场景点击。
- [ ] 多分辨率下信息框不跑出屏幕关键区域。
- [ ] Level0/1/2/3 的 GoalBar Content 子节点完整性无变化。

---

## 注意事项

- 避免直接改动/应用共享 GoalBar prefab 前未备份场景。
- 涉及 GoalBar 结构调整时，必须同步抽查多个关卡，防止 prefab 覆盖导致 Content 槽位丢失。
- 不删除或重建既有 GoalBar 相关 prefab，遵守已记录的 GoalBar prefab 防删守则。

---

## 关联文件

- `Assets/Script/UI/GoalIconBarController.cs`
- `Assets/Script/UI/GoalIconUIController.cs`
- `Assets/Script/UI/ShowTextOnUI.cs`
- `.github/notes/tasks/resolved/goalbar-prefab-overwrite-recovery-and-guardrails.md`
