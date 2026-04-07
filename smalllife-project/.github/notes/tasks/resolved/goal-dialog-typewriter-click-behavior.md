# [已完成] GoalDialog 打字与点击交互优化

**状态：** ✅ 已完成  
**完成日期：** 2026-04-06

---

## 目标

统一 GoalDialog 的文本播放与关闭交互，覆盖以下需求：

1. 同一条 GoalDialog 首次播放打字，后续重复查看直接全文。
2. 打字过程中允许点击一次直接全文（SkipToEnd）。
3. 左键和右键点击空白都可关闭 GoalDialog，避免右键拖拽模式下对话残留。

---

## 修改文件

- `Assets/Script/Manager/DialogueManager.cs`
- `Assets/Script/Core/InputRouter.cs`
- `Assets/Script/Player/GoalManager.cs`

---

## 实现摘要

### 1) 首播打字 / 复播秒出

在 `DialogueManager` 中为每个对话根节点记录是否已播放：

- 首次：`LocalizedTypewriterEffect.Play(..., instant: false)`
- 再次：`LocalizedTypewriterEffect.Play(..., instant: true)`

并缓存 `LocalizedTypewriterEffect` 组件，避免每次显示都层级查找。

### 2) 打字中点击一次全文

当 GoalDialog 处于打字中，点击空白时优先执行 `SkipToEnd()`，本次点击不关闭对话。

为避免同一点击穿透触发场景点击流程，增加一次性抑制标记，由 `GoalManager` 在处理 `OnClick` 时先消费。

### 3) 左右键统一可关闭

在 `InputRouter` 中新增：

- `OnBlankClickAnyButton(int mouseButton)`

并保持兼容：

- 左键仍派发旧事件 `OnBlankClick`（兼容既有订阅方）
- 右键通过 `OnBlankClickAnyButton` 驱动 GoalDialog 隐藏

`DialogueManager` 中仅对右键分支做额外处理，避免左键在同帧重复触发造成“刚弹出就跳过”的问题。

---

## 修复过程中的回归与处理

### 回归现象（已修复）

初版将 `DialogueManager` 仅绑定到 `OnBlankClickAnyButton`，导致左键同一帧出现双路径处理：

- 对话刚显示即被判定为“打字中点击”并 `SkipToEnd()`
- 下一次点击可能被抑制标记吞掉，出现“只能点一次，后续不再弹出”的体验

### 修复方案

- 恢复 `OnBlankClick` 订阅（左键主路径）
- `OnBlankClickAnyButton` 仅处理 `mouseButton == 1`（右键）

---

## 性能评估

整体为轻量改动，无明显帧风险。

已落地的优化点：

1. 对同一对话根节点只做一次 RaycastBlocking 关闭（避免重复 `GetComponentsInChildren`）。
2. `LocalizedTypewriterEffect` 查找缓存（减少重复层级遍历）。

---

## 验证清单

- [x] 首次点开 GoalDialog 有打字效果
- [x] 打字中点击可一次性全文显示
- [x] 再次点开同一 GoalDialog 直接全文
- [x] 左键空白可关闭 GoalDialog
- [x] 右键空白可关闭 GoalDialog
- [x] 相关脚本无编译错误

---

## 经验

1. 输入路由扩展事件时，必须明确左键主流程与额外按键流程的时序边界。
2. “同次点击防穿透”机制应只作用于目标场景，不要覆盖全局默认点击链路。
3. UI 对话类流程易被 `OnClick`/`OnBlankClick` 双链路影响，修改后应重点回归“首帧点击”行为。
