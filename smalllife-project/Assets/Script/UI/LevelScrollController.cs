using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LevelScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;// 你的 ScrollView
    public RectTransform content;// Content 容器
    public RectTransform selectorIcon;// ?图标的位置（世界空间坐标）
    public GameObject levelItemPrefab;//用于动态生成的LevelItemUI prefab

    public RightPanelUI rightPanel;// 右边信息面板脚本
    public GameData gameData;

    private LevelItemUI currentSelected;
    private List<LevelItemUI> levelItems = new List<LevelItemUI>();

    void Start()
    {
        var allLevels = Resources.LoadAll<LevelDataAsset>("LevelDataAssets");
        System.Array.Sort(allLevels, (a, b) => string.Compare(a.levelID, b.levelID));

        for (int i= 0; i< allLevels.Length; i++){
            LevelDataAsset data = allLevels[i];

            GameObject itemGO = Instantiate(levelItemPrefab, content);
            LevelItemUI itemUI =  itemGO.GetComponent<LevelItemUI>();
            itemUI.Init(data, i, this);// 把控制器也传给它，方便点击回调
            levelItems.Add(itemUI);
        }
        if (levelItems.Count > 0){
            OnLevelClicked(levelItems[0]);
        }
    }

    // 玩家点击关卡按钮时调用
    public void OnLevelClicked(LevelItemUI clickedItem)
    {
        Debug.Log($"点击了关卡按钮：{clickedItem.levelIndex}, ID: {clickedItem.data.levelID}");
        
        if (clickedItem == currentSelected) return;

        currentSelected = clickedItem;
        // 获取 clickedItem 的中心点世界坐标
        Vector3 itemWorldPos = clickedItem.GetComponent<RectTransform>().position;
        // 将它转换为 Viewport 下的 localPosition（因为 selectorIcon 是 Viewport 的子物体）
        Vector3 localPoint = selectorIcon.transform.parent.InverseTransformPoint(itemWorldPos);
        // 保留 selectorIcon 的 X 坐标，仅更新 Y
        Vector3 currentPos = selectorIcon.localPosition;
        selectorIcon.localPosition = new Vector3(currentPos.x, localPoint.y, currentPos.z);

        // 更新右边的详情面板?调用 UpdateContent 时，传入 levelIndex
        rightPanel.UpdateContent(clickedItem.data, clickedItem.levelIndex);
    }
}
