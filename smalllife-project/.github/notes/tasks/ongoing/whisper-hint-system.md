# [进行中] Whisper Hint 朋友耳语软提示系统

**状态：** ⚠️ 已完成第一版脚本与 UI 接入，仍在关卡调参/验证中  
**优先级：** 高（Level2/Level3 玩家卡住体验优化）  
**记录日期：** 2026-05-09

---

## 背景

测试反馈显示，玩家在没有朋友旁边解释时，可能会在较大的关卡里卡住并产生焦虑。目标不是直接告诉答案，而是做一个“朋友在旁边轻声提醒”的软提示系统：

1. 先降低焦虑。
2. 再提示观察方法。
3. 最后根据玩家关注对象给阶段性软提示。
4. 暂时不做定位、高亮、镜头移动等强提示。

当前设计重点：如果玩家长时间没有推进 `Step1/Step2`，自动出现 30s / 60s / 90s 分层提示。

---

## 已实现内容

### 1. 通用 30/60/90 提示

新增 `WhisperHintManager`：

- 30 秒：读取 `comfortHintKeys`。
- 60 秒：读取 `observationHintKeys`。
- 90 秒：若没有明确关注的 Goal，读取 `stageFallbackHintKeys`。
- 完成任意 `GoalNoteEvents.GoalCompleted` 后重置计时。
- 使用 LeanLocalization key；如果 `Text` 上挂了 `LeanLocalizedText`，运行时会动态切换 `TranslationName`，避免被固定 key 覆盖。

相关文件：

- `Assets/Script/Goals/WhisperHintManager.cs`

### 2. Goal-specific 阶段提示

新增 `GoalWhisperHintConfig`，挂在具体 `Goal` 上：

- `preAnim1HintKeys`
- `postAnim1HintKeys`
- `postAnim2HintKeys`

90 秒时，如果系统判断玩家有明确关注对象，会根据该 `Goal` 当前阶段读取对应 key。

相关文件：

- `Assets/Script/Goals/GoalWhisperHintConfig.cs`
- `Assets/Script/Goals/Goal.Hints.cs`

### 3. 玩家注意力追踪

新增 `GoalAttentionTracker`：

注意力来源：

- 玩家 hover / 阅读 `goal-icon description`。
- 玩家鼠标稳定停留在 `GoalHintZone`。
- 某个 Goal 完成 `Step1` 后，给该 Goal 较高注意力分。

当前默认参数已调整为更容易保留注意力到 90 秒：

```text
goalIconHoverScore = 3.5
step1ProgressScore = 5
scoreDecayPerSecond = 0.02
```

注意：如果场景内组件已存在，Unity Inspector 可能保留旧值，需要手动检查。

相关文件：

- `Assets/Script/Goals/GoalAttentionTracker.cs`
- `Assets/Script/UI/GoalInlineDescriptionPanel.cs`

### 4. 场景故事范围 GoalHintZone

新增 `GoalHintZone`：

- 用于给“玩家正在看哪个故事区域”加注意力分。
- 支持 `Collider2D` / `Collider`。
- 可配置阶段启用：`PreAnim1` / `PostAnim1` / `PostAnim2`。
- 建议放在独立 Layer，例如 `GoalHintZone`，不要包含在 `GoalManager.stepClickLayerMask` / `dialogueClickLayerMask` 中，避免大范围 collider 干扰点击。

相关文件：

- `Assets/Script/Goals/GoalHintZone.cs`
- `ProjectSettings/TagManager.asset`（已新增 Layer）

### 5. 提示显示位置

`WhisperHintManager` 新增：

- `globalHintAnchor`：通用 30/60/90 提示固定位置，例如左下角 UI anchor。

`GoalWhisperHintConfig` 新增：

- `defaultHintAnchor`
- `preAnim1HintAnchor`
- `postAnim1HintAnchor`
- `postAnim2HintAnchor`

Goal-specific hint 会优先显示在对应阶段 anchor 附近；如果 anchor 没填，或世界坐标 anchor 当前不在屏幕内，会 fallback 到 `globalHintAnchor`，避免大场景中提示出现在玩家看不到的位置。

### 6. 提示关闭方式

当前提示不再按固定 `showDuration` 自动消失。

显示后关闭规则：

- 至少显示 `minVisibleDuration` 秒。
- 玩家点击提示框本身时关闭。
- 玩家继续进行明确操作时，延迟 `playerActionDismissDelay` 秒淡出：
  - 点击场景。
  - 拖动画布。
  - 滚轮缩放。
  - 完成 Step1 / Step2。
- 鼠标移动本身不会关闭提示。

相关参数：

```text
minVisibleDuration = 3
playerActionDismissDelay = 1
closeOnHintClick = true
dismissOnPlayerAction = true
```

---

## Hinttext 出现规则（测试用）

### 0. 计时基准

`WhisperHintManager` 不是按鼠标不动计时，而是按“距离上一次 Goal 进展过去多久”计时。

会重置计时的事件：

- 进入关卡后 `Start()` 调用 `ResetIdleTimer()`。
- 任意 `GoalNoteEvents.GoalCompleted` 触发后重置计时。
  - 两步 Goal 的 `Step1` 完成会触发。
  - 两步 Goal 的 `Step2` 完成会触发。
  - 单步 Goal 完成也会触发。

不会重置计时的行为：

- 鼠标移动。
- 拖动画布。
- 缩放画布。
- 点击空白处。
- hover `goal-icon description`。
- 鼠标停留在 `GoalHintZone`。

这些行为只会影响“注意力分数”，不会代表玩家已经推进了故事。

### 1. 30 秒提示：安心提示

触发条件：

```text
当前距离上次 Goal 进展 >= comfortHintTime
且本轮尚未显示过 30s 提示
```

显示内容：

- 从 `WhisperHintManager.comfortHintKeys` 中选择一个非空 LeanLocalization key。
- 当前代码用 `hintVariantSeed` 做轮换式选择，不是完全随机。

显示位置：

- `WhisperHintManager.globalHintAnchor`。

测试方式：

1. 进入关卡后不推进任何 Goal。
2. 等待 `comfortHintTime`。
3. 应显示 `comfortHintKeys` 中的一条。

### 2. 60 秒提示：观察方式提示

触发条件：

```text
当前距离上次 Goal 进展 >= observationHintTime
且本轮尚未显示过 60s 提示
```

显示内容：

- 从 `WhisperHintManager.observationHintKeys` 中选择一个非空 LeanLocalization key。

显示位置：

- `WhisperHintManager.globalHintAnchor`。

测试方式：

1. 进入关卡后不推进任何 Goal。
2. 等待到 `observationHintTime`。
3. 应先在 30s 出现安心提示，再在 60s 出现观察方式提示。

### 3. 90 秒提示：阶段提示 / fallback 提示

触发条件：

```text
当前距离上次 Goal 进展 >= stageHintTime
且本轮尚未显示过 90s 提示
```

90 秒时会先尝试找“当前最可能关注的 Goal”。

如果找到高置信度 Goal：

- 读取该 Goal 上的 `GoalWhisperHintConfig`。
- 根据当前阶段选择 key：
  - `PreAnim1` → `preAnim1HintKeys`
  - `PostAnim1` → `postAnim1HintKeys`
  - `PostAnim2` → `postAnim2HintKeys`
- 显示位置使用该阶段 Anchor：
  - `preAnim1HintAnchor`
  - `postAnim1HintAnchor`
  - `postAnim2HintAnchor`
  - 若为空，fallback 到 `defaultHintAnchor`。
  - 若仍为空，fallback 到 `WhisperHintManager.globalHintAnchor`。
  - 若世界坐标 anchor 当前不在屏幕内，也会显示到 `globalHintAnchor`，但内容仍然是 Goal-specific hint。

如果没有找到高置信度 Goal：

- 从 `WhisperHintManager.stageFallbackHintKeys` 中选择一个非空 key。
- 显示位置为 `globalHintAnchor`。

测试方式 A：测试 fallback 90s

1. 进入关卡。
2. 不 hover goal-icon，不停留 `GoalHintZone`，不推进 Goal。
3. 等待到 `stageHintTime`。
4. 应显示 `stageFallbackHintKeys`。

测试方式 B：测试 Goal-specific 90s（Step1 后）

1. 触发某个两步 Goal 的 `Step1`。
2. 不继续触发 `Step2`。
3. 等待 `stageHintTime`。
4. 应显示该 Goal 的 `postAnim1HintKeys`。

测试方式 C：测试 Goal-specific 90s（读 goal-icon 后）

1. hover / 阅读某个 goal-icon description。
2. 不推进任何 Goal。
3. 等待到 `stageHintTime`。
4. 若注意力分数足够，应显示该 Goal 当前阶段的 hint key。

测试方式 D：测试 Goal-specific 90s（停留故事区域后）

1. 鼠标稳定停留在某个 `GoalHintZone` 上。
2. 不推进任何 Goal。
3. 等待到 `stageHintTime`。
4. 若注意力分数足够，应显示该 Goal 当前阶段的 hint key。

### 4. 注意力分数来源

`GoalAttentionTracker` 负责判断 90s 时是否有高置信度 Goal。

当前来源：

| 来源 | 默认加分/规则 | 说明 |
|---|---:|---|
| hover / 阅读 `goal-icon description` | `goalIconHoverScore = 3.5` | 通过 `GoalInlineDescriptionPanel` 上报 |
| 完成某 Goal 的 `Step1` | `step1ProgressScore = 5` | 让 90s 更容易提示该 Goal 的 `PostAnim1` |
| 鼠标稳定停留在 `GoalHintZone` | `zoneScorePerSecond * dt * zoomWeight * attentionMultiplier` | 缩小时权重降低，近景停留更容易累计 |

分数衰减：

```text
scoreDecayPerSecond = 0.02
```

90s 判定阈值：

```text
WhisperHintManager.stageHintMinAttentionScore = 1.4
```

如果 Goal-specific hint 没出现，优先检查：

- 场景里是否挂了 `GoalAttentionTracker`。
- `GoalAttentionTracker` 的 Inspector 参数是否还是旧值。
- 对应 Goal 是否挂了 `GoalWhisperHintConfig`。
- 对应阶段 key 是否非空。
- 对应 Goal 是否已经完成；已完成 Goal 默认不会作为候选。
- `GoalHintZone` 是否启用、是否指向正确 Goal、Collider 是否启用。

### 5. 文本显示与 LeanLocalization

`WhisperHint/Text` 可以挂 `LeanLocalizedText`，但需要确保：

- `WhisperHintManager.localizedText` 指向该组件。
- 运行时 `WhisperHintManager` 会动态设置 `TranslationName`。
- 如果 30s / 60s / 90s 都显示同一句，通常是 `LeanLocalizedText` 仍在用 Inspector 固定 key 覆盖文本。

### 6. 一轮计时内的出现顺序

如果玩家一直没有推进 Goal：

```text
30s → comfortHintKeys
60s → observationHintKeys
90s → Goal-specific hint 或 stageFallbackHintKeys
```

注意：如果上一条 hint 仍在显示，下一层 hint 不会立刻覆盖它；玩家点击提示框或继续操作使当前 hint 淡出后，下一层到时的 hint 才会继续出现。

如果中途完成任意 `Step1/Step2`：

```text
计时重置
重新从 30s → 60s → 90s 开始
当前正在显示的 hint 会按关闭规则淡出
```

### 7. Hint 消失规则

当前 hint 不再自动读秒消失。

关闭方式：

- 点击 hint 框本身：关闭。
- 点击场景：如果已经达到最短显示时间，延迟淡出。
- 拖动画布：如果已经达到最短显示时间，延迟淡出。
- 滚轮缩放：如果已经达到最短显示时间，延迟淡出。
- 完成 Step1 / Step2：重置计时，并关闭当前 hint。

不会关闭：

- 仅鼠标移动。
- 仅等待时间流逝。

测试方式：

1. 等 30s 出现 hint。
2. 不操作，确认 hint 不会自动消失。
3. 点击 hint 框，确认关闭。
4. 再次等 hint 出现后拖动画布/滚轮缩放，确认至少显示 `minVisibleDuration` 后淡出。

---

## 当前 Unity 接线建议

场景中建议结构：

```text
WhisperHintSystem
├─ WhisperHintManager
└─ GoalAttentionTracker

Canvas
├─ WhisperHint
└─ WhisperGlobalAnchor
```

`WhisperHintManager` 需要绑定：

- `Panel Root` → `WhisperHint`
- `Hint Rect Transform` → `WhisperHint` 根节点 RectTransform
- `Hint Text` → `WhisperHint/Text`
- `Localized Text` → `WhisperHint/Text` 上的 `LeanLocalizedText`
- `Canvas` → 当前 UI Canvas
- `Global Hint Anchor` → 左下角固定 UI anchor

每个需要阶段提示的 `Goal`：

- 挂 `GoalWhisperHintConfig`
- 填 LeanLocalization key
- 填对应阶段 hint anchor

每个需要追踪注意力的故事区域：

- 放宽松 collider
- 挂 `GoalHintZone`
- 指向对应 `Goal`
- Layer 设为 `GoalHintZone`

---

## 当前已发现/已处理问题

### 1. 30/60/90 都显示同一句

现象：`WhisperHint` 上的 `LeanLocalizedText` 固定为 `level2_whisper_30_02`，运行时激活后覆盖了代码设置的 `Text.text`。

处理：`WhisperHintManager` 改为动态设置 `LeanLocalizedText.TranslationName`，不再只写 `Text.text`。

### 2. Goal whisper 不触发

可能原因：

- 场景没有挂 `GoalAttentionTracker`。
- 90 秒时注意力分不足。
- 对应 `Goal` 未挂 `GoalWhisperHintConfig`。
- 对应阶段没有填 key。
- `GoalHintZone` 未配置或 Layer/Collider 未启用。

已处理：降低注意力衰减、提高 goal icon hover 与 Step1 分数。

---

## 最终测试方法（目前测试未完成）

**当前状态：** 2026-05-11 已完成脚本逻辑/性能复查；完整关卡回归测试尚未完成。  
**测试目标：** 验证提示触发、显示位置、关闭方式、注意力判断、LeanLocalization 切换、点击不干扰。

### A. 基础触发与本地化

- [ ] 进入关卡后不推进 Goal，等待 30s：显示 `comfortHintKeys` 中的正确 LeanLocalization 文本。
- [ ] 关闭 30s hint 后继续等待到 60s：显示 `observationHintKeys`。
- [ ] 关闭 60s hint 后继续等待到 90s：若无关注对象，显示 `stageFallbackHintKeys`。
- [ ] 运行时检查 `WhisperHint/Text` 上的 `LeanLocalizedText.TranslationName` 会随每次 hint key 改变。
- [ ] 如果某个 key 缺失，确认只显示 key 本身，便于定位本地化漏配。

### B. Goal-specific 90s 触发

- [ ] 触发某个两步 Goal 的 `Step1` 后不继续 `Step2`，等待 90s：显示该 Goal 的 `postAnim1HintKeys`。
- [ ] hover / 阅读某个 `goal-icon description` 后不推进，等待 90s：若注意力分足够，显示该 Goal 当前阶段 hint。
- [ ] 鼠标稳定停留在某个 `GoalHintZone` 后不推进，等待 90s：若注意力分足够，显示该 Goal 当前阶段 hint。
- [ ] 未配置该 Goal 当前阶段 key 时，不显示默认 key 字符串，应 fallback 到 `stageFallbackHintKeys`。
- [ ] 已完成的 Goal 不再作为 Goal-specific hint 候选。

### C. 显示位置

- [ ] 通用 30s / 60s / fallback 90s 显示在 `globalHintAnchor`。
- [ ] Goal-specific hint 的 anchor 在屏幕内时，显示在对应 `preAnim1HintAnchor` / `postAnim1HintAnchor` / `defaultHintAnchor`。
- [ ] Goal-specific hint 的世界坐标 anchor 不在屏幕内时，内容仍为 Goal-specific hint，但显示在 `globalHintAnchor`。
- [ ] 多分辨率下，世界坐标 anchor 转 UI 位置不会跑出 Canvas 安全范围。

### D. 关闭方式

- [ ] hint 出现后不操作，确认不会按旧 `showDuration` 自动消失。
- [ ] 点击 hint 框本身，确认关闭。
- [ ] 点击场景，确认至少显示 `minVisibleDuration` 后淡出。
- [ ] 拖动画布，确认至少显示 `minVisibleDuration` 后淡出。
- [ ] 滚轮缩放，确认至少显示 `minVisibleDuration` 后淡出。
- [ ] 鼠标移动但不点击/拖动/缩放，确认不会关闭。
- [ ] 完成 Step1 / Step2 时，当前 hint 会关闭，并重置 30s / 60s / 90s 计时。

### E. 点击与 Layer 安全

- [ ] `GoalHintZone` 使用独立 Layer，例如 `GoalHintZone`。
- [ ] `GoalManager.stepClickLayerMask` 不包含 `GoalHintZone` Layer。
- [ ] `GoalManager.dialogueClickLayerMask` 不包含 `GoalHintZone` Layer。
- [ ] 大范围 `GoalHintZone` 不会遮挡真实 Goal 点击。
- [ ] 点击 hint 框不会误触发场景 Goal。

### F. 缩放与注意力判断

- [ ] 缩小到全景时，`GoalHintZone` 不会过度误判为某个具体 Goal。
- [ ] 放大后稳定停留在某个 `GoalHintZone`，能更容易累计该 Goal 注意力。
- [ ] `GoalAttentionTracker` Inspector 参数确认是当前推荐值：`goalIconHoverScore = 3.5`、`step1ProgressScore = 5`、`scoreDecayPerSecond = 0.02`。

---

## 逻辑与性能复查备注（2026-05-11）

### 逻辑结论

- `WhisperHintManager` 每帧只做轻量计时与显示状态判断；hint 显示中不会继续覆盖新 hint，避免阅读被打断。
- 30s / 60s / 90s 触发基于“距离上次 Goal 进展的时间”，不是基于鼠标静止时间。
- `GoalNoteEvents.GoalCompleted` 会重置计时，并关闭当前 hint。
- `LeanLocalizedText` 由 `WhisperHintManager` 动态设置 `TranslationName`，避免固定 key 覆盖。
- Goal-specific key 未配置时，不再显示默认 key 字符串，会 fallback 到通用 90s hint。
- Goal-specific 世界 anchor 是否在屏幕内只在 hint 出现时判断一次；屏幕外则显示到 `globalHintAnchor`。

### 性能结论

- `GoalAttentionTracker` 默认每 `0.1s` 采样一次，不是每帧遍历所有区域。
- `GoalHintZone` 只遍历当前 active zones，Level2/Level3 数量级下成本很低。
- anchor 可见性判断只在 hint 出现瞬间执行一次 `WorldToScreenPoint`，不会持续追踪。
- LeanLocalization key 只在 hint 出现时切换，不每帧刷新。
- 关闭检测仅在 hint 可见时处理点击/滚轮；场景点击和拖动通过 `InputRouter` 事件触发。
- 当前实现未发现明显 GC/频繁分配风险；避免了 LINQ、每帧 `FindObjectOfType`、每帧 UI 文本刷新。

### 暂不做的复杂逻辑

- 不做“玩家回到 goal anchor 后再弹一次”的 pending hint。
- 不做强定位、高亮、连线或镜头移动。
- 不每帧追踪所有 Goal anchor 是否进入屏幕。

---

## Hinttext 写作 Skill 使用规则

已整理为项目级 skill：

- `.codebuddy/skills/small-life-hinttext-writer/SKILL.md`
- 参考示例：`.codebuddy/skills/small-life-hinttext-writer/references/level2-old-street-example.md`

使用时机：

- 新增或改写 `WhisperHintManager` 的 30s / 60s / 90s 通用提示。
- 新增或改写 `GoalWhisperHintConfig` 的 Goal-specific 阶段提示。
- 需要把中文 hinttext 翻译成更短的英文 / 日文。
- 需要结合场景图、地点描述、项目目标、两步 Goal 剧情来调整提示语气。

写作规则：

- 先判断职责：30s 降低焦虑；60s 给观察方法；90s 提醒故事可能还没结束。
- Goal-specific hint 需要根据阶段写：`PreAnim1` 轻轻指向 Step1 相关角色/物体；`PostAnim1` 暗示 Step2 的下一步方向。
- 不写 `点击` / `trigger` / 直接答案，不提前说出最终收集物。
- 语气像主角在上海闲逛时的小声碎碎念，可以轻微使用“我再看看 / 我想 / 我看”，但不要每句都强行第一人称。
- 句子要短，一句只放一个观察点。
- 英文和日文不逐字硬翻，优先保留提示功能和口吻，并尽量压短。

---

## Level2 当前文案方向

Level2 场景：上海西边一条老街附近的烧饼店，包含烧饼店、菜摊、墙上题目、炒菜大姐、看番茄的小孩等小故事。

当前 17 条 hinttext 方向：

| # | 用途 | 中文 |
|---|---|---|
| 1 | 30s | 这条老街有点慢，我也慢慢看看。 |
| 2 | 30s | 烧饼店门口真热闹，先别急。 |
| 3 | 30s | 咦，有些变化小得差点错过。 |
| 4 | 60s | 谁在等？谁手里有东西？我再看看。 |
| 5 | 60s | 老街的小故事，好像都先动一下。 |
| 6 | 60s | 店里、摊上、墙上，好像都藏着点事。 |
| 7 | 90s | 刚才有人动过，这事可能还没完。 |
| 8 | 90s | 刚变过的地方，我想再看一眼。 |
| 9 | 90s | 有些小东西，等人注意到才有用。 |
| 10 | Goal1 PreAnim1 | 那个人想买烧饼，阿姨也正忙着。 |
| 11 | Goal1 PostAnim1 | 饼没了怪可惜的，他还会买吗？ |
| 12 | Goal2 PreAnim1 | 他好像快被别的东西吸引过去了。 |
| 13 | Goal2 PostAnim1 | 墙上那道题，我看他有点手痒了。 |
| 14 | Goal3 PreAnim1 | 碗和锅都有了，好像还差点新鲜味。 |
| 15 | Goal3 PostAnim1 | 香味出来了，我看可以盛起来了。 |
| 16 | Goal4 PreAnim1 | 小孩看番茄，比我认真多了。 |
| 17 | Goal4 PostAnim1 | 他还在找，摊上是不是还有怪番茄？ |

Level2 四个两步 Goal：

1. 第二张烧饼：大妈 / 男子 / 烧饼 / 意外 / 第二张饼。
2. 数学题答案：吃饭男人 / 饭煲 / 墙上数学题 / 解题乐趣。
3. 加紫苏的盖饭：大姐 / 紫苏 / 炒菜 / 尝新味道。
4. 鸭子形状的番茄：男孩 / 特别形状番茄 / 鸭子番茄。

---

## 关联文件

- `Assets/Script/Goals/WhisperHintManager.cs`
- `Assets/Script/Goals/GoalAttentionTracker.cs`
- `Assets/Script/Goals/GoalHintZone.cs`
- `Assets/Script/Goals/GoalWhisperHintConfig.cs`
- `Assets/Script/Goals/Goal.Hints.cs`
- `Assets/Script/UI/GoalInlineDescriptionPanel.cs`
- `Assets/Prefabs/UI/WhisperHint.prefab`
- `Assets/Scenes/Level2.unity`
- `ProjectSettings/TagManager.asset`
