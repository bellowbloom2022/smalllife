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

        // ��ʼ��guideImagesInTextobjects����
        guideImages = new Image[guideTextObjects.Length];
        for (int i = 0; i < guideTextObjects.Length; i++)
        {
            guideImages[i] = guideTextObjects[i].GetComponentInChildren<Image>();
        }
        ShowNextText();
    }

    private void Update()
    {
        // ������루�����������
        if (Input.GetMouseButtonDown(0) && !isPrinting && currentTextIndex < guideTexts.Length - 1)
        {
            // ����Ƿ������� UI
            if (coverUI != null && coverUI.activeSelf)
            {
                Debug.Log("���ּ������¼�����ֹ");
                return; // ���ּ���ʱ��ֱ�ӷ��أ����������¼�
            }

            if (nextButton == null || !nextButton.activeSelf)
            {
                // ��� NextButton �����ڻ�δ�������� ShowNextText()
                ShowNextText();
            }
            else
            {
                // ��������λ���Ƿ��� nextButton ������
                RectTransform rectTransform = nextButton.GetComponent<RectTransform>();
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);
                if (rectTransform.rect.Contains(localPoint))
                {
                    // ���������� nextButton �����ڣ������ OnNextButtonClick()
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

            //ֱ������guideImagesInTextObject�����е�Image���
            if (currentTextIndex < guideImages.Length && guideImages[currentTextIndex] != null)
            {
                guideImages[currentTextIndex].sprite = guideImages[currentTextIndex].sprite;
            }

            StartCoroutine(ShowTextWithTypewriterEffect(currentTextIndex));
        }
    }

    public void OnNextButtonClick()
    {
        //���next��ť�����ذ�ť��������ʾ��һ�仰
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
            //���Ŵ�����Ч
            if (typingSound != null && index < textToShow.Length - 1)//��� index < currentText.Length - 1 �������һ���ַ�������Ч
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

