using UnityEngine;

public partial class Goal : MonoBehaviour
{
    public void HandleClick(Collider2D hitCollider)
    {
        switch (currentStage)
        {
            case Stage.PreAnim1:
                HandleDialogueClick(collidersPreAnim1, dialogueSpritesPreAnim1, dialogueAnchorsPreAnim1, hitCollider);
                break;
            case Stage.PostAnim1:
                HandleDialogueClick(collidersPostAnim1, dialogueSpritesPostAnim1, dialogueAnchorsPostAnim1, hitCollider);
                break;
            case Stage.PostAnim2:
                HandleDialogueClick(collidersPostAnim2, dialogueSpritesPostAnim2, dialogueAnchorsPostAnim2, hitCollider);
                break;
        }
    }

    protected void HandleDialogueClick(Collider2D[] colliders, GameObject[] dialogueSprites, Transform[] dialogueAnchors, Collider2D hitCollider)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == hitCollider)
            {
                DialogueManager.Instance.ShowDialogue(dialogueSprites[i], dialogueAnchors[i]);
                break;
            }
        }
    }

    public bool IsMyGoalCollider(Collider2D collider)
    {
        switch (currentStage)
        {
            case Stage.PreAnim1:
                return System.Array.Exists(collidersPreAnim1, c => c == collider);
            case Stage.PostAnim1:
                return System.Array.Exists(collidersPostAnim1, c => c == collider);
            case Stage.PostAnim2:
                return System.Array.Exists(collidersPostAnim2, c => c == collider);
            default:
                return false;
        }
    }

    private void RestoreDialoguePlayedState()
    {
        if (DialogueManager.Instance == null)
            return;

        // 如果Step1已完成，标记PreAnim1对话为已播放
        if (step1Completed)
        {
            foreach (var spriteObj in dialogueSpritesPreAnim1)
                if (spriteObj != null)
                    DialogueManager.Instance.MarkSpriteAsPlayed(spriteObj);
        }

        // 如果Step2已完成，标记PostAnim2对话为已播放
        if (step2Completed)
        {
            foreach (var spriteObj in dialogueSpritesPostAnim2)
                if (spriteObj != null)
                    DialogueManager.Instance.MarkSpriteAsPlayed(spriteObj);
        }
        // 否则如果只Step1完成，标记PostAnim1对话为已播放
        else if (step1Completed)
        {
            foreach (var spriteObj in dialogueSpritesPostAnim1)
                if (spriteObj != null)
                    DialogueManager.Instance.MarkSpriteAsPlayed(spriteObj);
        }
    }

    public void ShowFirstDialogueOfCurrentStage()
    {
        DialogueManager.Instance.HideDialogue();

        switch (currentStage)
        {
            case Stage.PreAnim1:
                if (dialogueSpritesPreAnim1.Length > 0)
                    DialogueManager.Instance.ShowDialogue(dialogueSpritesPreAnim1[0], dialogueAnchorsPreAnim1[0]);
                break;

            case Stage.PostAnim1:
                if (dialogueSpritesPostAnim1.Length > 0)
                    DialogueManager.Instance.ShowDialogue(dialogueSpritesPostAnim1[0], dialogueAnchorsPostAnim1[0]);
                break;

            case Stage.PostAnim2:
                if (dialogueSpritesPostAnim2.Length > 0)
                    DialogueManager.Instance.ShowDialogue(dialogueSpritesPostAnim2[0], dialogueAnchorsPostAnim2[0]);
                break;
        }
    }
}
