# [已完成] 自定义鼠标光标系统

**状态：** ✅ 已完成  
**优先级：** 中  
**记录日期：** 2026-05-01
**完成日期：** 2026-05-01

---

## 背景与目标

替换系统默认鼠标为自定义光标图片，支持三种状态切换：
- **normal**：默认箭头
- **click**：按下鼠标时
- **drag**：拖拽画布时

需兼容 macOS Retina 和 Windows 不同 DPI 分辨率。

---

## 技术要点与踩坑记录

### 1. macOS 硬件光标 64px 上限

macOS 硬件光标（`CursorMode.Auto`）最大支持 **64×64 物理像素**，超过此尺寸光标会直接**消失不显示**。

- 128×128 源图必须运行时缩放到 62px 以内
- 留 2px 余量防止边界问题

### 2. `CursorMode.ForceSoftware` vs `CursorMode.Auto`

| | ForceSoftware | Auto（硬件光标） |
|---|---|---|
| macOS 系统光标 | 不隐藏，两层重叠 ❌ | 正确替换 ✓ |
| DPI 适配 | 需手动缩放 | OS 自动处理 |
| 光标大小限制 | 无硬性上限 | macOS 64px 上限 |
| 渲染方式 | 软件渲染，偶有闪烁 | 硬件渲染，流畅 |

**最终方案：Auto + 运行时缩放**，结合两者优点。

### 3. macOS `Screen.dpi` 返回物理 PPI 而非逻辑 DPI

macOS 上 `Screen.dpi` 返回物理 PPI（14" MBP 约 255），而非逻辑 DPI（应为 192/2x=96 对应的值）。
如果用 `dpi / 96` 计算缩放比，会得到 2.66x，算出 85px 超过上限导致光标消失。

**解决方案**：macOS 上不使用 `Screen.dpi`，改用屏幕物理分辨率判断是否 Retina → 固定 2x 缩放。

### 4. 光标图片设计规格

- **源图尺寸**：128×128（不要用 256×256，缩小时描边会消失）
- **描边粗细**：3~4px（1x 下缩小到 32px 时描边约 1px，刚好可见）
- **Import Settings**：Texture Type = Default, Read/Write Enabled = true, 无压缩（textureCompression = 0）

### 5. Left-drag 模式下 click 与 drag 区分

当 dragMode 设为 left 时，左键同时触发 click 和 drag，需要用**移动距离阈值**区分：
- 累计移动 < 4px → 视为 click（显示 arrow-click）
- 累计移动 ≥ 4px → 视为 drag（显示 arrow-drag）

### 6. Unity Editor 光标缓存 Bug

长时间运行 Unity Editor 后，macOS 硬件光标状态可能不同步（系统光标不消失）。
重启电脑/Unity 后恢复正常。此问题**仅在 Editor 中偶发，build 出的 Player 不会出现**。

---

## 📋 变更记录

### 2026-05-01 变更

**新增文件：**
- `Assets/Script/UI/CustomCursorManager.cs` — 全局自定义光标管理器
- `Assets/Script/UI/CustomCursorManager.cs.meta`
- `Assets/Textures/UI/arrow-normal.png` — 默认箭头光标
- `Assets/Textures/UI/arrow-click.png` — 点击状态光标
- `Assets/Textures/UI/arrow-drag.png` — 拖拽状态光标
- 对应 .meta 文件

**CustomCursorManager.cs 核心功能：**

1. **三种光标状态**：normal / click / drag，优先级 drag > click > normal
2. **DPI 自适应缩放**：macOS 用分辨率判断 Retina，Windows 用 Screen.dpi
3. **拖拽阈值**：累计移动 ≥ 4px 才切换 drag，区分 click 和 drag
4. **订阅 InputRouter.OnDrag**：与游戏拖拽系统联动
5. **跨场景持久**：DontDestroyOnLoad
6. **焦点恢复**：OnApplicationFocus 时重新设置光标

**图片 Import Settings 统一配置：**
- textureType: 0 (Default)
- isReadable: 1 (Read/Write Enabled)
- textureCompression: 0 (无压缩，所有平台)
- alphaIsTransparency: 1

---

## 平台适配效果

| 屏幕 | DPI/缩放 | 缩放比 | 纹理尺寸 | 逻辑显示 |
|------|---------|--------|---------|---------|
| macOS Retina (3456×2234) | ~192 (2x) | 2x | 62px | ~31pt ✓ |
| Windows 1080p | 96 | 1x | 32px | 32pt ✓ |
| Windows 125% | 120 | 1.25x | 40px | 32pt ✓ |
| Windows 150% | 144 | 1.5x | 48px | 32pt ✓ |
