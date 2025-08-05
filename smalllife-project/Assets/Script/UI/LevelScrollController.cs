using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class LevelScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;// 你的 ScrollView
    public RectTransform content;// Content 容器
    public RectTransform selectorIcon;// 图标的位置（世界空间坐标）
    public GameObject levelItemPrefab;//用于动态生成的LevelItemUI prefab

    public RightPanelUI rightPanel;// 右边信息面板脚本
    public GameData gameData;
    public LevelItemUI LastSelectedLevel => currentSelected;
    private LevelItemUI currentSelected;
    private List<LevelItemUI> levelItems = new List<LevelItemUI>();

    public void Initialize(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelItems.Count)
        {
            OnLevelClicked(levelItems[levelIndex]); // 触发选择器和右侧更新
        }
        else if (levelItems.Count > 0)
        {
            OnLevelClicked(levelItems[0]); // fallback
        }
    }

    void Start()
    {
        var allLevels = Resources.LoadAll<LevelDataAsset>("LevelDataAssets");
        System.Array.Sort(allLevels, (a, b) => string.Compare(a.levelID, b.levelID));

        for (int i = 0; i < allLevels.Length; i++)
        {
            LevelDataAsset data = allLevels[i];

            GameObject itemGO = Instantiate(levelItemPrefab, content);
            LevelItemUI itemUI = itemGO.GetComponent<LevelItemUI>();
            itemUI.Init(data, i, this);// 把控制器也传给它，方便点击回调
            levelItems.Add(itemUI);
        }

        // ✅ 显示 checkmark
        UpdateCheckMarkStatus();
        //刷新每个关卡的解锁状态
        UpdateLockStatus(); 
        
        // 根据 lastLevelIndex 初始化 SelectorIcon 位置
        int indexToSelect = SaveSystem.GameData.lastLevelIndex;

        // 防止 index 越界（可能是初始值 -1）
        if (indexToSelect < 0 || indexToSelect >= levelItems.Count)
        {
            indexToSelect = 0;
        }
        // 设置 currentSelected 以避免首次点击再次触发
        currentSelected = levelItems[indexToSelect];
        bool unlocked = IsUnlocked(indexToSelect);
        rightPanel.UpdateContent(currentSelected.data, indexToSelect, unlocked);

        //延迟调用 UpdateSelectorPosition() 确保 layout 完成后再移动 selector
        StartCoroutine(DelayUpdateSelectorPosition(indexToSelect));
    }

    // 玩家点击关卡按钮时调用
    public void OnLevelClicked(LevelItemUI clickedItem)
    {
        Debug.Log($"点击了关卡按钮：{clickedItem.levelIndex}, ID: {clickedItem.data.levelID}");

        if (clickedItem == currentSelected) return;

        currentSelected = clickedItem;
        // 获取 clickedItem 的中心点世界坐标
        Vector3 anchorWorldPos = clickedItem.selectorAnchor.position;
        // 将它转换为 Viewport 下的 localPosition（因为 selectorIcon 是 Viewport 的子物体）
        Vector3 localPoint = selectorIcon.transform.parent.InverseTransformPoint(anchorWorldPos);
        // 保留 selectorIcon 的 X 坐标，仅更新 Y
        Vector3 currentPos = selectorIcon.localPosition;
        DOTween.Kill(selectorIcon); // 避免叠加动画
        selectorIcon.DOLocalMoveY(localPoint.y, 0.25f).SetEase(Ease.OutCubic);

        // 更新右边的详情面板?调用 UpdateContent 时，传入 levelIndex传入解锁状态
        bool unlocked = IsUnlocked(clickedItem.levelIndex);
        rightPanel.UpdateContent(clickedItem.data, clickedItem.levelIndex, unlocked);   
    }

    private void UpdateSelectorPosition(int index)
    {
        if (index < 0 || index >= levelItems.Count) return;

        LevelItemUI item = levelItems[index];
        if (item.selectorAnchor == null || selectorIcon == null) return;

        Vector3 anchorWorldPos = item.selectorAnchor.position;
        Vector3 localPoint = selectorIcon.transform.parent.InverseTransformPoint(anchorWorldPos);
        selectorIcon.localPosition = new Vector3(selectorIcon.localPosition.x, localPoint.y, selectorIcon.localPosition.z);
    }

    private IEnumerator DelayUpdateSelectorPosition(int index)
    {
        yield return null; // 等待一帧，确保 layout 完成

        UpdateSelectorPosition(index);

        bool unlocked = IsUnlocked(index);
        rightPanel.UpdateContent(levelItems[index].data, index, unlocked);
        currentSelected = levelItems[index];
    }

    private void UpdateCheckMarkStatus()
    {
        // 从 GameData 中获取新完成的 levelIDs
        var completedIDs = SaveSystem.GameData.newlyCompletedLevelIDs;

        foreach (var item in levelItems)
        {
            if (completedIDs.Contains(item.data.levelID))
            {
                item.ShowCheckMark(true);
            }
            else
            {
                item.ShowCheckMark(false);
            }
        }
    }

    private void UpdateLockStatus()
    {
        for (int i = 0; i < levelItems.Count; i++)
        {
            var item = levelItems[i];
            bool unlocked = IsUnlocked(i);
            item.UpdateLockStatus(unlocked);
        }

        // 确保刷新当前选中项的右侧内容
        if (currentSelected != null && rightPanel != null)
        {
            int idx = levelItems.IndexOf(currentSelected);
            bool unlocked = IsUnlocked(idx);
            rightPanel.UpdateContent(currentSelected.data, idx, unlocked);
        }
    }

    private bool IsUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true;

        string prevLevelID = levelItems[levelIndex - 1].data.levelID;

        return SaveSystem.GameData.completedLevels.ContainsKey(prevLevelID)
            && SaveSystem.GameData.completedLevels[prevLevelID];
    }
}
