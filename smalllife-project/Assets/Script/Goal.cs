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

    // 对白相关
    public Text dialogueText;
    private string[] dialoguesPreAnim1;
    private string[] dialoguesPostAnim1;
    private string[] dialoguesPostAnim2;
    private int dialogueIndex;

    // 不同阶段的Collider
    public Collider2D[] collidersPreAnim1;
    public Collider2D[] collidersPostAnim1;
    public Collider2D[] collidersPostAnim2;

    private enum Stage
    {
        PreAnim1,
        PostAnim1,
        PostAnim2
    }

    private Stage currentStage;

    private void Start()
    {
        //在start方法中查找场景中的CameraController 组件，并将其保存在 cameraController 变量中。
        cameraController = FindObjectOfType<CameraController>();
        // Instantiate the goal achieve prefab and set up the animators and image
        GameObject goalAchieveInstance = Instantiate(goalAchievePrefab, mCanvas.transform);
        goalAchieveAnimator = goalAchieveInstance.GetComponent<Animator>();
        achieveImage = goalAchieveInstance.transform.Find("goalimage").GetComponent<Image>();
        achieveImage.sprite = goalImage;

        // 初始化对白文本
        dialoguesPreAnim1 = new string[] { "Dialogue 1", "Dialogue 2", "Dialogue 3" };
        dialoguesPostAnim1 = new string[] { "Dialogue 4", "Dialogue 5", "Dialogue 6" };
        dialoguesPostAnim2 = new string[] { "Dialogue 7", "Dialogue 8", "Dialogue 9" };

        currentStage = Stage.PreAnim1;
        SetCollidersActive(collidersPreAnim1);

        // 确保对话框文本一开始是隐藏的
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            switch (currentStage)
            {
                case Stage.PreAnim1:
                    HandleDialogue(collidersPreAnim1, dialoguesPreAnim1, hit.collider);
                    break;
                case Stage.PostAnim1:
                    HandleDialogue(collidersPostAnim1, dialoguesPostAnim1, hit.collider);
                    break;
                case Stage.PostAnim2:
                    HandleDialogue(collidersPostAnim2, dialoguesPostAnim2, hit.collider);
                    break;
            }
        }
    }

    private void HandleDialogue(Collider2D[] colliders, string[] dialogues, Collider2D hitCollider)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (hitCollider == colliders[i])
            {
                ShowDialogue(dialogues[i]);
                break;
            }
        }
    }
    private void ShowDialogue(string dialogue)
    {
        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
            dialogueText.gameObject.SetActive(true);

            // 可以设置在一定时间后隐藏
            DOVirtual.DelayedCall(3f, () =>
            {
                dialogueText.gameObject.SetActive(false);
            });
        }
    }
    private void SetCollidersActive(Collider2D[] activeColliders)
    {
        // 先禁用所有Collider
        foreach (var col in collidersPreAnim1) col.enabled = false;
        foreach (var col in collidersPostAnim1) col.enabled = false;
        foreach (var col in collidersPostAnim2) col.enabled = false;

        // 启用当前阶段的Collider
        foreach (var col in activeColliders) col.enabled = true;
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
        SetCollidersActive(collidersPostAnim1);
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
            mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.8f);
            rectPenZai.DOLocalMove(uiPos, 0.8f).onComplete = () =>
            {
                //然后再将 mGameObjectNovel 从中间位置缩小回原来的大小，并移动到 mNovelPos 对应位置在 Canvas 中的坐标系中。
                mGameObjectNovel.transform.DOScale(Vector3.one, .5f);
                RectTransform r = mNovelPos.transform as RectTransform;
                uiPos = r.anchoredPosition;
                rectPenZai.DOLocalMove(uiPos, .5f).onComplete = () =>
                {
                    //最后，在 mGameObjectNovel 上执行 Animator 组件上名为 "click" 的触发器
                    rectPenZai.GetComponent<Animator>().SetTrigger("click");
                    Level.ins.AddCount();
                    Debug.Log("可以进入下一关");
                };
            };

            // 切换到anim2后的阶段
            currentStage = Stage.PostAnim2;
            SetCollidersActive(collidersPostAnim2);
        }//如果 mIsTriggered 为 true，则什么也不做。
    }
 }
