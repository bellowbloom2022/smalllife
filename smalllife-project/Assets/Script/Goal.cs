using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Goal : MonoBehaviour
{

    public GameObject mGameObjectNovel;
    public GameObject mNovelPosStart;
    public GameObject mNovelPosMid;
    public GameObject mNovelPos;
    public GameObject mCamPosA;
    public GameObject mCamPosB;
    public Canvas mCanvas;
    private bool mIsTriggered;
    public bool mMoveCamera;//公共的bool变量用于控制是否移动摄像机
    public float mCamMoveSpeedA = 3f;//移动到A点的默认速度
    public float mCamMoveSpeedB = 3f;//移动到B点的默认速度

    private CameraController cameraController;

    public GameObject goalAchievePrefab;
    public Sprite goalImage;

    private Animator goalAchieveAnimator;
    private Image achieveImage;

    // 每个阶段的 Collider 数组
    public Collider2D[] collidersPreAnim1;
    public Collider2D[] collidersPostAnim1;
    public Collider2D[] collidersPostAnim2;

    // 每个阶段的对话框 Sprite 数组
    public GameObject[] dialogueSpritesPreAnim1;
    public GameObject[] dialogueSpritesPostAnim1;
    public GameObject[] dialogueSpritesPostAnim2;

    // 角色对白的 TextBox
    public GameObject dialogueTextBox;
    private enum Stage
    {
        PreAnim1,
        PostAnim1,
        PostAnim2
    }

    private Stage currentStage;
    private GameObject activeDialogueSprite;

    private void Start()
    {
        //在start方法中查找场景中的CameraController 组件，并将其保存在 cameraController 变量中。
        cameraController = FindObjectOfType<CameraController>();
        // Instantiate the goal achieve prefab and set up the animators and image
        GameObject goalAchieveInstance = Instantiate(goalAchievePrefab, mCanvas.transform);
        goalAchieveAnimator = goalAchieveInstance.GetComponent<Animator>();
        achieveImage = goalAchieveInstance.transform.Find("goalimage").GetComponent<Image>();
        achieveImage.sprite = goalImage;

        currentStage = Stage.PreAnim1;
        // 初始时隐藏所有对话框
        HideAllDialogueSprites();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

            if(activeDialogueSprite != null)
            {
                //如果对话框已经显示，点击任何地方关闭对话框
                HideActiveDialogueSprite();
            }

            else if (hitCollider != null)
            {
                switch (currentStage)
                {
                    case Stage.PreAnim1:
                        HandleDialogueClick(collidersPreAnim1, dialogueSpritesPreAnim1, hitCollider);
                        break;
                    case Stage.PostAnim1:
                        HandleDialogueClick(collidersPostAnim1, dialogueSpritesPostAnim1, hitCollider);
                        break;
                    case Stage.PostAnim2:
                        HandleDialogueClick(collidersPostAnim2, dialogueSpritesPostAnim2, hitCollider);
                        break;
                }
            }
        }
    }

    private void HandleDialogueClick(Collider2D[] colliders, GameObject[] dialogueSprites, Collider2D hitCollider)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == hitCollider)
            {
                ShowDialogueSprite(dialogueSprites[i]);
                break;
            }
        }
    }
    private void ShowDialogueSprite(GameObject dialogueSprite)
    {
        //显示对话框
        dialogueTextBox.SetActive(true);

        activeDialogueSprite = dialogueSprite;
        activeDialogueSprite.SetActive(true);

        // 一定时间后隐藏对话框
        //DOVirtual.DelayedCall(3f, () =>
        //{
        //dialogueSprite.SetActive(false);
        //});
    }

    private void HideActiveDialogueSprite()
    {
        if(activeDialogueSprite != null)
        {
            activeDialogueSprite.SetActive(false);
            activeDialogueSprite = null;

            //隐藏对话框
            dialogueTextBox.SetActive(false);
        }
    }
    private void HideAllDialogueSprites()
    {
        foreach (var sprite in dialogueSpritesPreAnim1)
        {
            sprite.SetActive(false);
        }
        foreach (var sprite in dialogueSpritesPostAnim1)
        {
            sprite.SetActive(false);
        }
        foreach (var sprite in dialogueSpritesPostAnim2)
        {
            sprite.SetActive(false);
        }
        activeDialogueSprite = null;

        //初始隐藏对话框
        dialogueTextBox.SetActive(false);
    }
    void OnAnim1End()
    {
        //当 Goal对象的第一个动画（Anim1）播放完毕时，关闭 Goal 对象上的所有 BoxCollider 组件。
        this.GetComponent<Animator>().ResetTrigger("click");
        BoxCollider[] clis = this.GetComponents<BoxCollider>();
        for (int i = 0; i < clis.Length; ++i)
        {
            clis[i].enabled = !clis[i].enabled;
        }

        //播放UI层级的成就动画goal_step1achieve
        if (goalAchieveAnimator != null)
        {
            goalAchieveAnimator.SetTrigger("goal_step1achieve");
        }

        //如果 cameraController 变量不为空，将相机移动到 mCamPosA 对应的位置。
        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosA.transform.position, mCamMoveSpeedA);
        }

        //切换到anim1后的阶段
        currentStage = Stage.PostAnim1;
    }

    void OnAnim2End()
    {
        //当 Goal对象的第二个动画（Anim2）播放完毕时，根据 mIsTriggered 变量决定是否执行以下操作：
        if (!mIsTriggered)//如果 mIsTriggered 为 false，则执行以下操作：
        {
            // 播放UI层级的成就动画 goal_step2achieve
            if (goalAchieveAnimator != null)
            {
                goalAchieveAnimator.SetTrigger("goal_step2achieve");
            }
            //如果 cameraController 变量不为空，将相机移动到 mCamPosB 对应的位置。
            if (cameraController != null && mMoveCamera)
            {
                cameraController.MoveCameraToPosition(mCamPosB.transform.position, mCamMoveSpeedB);
            }
            Debug.Log("goal get");
            mIsTriggered = true;

            //根据 mNovelPosStart 对应的位置，在 Canvas 中的坐标系中计算出小说对象 mGameObjectNovel 的起始位置。
            Vector3 screenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
            Vector2 uiPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);
            RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
            rectPenZai.anchoredPosition = uiPos;

            //根据 mNovelPosMid 对应的位置，在 Canvas 中的坐标系中计算出小说对象 mGameObjectNovel 的中间位置。
            screenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

            //使用 DOTween 库实现动画效果，将 mGameObjectNovel 从起始位置放大两倍，同时在 Canvas 中的坐标系中移动到中间位置。
            mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.3f);
            rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
            {
                //然后再将 mGameObjectNovel 从中间位置缩小回原来的大小，并移动到 mNovelPos 对应位置在 Canvas 中的坐标系中。
                mGameObjectNovel.transform.DOScale(Vector3.one, 0.3f);
                RectTransform r = mNovelPos.transform as RectTransform;
                uiPos = r.anchoredPosition;
                rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
                {
                    //最后，在 mGameObjectNovel 上执行 Animator 组件上名为 "click" 的触发器
                    rectPenZai.GetComponent<Animator>().SetTrigger("click");
                    Level.ins.AddCount();
                    Debug.Log("可以进入下一关");
                };
            };

            // 切换到anim2后的阶段
            currentStage = Stage.PostAnim2;
        }//如果 mIsTriggered 为 true，则什么也不做。
    }
 }
