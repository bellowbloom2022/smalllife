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
    public bool mMoveCamera;//������bool�������ڿ����Ƿ��ƶ������
    public float mCamMoveSpeedA = 3f;//�ƶ���A���Ĭ���ٶ�
    public float mCamMoveSpeedB = 3f;//�ƶ���B���Ĭ���ٶ�

    private CameraController cameraController;

    public GameObject goalAchievePrefab;
    public Sprite goalImage;

    private Animator goalAchieveAnimator;
    private Image achieveImage;

    private void Start()
    {
        //��start�����в��ҳ����е�CameraController ����������䱣���� cameraController �����С�
        cameraController = FindObjectOfType<CameraController>();
        // Instantiate the goal achieve prefab and set up the animators and image
        GameObject goalAchieveInstance = Instantiate(goalAchievePrefab, mCanvas.transform);
        goalAchieveAnimator = goalAchieveInstance.GetComponent<Animator>();
        achieveImage = goalAchieveInstance.transform.Find("goalimage").GetComponent<Image>();
        achieveImage.sprite = goalImage;
    }

    void OnAnim1End()
    {
        //�� Goal����ĵ�һ��������Anim1���������ʱ���ر� Goal �����ϵ����� BoxCollider �����
        this.GetComponent<Animator>().ResetTrigger("click");
        BoxCollider[] clis = this.GetComponents<BoxCollider>();
        for (int i = 0; i < clis.Length; ++i)
        {
            clis[i].enabled = !clis[i].enabled;
        }

        //����UI�㼶�ĳɾͶ���goal_step1achieve
        if (goalAchieveAnimator != null)
        {
            goalAchieveAnimator.SetTrigger("goal_step1achieve");
        }

        //��� cameraController ������Ϊ�գ�������ƶ��� mCamPosA ��Ӧ��λ�á�
        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosA.transform.position, mCamMoveSpeedA);
        }
    }

    void OnAnim2End()
    {
        //�� Goal����ĵڶ���������Anim2���������ʱ������ mIsTriggered ���������Ƿ�ִ�����²�����
        if (!mIsTriggered)//��� mIsTriggered Ϊ false����ִ�����²�����
        {
            // ����UI�㼶�ĳɾͶ��� goal_step2achieve
            if (goalAchieveAnimator != null)
            {
                goalAchieveAnimator.SetTrigger("goal_step2achieve");
            }
            //��� cameraController ������Ϊ�գ�������ƶ��� mCamPosB ��Ӧ��λ�á�
            if (cameraController != null && mMoveCamera)
            {
                cameraController.MoveCameraToPosition(mCamPosB.transform.position, mCamMoveSpeedB);
            }
            Debug.Log("goal get");
            mIsTriggered = true;

            //���� mNovelPosStart ��Ӧ��λ�ã��� Canvas �е�����ϵ�м����С˵���� mGameObjectNovel ����ʼλ�á�
            Vector3 screenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
            Vector2 uiPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);
            RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
            rectPenZai.anchoredPosition = uiPos;

            //���� mNovelPosMid ��Ӧ��λ�ã��� Canvas �е�����ϵ�м����С˵���� mGameObjectNovel ���м�λ�á�
            screenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

            //ʹ�� DOTween ��ʵ�ֶ���Ч������ mGameObjectNovel ����ʼλ�÷Ŵ�������ͬʱ�� Canvas �е�����ϵ���ƶ����м�λ�á�
            mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.8f);
            rectPenZai.DOLocalMove(uiPos, 0.8f).onComplete = () =>
            {
                //Ȼ���ٽ� mGameObjectNovel ���м�λ����С��ԭ���Ĵ�С�����ƶ��� mNovelPos ��Ӧλ���� Canvas �е�����ϵ�С�
                mGameObjectNovel.transform.DOScale(Vector3.one, .5f);
                RectTransform r = mNovelPos.transform as RectTransform;
                uiPos = r.anchoredPosition;
                rectPenZai.DOLocalMove(uiPos, .5f).onComplete = () =>
                {
                    //����� mGameObjectNovel ��ִ�� Animator �������Ϊ "click" �Ĵ�����
                    rectPenZai.GetComponent<Animator>().SetTrigger("click");
                    Level.ins.AddCount();
                    Debug.Log("���Խ�����һ��");
                };
            };
        }//��� mIsTriggered Ϊ true����ʲôҲ������
    }
 }
