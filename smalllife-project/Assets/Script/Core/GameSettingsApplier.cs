using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public static class GameSettingsApplier
{
    private static LeanLocalization cachedLocalization;

    public static void ApplyAll(GameSettings gameSettings, BGMController bgmController, DisplaySettingsController displaySettings = null, bool applyLanguage = false)
    {
        // 应用到音频系统
        AudioListener.volume = gameSettings.masterVolume;
        bgmController?.SetVolume(gameSettings.musicVolume);
        AudioHub.Instance?.SetSFXVolume(gameSettings.sfxVolume);

        // 拖拽控制
        InputRouter.Instance?.SetDragMode(gameSettings.dragMode);

        // 应用 overlay 色调
        if (displaySettings == null)
        {
            displaySettings = GameObject.FindObjectOfType<DisplaySettingsController>();
        }

        if (displaySettings != null)
        {
            displaySettings.SyncOverlayFromSettings();
        }
        // 应用语言设置（按需）
        if (applyLanguage)
        {
            ApplyLanguage(gameSettings.language, false);
        }
    }
    
    public static void ApplyLanguage(string langCode, bool saveToDisk = false)
    {
        if (cachedLocalization == null)
        {
            cachedLocalization = GameObject.FindObjectOfType<LeanLocalization>();
        }

        if (cachedLocalization != null)
        {
            cachedLocalization.SetCurrentLanguage(langCode);
        }

        if (saveToDisk)
        {
            SaveSystem.SaveGame();
        }
    }
}
