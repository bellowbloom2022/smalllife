using UnityEngine;
using DG.Tweening;

public class Goal1 : MonoBehaviour
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

    public HintAnimator3 hintAnimator3;

    private CameraController cameraController;

    private void Start()
    {
        //在start方法中查找场景中的CameraController 组件，并将其保存在 cameraController 变量中。
        cameraController = FindObjectOfType<CameraController>();
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
        //如果 cameraController 变量不为空，将相机移动到 mCamPosA 对应的位置。
        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosA.transform.position, mCamMoveSpeedA);
        }
    }

    void OnAnim2End()
    {
        //当 Goal对象的第二个动画（Anim2）播放完毕时，根据 mIsTriggered 变量决定是否执行以下操作：
        if (!mIsTriggered)//如果 mIsTriggered 为 false，则执行以下操作：
        {
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
            if (hintAnimator3 != null)
            {
                hintAnimator3.GoalAchieved();
                Debug.Log("goal事件通知发送成功");
            }
            else
            {
                Debug.LogError("HintAnimator3 reference is not set in the Goal script");
            }
        }//如果 mIsTriggered 为 true，则什么也不做。
    }
 }
