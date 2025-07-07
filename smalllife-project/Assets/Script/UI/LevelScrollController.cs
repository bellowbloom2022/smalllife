using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LevelScrollController : MonoBehaviour
{
    public ScrollRect scrollRect;// ��� ScrollView
    public RectTransform content;// Content ����
    public RectTransform selectorIcon;// ?ͼ���λ�ã�����ռ����꣩
    public GameObject levelItemPrefab;//���ڶ�̬���ɵ�LevelItemUI prefab

    public RightPanelUI rightPanel;// �ұ���Ϣ���ű�
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
            itemUI.Init(data, i, this);// �ѿ�����Ҳ���������������ص�
            levelItems.Add(itemUI);
        }
        if (levelItems.Count > 0){
            OnLevelClicked(levelItems[0]);
        }
    }

    // ��ҵ���ؿ���ťʱ����
    public void OnLevelClicked(LevelItemUI clickedItem)
    {
        Debug.Log($"����˹ؿ���ť��{clickedItem.levelIndex}, ID: {clickedItem.data.levelID}");
        
        if (clickedItem == currentSelected) return;

        currentSelected = clickedItem;
        // ��ȡ clickedItem �����ĵ���������
        Vector3 itemWorldPos = clickedItem.GetComponent<RectTransform>().position;
        // ����ת��Ϊ Viewport �µ� localPosition����Ϊ selectorIcon �� Viewport �������壩
        Vector3 localPoint = selectorIcon.transform.parent.InverseTransformPoint(itemWorldPos);
        // ���� selectorIcon �� X ���꣬������ Y
        Vector3 currentPos = selectorIcon.localPosition;
        selectorIcon.localPosition = new Vector3(currentPos.x, localPoint.y, currentPos.z);

        // �����ұߵ��������?���� UpdateContent ʱ������ levelIndex
        rightPanel.UpdateContent(clickedItem.data, clickedItem.levelIndex);
    }
}
