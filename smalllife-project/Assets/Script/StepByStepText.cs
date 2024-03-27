using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public class StepByStepText : MonoBehaviour
{
    public float delayBetweenCharacters = 0.1f;

    public GameObject textContainer;
    public string[] guideTexts;
    public GameObject[] guideTextObjects;
    public Image[] guideImages;
    public GameObject nextButton;

    private int currentTextIndex = 0;
    private string currentText;

    private bool isPrinting = false;


    private void Start()
    {
        nextButton.SetActive(false);

        //Hide all LeanLocalizedText game objects
        foreach (var guideTextObject in guideTextObjects)
        {
            guideTextObject.SetActive(false);
        }

        // 初始化guideImagesInTextobjects数组
        guideImages = new Image[guideTextObjects.Length];
        for (int i = 0; i < guideTextObjects.Length; i++)
        {
            guideImages[i] = guideTextObjects[i].GetComponentInChildren<Image>();
        }
        ShowNextText();
    }

    private void Update()
    {
        //Check for input (mouse click or touch)
        if (Input.GetMouseButtonDown(0) && !isPrinting)
        {
            ShowNextText();
        }
    }

    private void ShowNextText()
    {
        if(currentTextIndex < guideTexts.Length)
        {
            //Hide the previous text
            if (currentTextIndex > 0)
            {
                guideTextObjects[currentTextIndex - 1].SetActive(false);
            }

            currentText = GetLocalizedText(guideTexts[currentTextIndex]);
            guideTextObjects[currentTextIndex].SetActive(true);

            //直接引用guideImagesInTextObject数组中的Image组件
            if (currentTextIndex < guideImages.Length && guideImages[currentTextIndex] != null)
            {
                guideImages[currentTextIndex].sprite = guideImages[currentTextIndex].sprite;
            }

            StartCoroutine(ShowTextWithTypewriterEffect());
        }
        else
        {
            //All texts are printed, show the "Next" button
            nextButton.SetActive(true);
        }
    }

    private IEnumerator ShowTextWithTypewriterEffect()
    {
        isPrinting = true;
        //textContainer.SetActive(true);

        int index = 0;
        while (index < currentText.Length)
        {
            foreach (var textComponent in guideTextObjects[currentTextIndex].GetComponentsInChildren<Text>())
            {
                textComponent.text += currentText[index];
            }
            index++;
            yield return new WaitForSeconds(delayBetweenCharacters);
        }

        isPrinting = false;

        //Move to the next text
        currentTextIndex++;
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
            return phraseName;
        }
    }
}
