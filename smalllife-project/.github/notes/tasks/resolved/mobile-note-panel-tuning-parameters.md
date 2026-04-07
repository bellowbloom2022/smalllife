# 手机笔记面板界面参数调整记录

**最后更新**：2026年4月7日  
**状态**：✅ 已完成和验证  
**相关代码**：[GoalNotePanelController.cs](../../../Assets/Script/UI/GoalNotePanelController.cs)

---

## 概述

本文档记录手机笔记面板（Note Panel）的所有视觉参数调整。通过这些参数，可以微调面板的布局、排版和交互，使其更接近日常使用的手机笔记本应用。

**设计目标**：让手机界面的感觉尽量接近日常使用的手机笔记本界面。

---

## 📐 布局参数（Auto Layout）

这些参数控制笔记面板的行布局与间距。所有参数都在 `GoalNotePanelController` 的 Inspector 中实时可调。

| 参数名 | 当前值 | 类型 | 单位 | 说明 |
|--------|--------|------|------|------|
| `autoLayoutLeftInset` | **50** | float | px | 左边距；增加会让文本更靠右 |
| `autoLayoutRightInset` | **55** | float | px | 右边距；增加会让文本更靠左 |
| `autoLayoutTopInset` | **80.5** | float | px | 上边距（从面板标题到第一行的距离）；防止与标题重叠 |
| `autoLayoutBottomInset` | **30** | float | px | 下边距（行结束到分页控件的距离） |
| `autoLayoutSpacing` | **12** | float | px | 行之间的垂直间距；减小为紧凑，增加为松散 |
| `autoLayoutRowMinHeight` | **68** | float | px | 单行最小高度；若文本显示压缩，增加此值 |
| `rowInternalTextSpacing` | **8** | float | px | 行内部描述与总结之间的间距 |

### 推荐调整范围

- **左右边距**：20–60 px（根据屏幕宽度和美感调整）
- **上下边距**：50–150 px（上边距需考虑标题位置）
- **行间距**：5–15 px（过小易显得拥挤，过大则显得疏松）
- **行最小高度**：60–90 px（取决于字号与行高）
- **行内文本间距**：2–10 px

---

## 🔤 排版参数（Typography）

这些参数控制文本的字号、样式和特殊符号。

| 参数名 | 当前值 | 类型 | 说明 |
|--------|--------|------|------|
| `descriptionFontSize` | **25** | int pt | 目标描述文本字号；统一支持中英日 |
| `summaryFontSize` | **28** | int pt | 目标总结文本字号（强调用，较大） |
| `summaryQuotePrefix` | **"│ "** | string | 总结文本前缀；可改为 "❝"、">"、"→" 等风格 |
| `descriptionFinalBold` | **false** | bool | 描述文本最终是否加粗 |
| `descriptionFinalItalic` | **false** | bool | 描述文本最终是否斜体 |
| `summaryFinalBold` | **true** | bool | 总结文本最终是否加粗 |
| `summaryFinalItalic` | **true** | bool | 总结文本最终是否斜体 |

### 推荐调整范围

- **字号**：描述 23–26 pt（保证可读性）；总结 26–30 pt（应大于描述有层级感）
- **前缀符号**：保持简洁，1–2 字符；建议例子：
  - `"│ "` — 左边框线感（当前）
  - `"❝ "` — 引号风格
  - `"▪ "` — 点号风格
  - `"→ "` — 方向箭头

---

## 🎨 文本颜色与交互（Text Trigger Visual）

触发词（trigger words）的着色。

| 参数名 | 当前值 | 说明 |
|--------|--------|------|
| `triggerNormalColor` | **黑色** | 普通触发词颜色 |
| `triggerAccentColor` | **#CC6666**（淡红） | 重点触发词颜色 |
| `resetColorOnFinish` | **true** | 动画完成后是否恢复原色 |

---

## 💾 文件映射

### 脚本文件

- **主控制器**：[GoalNotePanelController.cs](../../../Assets/Script/UI/GoalNotePanelController.cs)
  - 包含所有上述参数的 `[SerializeField]` 声明
  - 实现实时参数更新的 `LateUpdate()` 循环

- **行内容更新**：[GoalNoteRowUpdater.cs](../../../Assets/Script/UI/GoalNoteRowUpdater.cs)
  - 处理前缀符号注入（`summaryQuotePrefix`）
  - 字符清理（移除 `*` 和 `★`）

- **字体与语言**：[LanguageFontAutoApplier.cs](../../../Assets/Script/Localization/LanguageFontAutoApplier.cs)
  - 仅处理语言字体的切换，**不** 修改行高
  - 行高由下述 prefab 独立控制

### Prefab 文件

- **主面板**：[Panel-note.prefab](../../../Assets/Prefabs/UI/Panel/Panel-note.prefab)
  - 包含 18 个行实例（rowRoot）
  - 运行时由脚本自动排列到 `RuntimeAutoLayoutRoot`

- **行模板**：[rowRoot.prefab](../../../Assets/Prefabs/UI/Panel/rowRoot.prefab)
  - 描述文本（descriptionText）行高：`1.0`（不被代码覆盖）
  - 总结文本（summaryText）行高：`1.0`（不被代码覆盖）
  - **关键**：此文件中的行高设置是最终权威；脚本不会覆盖

---

## 🌍 多语言支持

### 当前状态

✅ **中文 (Simplified Chinese)**  
✅ **英文 (English)**  
✅ **日文 (Japanese)**

所有三种语言使用**统一的字号**（description 25pt，summary 28pt）和**统一的行高** (1.0)。

### 关键约束

- **不要** 向 `LanguageFontAutoApplier.cs` 中添加语言特定的行高覆盖
  - 会导致不同语言显示不一致
  
- **若需要语言特定的排版**，修改以下选项（按优先级）：
  1. 编辑 [rowRoot.prefab](../../../Assets/Prefabs/UI/Panel/rowRoot.prefab) 中的 `lineSpacing` 字段来全局调整
  2. 调整 `descriptionFontSize` / `summaryFontSize` 来改变字号
  3. 编辑 `LanguageFontAutoApplier.cs` 中的字体选择逻辑（仅字体，不要触及行高）

---

## 🔄 Play Mode 实时调整

参数支持在 Unity Play Mode 中实时修改：

1. 进入 Play Mode
2. 在 Inspector 中修改 `GoalNotePanelController` 的参数
3. 按下面板中的翻页按钮或加载新关卡，观察效果
4. **参数修改立即生效**（通过 `LateUpdate()` 逐帧检查）

> 💡 **提示**：这允许你快速迭代设计，无需重新启动游戏。

---

## ✅ 验证清单

运行时检查以下内容，确保参数正确生效：

- [ ] **布局**：18 行均匀分布于面板内，无重叠或挤压
- [ ] **边距**：文本不与面板边框或标题重叠
- [ ] **字号**：描述文本清晰可读（25pt）；总结文本清晰且有强调感（28pt）
- [ ] **前缀符号**：总结文本开头显示 `│` 或选定的符号
- [ ] **多语言**：
  - 切换至中文 → 文本排版一致
  - 切换至英文 → 文本排版一致
  - 切换至日文 → 文本排版一致
- [ ] **分页**：翻页按钮功能正常，灰点指示器显示当前页
- [ ] **颜色**：触发词着色正确，动画结束后恢复原色

---

## 🐛 已知情况与修复

### 曾经的问题及解决方案

| 问题 | 原因 | 解决方案 |
|------|------|--------|
| 行大小固定为 100×100，不遵循模板 | Prefab 实例覆盖 (instance overrides) 导致设置被锁定 | 改为运行时自动布局系统 |
| 文本从中心向上显示，非顶部向下 | RectTransform anchor 设为 (0.5, 0.5) | 改为 (0, 1)，实现顶部对齐 |
| 第2页的行不在自动布局容器内 | 层级不同步 | 添加行发现与重新父组件的逻辑 |
| 多语言时行高不一致 | 代码中硬编码的语言特定行高 | 删除所有语言特定行高逻辑 |
| 总结前缀在语言切换后消失 | 前缀在后处理回调中注入，语言切换时被覆盖 | 前缀改为嵌入文本源 |

### 维护警告

⚠️ **不要**：
- 向 `LanguageFontAutoApplier.cs` 添加 `lineSpacing` 赋值
- 在运行时手动设置 row 的 LocalScale（会破坏自动布局）
- 使用手动 Anchor/Position，而不是依赖 VerticalLayoutGroup + ContentSizeFitter

✅ **应该**：
- 所有布局参数调整通过 GoalNotePanelController Inspector 进行
- 所有字体相关调整也通过 Inspector 进行
- 若需永久性改动，先在 Play Mode 验证效果，再更新脚本默认值

---

## 📝 更新记录

| 日期 | 变化 | 记录者 |
|------|------|--------|
| 2026-04-01 | 初始记录；参数数值与代码/Inspector 一致化 | @User |
| 2026-04-07 | 完成语言字体路由验证：中文使用简中字体，日文使用日文字体；切换语言与切场景后显示稳定 | @Copilot |

---

## 💬 相关讨论

- [Goal Input Lock Bug](./goal-input-lock-bug.md) — 输入锁定相关
- [Goal Step System Architecture](../../architecture/goal-step-system.md) — 目标步骤系统
- [Input System Architecture](../../architecture/input-system.md) — 输入系统

---

## 🎯 快速参考

**最常调的参数**（日常美化时）：
- `autoLayoutSpacing` — 行距离感
- `autoLayoutTopInset` — 与标题的距离
- `descriptionFontSize` / `summaryFontSize` — 整体字号
- `summaryQuotePrefix` — 总结符号风格

**最不常改的参数**（结构性）：
- `rowsPerPage` — 每屏行数
- `resetColorOnFinish` — 色彩逻辑

