using UnityEngine;

public partial class Goal : MonoBehaviour
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

    [Header("Collect Visual")]
    [SerializeField] private bool cleanupNovelAfterCollect = true;
    [SerializeField] private bool destroyNovelObjectAfterCollect = true;
    [SerializeField] private float cleanupDelayAfterCollect = 1f;

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
    private const float FallbackUnlockTimeout = 8f;
    private bool stepCutsceneActive;
    private float stepCutsceneStartTime;

    protected virtual void Start()
    {
        cameraController = FindObjectOfType<CameraController>();

        // 仅在无保存进度时初始化为PreAnim1，否则让ApplySavedProgress()设置正确的stage
        if (Level.ins == null || !SaveSystem.GameData.goalProgressMap.ContainsKey($"{Level.ins.currentLevelIndex}_{goalID}"))
            currentStage = Stage.PreAnim1;

        InitializeClickableColliderConfig();
        ApplyClickableCollidersByStepState();
        SFXZone.TryRegister(GetComponent<AudioSource>());
    }

    public virtual void OnClicked()
    {
        // Block any new goal trigger while a step cutscene is running.
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        // Block re-entry while Step2 collect flow is already in progress.
        if (mIsTriggered)
            return;

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
        isFound = GoalProgressRules.IsCollected(this, step1Completed, step2Completed);
        ApplyClickableCollidersByStepState();
        PlayLoopAnimationAccordingToStep();
        iconController?.ApplyProgress(step1Completed, step2Completed);
        ApplyCollectedVisualStateFromSave();
        RestoreDialoguePlayedState();
    }

    private void ApplyCollectedVisualStateFromSave()
    {
        bool isCollectedOnSave = GoalProgressRules.IsCollected(this, step1Completed, step2Completed);
        if (!isCollectedOnSave || mGameObjectNovel == null)
            return;

        if (destroyNovelObjectAfterCollect)
            Destroy(mGameObjectNovel);
        else
            mGameObjectNovel.SetActive(false);
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
        if (step2Completed)
        {
            SetClickableColliders(null);
            return;
        }

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

        // Disable clickable colliders immediately after Step2 trigger.
        SetClickableColliders(null);

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

    protected void HandleStep1AnimEnd()
    {
        EndStep(step1Config);
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
        EndStep(step2Config);
        if (!mIsTriggered)
        {
            mIsTriggered = true;
            TriggerCollectAnimation(true);
        }
    }
}
