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

    // ÿ���׶ε� Collider ����
    public Collider2D[] collidersPreAnim1;
    public Collider2D[] collidersPostAnim1;
    public Collider2D[] collidersPostAnim2;

    // ÿ���׶εĶԻ��� Sprite ����
    public GameObject[] dialogueSpritesPreAnim1;
    public GameObject[] dialogueSpritesPostAnim1;
    public GameObject[] dialogueSpritesPostAnim2;

    // ��ɫ�԰׵� TextBox
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
        //��start�����в��ҳ����е�CameraController ����������䱣���� cameraController �����С�
        cameraController = FindObjectOfType<CameraController>();
        // Instantiate the goal achieve prefab and set up the animators and image
        GameObject goalAchieveInstance = Instantiate(goalAchievePrefab, mCanvas.transform);
        goalAchieveAnimator = goalAchieveInstance.GetComponent<Animator>();
        achieveImage = goalAchieveInstance.transform.Find("goalimage").GetComponent<Image>();
        achieveImage.sprite = goalImage;

        currentStage = Stage.PreAnim1;
        // ��ʼʱ�������жԻ���
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
                //����Ի����Ѿ���ʾ������κεط��رնԻ���
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
        //��ʾ�Ի���
        dialogueTextBox.SetActive(true);

        activeDialogueSprite = dialogueSprite;
        activeDialogueSprite.SetActive(true);

        // һ��ʱ������ضԻ���
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

            //���ضԻ���
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

        //��ʼ���ضԻ���
        dialogueTextBox.SetActive(false);
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

        //�л���anim1��Ľ׶�
        currentStage = Stage.PostAnim1;
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
            mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.3f);
            rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
            {
                //Ȼ���ٽ� mGameObjectNovel ���м�λ����С��ԭ���Ĵ�С�����ƶ��� mNovelPos ��Ӧλ���� Canvas �е�����ϵ�С�
                mGameObjectNovel.transform.DOScale(Vector3.one, 0.3f);
                RectTransform r = mNovelPos.transform as RectTransform;
                uiPos = r.anchoredPosition;
                rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
                {
                    //����� mGameObjectNovel ��ִ�� Animator �������Ϊ "click" �Ĵ�����
                    rectPenZai.GetComponent<Animator>().SetTrigger("click");
                    Level.ins.AddCount();
                    Debug.Log("���Խ�����һ��");
                };
            };

            // �л���anim2��Ľ׶�
            currentStage = Stage.PostAnim2;
        }//��� mIsTriggered Ϊ true����ʲôҲ������
    }
 }
