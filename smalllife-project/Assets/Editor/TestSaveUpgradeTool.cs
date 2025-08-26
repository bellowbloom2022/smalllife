#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TestSaveUpgradeTool : EditorWindow
{
    [MenuItem("Tools/测试/模拟存档版本升级")]
    public static void ShowWindow()
    {
        GetWindow<TestSaveUpgradeTool>("升级存档测试工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("模拟旧版本存档升级", EditorStyles.boldLabel);

        if (GUILayout.Button("运行升级测试"))
        {
            RunTest();
        }
    }

    private void RunTest()
    {
        Debug.Log("🔧 开始测试旧版本存档升级...");

        // 模拟一个旧版 GameData（缺失某些字段）
        GameData oldData = new GameData
        {
            version = "0.0.7",
            settings = null, // 故意缺失设置项
            lastLevelIndex = 2
            // 其他字段留空，以测试是否被补全
        };

        var upgraded = SaveDataUpdater.UpdateSaveData(oldData);
        upgraded.DeserializeGoalData();

        // 简单断言
        if (upgraded.settings == null)
        {
            Debug.LogError("❌ 升级失败：settings 为 null");
        }
        else
        {
            Debug.Log("✅ 升级成功：settings 初始化正常");
        }

        Debug.Log($"升级后版本：{upgraded.version}");
    }
}
#endif
