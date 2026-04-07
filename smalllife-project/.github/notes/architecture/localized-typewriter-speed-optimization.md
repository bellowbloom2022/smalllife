# 多语言打字效果优化—设计与参数

**相关实现**：[localized-typewriter-speed-optimization.md](../tasks/resolved/localized-typewriter-speed-optimization.md)  
**核心脚本**：[LocalizedTypewriterEffect.cs](../../Assets/Script/Localization/LocalizedTypewriterEffect.cs)

---

## 问题

英文打字效果因为按字母逐个显示，导致同样的文本内容下，英文播放时长比中日文长 2–3 倍，用户感知上"英文慢"。

---

## 解决思路

采用"轻量双层加速"：

### 层 1：标点/空白快进
- 识别标点和空白字符（`char.IsWhiteSpace()` + `char.IsPunctuation()`）
- 快速通过（乘数默认 0.3）
- 符合真实打字节奏

### 层 2：英文轻量加速
- 检测当前语言（`LeanLocalization.GetFirstCurrentLanguage()`）
- 如果为英文，再应用语言乘数（默认 0.85）
- 保持其他语言不变

### 两层叠加公式
```
最终延迟 = delayBetweenCharacters
         × (是否标点 ? punctuationDelayMultiplier : 1.0)
         × (是否英文 ? englishDelayMultiplier : 1.0)
再做最小延迟保护（确保 >= MIN_DELAY = 0.005s）
```

---

## 参数表

| 参数 | 默认值 | 范围 | 何时调 |
|------|--------|------|--------|
| `enableEnglishSpeedBoost` | `true` | bool | 想禁用英文加速时关闭 |
| `englishDelayMultiplier` | `0.85` | 0.5–1.0 | 英文仍慢→降低；太快→提高 |
| `enableFastPunctuation` | `true` | bool | 想禁用标点快进时关闭 |
| `punctuationDelayMultiplier` | `0.3` | 0.05–1.0 | 标点太快→提高；太慢→降低 |

---

## 调用链

```
LocalizedTypewriterEffect.Play()
  → ShowTextWithTypewriterEffect() [协程]
    → for 每个字符 c
      → GetCharacterDelay(c)  [新增]
      → yield new WaitForSeconds(GetCharacterDelay(c))
```

上游调用者（无改动）：
- `GoalNoteRowUpdater.SetSummaryText()`
- `IntroChatController.PlayStep()`

---

## 实现要点

1. **最小延迟保护**：`Mathf.Max(MIN_DELAY, calculatedDelay)` 防止过快引发不稳定
2. **语言检测缓存**：`LeanLocalization.GetFirstCurrentLanguage()` 是轻量操作，可逐字调用
3. **标点识别**：用 `char.IsPunctuation()` 足够覆盖需求，无需显式枚举 Unicode 标点集合
4. **其他行为保持**：音效、色彩渐变、布局降频策略都不动

---

## 后续扩展空间

- 预设系统（快/标准/慢方案）
- 按字符族差异化（CJK vs 拉丁）
- 音效同步优化（标点快进时减少音效触发）

---

## 2026-04-07 增补：LoadingPage Congrats 点击跳过

为统一文字播放交互，`LocalizedTypewriterEffect` 新增了“点击空白时打字立即全出”的能力，并默认对 `congrats` 类 key 自动生效。

### 目标范围

- `Level0LoadingPage.unity`：`level0_congrats`
- `Level1LoadingPage.unity`：`level1_congrats`
- `Level2LoadingPage.unity`：`level2_congrats`
- `Level3LoadingPage.unity`：`level3_congrats`

### 触发机制

- 监听 `InputRouter.OnBlankClickAnyButton`
- 当文本处于 `IsTyping` 时，点击空白触发 `SkipToEnd()`
- 左键/右键均可触发（与 GoalDialog 的“点击可全出”体验统一）

### 参数与策略

新增参数：

- `enableBlankClickSkip`（默认 `false`）：手动强制启用点击跳过
- `autoEnableBlankClickSkipForCongrats`（默认 `true`）：当 key 包含 `congrats` 时自动启用

自动判定规则：`phraseName` 包含 `congrats`（不区分大小写）即启用。

### 兼容性

- 不修改现有 `Play()` 调用链
- 非 `congrats` 文本默认不受影响（除非显式打开 `enableBlankClickSkip`）
- 保持原有多语言速度优化参数与布局优化策略不变
