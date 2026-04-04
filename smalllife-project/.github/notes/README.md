# Smalllife Project — 任务 & 问题追踪

> 供 Copilot / AI Agent 和开发者使用的文档索引。
> 每次开始新对话时，可以告诉 Copilot "查看 .github/notes 目录" 以快速恢复上下文。

---

## 目录结构

```
.github/notes/
  README.md                        ← 本文件，全局索引
  tasks/
    ongoing/                       ← 进行中任务（未完全解决）
      goal-input-lock-bug.md
    resolved/                      ← 已完成任务（归档）
      goalmanager-raycastall.md
  architecture/                    ← 系统机制说明（帮助 AI 理解项目）
    input-system.md
    goal-step-system.md
```

---

## 进行中任务

| 文件 | 简述 | 状态 |
|---|---|---|
| [goal-input-lock-bug.md](tasks/ongoing/goal-input-lock-bug.md) | Level2 goal201/203/204 Step2 点击输入冻结问题 | ⚠️ 部分修复，待验证 |

---

## 已完成任务

| 文件 | 简述 |
|---|---|
| [goalmanager-raycastall.md](tasks/resolved/goalmanager-raycastall.md) | GoalManager 改为 RaycastAll 多命中版本 |
| [goal-camera-duration-inspector.md](tasks/resolved/goal-camera-duration-inspector.md) | Goal 镜头移动速度改为直接调整场景内的 cameraDuration |
| [goal-note-panel-step-feedback.md](tasks/resolved/goal-note-panel-step-feedback.md) | Goal Step 与 panel-note 联动、控制器拆分、文本动效可配置 |
| [goal-note-panel-controller-refactor.md](tasks/resolved/goal-note-panel-controller-refactor.md) | GoalNotePanelController 等价重构：减少 if 重复、索引缓存、分页与行渲染逻辑收敛 |
| [display-overlay-color-persistence-and-rendering.md](tasks/resolved/display-overlay-color-persistence-and-rendering.md) | Display 色调叠加修复：渲染链稳定、设置持久化与清档保留偏好 |
| [panel-info-completion-flow.md](tasks/resolved/panel-info-completion-flow.md) | Panel_info 完成态、折叠侧边栏、音效与弹出节奏调整 |

---

## 架构说明

| 文件 | 简述 |
|---|---|
| [input-system.md](architecture/input-system.md) | InputRouter 单计数器机制 |
| [goal-step-system.md](architecture/goal-step-system.md) | Goal Step1/Step2 流程与动画事件 |
