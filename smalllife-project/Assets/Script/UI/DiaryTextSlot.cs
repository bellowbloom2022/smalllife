using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;
using DG.Tweening;

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
        Refresh();
    }

    /// <summary>
    /// 在贴纸贴入后调用，用于显示文本
    /// </summary>
    public void ShowText()
    {
        isPlaced = true;
        Refresh();
        AnimateShow(); // 可选：显示动画
    }
    protected override void Refresh()
    {
        if (textField == null) return;
        if (isPlaced && !string.IsNullOrEmpty(diaryKey))
        {
            textField.text = LeanLocalization.GetTranslationText(diaryKey);
            textField.gameObject.SetActive(true);            
        }
        else
        {
            textField.text = "";
            textField.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 翻页或切换 Page 时调用，清除显示但不丢 diaryKey
    /// </summary>
    public void Clear()
    {
        isPlaced = false;// 回到未贴入状态
        unlocked = false;// 也可视为未显示状态
        textField.text = "";
        textField.gameObject.SetActive(false);
        goalKey = null;
        unlocked = false;// 页面 slot 不再对应 goal
        // 🔒 不清 diaryKey，保留文案 Key 以便恢复
    }

    public void AnimateShow()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.DOFade(1f, 0.3f);
    }
}
