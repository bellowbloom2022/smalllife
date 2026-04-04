# [已完成] Display Overlay 色调持久化与渲染链修复

**状态：** ✅ 已完成  
**完成日期：** 2026-04-04

---

## 问题现象

1. 勾选颜色后，画面颜色不跟随变化（UI 状态与视觉状态不一致）。
2. 首页设置颜色后，进入 Level 打开设置面板会回到白色（`overlayColorIndex=0`）。
3. 运行时出现 `Missing(Material)` / `Missing(Sprite)` 与 `Failed to find UI/Skin/*.psd`。
4. Display 设置脚本过长，调试和维护成本高。

---

## 根因总结

### A. 渲染链问题（看得到状态但看不到颜色）

- 运行时 overlay 的 `Image` 在某些路径下材质丢失（`Missing(Material)`）。
- 乘法 shader 与 UI 颜色传递链路不稳定（`Image.color` 未稳定作用到最终叠加）。
- 运行时曾依赖 `Resources.GetBuiltinResource("UI/Skin/*.psd")`，在当前环境不可用，导致 `Source Image` 缺失。

### B. 持久化问题（进入 Level 后回白）

- 核心触发链：新游戏流程触发 `SaveSystem.ClearData()` 后，`gameData = new GameData()` 重置了 `settings`，导致 `overlayColorIndex` 回到默认 `0`。
- 后续 `Level.Start()` 会保存当前 `GameData`，把白色索引继续写盘，表现为“打开 Display 就是白色”。

---

## 关键修复

### 1) Overlay 运行时对象统一

- 切换为唯一运行时 overlay：`__OverlayColorRuntimeCanvas` + `__OverlayColorRuntimeImage`。
- 保持顶层排序并使用 `OverlayOrderLocker` 维护层级。
- 颜色应用收敛到单一路径，避免多对象扫描导致状态分叉。

### 2) 叠加效果改为乘法染色（非雾化）

- 使用 `UI/MultiplyOverlay` 实现正片叠底感。
- 修正 shader 输入链路，使 `Image.color` 能稳定驱动叠加结果。

### 3) 资源依赖去风险

- 不再依赖 `UI/Skin/Background.psd` / `UISprite.psd`。
- 改为运行时生成 `1x1` 白色纹理与 sprite，保证任意环境可渲染。

### 4) 切页签触发回写防护

- Display 页签激活前后做同步保护，避免 Toggle 激活时写回默认值。

### 5) 新游戏清档策略修复（最关键）

- `SaveSystem.ClearData()` 改为：清进度但保留 `GameSettings`（含 `overlayColorIndex`、音量、语言、显示模式等）。
- 清档后立即保存，保证内存与磁盘一致。

### 6) 脚本模块化与清理

- `DisplaySettingsController` 拆分为 partial：
  - 主文件：分辨率/显示模式
  - `DisplaySettingsController.Overlay.cs`：overlay 逻辑
- 新增独立文件：`OverlayOrderLocker.cs`
- 清理了冗余与临时代码（包括本轮调试日志）。

---

## 结果

- 首页/Level/Pause 的色调选择与显示一致。
- 新游戏后玩家显示偏好不再丢失。
- 叠加效果稳定且不再出现 `Missing(Material)` / `Missing(Sprite)` / `UI/Skin` 找不到问题。
- 代码结构更清晰，后续维护成本下降。

---

## 避坑要点

1. `ClearData()` 不要直接重置所有设置：
   - 进度可清，但 `GameSettings` 应保留。
2. `DontDestroyOnLoad` 对象的材质生命周期要谨慎：
   - 场景局部控制器销毁时不要误销毁其运行时共享材质。
3. UI 叠加建议单实例：
   - 避免“多 overlay 对象 + 扫描匹配”导致状态不同步。
4. 不要依赖环境差异大的内置 UI 资源路径：
   - 优先运行时生成可控资源。

---

## 相关文件

- `Assets/Script/UI/DisplaySettingsController.cs`
- `Assets/Script/UI/DisplaySettingsController.Overlay.cs`
- `Assets/Script/UI/OverlayOrderLocker.cs`
- `Assets/Script/UI/SettingsPanel.cs`
- `Assets/Script/SaveSystem/SaveSystem.cs`
- `Assets/Script/Manager/Level.cs`
- `Assets/Script/UI-Multiply.shader`
