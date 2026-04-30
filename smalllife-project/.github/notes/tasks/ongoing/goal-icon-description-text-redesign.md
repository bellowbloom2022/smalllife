# [已完成] Goal Icon Hover 描述面板重构

**状态：** ✅ 已完成  
**优先级：** 中  
**记录日期：** 2026-06-11
**完成日期：** 2026-04-30

---

## 背景与目标

为 goal-icon 下方添加 hover 时显示的描述面板（descriptionPanel），包含 description text 和 summary text 两页，支持翻页，并实现：
- Step1 完成后自动显示 description 页 2 秒提示
- Step2 完成后自动显示 summary 页 2 秒提示
- 鼠标 hover 保持显示，离开后延迟隐藏
- 鼠标从 icon 移到 panel 时保持显示（子区域检测）

---

## 📋 变更记录

### 2026-04-30 变更（主要重构）

**新增文件：**
- `Assets/Script/UI/GoalInlineDescriptionPanel.cs` — 核心 panel 控制脚本
- `Assets/Script/UI/GoalInlineDescriptionPanel.cs.meta` — Unity meta

**GoalInlineDescriptionPanel.cs 核心功能：**

1. **Hover 显示/隐藏控制**
   - 基于 `Update()` + `IsPointerInHoverZone()` 持续检测鼠标位置
   - 使用 `RectTransform.GetWorldCorners` 判断鼠标是否在 icon/panel 范围内
   - `iconPadding`（默认 20px）扩展 icon 检测区域，弥补 icon 与 panel 之间的间距和圆形 icon 的检测盲区
   - 延迟隐藏（`hideDelay = 0.15s`），允许鼠标从 icon 移到 panel

2. **翻页逻辑**
   - `currentPage = 0` → description text, `currentPage = 1` → summary text
   - Step2 完成前只能看第 1 页，完成后可翻页
   - prevButton / nextButton / pageIndicatorText

3. **文本显示方式**
   - 两个 Text 绝对定位重叠，通过 `CanvasGroup` 控制 alpha 显示/隐藏
   - 不使用 `SetActive`，避免 VerticalLayoutGroup + ContentSizeFitter 布局刷新问题
   - CanvasGroup 首次获取后缓存，避免每帧 `GetComponent`

4. **Step 完成自动提示**
   - Step1/Step2 完成后自动显示对应页面 `completedShowDuration`（默认 2s）
   - 提示期间 hover 进来会取消自动隐藏，由 hover 接管控制
   - 2s 后如果鼠标不在 hover 区域则隐藏

5. **初始隐藏**
   - `Start()` 中 `HideAll(true)` 立即隐藏，避免首帧闪烁

**GoalInlineDescriptionPanel Inspector 字段：**
```
[Header("Goal Identity")]
- levelID: string
- goalID: int

[Header("Info Text 控制")]
- showTextOnUI: ShowTextOnUI

[Header("描述面板根节点")]
- descriptionPanelRoot: GameObject  (单步 goal 可为空)

[Header("文本内容")]
- descriptionText: Text
- summaryText: Text

[Header("翻页控件")]
- prevButton: Button
- nextButton: Button
- pageIndicatorText: Text

[Header("Hover 延迟设置")]
- hideDelay: float = 0.15

[Header("完成提示显示时长")]
- completedShowDuration: float = 2

[Header("Icon hover 检测扩展边距")]
- iconPadding: float = 20
```

**ShowTextOnUI.cs 修改：**
```csharp
// 修改前（错误）
string translated = LeanLocalization.GetTranslationText(pageLabelFormatPhrase.gameObject.name);

// 修改后
string translated = LeanLocalization.GetTranslationText(pageLabelFormatPhrase.Name);
```

**goal1_get.prefab 修改：**
- 在 `goal-note-background` 的 `m_Children` 中添加了 descriptionText 引用
- 添加 descriptionPanel 子节点结构

---

## 🐛 踩坑与解决方案

### 1. VerticalLayoutGroup + ContentSizeFitter 导致 summaryText 不显示
**现象：** Step2 trigger 后切换到第 2 页，summaryText 不显示；hover 离开再进入又能看到  
**原因：** `SetActive` 切换子元素后，ContentSizeFitter 不会在同一帧重新计算布局，容器高度未更新导致文本被裁剪  
**解决：** 两个 Text 绝对定位重叠，用 CanvasGroup 的 alpha 控制可见性，不再使用 SetActive

### 2. Text 的 Anchors 运行时被修改
**现象：** 运行后 descriptionText 和 summaryText 的 anchorMin/anchorMax Y 从 0.5 变成 0  
**原因：** ContentArea 上残留的 VerticalLayoutGroup 会在运行时重置子元素布局  
**解决：** 移除 ContentArea 上的 VerticalLayoutGroup 和 ContentSizeFitter

### 3. 鼠标从 panel 左右/下方移出后面板不消失
**现象：** 鼠标从 icon 方向移出能正常消失，但从 panel 移出则面板一直显示  
**原因：** 只有根节点有 `IPointerExitHandler`，descriptionPanel 没有鼠标事件检测  
**解决：** 最初添加 `PanelHoverDetector` 组件检测 panel 区域鼠标进出，后改为 `Update()` + `RectTransform.GetWorldCorners` 范围检测，统一在 `IsPointerInHoverZone()` 中处理

### 4. 自动提示期间 hover 进来被错误隐藏
**原因：** `OnPointerEnter` 未取消 `autoShowCoroutine`，2s 倒计时到期仍会隐藏  
**解决：** 在 `OnPointerEnter` 中加入 `CancelAutoShow()`

### 5. Hover 检测不灵敏（icon 与 panel 间距 + 圆形 icon）
**现象：** 鼠标 hover 后 panel 会闪烁消失，只有在 icon 中心才能稳定保持显示  
**原因：** `IPointerEnter/Exit` 基于 Graphic Raycast，圆形 icon 边缘和 icon-panel 间距区域检测不到  
**解决：** 放弃 IPointer 事件，改用 `Update()` 每帧检测鼠标位置，`IsPointerInHoverZone()` 通过 `RectTransform.GetWorldCorners` 计算 icon 和 panel 的实际屏幕范围，并增加 `iconPadding`（20px）扩展 icon 检测区域

### 6. 单步 goal info text hover 后不消失
**现象：** 单步 goal 使用独立 prefab（无 descriptionPanel），hover 后 info text 一直显示不消失  
**原因：** `Update()` 中 `isShowing` 判断仅检查 `descriptionPanelRoot.activeSelf`，单步 goal 的 `descriptionPanelRoot` 为 null，导致 `isShowing` 始终 false，`ScheduleHide()` 永远不触发  
**解决：** 新增 `IsAnyUIVisible()` 方法，同时检查 `descriptionPanelRoot` 和 `showTextOnUI.infoText` 的激活状态

---

## ⚡ 代码优化记录（2026-04-30）

| # | 优化项 | 改动 |
|---|--------|------|
| 1 | GC 优化 | `IsPointInRect` 每帧 `new Vector3[4]` → 类级别 `readonly worldCorners` 复用 |
| 2 | 删除死代码 | `isPointerOverRoot` 字段赋值后从未读取，已删除 |
| 3 | 缓存 GetComponent | `GetComponent<RectTransform>()` 每帧调用 → `Awake` 缓存到 `cachedRootRect` |
| 4 | Update 提前返回 | 未显示时 `if (!isShowing) return;` 减少 RectTransform 计算开销 |
| 5 | 消除重复代码 | `AutoHideRoutine`/`DelayedHideRoutine` 各自调用 `HideInfoText+HideDescriptionPanel` → 统一 `HideAll(true)` |

---

## 🔄 初始化顺序（关键！）

```
1. Level.Awake()
   └─ ins = this（Level 单例初始化）

2. Level.Start()
   ├─ CacheGoals()        ← 注入 levelDataAsset 到每个 Goal 组件
   ├─ LoadGameData()       ← 读取存档
   ├─ LoadAllGoalStates()  ← 应用进度到 Goal
   ├─ UpdateGoalText()    ← 更新 UI 文本
   └─ UpdateLevelGoals()  ← 更新 Goal 对象状态

3. GoalIconUIController.Start()
   ├─ InitializeInlineGoalNote()
   │  └─ 依赖 Level.ins.levelDataAsset 获取描述文本
   └─ ApplyProgressFromSave()

4. ShowTextOnUI 生命周期
   ├─ Awake() → ResolveInlineGoalNoteReferences()
   ├─ Start() → AutoConfigureFromGoalContextIfNeeded()
   └─ OnEnable() → 注册 GoalCompleted 事件

5. GoalInlineDescriptionPanel.Start()
   ├─ ResolveGoalIdentity()    ← 从 GoalIconUIController 获取 levelID/goalID
   ├─ ResolveTextKeys()        ← 从 levelDataAsset 获取本地化 key
   ├─ RestoreStateFromSave()   ← 恢复 isStep2Done
   ├─ BindButtons()            ← 翻页按钮绑定
   └─ HideAll(true)            ← 初始隐藏
```

---

## 📁 相关文件

| 文件 | 作用 |
|------|------|
| `Assets/Script/UI/GoalInlineDescriptionPanel.cs` | 描述面板核心控制（新增） |
| `Assets/Script/UI/ShowTextOnUI.cs` | Info text 显示控制 |
| `Assets/Script/UI/GoalIconUIController.cs` | Icon 状态管理 |
| `Assets/Script/Manager/Level.cs` | 关卡管理器，注入 levelDataAsset |
| `Assets/Script/SaveSystem/LevelDataAsset.cs` | ScriptableObject，存储关卡数据 |
| `Assets/Prefabs/all_goal_item_shadow/goal1_get.prefab` | 多步 Goal icon UI 预制体 |
| `Assets/Prefabs/all_goal_item_shadow/Sgoal_get.prefab` | 单步 Goal icon UI 预制体（无 descriptionPanel） |

---

## 📖 关联笔记

- `.github/notes/tasks/ongoing/goal-icon-hover-info-panel-detach-plan.md` - Hover 信息框脱离 Viewport 方案
- `.github/notes/tasks/resolved/goalbar-prefab-overwrite-recovery-and-guardrails.md` - GoalBar Prefab 保护守则
