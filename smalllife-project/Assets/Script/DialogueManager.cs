using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialogueTextBox; // UI ���
    public RectTransform dialogueTextBoxRect; // UI ���� RectTransform
    public Canvas mCanvas; // UI Canvas

    private GameObject activeDialogueSprite;

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
        dialogueTextBox.SetActive(true);

        // ���������� anchor ת��Ϊ UI ����
        Vector3 screenPos = Camera.main.WorldToScreenPoint(anchor.position);
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mCanvas.transform as RectTransform,
            screenPos,
            null,
            out anchoredPos
        );

        dialogueTextBoxRect.anchoredPosition = anchoredPos;

        RectTransform spriteRect = dialogueSprite.GetComponent<RectTransform>();
        if (spriteRect != null)
        {
            spriteRect.anchoredPosition = dialogueTextBoxRect.anchoredPosition;
        }
    }

    public void HideDialogue()
    {
        if (activeDialogueSprite != null)
        {
            activeDialogueSprite.SetActive(false);
            activeDialogueSprite = null;
        }

        if (dialogueTextBox != null)
        {
            dialogueTextBox.SetActive(false);
        }
    }

    public bool IsDialogueActive()
    {
        return activeDialogueSprite != null;
    }
}
