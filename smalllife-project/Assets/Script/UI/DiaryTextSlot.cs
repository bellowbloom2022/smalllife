using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

/// <summary>
/// 日记文字槽：一图一文模式（每个 goal 对应一段日记）
/// Setup(goalKey, diaryLocalizationKey)
/// </summary>
public class DiaryTextSlot : BaseDiarySlot
{
    [Header("UI")]
    public Text textField;
    private string diaryKey;
    private bool isPlaced = false;  // UI 内部状态

    public void Setup(string key, string diaryKey)
    {
        this.diaryKey = diaryKey;
        base.Setup(key);
    }
    public void ShowText()
    {
        Refresh();
    }
    protected override void Refresh()
    {
        if (textField == null) return;
        if (isPlaced && !string.IsNullOrEmpty(diaryKey))
            textField.text = LeanLocalization.GetTranslationText(diaryKey);
        else
            textField.text = "";
    }

    public void Clear()
    {
        textField.text = "";
        diaryKey = null;
        goalKey = null;
        unlocked = false;
    }
}
