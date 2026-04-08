using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Canvas mCanvas; // UI Canvas

    private GameObject activeDialogueSprite;
    private RectTransform activeSpriteRect;
    private ContentSizeFitter sizeFitter;
    private LocalizedTypewriterEffect activeTypewriter;
    private readonly HashSet<GameObject> playedTypewriterDialogues = new HashSet<GameObject>();
    private readonly HashSet<GameObject> raycastDisabledDialogueRoots = new HashSet<GameObject>();
    private readonly Dictionary<GameObject, LocalizedTypewriterEffect> typewriterCache = new Dictionary<GameObject, LocalizedTypewriterEffect>();
    private bool suppressNextSceneClick;

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
        InputRouter.OnBlankClickAnyButton += TryHideByAnyButton;
    }

    void OnDisable()
    {
        InputRouter.OnBlankClick -= TryHide;
        InputRouter.OnBlankClickAnyButton -= TryHideByAnyButton;
    }

    public void ShowDialogue(GameObject dialogueSprite, Transform anchor)
    {
        HideDialogue();
        suppressNextSceneClick = false;

        activeDialogueSprite = dialogueSprite;
        activeDialogueSprite.SetActive(true);
        DisableDialogueRaycastBlocking(activeDialogueSprite);

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

        PrepareGoalTypewriter(dialogueSprite);
    }

    public void HideDialogue()
    {
        if (activeDialogueSprite != null)
        {
            activeDialogueSprite.SetActive(false);
            activeDialogueSprite = null;
            activeTypewriter = null;
        }

        suppressNextSceneClick = false;
    }

    private void DisableDialogueRaycastBlocking(GameObject dialogueRoot)
    {
        if (dialogueRoot == null)
            return;

        // 对同一个对话根节点只做一次遍历，避免重复 GetComponentsInChildren 开销。
        if (raycastDisabledDialogueRoots.Contains(dialogueRoot))
            return;

        Graphic[] graphics = dialogueRoot.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }

        CanvasGroup[] canvasGroups = dialogueRoot.GetComponentsInChildren<CanvasGroup>(true);
        foreach (CanvasGroup canvasGroup in canvasGroups)
        {
            canvasGroup.blocksRaycasts = false;
        }

        raycastDisabledDialogueRoots.Add(dialogueRoot);
    }
    
    public void TryHide()
    {
        TryHandleDialogueBlankClick();
    }

    public bool ConsumeSuppressedSceneClick()
    {
        if (!suppressNextSceneClick)
            return false;

        suppressNextSceneClick = false;
        return true;
    }

    private void TryHideByAnyButton(int mouseButton)
    {
        if (mouseButton != 1)
            return;

        TryHandleDialogueBlankClick();
    }

    private void TryHandleDialogueBlankClick()
    {
        if (IsDialogueActive())
        {
            if (activeTypewriter != null && activeTypewriter.IsTyping)
            {
                activeTypewriter.SkipToEnd();
                suppressNextSceneClick = true;
                return;
            }

            HideDialogue();
        }
    }

    private void PrepareGoalTypewriter(GameObject dialogueRoot)
    {
        activeTypewriter = null;

        if (dialogueRoot == null)
            return;

        if (!typewriterCache.TryGetValue(dialogueRoot, out LocalizedTypewriterEffect typewriter))
        {
            typewriter = dialogueRoot.GetComponentInChildren<LocalizedTypewriterEffect>(true);
            typewriterCache[dialogueRoot] = typewriter;
        }

        if (typewriter == null)
            return;

        activeTypewriter = typewriter;

        bool hasPlayedBefore = playedTypewriterDialogues.Contains(dialogueRoot);
        typewriter.Play(typewriter.phraseName, instant: hasPlayedBefore);

        if (!hasPlayedBefore)
            playedTypewriterDialogues.Add(dialogueRoot);
    }

    public bool IsDialogueActive() => activeDialogueSprite != null;
}
