using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipButtonController : MonoBehaviour
{
    public Button skipButton;
    private bool isFirstClick = true;

    void Start()
    {
        SetButtonTransparency(0.5f);

        skipButton.onClick.AddListener(OnSkipButtonClick);
    }

    void OnSkipButtonClick()
    {
        Debug.Log("Button clicked!");
        if (isFirstClick)
        {
            SetButtonTransparency(1.0f);
            isFirstClick = false;
        }
        else
        {
            GoToMenuPage();
        }
    }

    void SetButtonTransparency(float alpha)
    {
        Color color = skipButton.image.color;
        color.a = alpha;
        skipButton.image.color = color;
    }

    void GoToMenuPage()
    {
        Debug.Log("Navigating to Menu Page...");
        SceneManager.LoadScene("MenuPage");
    }
}
