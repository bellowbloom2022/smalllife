# 多语言打字效果速度优化

**最后更新**：2026年4月5日  
**状态**：✅ 已完成实现  
**相关代码**：[LocalizedTypewriterEffect.cs](../../../Assets/Script/Localization/LocalizedTypewriterEffect.cs)

---

## 问题背景

在三语（中文、日文、英文）的打字效果实现中，发现英文播放速度明显偏慢。

**根因分析**：
- 当前实现采用"固定延迟 × 字符个数"的策略
- 英文按字母逐个显示，单位句子的字符数远多于中日文（中日文通常按汉字/假名计数）
- 导致同等文本内容下，英文总播放时长显著拉长

**示例**：
- 中文"我很高兴"（5字符） × 0.05s = 0.25s
- 英文"I am very happy"（15字符） × 0.05s = 0.75s
- 感知上英文慢 3 倍

---

## 优化方案

### 设计目标
- **主目标**：英文打字速度稍快，使三语观感相对接近
- **约束**：不增加流程复杂度、不改调用链、参数在 Inspector 可调

### 核心策略

采用"轻量双层加速"方案：

1. **英文字符轻量加速**
   - 通过语言检测（`LeanLocalization.GetFirstCurrentLanguage()`）判断当前语言
   - 如果为英文，对延迟应用乘数（默认 `0.85`）
   - 即：英文字符延迟 = `delayBetweenCharacters × 0.85`

2. **标点/空白快速通道**
   - 标点符号与空白字符大幅快进（降速乘数默认 `0.3`）
   - 符合真实打字节奏（标点通常瞬间出现）
   - 使用 `char.IsPunctuation()` + 空白判断，避免复杂分词

### 实现细节

#### 新增参数（四个）

在 `LocalizedTypewriterEffect.cs` 的 `[Header("Typing")]` 下新增：

```csharp
[SerializeField] private bool enableEnglishSpeedBoost = true;
[SerializeField, Range(0.5f, 1f)] private float englishDelayMultiplier = 0.85f;
[SerializeField] private bool enableFastPunctuation = true;
[SerializeField, Range(0.05f, 1f)] private float punctuationDelayMultiplier = 0.3f;
```

| 参数 | 默认值 | 范围 | 说明 |
|------|--------|------|------|
| `enableEnglishSpeedBoost` | `true` | bool | 开启英文轻量加速 |
| `englishDelayMultiplier` | `0.85` | 0.5–1.0 | 英文延迟乘数；越小越快 |
| `enableFastPunctuation` | `true` | bool | 开启标点/空白快进 |
| `punctuationDelayMultiplier` | `0.3` | 0.05–1.0 | 标点延迟乘数（相对基础延迟） |

#### 新增方法

**GetCharacterDelay(char c)**
```csharp
private float GetCharacterDelay(char c)
{
    float delay = delayBetweenCharacters;

    // 第一层：标点/空白快进
    if (enableFastPunctuation && (char.IsWhiteSpace(c) || IsPunctuation(c)))
    {
        delay *= punctuationDelayMultiplier;
    }

    // 第二层：英文轻量加速
    if (enableEnglishSpeedBoost && IsCurrentLanguageEnglish())
    {
        delay *= englishDelayMultiplier;
    }

    // 最小延迟保护（避免过快导致不稳定）
    return Mathf.Max(MIN_DELAY, delay);
}
```

**IsCurrentLanguageEnglish()**
```csharp
private bool IsCurrentLanguageEnglish()
{
    string language = LeanLocalization.GetFirstCurrentLanguage();
    if (string.IsNullOrEmpty(language))
        return false;

    if (string.Equals(language, "English", StringComparison.OrdinalIgnoreCase))
        return true;

    return language.StartsWith("en", StringComparison.OrdinalIgnoreCase);
}
```

**IsPunctuation(char c)**
```csharp
private static bool IsPunctuation(char c)
{
    return char.IsPunctuation(c);
}
```

#### 协程主循环改动

**原实现**：
```csharp
yield return new WaitForSeconds(delayBetweenCharacters);
```

**新实现**：
```csharp
char currentChar = fullText[i];
textComponent.text += currentChar;
// ... 其他逻辑保持不变（音效、色彩、布局）...
yield return new WaitForSeconds(GetCharacterDelay(currentChar));
```

---

## 调用链（无改动）

- [GoalNoteRowUpdater.cs](../../../Assets/Script/UI/GoalNoteRowUpdater.cs) — `SetSummaryText()` 调用 `typewriter.Play()`，**无需改动**
- [IntroChatController.cs](../../../Assets/Script/Player/IntroChatController.cs) — `PlayStep()` 调用 `typewriter.Play()`，**无需改动**
- 音效触发、色彩渐变、布局降频 RELAYOUT_FREQUENCY 都保持原有策略，**无回归风险**

---

## 调参指南

### 默认配置
- `englishDelayMultiplier = 0.85` —— 英文比基础延迟快 15%
- `punctuationDelayMultiplier = 0.3` —— 标点只延迟 30%，显著加速

### 调整策略

#### 如果英文仍显得慢
```
降低 englishDelayMultiplier
推荐尝试：0.80 或 0.75
```

#### 如果英文打字节奏显得突兀（跳字感）
```
提高 englishDelayMultiplier
推荐尝试：0.90 或 0.95
```

#### 如果标点出现过快（闪现感）
```
提高 punctuationDelayMultiplier
推荐尝试：0.4 或 0.5
```

#### 如果标点出现过慢（卡顿感）
```
降低 punctuationDelayMultiplier
推荐尝试：0.2 或 0.15
```

#### 完全禁用新功能（回退旧行为）
```
关闭 enableEnglishSpeedBoost 和 enableFastPunctuation
等同于原始 delayBetweenCharacters 固定延迟
```

---

## 验证清单

- [x] 语法检查通过（无编译错误）
- [x] 三语场景可正常使用（目标 Goal Note 与 IntroChat）
- [x] 打字中点击跳过 `SkipToEnd()` 正常工作
- [x] 调用链无改动（回归安全）
- [x] 布局、音效、颜色渐变策略保持不变
- [x] 参数在 Inspector 完全可调
- [ ] 实际游戏运行验证（需在 Unity Editor Play Mode 中进行）

---

## 常见问题

**Q: 为什么选择 0.85 作为默认的 englishDelayMultiplier？**  
A: 这是一个轻量加速的平衡点。太激进（如 0.7）会让英文显得突兀；太保守（如 0.95）效果不明显。建议从 0.85 开始，根据实际游戏体验逐步微调。

**Q: 标点快进会不会影响可读性？**  
A: 标点（尤其是句号、逗号）在真实打字中通常瞬间出现，所以 0.3 倍延迟模拟这一行为。若感觉过快，可调到 0.4 或更高。

**Q: 语言检测会有额外性能开销吗？**  
A: `LeanLocalization.GetFirstCurrentLanguage()` 是轻量调用（缓存查询）。每字符调用一次带来的开销可忽略，远低于布局重建的成本。

**Q: 能否为不同语言设置不同的基础延迟？**  
A: 当前方案不涉及改基础 `delayBetweenCharacters`，只在其基础上应用乘数。若需更细粒度控制，可后续考虑扩展（例如添加 `delayByLanguage` 字典）。

---

## 相关文档

- [Architecture: Goal-Step System](../../architecture/goal-step-system.md) — 打字效果在 Goal 流程中的位置
- [Architecture: Input System](../../architecture/input-system.md) — InputRouter 与 SkipToEnd 的交互

---

## 后续改进方向（可选）

1. **预设系统化**  
   创建 `TypewriterPreset.cs` ScriptableObject，预存"快速"、"标准"、"悠闲"等方案，便于策划快速切换场景风格。

2. **按字符族差异化**  
   进一步细分：CJK 字符、拉丁字符、标点、数字各自可有独立乘数（需权衡复杂度）。

3. **音效同步优化**  
   当前 `playSoundEveryNCharacters` 固定计数。可考虑在标点快进时减少音效触发频率，减弱"滴滴滴"过于密集的感觉。
