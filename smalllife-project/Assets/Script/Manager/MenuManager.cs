using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public LevelScrollController levelScrollController;
    public RightPanelUI rightPanelUI;

    void Start()
    {
        InitializeMenu();
    }

    void InitializeMenu()
    {
        // 1. 加载 GameData
        SaveSystem.LoadGame();
        GameData data = SaveSystem.GameData;

        // 2. 获取上次玩的关卡
        int lastLevelIndex = 0;
        if (data != null && data.lastLevelIndex >= 0)
        {
            lastLevelIndex = data.lastLevelIndex;
        }

        // 初始化 LevelScrollController，自动跳转选择器和展示内容
        levelScrollController.Initialize(lastLevelIndex);
    }
}
