# UI 尺寸约束规则（Canvas / Prefab）

**适用范围：** 本项目所有场景 UI 与 UI Prefab（尤其是 PausePanel / SettingPanel 一类弹窗）  
**最后更新：** 2026-04-07

---

## 目标

统一 UI 尺寸约束基线，避免以下常见误判：

- 把 Prefab Mode 中 `Canvas (Environment)` 的显示尺寸当成业务面板尺寸
- 在不同 Game 分辨率（如 1920x1080、3840x2160）切换后，误以为 prefab 被改坏
- 用根节点 `Scale` 修视觉尺寸，导致文本/图像清晰度和适配行为不稳定

---

## 项目基线

1. 场景 Canvas Scaler 统一使用：
   - `UI Scale Mode = Scale With Screen Size`
   - `Reference Resolution = 1920 x 1080`
   - `Screen Match Mode = Match Width Or Height`（按场景需求设置 Match 值）

2. `Canvas (Environment)` 的 `Width/Height` 会随当前 Game 视图分辨率变化：
   - 例如切到 4K UHD 时可见 `3840 x 2160`
   - 该值主要反映“当前预览环境”，不是业务 UI 设计尺寸本身

---

## 真正应关注的尺寸字段

在排查 UI 过大/过小时，优先检查以下三类：

1. 场景级：Canvas Scaler 的 `Reference Resolution` 是否为 `1920 x 1080`
2. Prefab 根级：业务根节点 RectTransform 的 `Anchor / SizeDelta / Scale`
3. 结构级：是否采用“全屏容器 + 内容面板”分层，而不是把单个根节点做成混合语义

---

## 推荐结构（长期方案）

对于弹窗类 UI，建议采用两层：

1. 外层容器（全屏交互层/遮罩层）
   - Anchor Stretch（Min 0,0 / Max 1,1）
   - 四边距为 0

2. 内层内容面板（视觉稿尺寸）
   - 居中锚点
   - 固定 SizeDelta（按设计稿）

并保持根与关键节点 `Scale = (1,1,1)`。

---

## 这条规则为何存在

历史上出现过以下情况：

- 在 Prefab Mode 中看到较大 `Canvas (Environment)` 尺寸（如 3840x2160）
- 同时业务面板根节点尺寸也偏大，导致“看起来问题相同但根因不同”

该规则用于明确区分“预览环境变化”与“业务节点约束错误”，减少排查时间。

---

## 快速检查清单

- [ ] Scene Canvas Scaler 参考分辨率是否为 1920x1080
- [ ] Prefab 业务根节点是否被异常放大（SizeDelta 或 Scale）
- [ ] 是否误把 Canvas Environment 尺寸变化当成 prefab 尺寸错误
- [ ] 在 1920x1080 与至少一个非基线分辨率下，UI 视觉和交互是否一致
