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

    public void ShowDialogue(GameObject dialogueSprite, Transform anchor)
    {
        HideDialogue();

        activeDialogueSprite = dialogueSprite;
        activeDialogueSprite.SetActive(true);

        // ���������� anchor ת��Ϊ UI ����
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

    public bool IsDialogueActive()
    {
        return activeDialogueSprite != null;
    }
}
