# Dropdown CaptionText 与 LeanLocalizedText 冲突

**最后更新**：2026年4月7日  
**状态**：✅ 已解决  
**相关预制体**：[SettingPanel 1.prefab](../../../Assets/Prefabs/UI/Panel/SettingPanel%201.prefab)

---

## 问题现象

`DisplayPanel` 下的 `Dropdown_DisplayMode`，其 Caption Text 绑定的 `Label（Text）` 上挂了 `LeanLocalizedText` 组件，但切换语言后 Caption 显示内容不随语言变化。

---

## 根因分析

两个问题叠加导致：

1. **translationName 为空**：`Label` 上的 `LeanLocalizedText` 组件 `translationName` 字段未填写任何 phrase，因此本地化系统根本不知道该翻译什么。

2. **Dropdown 接管 CaptionText**：Unity 的 `UI.Dropdown` 会在初始化及每次选项变更时，直接把当前选中项的 `m_Text` 写入 `CaptionText`（即这个 `Label`）。`LeanLocalizedText.UpdateTranslation()` 写入的文本会被 Dropdown 的刷新逻辑覆盖。

**结论**：凡是被 `Dropdown.CaptionText` 引用的 `Text` 对象，其显示内容由 `Dropdown` 本身完全掌控，直接挂 `LeanLocalizedText` 不生效。

---

## 修复方式

- **移除** `Dropdown_DisplayMode/Label` 上的 `LeanLocalizedText` 组件。
- **保留** `Dropdown_DisplayMode` 本体上的 `LeanLocalizedDropdown` 组件，由它负责更新各选项的翻译文本（phrase 配置：`global_fullscreen` / `global_windowed`）。
- 语言切换后 `LeanLocalizedDropdown` 会自动更新 options，Dropdown 随即刷新 Caption，无需额外代码。

---

## 适用范围

所有使用 `UI.Dropdown`（非 TMP）+ `CaptionText` 的场景均存在相同机制，不要对 CaptionText 所指向的 Text 子节点单独挂 `LeanLocalizedText`。  
TMP 版本（`TMP_Dropdown`）同理，应使用 `LeanLocalizedTMP_Dropdown` 而非在 Label 上挂翻译组件。
