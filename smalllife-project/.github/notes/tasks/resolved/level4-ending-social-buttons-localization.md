# [已完成] Level4 结束面板社媒按钮按语言切换

**状态：** ✅ 已完成  
**完成日期：** 2026-04-09

---

## 需求

在 Level4 结束面板中新增并统一管理 `Steam / QQ / Discord` 三个按钮，规则如下：

1. Steam 按钮始终显示。
2. 中文显示 QQ，隐藏 Discord。
3. 英文与日文隐藏 QQ，显示 Discord。
4. 三个按钮都需要补齐 `OnClick` 事件绑定。
5. 历史备份脚本 `FeedbackLink1` 不再使用，需清理。

---

## 修改文件

- `Assets/Script/FeedbackLink.cs`
- `Assets/Scenes/Level4.unity`
- `Assets/Scenes/EndingPage.unity`
- `Assets/Script/FeedbackLink1.cs`（删除）

---

## 实现摘要

### 1) `FeedbackLink` 脚本升级为统一入口

在 `FeedbackLink` 中新增：

1. 多链接字段：`steamUrl`、`qqUrl`、`discordUrl`。
2. 多按钮显示控制：`steamButton`、`qqButton`、`discordButton`。
3. 语言切换监听：订阅 `LeanLocalization.OnLocalizationChanged`。
4. 按语言刷新可见性：
   - Steam：始终 `SetActive(true)`
   - QQ：中文显示
   - Discord：非中文显示
5. 场景级兜底：通过对象名 `button_Steam / button_QQ / button_Discord` 在场景中查找并刷新（兼容按钮初始隐藏）。

### 2) Level4 三个按钮补齐 OnClick

在 `Level4.unity` 中将三个按钮 `Button.onClick` 绑定到同对象上的 `FeedbackLink`：

1. `button_Steam -> OpenSteam`
2. `button_QQ -> OpenQQ`
3. `button_Discord -> OpenDiscord`

### 3) 迁移并清理 `FeedbackLink1`

1. `EndingPage.unity` 中原 `FeedbackLink1` 引用迁移为 `FeedbackLink`。
2. 删除 `Assets/Script/FeedbackLink1.cs`。
3. 确认仓库内不再有 `FeedbackLink1` / 对应 GUID 引用残留。

---

## 验证结果

- [x] `FeedbackLink.cs` 无编译错误
- [x] Level4 三个按钮都存在 OnClick 方法绑定
- [x] `EndingPage.unity` 已改为 `FeedbackLink`
- [x] 无 `FeedbackLink1` 引用残留

---

## 备注

1. 语言判定采用 `Chinese` 或以 `zh` 开头。
2. 如后续需改链接地址，优先改按钮同对象上的 `steamUrl / qqUrl / discordUrl`。
3. 若 UI 名称调整，需同步更新场景兜底查找名（`button_Steam / button_QQ / button_Discord`）。
