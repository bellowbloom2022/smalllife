# Panel_info 系统清理记录

**状态：** ✅ 已完成  
**清理日期：** 2026-05-12

---

## 背景

每个 Level 已有独立的 `LevelIntroPage` 用于展示关卡名称、详情描述和图片。

关卡信息不再需要场景内的折叠侧边栏，`InfoPanel` 系统（折叠面板 + 告示牌触发）功能已被废弃。

---

## 已删除的代码文件

| 文件路径 | 说明 |
|---------|------|
| `Assets/Script/UI/InfoPanelController.cs` | InfoPanel 主控制器（含折叠/展开、完成态逻辑） |
| `Assets/Script/UI/InfoPanelController.cs.meta` | InfoPanelController 元数据文件 |

| 文件路径 | 说明 |
|---------|------|
| `Assets/Script/UI/SignboardTrigger.cs` | 告示牌点击触发器 |
| `Assets/Script/UI/SignboardTrigger.cs.meta` | SignboardTrigger 元数据文件 |

---

## 已删除的 Prefab 文件

| 文件路径 | 说明 |
|---------|------|
| `Assets/Prefabs/UI/Panel/Panel_infoBody.prefab` | InfoPanel 预制体 |
| `Assets/Prefabs/UI/Panel/Panel_infoBody.prefab.meta` | Panel_infoBody 元数据文件 |

---

## Level.cs 修改内容

### 已删除的字段

```csharp
// 删除了以下字段
public InfoPanelController infoPanel;
public float completionInfoPanelDelay = 1.5f;
private Coroutine completionInfoPanelCoroutine;
```

### 已删除的方法

- `OnDestroy()` — 协程清理代码
- `ScheduleCompletionInfoPanel()` — 延迟弹出调用
- `ShowCompletionInfoPanelAfterDelay()` — 延迟协程
- `ShowCompletionInfoPanelNextFrame()` — 下一帧协程
- `ShowCompletionInfoPanel()` — 完成态展示逻辑

### 已删除的 using

- `System.Collections`（协程相关）

---

## 场景中需要手动清理的部分

以下场景中仍有 `Panel_infoBody` 实例残留，需要在 Unity 编辑器中手动删除：

| 场景文件 | 操作 |
|---------|------|
| `Level0.unity` | 删除 Canvas 下的 Panel_infoBody 实例 |
| `Level1.unity` | 删除 Canvas 下的 Panel_infoBody 实例 |
| `Level2.unity` | 删除 Canvas 下的 Panel_infoBody 实例 |
| `Level3.unity` | 删除 Canvas 下的 Panel_infoBody 实例 |

**操作步骤：**
1. 在 Unity 中打开每个场景
2. 在 Hierarchy 面板中展开 Canvas，找到 `Panel_infoBody`（或类似名称）
3. 右键删除该对象
4. 保存场景（Ctrl/Cmd + S）

---

## 保留的基础设施

以下组件仍被其他系统使用，已保留：

| 文件 | 保留原因 |
|------|---------|
| `Assets/Script/UI/BasePanel.cs` | 多个 UI 面板的基类 |
| `Assets/Script/Manager/SceneChanger.cs` | 被右上角快捷下一关按钮使用 |
| `Assets/Script/UI/UIBlockChecker.cs` | UI 遮挡检测工具 |

---

## 相关文档

- `panel-info-completion-flow.md` ← 原始功能文档（已标记废弃）
- `../../architecture/input-system.md`
- `../../architecture/goal-step-system.md`