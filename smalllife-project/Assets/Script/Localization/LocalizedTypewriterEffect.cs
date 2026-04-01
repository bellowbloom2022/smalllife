using System;
using System.Collections;
using UnityEngine;
using Lean.Localization;
using UnityEngine.UI;

[RequireComponent(typeof(Text), typeof(AudioSource))]
public class LocalizedTypewriterEffect : MonoBehaviour
{
    [Header("Typing")]
    public float delayBetweenCharacters = 0.05f;
    [Tooltip("Lean.Localization 的词条名；Play时会被覆盖为传入的key")]
    public string phraseName = "YourPhraseName";
    public AudioClip typingSound;
    public int playSoundEveryNCharacters = 1;

    [Header("Lifecycle")]
    [SerializeField] private bool autoPlayOnEnable = false;

    [Header("Trigger Visual")]
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color triggerAccentColor = new Color32(0xCC, 0x66, 0x66, 0xFF);
    [SerializeField] private bool resetToNormalColorOnFinish = true;
    [SerializeField] private bool boldOnTriggerFinished = true;
    [SerializeField] private bool italicOnTriggerFinished = false;

    private Text textComponent;
    private AudioSource audioSource;
    private string fullText;

    private Coroutine typingCoroutine;
    private LayoutGroup layoutGroup;
    private ContentSizeFitter contentSizeFitter;

    private Action onFinishedCallback;
    private FontStyle baseFontStyle;
    private bool useTriggerVisual;
    private string textPrefix = string.Empty;

    // 优化：降频布局重建，每5字重建一次，大幅降低CPU开销同时保持视觉效果
    private const int RELAYOUT_FREQUENCY = 5;

    public bool IsTyping => typingCoroutine != null;

    public void ConfigureTriggerVisual(Color normalColor, Color accentColor, bool resetColorOnFinish, bool finalBold, bool finalItalic)
    {
        normalTextColor = normalColor;
        triggerAccentColor = accentColor;
        resetToNormalColorOnFinish = resetColorOnFinish;
        boldOnTriggerFinished = finalBold;
        italicOnTriggerFinished = finalItalic;
    }

    public void ConfigureTextPrefix(string prefix)
    {
        textPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix;
    }

    private void Awake()
    {
        textComponent = GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        layoutGroup = GetComponentInParent<LayoutGroup>();
        contentSizeFitter = GetComponentInParent<ContentSizeFitter>();
        baseFontStyle = textComponent.fontStyle;
    }

    private void OnEnable()
    {
        LeanLocalization.OnLocalizationChanged += UpdateLocalizationAndMaybeRestart;
        if (autoPlayOnEnable && !string.IsNullOrEmpty(phraseName))
        {
            Play(phraseName);
        }
    }

    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= UpdateLocalizationAndMaybeRestart;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        onFinishedCallback = null;
    }

    // 对外：播放指定 key 的打字机，并在完成时回调
    public void Play(string key, Action onComplete = null, bool instant = false, bool enableTriggerVisual = false)
    {
        phraseName = key;
        onFinishedCallback = onComplete;
        useTriggerVisual = enableTriggerVisual;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        fullText = GetLocalizedText(phraseName);

        PrepareVisualBeforePlay();

        if (instant)
        {
            textComponent.text = fullText;
            ForceRelayout();
            FinishTyping();
            return;
        }

        textComponent.text = string.Empty;
        typingCoroutine = StartCoroutine(ShowTextWithTypewriterEffect());
    }

    // 对外：跳过到全文
    public void SkipToEnd()
    {
        if (fullText == null) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textComponent.text = fullText;
        ForceRelayout();
        FinishTyping();
    }

    private void UpdateLocalizationAndMaybeRestart()
    {
        // 语言切换时刷新当前词条；如果正在打字，就重新用当前 key 播放
        if (string.IsNullOrEmpty(phraseName)) return;
        bool wasTyping = IsTyping;
        Play(phraseName, onFinishedCallback, instant: !wasTyping);
    }

    private IEnumerator ShowTextWithTypewriterEffect()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            textComponent.text += fullText[i];

            if (useTriggerVisual)
                UpdateTypingColorByProgress(i, fullText.Length);

            if (typingSound != null && audioSource != null && playSoundEveryNCharacters > 0 && (i % playSoundEveryNCharacters == 0))
            {
                audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound);
            }

            // 优化：每5字或最后一字时才重建布局，避免每字都强制重建导致CPU飙升
            if ((i + 1) % RELAYOUT_FREQUENCY == 0 || i == fullText.Length - 1)
            {
                ForceRelayout();
            }
            
            yield return new WaitForSeconds(delayBetweenCharacters);
        }

        typingCoroutine = null;
        FinishTyping();
    }

    private void FinishTyping()
    {
        ApplyFinalVisualStyle();

        // 确保最终一次布局刷新
        ForceRelayout();

        var cb = onFinishedCallback;
        onFinishedCallback = null;
        cb?.Invoke();
    }

    private void ForceRelayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(textComponent.rectTransform);
        if (layoutGroup != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    private string GetLocalizedText(string key)
    {
        var translation = LeanLocalization.GetTranslation(key);
        string resolvedText = textComponent.text;

        if (translation != null && translation.Data is string str)
            resolvedText = str;

        if (string.IsNullOrEmpty(textPrefix) || string.IsNullOrEmpty(resolvedText))
            return resolvedText;

        return resolvedText.StartsWith(textPrefix, StringComparison.Ordinal)
            ? resolvedText
            : textPrefix + resolvedText;
    }

    private void PrepareVisualBeforePlay()
    {
        if (!useTriggerVisual)
        {
            textComponent.fontStyle = baseFontStyle;
            return;
        }

        textComponent.color = normalTextColor;
        textComponent.fontStyle = FontStyle.Normal;
    }

    private void UpdateTypingColorByProgress(int index, int totalLength)
    {
        if (totalLength <= 1)
        {
            textComponent.color = triggerAccentColor;
            return;
        }

        float t = index / (float)(totalLength - 1);
        float upThenDown = 1f - Mathf.Abs(2f * t - 1f);
        textComponent.color = Color.Lerp(normalTextColor, triggerAccentColor, upThenDown);
    }

    private void ApplyFinalVisualStyle()
    {
        if (useTriggerVisual)
        {
            if (resetToNormalColorOnFinish)
                textComponent.color = normalTextColor;

            textComponent.fontStyle = ResolveFinalStyle();

            return;
        }

        textComponent.fontStyle = baseFontStyle;
    }

    private FontStyle ResolveFinalStyle()
    {
        if (boldOnTriggerFinished && italicOnTriggerFinished)
            return FontStyle.BoldAndItalic;
        if (boldOnTriggerFinished)
            return FontStyle.Bold;
        if (italicOnTriggerFinished)
            return FontStyle.Italic;

        return baseFontStyle;
    }
}
