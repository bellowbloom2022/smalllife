using System;
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
    public GameObject coverUI;
    public AudioSource typingSound;

    private int currentTextIndex = 0;
    private string currentText;

    private bool isPrinting = false;


    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }

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
        // 检查输入（鼠标点击或触摸）
        if (Input.GetMouseButtonDown(0) && !isPrinting && currentTextIndex < guideTexts.Length - 1)
        {
            // 检查是否有遮罩 UI
            if (coverUI != null && coverUI.activeSelf)
            {
                Debug.Log("遮罩激活，点击事件被阻止");
                return; // 遮罩激活时，直接返回，不处理点击事件
            }

            if (nextButton == null || !nextButton.activeSelf)
            {
                // 如果 NextButton 不存在或未激活，则调用 ShowNextText()
                ShowNextText();
            }
            else
            {
                // 检查鼠标点击位置是否在 nextButton 区域内
                RectTransform rectTransform = nextButton.GetComponent<RectTransform>();
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);
                if (rectTransform.rect.Contains(localPoint))
                {
                    // 如果鼠标点击在 nextButton 区域内，则调用 OnNextButtonClick()
                    OnNextButtonClick();
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

        string textToShow = GetLocalizedText(guideTexts[textIndex]);

        int index = 0;
        while (index < textToShow.Length)
        {
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

        //Hide the "Next" button if there are no more texts to display
        if (textIndex == guideTexts.Length - 1 && nextButton != null)
        {
            nextButton.SetActive(false);
        }
        //Show the "Next" button after each text is displayed
        else if (nextButton != null)
        {
            nextButton.SetActive(true);
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

