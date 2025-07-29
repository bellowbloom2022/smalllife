using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public static class GameSettingsApplier
{
    public static void ApplyAll(GameSettings gameSettings, BGMController bgmController)
    {
        // 应用到音频系统
        AudioListener.volume = gameSettings.masterVolume;
        bgmController?.SetVolume(gameSettings.musicVolume);
        AudioHub.Instance?.SetSFXVolume(gameSettings.sfxVolume);

        // 拖拽控制
        InputRouter.Instance?.SetDragMode(gameSettings.dragMode);

        // 应用 overlay 色调
        var displaySettings = GameObject.FindObjectOfType<DisplaySettingsController>();
        if (displaySettings != null)
        {
            Toggle targetToggle = displaySettings.GetToggleByIndex(gameSettings.overlayColorIndex);
            if (targetToggle != null)
            {
                targetToggle.isOn = true;
                displaySettings.SetOverlayColor(targetToggle);
            }
        }
        // 应用语言设置
        ApplyLanguage(gameSettings.language);
    }
    
    public static void ApplyLanguage(string langCode)
    {
        var localization = GameObject.FindObjectOfType<LeanLocalization>();
        if (localization != null)
        {
            localization.SetCurrentLanguage(langCode);
        }
        SaveSystem.SaveGame(); // 确保切换语言后立刻保存
    }
}
