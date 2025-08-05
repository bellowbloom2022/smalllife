using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public class LevelItemUI : MonoBehaviour
{
    public LeanLocalizedText titleText;
    public RectTransform selectorAnchor; // 在 Unity 编辑器中绑定
    public GameObject checkMark;
    public GameObject lockedMask; // 遮罩层 UI
    private bool isUnlocked = false;

    [HideInInspector] public LevelDataAsset data;
    [HideInInspector] public int levelIndex; //用于查找存档数据比如第几关
    [HideInInspector] public LevelScrollController controller;

    public void Init(LevelDataAsset d, int index, LevelScrollController ctrl)
    {

        data = d;
        levelIndex = index;
        controller = ctrl;

        if (titleText != null)
        {
            titleText.TranslationName = data.titleKey;
        }
        else
        {
            Debug.LogWarning($"titleText is null on LevelItemUI: {gameObject.name}");
        }

        GetComponent<Button>().onClick.RemoveAllListeners(); // 清空旧事件
        GetComponent<Button>().onClick.AddListener(() => controller.OnLevelClicked(this)); // 正确传参

        // 默认隐藏 ✅，稍后在外部根据 GameData 控制是否显示
        if (checkMark != null)
            checkMark.SetActive(false);
    }

    public void ShowCheckMark(bool show)
    {
        if (checkMark != null)
        {
            checkMark.SetActive(show);
        }
    }
    public void UpdateLockStatus(bool unlocked)
    {
        isUnlocked = unlocked;
        if (lockedMask != null)
            lockedMask.SetActive(!unlocked);
    }
}
