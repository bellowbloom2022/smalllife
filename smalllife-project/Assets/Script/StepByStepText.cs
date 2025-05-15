using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;
using UnityEngine.SceneManagement;

public class StepByStepText : MonoBehaviour
{
    public float delayBetweenCharacters = 0.1f;

    public GameObject textContainer;
    public string[] guideTexts;
    public GameObject[] guideTextObjects;
    public Image[] guideImages;
    public GameObject nextButton;
    public GameObject nextSceneButton;
    public GameObject coverUI;
    public AudioSource typingSound;

    private int currentTextIndex = 0;
    private string currentText;

    private bool isPrinting = false;
    private bool textFullyDisplayed = false; // Track if the text is fully displayed
    private bool skipToFullText = false;


    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }

        if (nextSceneButton != null)
        {
            nextSceneButton.SetActive(false);
        }

        //Hide all LeanLocalizedText game objects
        foreach (var guideTextObject in guideTextObjects)
        {
            guideTextObject.SetActive(false);
        }

        ShowNextText();
    }

    private void Update()
    {
        // Check input (mouse click or touch)
        if (Input.GetMouseButtonDown(0))
        {
            // If text is being printed, skip to full text
            if (isPrinting)
            {
                skipToFullText = true;
            }
            // If text is fully displayed and not printing, show the next text
            else if (textFullyDisplayed)
            {
                if (nextButton == null || !nextButton.activeSelf)
                {
                    OnNextButtonClick();
                }
                else
                {
                    // Check if mouse click position is within nextButton area
                    RectTransform rectTransform = nextButton.GetComponent<RectTransform>();
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);
                    if (rectTransform.rect.Contains(localPoint))
                    {
                        OnNextButtonClick();
                    }
                }
            }
        }
    }

    private void ShowNextText()
    {
        if (currentTextIndex < guideTexts.Length)
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

            textFullyDisplayed = false; // Reset display status
            StartCoroutine(ShowTextWithTypewriterEffect(currentTextIndex));
        }
    }

    public void OnNextButtonClick()
    {
        //点击next按钮后隐藏按钮，继续显示下一句话
        nextButton.SetActive(false);
        currentTextIndex++;//Move to the next text
        ShowNextText();
    }

    private IEnumerator ShowTextWithTypewriterEffect(int textIndex)
    {
        isPrinting = true;
        skipToFullText = false; // Reset skip flag

        string textToShow = GetLocalizedText(guideTexts[textIndex]);
        // Clear the text components before starting to display text
        foreach (var textComponent in guideTextObjects[textIndex].GetComponentsInChildren<Text>())
        {
            textComponent.text = string.Empty; // Clear the text
        }
        int index = 0;
        while (index < textToShow.Length)
        {
            // Check if skip to full text is requested
            if (skipToFullText)
            {
                foreach (var textComponent in guideTextObjects[textIndex].GetComponentsInChildren<Text>())
                {
                    textComponent.text = textToShow; // Immediately show the full text
                }
                break;
            }
            //播放打字音效
            if (typingSound != null && index < textToShow.Length - 1)//检查 index < currentText.Length - 1 避免最后一个字符播放音效
            {
                typingSound.Play();
            }

            foreach (var textComponent in guideTextObjects[textIndex].GetComponentsInChildren<Text>())
            {
                textComponent.text += textToShow[index];
            }
            yield return new WaitForSeconds(delayBetweenCharacters);
            index++;
        }
        isPrinting = false;
        textFullyDisplayed = true; // Set text display status

        // Show the next button if there are more texts to display
        if (textIndex < guideTexts.Length - 1 && nextButton != null)
        {
            nextButton.SetActive(true);
        }
        // Show the "Next Scene" button if this is the last text
        else if (textIndex == guideTexts.Length - 1 && nextSceneButton != null)
        {
            nextSceneButton.SetActive(true);
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
            return phraseName;
        }
    }
    public bool AllTextRead()
    {
        return currentTextIndex == guideTexts.Length - 1;
    }
}

