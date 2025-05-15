using System.Collections;
using UnityEngine;
using Lean.Localization;
using UnityEngine.UI;

public class LocalizedTypewriterEffect : MonoBehaviour
{
    public float delayBetweenCharacters = 0.1f;
    public string phraseName = "YourPhraseName";//The phrase name you set in Lean.Localization for this text

    private Text textComponent;
    private string fullText;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
    }

    private void OnEnable()
    {
        Lean.Localization.LeanLocalization.OnLocalizationChanged += UpdateLocalization;
    }

    private void OnDisable()
    {
        Lean.Localization.LeanLocalization.OnLocalizationChanged -= UpdateLocalization;
    }

    private void Start()
    {
        textComponent.text = "";//Clear the text initially
        fullText = GetLocalizedText(phraseName);
        StartCoroutine(ShowTextWithTypewriterEffect());
    }

    private void UpdateLocalization()
    {
        fullText = GetLocalizedText(phraseName);
    }
    private IEnumerator ShowTextWithTypewriterEffect()
    {
        int index = 0;
        while(index < fullText.Length)//Make sure we don't go out of bounds
        {
            textComponent.text += fullText[index];
            index++;
            yield return new WaitForSeconds(delayBetweenCharacters);
        }
    }

    private string GetLocalizedText(string phraseName)
    {
        var translation = LeanLocalization.GetTranslation(phraseName);

        if (translation != null && translation.Data is string)
        {
            return (string)translation.Data;
        }
        else
        {
            return textComponent.text;
        }
    }
}
