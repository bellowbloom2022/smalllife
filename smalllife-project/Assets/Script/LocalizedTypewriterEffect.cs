using System.Collections;
using UnityEngine;
using Lean.Localization;
using UnityEngine.UI;

[RequireComponent(typeof(Text), typeof(AudioSource))]
public class LocalizedTypewriterEffect : MonoBehaviour
{
    public float delayBetweenCharacters = 0.05f;
    public string phraseName = "YourPhraseName";//The phrase name you set in Lean.Localization for this text
    public AudioClip typingSound;
    public int playSoundEveryNCharacters = 1;

    private Text textComponent;
    private AudioSource audioSource;
    private string fullText;

    private Coroutine typingCoroutine;
    private LayoutGroup layoutGroup;
    private ContentSizeFitter contentSizeFitter;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();

        //获取上层TextContainer，用于控制自适应大小
        layoutGroup = GetComponentInParent<LayoutGroup>();
        contentSizeFitter = GetComponentInParent<ContentSizeFitter>();
    }

    private void OnEnable()
    {
        LeanLocalization.OnLocalizationChanged += UpdateLocalizationAndRestart;
        RestartTypewriter();
    }

    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= UpdateLocalizationAndRestart;
    }

    private void RestartTypewriter(){
        if(typingCoroutine != null)
           StopCoroutine(typingCoroutine);

        textComponent.text = "";
        fullText = GetLocalizedText(phraseName);
        typingCoroutine = StartCoroutine(ShowTextWithTypewriterEffect());
        Debug.Log($"[RestartTypewriter] phraseName: {phraseName}, fullText: {GetLocalizedText(phraseName)}");
    }

    private void UpdateLocalizationAndRestart(){
        RestartTypewriter();
    }

    private IEnumerator ShowTextWithTypewriterEffect()
    {
        Debug.Log($"[TypewriterEffect] Starting typing coroutine...");
        for (int i = 0; i < fullText.Length; i++){
            textComponent.text += fullText[i];

            if (typingSound != null && audioSource != null && i % playSoundEveryNCharacters == 0){
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound);
            }

            //每次加字后，重新触发Layout（关键步骤）
            LayoutRebuilder.ForceRebuildLayoutImmediate(textComponent.rectTransform);
            if (layoutGroup != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

            yield return new WaitForSeconds(delayBetweenCharacters);
        }
    }

    private string GetLocalizedText(string phraseName)
    {
        var translation = LeanLocalization.GetTranslation(phraseName);

        if (translation != null && translation.Data is string str)
            return str;
        
        return textComponent.text;
    }
}
