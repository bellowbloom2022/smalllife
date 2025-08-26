using UnityEngine;

public static class SaveDataUpdater
{
    const string CURRENT_VERSION = "0.0.8";

    public static GameData UpdateSaveData(GameData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveDataUpdater: 输入 GameData 为 null。");
            return new GameData(); // fallback 空数据
        }

        // 如果版本字段为空，认为是旧版本
        string oldVersion = data.version ?? "0.0.0";

        // 若版本已是最新，不做处理
        if (oldVersion == CURRENT_VERSION)
            return data;

        Debug.Log($"[SaveDataUpdater] 旧版本为 {oldVersion}，升级为 {CURRENT_VERSION}");

        // 示例升级逻辑：未来添加版本分支时按顺序递增处理
        // 示例：从v0.0.7 升级到 v0.0.8
        if (oldVersion == "0.0.7")
        {
            if (data.settings == null)
                data.settings = new GameSettings();

            // 未来若新增设置项，可在此处设定默认值
            //data.settings.overlayColorIndex = data.settings.overlayColorIndex;
        }

        // 🔁 未来继续写入升级路径，如：
        // if (oldVersion == "0.0.8") { ... }

        // 最终统一更新版本号
        data.version = CURRENT_VERSION;

        return data;
    }
}
