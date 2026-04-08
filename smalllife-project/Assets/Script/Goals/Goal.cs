using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Goal : MonoBehaviour
{
    public LevelDataAsset levelData;
    [Header("Step Config")]
    public StepConfig step1Config;
    public StepConfig step2Config;
    [SerializeField] private int goalID; // 唯一标识符
    public int GoalID => goalID;
    public bool isFound;     // 是否已找到
    public GameObject mGameObjectNovel;
    public GameObject mNovelPosStart;
    public GameObject mNovelPosMid;
    public GameObject mNovelPos;
    public Canvas mCanvas;
    private bool mIsTriggered;
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

    [Header("Animator")]
    [SerializeField] private Animator animator;
    public bool step1Completed = false;
    public bool step2Completed = false;

    [Header("Goal Icon UI")]
    [SerializeField] private GoalIconUIController iconController;

    [Header("Clickable 3D Colliders")]
    [SerializeField] private BoxCollider[] step1ClickableColliders;
    [SerializeField] private BoxCollider[] step2ClickableColliders;

    private BoxCollider[] allGoalBoxColliders;
    private int inputLockVersion;
    private const float MinInputUnlockFallbackDelay = 0.4f;


    protected virtual void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        currentStage = Stage.PreAnim1;
        InitializeClickableColliderConfig();
        ApplyClickableCollidersByStepState();
        SFXZone.TryRegister(GetComponent<AudioSource>());
    }
    public virtual void OnClicked()
    {
        // 防止 Step2 期间重复点击（可选但推荐）
        if (step2Completed)
            return;

        if (!step1Completed)
        {
            PlayStep1();
            return;
        }
        else if (!step2Completed)
        {
            PlayStep2();
            return;
        }
    }

    public virtual void ApplySavedProgress(GoalProgress progress)
    {
        step1Completed = progress.step1Completed;
        step2Completed = progress.step2Completed;
        isFound = step1Completed && step2Completed;
        ApplyClickableCollidersByStepState();
        PlayLoopAnimationAccordingToStep();
        iconController?.ApplyProgress(step1Completed, step2Completed);
    }

    private void InitializeClickableColliderConfig()
    {
        allGoalBoxColliders = GetComponents<BoxCollider>();

        // Backward compatibility: if no explicit config is set in inspector,
        // infer step1/step2 clickables from current enabled state.
        if ((step1ClickableColliders == null || step1ClickableColliders.Length == 0) &&
            (step2ClickableColliders == null || step2ClickableColliders.Length == 0) &&
            allGoalBoxColliders != null && allGoalBoxColliders.Length > 0)
        {
            System.Collections.Generic.List<BoxCollider> step1List = new System.Collections.Generic.List<BoxCollider>();
            System.Collections.Generic.List<BoxCollider> step2List = new System.Collections.Generic.List<BoxCollider>();

            foreach (BoxCollider collider in allGoalBoxColliders)
            {
                if (collider == null) continue;
                if (collider.enabled) step1List.Add(collider);
                else step2List.Add(collider);
            }

            step1ClickableColliders = step1List.ToArray();
            step2ClickableColliders = step2List.ToArray();
        }
    }

    private void ApplyClickableCollidersByStepState()
    {
        // Step1 complete but Step2 not complete => switch to step2 click area.
        bool useStep2Clickable = step1Completed && !step2Completed;
        SetClickableColliders(useStep2Clickable ? step2ClickableColliders : step1ClickableColliders);
    }

    private void SetClickableColliders(BoxCollider[] activeColliders)
    {
        if (allGoalBoxColliders == null || allGoalBoxColliders.Length == 0)
            allGoalBoxColliders = GetComponents<BoxCollider>();

        if (allGoalBoxColliders == null || allGoalBoxColliders.Length == 0)
            return;

        foreach (BoxCollider collider in allGoalBoxColliders)
        {
            if (collider != null)
                collider.enabled = false;
        }

        if (activeColliders == null) return;

        foreach (BoxCollider collider in activeColliders)
        {
            if (collider != null)
                collider.enabled = true;
        }
    }

    public virtual void PlayLoopAnimationAccordingToStep()
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

                if (markStep2)
                {
                    SaveSystem.SaveGame();
                    GoalNoteEvents.RaiseGoalCompleted(levelData.levelID, goalID, GoalNoteStep.Step2);
                }
                else
                {
                    // SingleGoal 单步完成：SaveGame 并通知 UI
                    SaveSystem.SaveGame();
                    GoalNoteEvents.RaiseGoalCompleted(levelData.levelID, goalID, GoalNoteStep.Step1);
                }
            };
        };

        AudioHub.Instance.PlayGlobal("goal_found");
        currentStage = markStep2 ? Stage.PostAnim2 : Stage.PostAnim1;
        ShowFirstDialogueOfCurrentStage();
    }
    protected void PlayStep1()
    {
        if (animator == null)
        {
            Debug.LogError("[PlayStep1] Animator is null");
            return;
        }
        Debug.Log("[PlayStep1]");
        animator.ResetTrigger("step2");
        if (animator != null)
            animator.SetTrigger("step1");

        BeginStep1(); // 你已有的方法：镜头 / 聚焦 / InputLock
    }

    private void PlayStep2()
    {
        if (animator == null)
        {
            Debug.LogError("[PlayStep2] Animator is null");
            return;
        }
        Debug.Log("[PlayStep2]");
        animator.ResetTrigger("step1");

        if (animator != null)
            animator.SetTrigger("step2");

        BeginStep2();
    }

    public void BeginStep1()
    {
        ExecuteStep(step1Config);
    }

    public void BeginStep2()
    {
        ExecuteStep(step2Config);
    }
    protected void ExecuteStep(StepConfig config)
    {
        if (config == null) return;

        if (config.lockInput)
        {
            ScheduleInputUnlockFallback(config);
            if (InputRouter.Instance != null)
                InputRouter.Instance.LockInput();
        }

        if (config.useFocus && config.focusTarget != null)
        {
            FocusMaskController.Instance.Show(
                config.focusTarget,
                config.focusRadius,
                config.focusShowDuration
            );
        }

        if (config.moveCamera && config.cameraTarget != null)
        {
            DOVirtual.DelayedCall(config.cameraDelay, () =>
            {
                if (Camera.main == null) return;
                CameraController controller = Camera.main.GetComponent<CameraController>();
                if (controller == null) return;
                controller.MoveCameraToPositionByDuration(
                    config.cameraTarget.position,
                    config.cameraDuration
                );
            });
        }
    }

    private void ScheduleInputUnlockFallback(StepConfig config)
    {
        int lockVersion = ++inputLockVersion;
        float fallbackDelay = Mathf.Max(
            MinInputUnlockFallbackDelay,
            config.cameraDelay + config.cameraDuration + 0.5f
        );

        DOVirtual.DelayedCall(fallbackDelay, () =>
        {
            if (lockVersion != inputLockVersion)
                return;

            if (InputRouter.Instance == null || !InputRouter.Instance.InputLocked)
                return;

            Debug.LogWarning($"[Goal {goalID}] Fallback unlock triggered.");
            InputRouter.Instance.UnlockInput();
        });
    }

    private void CancelInputUnlockFallback()
    {
        inputLockVersion++;
    }
    public virtual void OnAnimEnd()
    {
        // Step1 动画刚播完
        if (!step1Completed)
        {
            HandleStep1AnimEnd();
            return;
        }

        // Step2 动画刚播完
        if (step1Completed && !step2Completed)
        {
            HandleStep2AnimEnd();
            return;
        }
    }  

    protected void EndStep(StepConfig config)
    {
        if (config == null) return;

        if (config.useFocus)
            FocusMaskController.Instance.Hide(config.focusHideDuration);

        if (config.lockInput)
        {
            CancelInputUnlockFallback();
            if (InputRouter.Instance != null)
                InputRouter.Instance.UnlockInput();
        }
    }

    protected void HandleStep1AnimEnd()
    {
        // 保险：确保输入解锁，即使 OnAnimEnd 触发异常或没有调用
        if (step1Config != null && step1Config.lockInput)
            InputRouter.Instance.UnlockInput();

        EndStep(step1Config);
        //this.GetComponent<Animator>().ResetTrigger("click");
        Animator anim = GetComponent<Animator>();
        anim.ResetTrigger("step1"); // 对齐 Step 系统
        SetClickableColliders(step2ClickableColliders);
        AudioHub.Instance.PlayGlobal("goal_step1");
        step1Completed = true;

        currentStage = Stage.PostAnim1;

        GameDataUtils.SetGoalStep(
            SaveSystem.GameData, 
            Level.ins.currentLevelIndex, 
            goalID, 
            true, 
            false
        );
        SaveSystem.SaveGame();
        if (levelData != null)
            GoalNoteEvents.RaiseGoalCompleted(levelData.levelID, goalID, GoalNoteStep.Step1);

        ShowFirstDialogueOfCurrentStage();
    }

    protected void HandleStep2AnimEnd()
    {
        // 保险：确保输入解锁，即使 OnAnimEnd 触发异常或没有调用
        if (step2Config != null && step2Config.lockInput)
            InputRouter.Instance.UnlockInput();

        EndStep(step2Config);
        if (!mIsTriggered)
        {
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
