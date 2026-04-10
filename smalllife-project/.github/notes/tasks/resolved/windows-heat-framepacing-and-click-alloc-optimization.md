# [已完成] Windows 发热优化：帧率策略 + 点击路径 GC 消减

**状态：** ✅ 已完成  
**完成日期：** 2026-04-10

---

## 背景

发行商测试反馈 Windows 机器发热明显。排查确认主因有两个：

1. **VSync 策略不当**：GameManager 开启 `vSyncCount = 1` 后，高刷新率显示器（120Hz/144Hz/165Hz）实际渲染帧率仍较高，与防撕裂目标冲突，导致 GPU 持续高负载。
2. **点击路径每帧分配**：GoalManager 每次点击调用 `Physics.RaycastAll` 和 `Physics2D.OverlapPointAll`，每次在堆分配新数组，频繁点击时 GC 有轻微抖动（非主因，但级联叠加）。

---

## 修改内容

### 1. GameManager — 自适应帧率策略

**文件：** `Assets/Script/Core/GameManager.cs`

**改动：**
- 替换原来的硬编码 `targetFrameRate = 60; vSyncCount = 1`
- 改为根据显示器刷新率自动选择 VSync 档位：
  - 刷新率 ≤ 90Hz → `vSyncCount = 1`（原行为，单同步）
  - 刷新率 > 90Hz → `vSyncCount = 2`（双同步，约等于锁 60/80，降低高刷发热）
- 新增前后台切换回调：
  - 失焦 / 后台：`vSyncCount = 0`，`targetFrameRate = 15`，彻底节流
  - 回前台：复原为刷新率自适应策略

**Inspector 可调参数（均有 SerializeField）：**

| 参数 | 默认值 | 说明 |
|---|---|---|
| foregroundTargetFrameRate | 60 | 前台目标帧率 |
| backgroundTargetFrameRate | 15 | 后台限制帧率 |
| highRefreshThreshold | 90 | 判断为高刷的阈值 |
| normalRefreshVSyncCount | 1 | 普通屏 VSync 档 |
| highRefreshVSyncCount | 2 | 高刷屏 VSync 档 |

**关键代码：**
```csharp
private void ApplyForegroundFramePacing()
{
    int refreshRate = Mathf.Max(1, Screen.currentResolution.refreshRate);
    bool isHighRefresh = refreshRate > highRefreshThreshold;
    QualitySettings.vSyncCount = isHighRefresh ? highRefreshVSyncCount : normalRefreshVSyncCount;
    Application.targetFrameRate = Mathf.Max(30, foregroundTargetFrameRate);
}

private void ApplyBackgroundFramePacing()
{
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = Mathf.Max(5, backgroundTargetFrameRate);
}
```

---

### 2. GoalManager — 点击路径零分配改造

**文件：** `Assets/Script/Player/GoalManager.cs`

**改动：**
- 新增两个预分配缓冲区字段：`RaycastHit[]` 和 `Collider2D[]`
- `Physics.RaycastAll` → `Physics.RaycastNonAlloc`（复用缓冲区）
- `Physics2D.OverlapPointAll` → `Physics2D.OverlapPointNonAlloc`（复用缓冲区）
- Camera 改为缓存引用（`cachedMainCamera`），避免每次点击访问 `Camera.main`
- 新增 Inspector 可调参数 `stepClickLayerMask` / `dialogueClickLayerMask`，可限定射线打到指定层，缩小目标数

**关键代码：**
```csharp
// 替换前
RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPos);

// 替换后
int hitCount = Physics.RaycastNonAlloc(ray, raycastHitsBuffer, 1000f, stepClickLayerMask);
int colliderCount = Physics2D.OverlapPointNonAlloc(worldPoint, overlap2DBuffer, dialogueClickLayerMask);
```

**注意事项：**
- 缓冲区默认大小 `max3DHits = 16`，`max2DHits = 16`，如场景内同区域 Collider 密集时需适当上调
- LayerMask 默认值 `~0`（全层），行为与旧逻辑完全兼容，不需要在 Inspector 里单独配置

---

## 测试要点

- [ ] Windows 高刷屏（120+ Hz）拖动画布时无撕裂，温度明显低于 vSync=1 时
- [ ] 60Hz 机打开是否正常无撕裂（走 vSyncCount=1 路径）
- [ ] 切后台 10-20 秒再回来，画面和输入恢复正常
- [ ] 快速点击 Goal 步骤交互无异常（NonAlloc 缓冲区不越界）

---

## 关联记录

- 昨日已完成的 VSync 撕裂修复背景：`/memories/repo/windows-vsync-tearing.md`
- GoalManager 历史版本（RaycastAll 单命中改多命中）：`tasks/resolved/goalmanager-raycastall.md`

---

## 相关文件

- `Assets/Script/Core/GameManager.cs`
- `Assets/Script/Player/GoalManager.cs`
