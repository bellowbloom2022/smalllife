using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责 Goal Note 文本的动画效果（如放大回弹）
/// 职责: 文本强调动画、Tween 生命周期管理
/// </summary>
public class GoalNoteTextAnimator : MonoBehaviour
{
    private const float TextEmphasisScale = 1.25f;
    private const float TextEmphasisGrowDuration = 0.18f;
    private const float TextEmphasisShrinkDuration = 0.22f;
    private const float ColorHalfDuration = 0.2f;

    private readonly Dictionary<Text, Tween> emphasisTweenByText = new Dictionary<Text, Tween>();

    private Color normalTextColor = Color.black;
    private Color triggerAccentColor = new Color32(0xCC, 0x66, 0x66, 0xFF);
    private bool resetToNormalColorOnFinish = true;
    private bool descriptionFinalBold = true;
    private bool descriptionFinalItalic = false;
    private bool summaryFinalBold = true;
    private bool summaryFinalItalic = true;

    public enum TextRole
    {
        Description,
        Summary
    }

    public void ConfigureEmphasisVisual(
        Color normalColor,
        Color accentColor,
        bool resetColorOnFinish,
        bool descriptionBold,
        bool descriptionItalic,
        bool summaryBold,
        bool summaryItalic)
    {
        normalTextColor = normalColor;
        triggerAccentColor = accentColor;
        resetToNormalColorOnFinish = resetColorOnFinish;
        descriptionFinalBold = descriptionBold;
        descriptionFinalItalic = descriptionItalic;
        summaryFinalBold = summaryBold;
        summaryFinalItalic = summaryItalic;
    }

    /// <summary>
    /// 播放文本强调动画（放大后回弹）
    /// </summary>
    public void PlayTextEmphasis(Text text, TextRole role = TextRole.Summary)
    {
        if (text == null)
            return;

        // 如果该文本已有动画在播放，先终止掉
        if (emphasisTweenByText.TryGetValue(text, out var running) && running != null && running.IsActive())
            running.Kill();

        RectTransform rect = text.rectTransform;
        rect.localScale = Vector3.one;
        text.color = normalTextColor;
        text.fontStyle = FontStyle.Normal;

        // 创建序列：先放大到 1.25x，再回弹回 1x
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rect.DOScale(Vector3.one * TextEmphasisScale, TextEmphasisGrowDuration).SetEase(Ease.OutQuad));
        sequence.Append(rect.DOScale(Vector3.one, TextEmphasisShrinkDuration).SetEase(Ease.OutBack));
        sequence.Join(text.DOColor(triggerAccentColor, ColorHalfDuration).SetEase(Ease.OutQuad));
        sequence.Join(text.DOColor(normalTextColor, ColorHalfDuration).SetEase(Ease.InQuad).SetDelay(ColorHalfDuration));
        sequence.OnComplete(() =>
        {
            if (text == null)
                return;

            if (resetToNormalColorOnFinish)
                text.color = normalTextColor;

            text.fontStyle = ResolveFinalStyle(role);
        });

        emphasisTweenByText[text] = sequence;
    }

    private FontStyle ResolveFinalStyle(TextRole role)
    {
        bool bold = role == TextRole.Description ? descriptionFinalBold : summaryFinalBold;
        bool italic = role == TextRole.Description ? descriptionFinalItalic : summaryFinalItalic;

        if (bold && italic)
            return FontStyle.BoldAndItalic;
        if (bold)
            return FontStyle.Bold;
        if (italic)
            return FontStyle.Italic;

        return FontStyle.Normal;
    }

    /// <summary>
    /// 清理所有正在播放的动画
    /// 通常在 Panel 关闭或销毁时调用
    /// </summary>
    public void KillAllEmphasis()
    {
        foreach (var pair in emphasisTweenByText)
        {
            if (pair.Value != null && pair.Value.IsActive())
                pair.Value.Kill();
        }

        emphasisTweenByText.Clear();
    }

    private void OnDestroy()
    {
        KillAllEmphasis();
    }
}
