using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Canvas mCanvas; // UI Canvas

    private GameObject activeDialogueSprite;
    private RectTransform activeSpriteRect;
    private ContentSizeFitter sizeFitter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        InputRouter.OnBlankClick += TryHide;
    }

    void OnDisable()
    {
        InputRouter.OnBlankClick -= TryHide;
    }

    public void ShowDialogue(GameObject dialogueSprite, Transform anchor)
    {
        HideDialogue();

        activeDialogueSprite = dialogueSprite;
        activeDialogueSprite.SetActive(true);

        // 把世界坐标 anchor 转换为 UI 坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(anchor.position);
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mCanvas.transform as RectTransform,
            screenPos,
            null,
            out anchoredPos
        );

        activeSpriteRect = dialogueSprite.GetComponent<RectTransform>();
        if (activeSpriteRect != null)
        {
            activeSpriteRect.anchoredPosition = anchoredPos;
        }
    }

    public void HideDialogue()
    {
        if (activeDialogueSprite != null)
        {
            activeDialogueSprite.SetActive(false);
            activeDialogueSprite = null;
        }
    }
    
    public void TryHide()
    {
        if (IsDialogueActive())
        {
            HideDialogue();
            InputRouter.Instance.UnlockInput();
        }
    }

    public bool IsDialogueActive() => activeDialogueSprite != null;
}
