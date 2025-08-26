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
    [Tooltip("Lean.Localization �Ĵ�������Playʱ�ᱻ����Ϊ�����key")]
    public string phraseName = "YourPhraseName";
    public AudioClip typingSound;
    public int playSoundEveryNCharacters = 1;

    [Header("Lifecycle")]
    [SerializeField] private bool autoPlayOnEnable = false;

    private Text textComponent;
    private AudioSource audioSource;
    private string fullText;

    private Coroutine typingCoroutine;
    private LayoutGroup layoutGroup;
    private ContentSizeFitter contentSizeFitter;

    private Action onFinishedCallback;

    public bool IsTyping => typingCoroutine != null;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        layoutGroup = GetComponentInParent<LayoutGroup>();
        contentSizeFitter = GetComponentInParent<ContentSizeFitter>();
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

    // ���⣺����ָ�� key �Ĵ��ֻ����������ʱ�ص�
    public void Play(string key, Action onComplete = null, bool instant = false)
    {
        phraseName = key;
        onFinishedCallback = onComplete;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        fullText = GetLocalizedText(phraseName);

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

    // ���⣺������ȫ��
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
        // �����л�ʱˢ�µ�ǰ������������ڴ��֣��������õ�ǰ key ����
        if (string.IsNullOrEmpty(phraseName)) return;
        bool wasTyping = IsTyping;
        Play(phraseName, onFinishedCallback, instant: !wasTyping);
    }

    private IEnumerator ShowTextWithTypewriterEffect()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            textComponent.text += fullText[i];

            if (typingSound != null && audioSource != null && playSoundEveryNCharacters > 0 && (i % playSoundEveryNCharacters == 0))
            {
                audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound);
            }

            ForceRelayout();
            yield return new WaitForSeconds(delayBetweenCharacters);
        }

        typingCoroutine = null;
        FinishTyping();
    }

    private void FinishTyping()
    {
        // ȷ������һ�β���ˢ��
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
        if (translation != null && translation.Data is string str)
            return str;

        return textComponent.text;
    }
}
