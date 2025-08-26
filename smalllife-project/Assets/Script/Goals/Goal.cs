using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Goal : MonoBehaviour
{
    [SerializeField] private int goalID; //唯一标识符
    public int GoalID => goalID; // 添加一个只读属性
    public bool isFound;     //是否已找到
    public GameObject mGameObjectNovel;
    public GameObject mNovelPosStart;
    public GameObject mNovelPosMid;
    public GameObject mNovelPos;
    public GameObject mCamPosA;
    public GameObject mCamPosB;
    public Canvas mCanvas;
    private bool mIsTriggered;
    public bool mMoveCamera;//公共的bool变量用于控制是否移动摄像机
    public float mCamMoveSpeedA = 1f;//移动到A点的默认速度
    public float mCamMoveSpeedB = 1f;//移动到B点的默认速度

    private CameraController cameraController;

    // anim1前的Collider、对话框Sprite、对白内容设置锚点位置数组
    public Collider2D[] collidersPreAnim1;
    public GameObject[] dialogueSpritesPreAnim1;
    public Transform[] dialogueAnchorsPreAnim1;

    // anim1后的Collider、对话框Sprite、对白内容设置锚点位置数组
    public Collider2D[] collidersPostAnim1;
    public GameObject[] dialogueSpritesPostAnim1;
    public Transform[] dialogueAnchorsPostAnim1;

    // anim2后的Collider、对话框Sprite、对白内容设置锚点位置数组
    public Collider2D[] collidersPostAnim2;
    public GameObject[] dialogueSpritesPostAnim2;
    public Transform[] dialogueAnchorsPostAnim2;

    private enum Stage { PreAnim1, PostAnim1, PostAnim2}
    private Stage currentStage;

    private Animator animator;
    public bool step1Completed;
    public bool step2Completed;

    private void Start()
    {
        //在start方法中查找场景中的CameraController组件，并将其保存在cameraController变量中。
        cameraController = FindObjectOfType<CameraController>();
        currentStage = Stage.PreAnim1;
        //自动注册 Goal Audio 到 SFXZone（衰减控制）
        SFXZone.TryRegister(GetComponent<AudioSource>());
    }

    public void ApplySavedProgress(GoalProgress progress){
        step1Completed = progress.step1Completed;
        step2Completed = progress.step2Completed;
        isFound = step1Completed && step2Completed;
        PlayLoopAnimationAccordingToStep(); // 在设置完状态后自动播放动画
    }

    public void PlayLoopAnimationAccordingToStep(){
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null){
            Debug.LogWarning($"Animator not found on {gameObject.name}");
            return;
        }

        string animName = "";

        if (step1Completed && step2Completed){
            animName = $"2_goal{goalID}_loop";
            currentStage = Stage.PostAnim2;
        }
        else if (step1Completed){
            animName = $"1_goal{goalID}_loop";
            currentStage = Stage.PostAnim1;
        }
        else{
            animName = $"0_goal{goalID}_normal";
            currentStage = Stage.PreAnim1;
        }
        animator.Play(animName);
    }

    public void HandleClick(Collider2D hitCollider){
        switch (currentStage){
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

    private void HandleDialogueClick(Collider2D[] colliders, GameObject[] dialogueSprites, Transform[] dialogueAnchors, Collider2D hitCollider)
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

    void OnAnim1End()
    {
        //当Goal对象的第一个动画（Anim1）播放完毕时，关闭Goal对象上的所有boxcollider组件。
        this.GetComponent<Animator>().ResetTrigger("click");
        BoxCollider[] clis = this.GetComponents<BoxCollider>();
        for (int i = 0; i < clis.Length; ++i)
        {
            clis[i].enabled = !clis[i].enabled;
        }

        //如果cameraController 变量不为空，将相机移动到 mCamPosA 对应的位置。
        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosA.transform.position, mCamMoveSpeedA);
        }
        AudioHub.Instance.PlayGlobal("goal_step1");
        //切换到anim1后的阶段
        currentStage = Stage.PostAnim1;
        //记录step1完成
        GameDataUtils.SetGoalStep(SaveSystem.GameData, Level.ins.currentLevelIndex, goalID, true, false);
        SaveSystem.SaveGame();

        // 自动显示 PostAnim1 阶段的第一个对话框
        ShowFirstDialogueOfCurrentStage();
    }

    void OnAnim2End()
    {
        //当goal对象的第二个动画（Anim2）播放完毕时，根据 mIsTrigger 变量决定是否执行一下操作
        if (!mIsTriggered)
        {
            //如果 cameraController 变量不为空，将相机移动到 mCamPosB 对应的位置
            if (cameraController != null && mMoveCamera)
            {
                cameraController.MoveCameraToPosition(mCamPosB.transform.position, mCamMoveSpeedB);
            }
            mIsTriggered = true;

            //根据 mNovelPosStart 对应的位置，在 Canvas 中的坐标系中计算出小说对象 mGameObjectNovel 的起始位置
            Vector3 screenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
            Vector2 uiPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);
            RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
            rectPenZai.anchoredPosition = uiPos;

            //根据 mNovelPosMid 对应的位置，在 Canvas 中的坐标系中计算出小说对象 mGameObjectNovel 的中间位置
            screenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

            //使用 DOTween 库实现动画效果，将 mGameObjectNovel 从起始位置放大两倍，同时在 Canvas 中的坐标系中移动到中间位置
            mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.3f);
            rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
            {
                //然后再将 mGameObjectNovel 从中间位置缩小回原来的大小，并移动到 mNovelPos 对应位置在 Canvas 中的坐标系中。
                mGameObjectNovel.transform.DOScale(Vector3.one, 0.3f);
                RectTransform r = mNovelPos.transform as RectTransform;
                uiPos = r.anchoredPosition;
                rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
                {
                    //最后，在mGameObjectNovel 上执行 Animator 组件上名为 "click" 的触发器
                    rectPenZai.GetComponent<Animator>().SetTrigger("click");
                    Level.ins.AddCount();
                    isFound = true;
                    Debug.Log($"Goal ID {goalID} marked has found.");
                    //标记step1+step2都完成
                    GameDataUtils.SetGoalStep(SaveSystem.GameData, Level.ins.currentLevelIndex, goalID, true, true);

                    //更新该关卡的已完成目标数量到存档中
                    int completedGoals = GameDataUtils.GetCompletedGoalCount(SaveSystem.GameData, Level.ins.currentLevelIndex);
                    SaveSystem.UpdateLevelStar(Level.ins.currentLevelIndex, completedGoals);
                };
            };
            AudioHub.Instance.PlayGlobal("goal_found");
            // 切换到anim2后的阶段
            currentStage = Stage.PostAnim2;
            // 自动显示 PostAnim2 阶段的第一个对话框
            ShowFirstDialogueOfCurrentStage();
        }
    }

    public void ShowFirstDialogueOfCurrentStage()
    {
        DialogueManager.Instance.HideDialogue(); // 先关掉可能遗留的对话

        switch (currentStage)
        {
            case Stage.PreAnim1:
                if (dialogueSpritesPreAnim1.Length > 0 && dialogueAnchorsPreAnim1.Length > 0)
                {
                    DialogueManager.Instance.ShowDialogue(dialogueSpritesPreAnim1[0], dialogueAnchorsPreAnim1[0]);
                }
                break;

            case Stage.PostAnim1:
                if (dialogueSpritesPostAnim1.Length > 0 && dialogueAnchorsPostAnim1.Length > 0)
                {
                    DialogueManager.Instance.ShowDialogue(dialogueSpritesPostAnim1[0], dialogueAnchorsPostAnim1[0]);
                }
                break;

            case Stage.PostAnim2:
                if (dialogueSpritesPostAnim2.Length > 0 && dialogueAnchorsPostAnim2.Length > 0)
                {
                    DialogueManager.Instance.ShowDialogue(dialogueSpritesPostAnim2[0], dialogueAnchorsPostAnim2[0]);
                }
                break;
        }
    }
}

