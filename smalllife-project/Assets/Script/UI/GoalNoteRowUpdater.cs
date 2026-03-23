using System;
using System.Collections.Generic;
using Lean.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 负责单行 Goal Note 的文本刷新、点击事件绑定、逐字显示
/// 职责: 文本更新、点击事件管理、typewriter 生命周期
/// </summary>
public class GoalNoteRowUpdater : MonoBehaviour
{
    private readonly Dictionary<Text, LocalizedTypewriterEffect> typewriterByText = new Dictionary<Text, LocalizedTypewriterEffect>();
    private Color triggerNormalColor = Color.black;
    private Color triggerAccentColor = new Color32(0xCC, 0x66, 0x66, 0xFF);
    private bool resetColorOnFinish = true;
    private bool summaryFinalBold = true;
    private bool summaryFinalItalic = true;

    public void ConfigureSummaryTypewriterVisual(Color normalColor, Color accentColor, bool resetColor, bool finalBold, bool finalItalic)
    {
        triggerNormalColor = normalColor;
        triggerAccentColor = accentColor;
        resetColorOnFinish = resetColor;
        summaryFinalBold = finalBold;
        summaryFinalItalic = finalItalic;
    }

    /// <summary>
    /// 更新单行的所有内容：文本刷新、点击绑定、动画播放
    /// </summary>
    public void UpdateRow(
        Text descriptionText,
        Text summaryText,
        int goalID,
        string descriptionKey,
        string summaryKey,
        bool showSummary,
        bool isStep1Completed,
        bool playSummaryTypewriter,
        Action onDescriptionClicked,
        Action onSummaryClicked)
    {
        // 刷新 description 文本
        SetLocalizedText(descriptionText, descriptionKey, true);
        
        // 仅在 step1 完成后才绑定 description 点击
        if (isStep1Completed)
            BindTextClick(descriptionText, onDescriptionClicked);

        // 刷新 summary 文本
        if (showSummary)
            SetSummaryText(summaryText, summaryKey, true, playSummaryTypewriter);
        else
            SetSummaryText(summaryText, string.Empty, false, false);

        // 绑定 summary 点击（step2 完成后即可点击）
        BindTextClick(summaryText, onSummaryClicked);

    }

    /// <summary>
    /// 设置本地化文本
    /// </summary>
    private void SetLocalizedText(Text text, string key, bool active)
    {
        if (text == null)
            return;

        if (string.IsNullOrEmpty(key))
        {
            text.text = "";
            text.gameObject.SetActive(false);
            return;
        }

        string localized = LeanLocalization.GetTranslationText(key);
        text.text = string.IsNullOrEmpty(localized) ? key : localized;
        text.gameObject.SetActive(active);
    }

    /// <summary>
    /// 设置 summary 文本并支持逐字显示
    /// </summary>
    private void SetSummaryText(Text text, string key, bool active, bool playTypewriter)
    {
        if (text == null)
            return;

        if (!active || string.IsNullOrEmpty(key))
        {
            text.text = string.Empty;
            text.gameObject.SetActive(false);
            return;
        }

        text.gameObject.SetActive(true);

        // 获取或创建 LocalizedTypewriterEffect
        if (!typewriterByText.TryGetValue(text, out var typewriter) || typewriter == null)
        {
            typewriter = text.GetComponent<LocalizedTypewriterEffect>();
            if (typewriter == null)
                typewriter = text.gameObject.AddComponent<LocalizedTypewriterEffect>();

            typewriterByText[text] = typewriter;
        }

        typewriter.ConfigureTriggerVisual(
            triggerNormalColor,
            triggerAccentColor,
            resetColorOnFinish,
            summaryFinalBold,
            summaryFinalItalic);

        // 播放逐字或直接显示
        if (playTypewriter)
            typewriter.Play(key, null, false, true);
        else
            typewriter.Play(key, null, true, true);
    }

    /// <summary>
    /// 为文本绑定点击事件
    /// </summary>
    private void BindTextClick(Text text, Action onClick)
    {
        if (text == null || onClick == null)
            return;

        var relay = text.GetComponent<GoalNoteTextClickRelay>();
        if (relay == null)
            relay = text.gameObject.AddComponent<GoalNoteTextClickRelay>();

        relay.SetOnClick(onClick);
    }

    /// <summary>
    /// 清理所有 typewriter 和点击事件
    /// </summary>
    public void CleanUp()
    {
        typewriterByText.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}

/// <summary>
/// 文本点击中继器：将指针事件转换为 Action 回调
/// </summary>
public class GoalNoteTextClickRelay : MonoBehaviour, IPointerClickHandler
{
    private Action onClick;

    public void SetOnClick(Action callback)
    {
        onClick = callback;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
