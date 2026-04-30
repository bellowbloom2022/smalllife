# [进行中] Goal Icon Hover 描述框（descriptionText）重构

**状态：** 🕒 已暂停，待继续  
**优先级：** 中  
**记录日期：** 2026-06-11

---

## 背景与目标

为 goal-icon 下方添加 hover 时显示的描述框（descriptionText），类似笔记本提示面板的简化版本。

---

## 📋 变更记录

### 2026-06-11 变更

**ShowTextOnUI.cs**
```csharp
// 修改前（错误）
string translated = LeanLocalization.GetTranslationText(pageLabelFormatPhrase.gameObject.name);

// 修改后
string translated = LeanLocalization.GetTranslationText(pageLabelFormatPhrase.Name);
```

**goal1_get.prefab**
- 在 `goal-note-background` 的 `m_Children` 中添加了 `descriptionText` 引用
- fileID: `8704782998656541746`

---

## 🔄 初始化顺序（关键！）

理解 Unity 组件生命周期顺序对于正确获取 `Level.ins.levelDataAsset` 至关重要：

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
```

**⚠️ 重要提醒**：
- `GoalIconUIController.InitializeInlineGoalNote()` 在 `Level.Start()` 之后执行
- 此时 `Level.ins.levelDataAsset` 已被正确注入
- **禁止在更早的生命周期（如 `Awake`、`OnEnable`）中调用需要 `Level.ins` 的方法**

---

## 📋 待办事项

- [ ] 方案重新设计：考虑不在 Prefab 中直接添加子节点，而是通过代码动态创建/控制
- [ ] LevelDataAsset 引用验证：确保 descriptionText 能正确读取 `levelDataAsset.goalDescriptionKeys`
- [ ] 回归测试：
  - [ ] goal-icon hover 显示描述框
  - [ ] GoalBar 滚动/拖拽正常
  - [ ] 存档/读档进度正确
  - [ ] Level0/1/2/3 无异常

---

## 📁 相关文件

| 文件 | 作用 |
|------|------|
| `Assets/Script/UI/ShowTextOnUI.cs` | Inline goal note 显示控制 |
| `Assets/Script/UI/GoalIconUIController.cs` | Icon 状态管理，调用 ShowTextOnUI |
| `Assets/Script/Manager/Level.cs` | 关卡管理器，注入 levelDataAsset |
| `Assets/Script/SaveSystem/LevelDataAsset.cs` | ScriptableObject，存储关卡数据 |
| `Assets/Prefabs/all_goal_item_shadow/goal1_get.prefab` | Goal icon UI 预制体 |

---

## 📖 关联笔记

- `.github/notes/tasks/ongoing/goal-icon-hover-info-panel-detach-plan.md` - Hover 信息框脱离 Viewport 方案
- `.github/notes/tasks/resolved/goalbar-prefab-overwrite-recovery-and-guardrails.md` - GoalBar Prefab 保护守则
