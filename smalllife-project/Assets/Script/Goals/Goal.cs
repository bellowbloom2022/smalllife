using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Goal : MonoBehaviour
{
    [SerializeField] private int goalID; // 唯一标识符
    public int GoalID => goalID;
    public bool isFound;     // 是否已找到
    public GameObject mGameObjectNovel;
    public GameObject mNovelPosStart;
    public GameObject mNovelPosMid;
    public GameObject mNovelPos;
    public GameObject mCamPosA;
    public GameObject mCamPosB;
    public Canvas mCanvas;
    private bool mIsTriggered;
    public bool mMoveCamera;
    public float mCamMoveSpeedA = 1f;
    public float mCamMoveSpeedB = 1f;

    private CameraController cameraController;

    // 三个阶段的交互配置
    public Collider2D[] collidersPreAnim1;
    public GameObject[] dialogueSpritesPreAnim1;
    public Transform[] dialogueAnchorsPreAnim1;

    public Collider2D[] collidersPostAnim1;
    public GameObject[] dialogueSpritesPostAnim1;
    public Transform[] dialogueAnchorsPostAnim1;

    public Collider2D[] collidersPostAnim2;
    public GameObject[] dialogueSpritesPostAnim2;
    public Transform[] dialogueAnchorsPostAnim2;

    protected enum Stage { PreAnim1, PostAnim1, PostAnim2 }
    protected Stage currentStage;

    private Animator animator;
    public bool step1Completed;
    public bool step2Completed;

    protected virtual void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        currentStage = Stage.PreAnim1;
        SFXZone.TryRegister(GetComponent<AudioSource>());
    }

    public void ApplySavedProgress(GoalProgress progress)
    {
        step1Completed = progress.step1Completed;
        step2Completed = progress.step2Completed;
        isFound = step1Completed && step2Completed;
        PlayLoopAnimationAccordingToStep();
    }

    public void PlayLoopAnimationAccordingToStep()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) return;

        string animName = "";

        if (step1Completed && step2Completed)
        {
            animName = $"2_goal{goalID}_loop";
            currentStage = Stage.PostAnim2;
        }
        else if (step1Completed)
        {
            animName = $"1_goal{goalID}_loop";
            currentStage = Stage.PostAnim1;
        }
        else
        {
            animName = $"0_goal{goalID}_normal";
            currentStage = Stage.PreAnim1;
        }
        animator.Play(animName);
    }

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

    protected void TriggerCollectAnimation(bool markStep2 = true)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out Vector2 uiPos);
        RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
        rectPenZai.anchoredPosition = uiPos;

        screenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

        mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.3f);
        rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
        {
            mGameObjectNovel.transform.DOScale(Vector3.one, 0.3f);
            RectTransform r = mNovelPos.transform as RectTransform;
            uiPos = r.anchoredPosition;
            rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
            {
                rectPenZai.GetComponent<Animator>().SetTrigger("click");
                Level.ins.AddCount();
                isFound = true;
                step1Completed = true;
                if (markStep2) step2Completed = true;

                Debug.Log($"Goal ID {goalID} marked as found.");
                GameDataUtils.SetGoalStep(SaveSystem.GameData, Level.ins.currentLevelIndex, goalID, true, markStep2);

                int completedGoals = GameDataUtils.GetCompletedGoalCount(SaveSystem.GameData, Level.ins.currentLevelIndex);
                SaveSystem.UpdateLevelStar(Level.ins.currentLevelIndex, completedGoals);
            };
        };

        AudioHub.Instance.PlayGlobal("goal_found");
        currentStage = markStep2 ? Stage.PostAnim2 : Stage.PostAnim1;
        ShowFirstDialogueOfCurrentStage();
    }

    protected void OnAnim1End()
    {
        this.GetComponent<Animator>().ResetTrigger("click");
        foreach (BoxCollider cli in this.GetComponents<BoxCollider>())
        {
            cli.enabled = !cli.enabled;
        }

        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosA.transform.position, mCamMoveSpeedA);
        }
        AudioHub.Instance.PlayGlobal("goal_step1");

        currentStage = Stage.PostAnim1;
        GameDataUtils.SetGoalStep(SaveSystem.GameData, Level.ins.currentLevelIndex, goalID, true, false);
        SaveSystem.SaveGame();

        ShowFirstDialogueOfCurrentStage();
    }

    protected void OnAnim2End()
    {
        if (!mIsTriggered)
        {
            if (cameraController != null && mMoveCamera)
            {
                cameraController.MoveCameraToPosition(mCamPosB.transform.position, mCamMoveSpeedB);
            }
            mIsTriggered = true;
            TriggerCollectAnimation(true);
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
