using UnityEngine;
using Lean.Localization;

public class LanguageSettingsController : MonoBehaviour
{
    /// <summary>
    /// 点击语言按钮时调用。会自动设置语言并保存。
    /// </summary>
    public void SetLanguage(string langCode)
    {
        SaveSystem.GameData.settings.language = langCode;
        GameSettingsApplier.ApplyLanguage(langCode);
    }

    /// <summary>
    /// 在启动或从存档恢复时调用，自动设置语言
    /// </summary>
    public void ApplySavedLanguage()
    {
        string savedLang = SaveSystem.GameData.settings.language;
        if (!string.IsNullOrEmpty(savedLang))
        {
            GameSettingsApplier.ApplyLanguage(savedLang);
        }
    }
}
