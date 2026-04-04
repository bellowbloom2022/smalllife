# 架构说明 — InputRouter 输入系统

**文件：** `Assets/Script/Core/InputRouter.cs`

---

## 核心机制

`InputRouter` 是单例 MonoBehaviour，控制全局输入开关。

```
InputLocked == true  →  Update() 中所有点击 / 拖拽 / 滚轮 事件全部跳过
InputLocked == false →  正常分发输入事件
```

### 关键特性：**单计数器（非栈）**

```csharp
// 伪代码
void LockInput()   { InputLocked = true; }
void UnlockInput() { InputLocked = false; }
```

只要调用 `LockInput()` 后没有对应的 `UnlockInput()`，输入将**永久冻结**。

---

## 危险模式

- `LockInput()` 调用后，若后续逻辑（如动画事件 `OnAnimEnd`）未被触发，`UnlockInput()` 就不会执行
- 这不会报错，玩家只会感觉"游戏卡住了"

---

## 防御性规范

凡是调用 `LockInput()` 的地方，必须同时安排 fallback unlock：

```csharp
// Goal.cs 中的实现
DOVirtual.DelayedCall(fallbackDelay, () =>
{
    if (lockVersion != inputLockVersion) return; // 已被正常 cancel
    InputRouter.Instance.UnlockInput();
});
```

---

## 与 Goal 系统的交互

见 [goal-step-system.md](goal-step-system.md)

---

## 与 Panel_info 的交互

- `InfoPanelController` 当前会监听 `InputRouter.OnBlankClick`，用于在展开态点击空白区域后折回侧边栏。
- 由于 `Panel_info` 已改为“常驻 active + 折叠/展开”模型，它不再完全符合传统 `BasePanel` 的显示语义。
- 若后续调整空白点击、UI 穿透或侧边栏关闭行为，建议同时查看 `../tasks/resolved/panel-info-completion-flow.md`。
