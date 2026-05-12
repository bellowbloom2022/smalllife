# NextButton 动态效果与防重复点击修复

**状态：** ✅ 已完成  
**完成日期：** 2026-05-12

---

## 一、NextButton 动态效果（红圈 + 星星）

### 任务描述

为关卡完成后弹出的 NextButton 添加动态效果：

1. **红圈（HintCircle）**：关卡完成时从小放大到包裹按钮的大小，然后持续旋转（Loop）
2. **三颗星星（Stars）**：从按钮周围分别向左上、右上、右下三个方向飞出并消失（Loop）
3. **点击消失**：点击按钮后，红圈和星星以按钮为中心缩小消失，然后场景切换

### 实现方案

#### 1. 新增脚本

**`Assets/Script/UI/NextButtonEffectController.cs`**

控制红圈和星星的动态效果，包含：
- `PlayAppearEffect()` — 播放出现动画（红圈放大旋转 + 星星循环飞出）
- `PlayDisappearEffect(float duration, Action onComplete)` — 播放消失动画（缩小到按钮中心消失）

动画使用 **DOTween** 实现。

#### 2. 修改 Level.cs

- 新增字段：`nextButtonEffect`（引用 NextButtonEffectController）
- `RefreshTopRightNextLevelButtonState()` — 按钮从隐藏变为显示时，自动触发 `PlayAppearEffect()`
- `OnTopRightNextLevelButtonClicked()` — 点击按钮时先播放消失动画，动画完成后再调用 `sceneChanger.ChangeScene()`

#### 3. 修改 SceneChanger.cs

- `fadeOutDuration` 从 `private` 改为 `public`，供外部读取淡出时长

### Prefab 结构要求

NextButton 的子层级结构需如下配置：

```
NextButton (Button)                     ← 已有，添加 NextButtonEffectController 组件
├── Image (按钮本身的图，已有)
├── HintCircle (Image)                ← 新建，放 hint-mark.png，颜色设为 #F99999
│   └── 初始 Scale 可比按钮大一些，如 (1.5, 1.5, 1)
└── StarContainer (Empty)            ← 新建空物体，居中
    ├── Star1 (Image)                 ← 新建，放 new-star-white.png
    ├── Star2 (Image)                 ← 新建
    └── Star3 (Image)                 ← 新建
```

### Inspector 绑定

在 Level 脚本的 Inspector 中：

| 字段 | 绑定对象 |
|------|---------|
| **Next Button Effect** | 拖入 NextButton GameObject |

在 NextButtonEffectController 脚本的 Inspector 中：

| 字段 | 绑定对象 |
|------|---------|
| **Hint Circle** | HintCircle Image |
| **Stars** (Size = 3) | Star1、Star2、Star3 Image |

### 可调参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| Circle Scale Duration | 0.4f | 红圈从小放大的时间 |
| Circle Rotate Duration | 3f | 红圈旋转一圈的时间 |
| Star Move Duration | 1f | 星星单次飞出的时间 |
| Star Move Distance | 60f | 星星飞出的距离（像素） |
| Star Spawn Interval | 0.3f | 星星出现的间隔 |
| Star Color | #F99999 | 星星颜色 |

### 行为描述

#### 出现动画（关卡完成时）
1. 按钮 `SetActive(true)` 变为可见
2. 红圈从 scale=0 放大到原始大小（Ease.OutBack，0.4s）
3. 放大完成后红圈开始持续旋转（360° / 3s，Loop）
4. 星星依次出现并飞出（左上、右上、右下），循环播放

#### 消失动画（点击按钮时）
1. 停止所有循环动画
2. 红圈缩小到 scale=0（Ease.InBack，0.5s）
3. 星星向按钮中心移动并同时缩小消失
4. 动画完成后回调 `sceneChanger.ChangeScene()`

---

## 二、NextButton 重复点击问题修复

### 问题描述

点击 NextButton 进入下一个场景时，偶尔会出现"闪一下"的现象，表现为：
- 场景切换后，动画重新播放（如 IntroScene3 的图片移动动画）
- 快速连续点击按钮时，会多次进入下一个场景

原因：点击按钮触发 `ChangeScene()` 后，由于场景切换需要时间（尤其使用 Loading 页面时），此时再次点击会再次调用 `ChangeScene()`，导致多次切换。

### 解决方案

在 `SceneChanger.cs` 中添加 `isChanging` 标志位，防止重复调用场景切换：

```csharp
// 防止重复点击切换场景
private bool isChanging = false;

public void ChangeScene()
{
    // 防止重复点击
    if (isChanging) return;
    isChanging = true;
    
    // ... 后续切换逻辑
}
```

### 修改文件

- `Assets/Script/Core/SceneChanger.cs` — 添加 isChanging 标志位

---

## 相关文件

- `Assets/Script/UI/NextButtonEffectController.cs` — 动态效果控制器
- `Assets/Script/Manager/Level.cs` — 触发逻辑
- `Assets/Script/Core/SceneChanger.cs` — 场景切换（防重复点击 + 暴露 fadeOutDuration）
- `Assets/Textures/Level0/hint-mark.png` — 红圈图片
- `Assets/Textures/UI/button/new-star-white.png` — 星星图片