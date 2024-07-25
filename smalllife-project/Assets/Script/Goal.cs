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

    // �԰����
    public Text dialogueText;
    private string[] dialoguesPreAnim1;
    private string[] dialoguesPostAnim1;
    private string[] dialoguesPostAnim2;
    private int dialogueIndex;

    // ��ͬ�׶ε�Collider
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
        //��start�����в��ҳ����е�CameraController ����������䱣���� cameraController �����С�
        cameraController = FindObjectOfType<CameraController>();
        // Instantiate the goal achieve prefab and set up the animators and image
        GameObject goalAchieveInstance = Instantiate(goalAchievePrefab, mCanvas.transform);
        goalAchieveAnimator = goalAchieveInstance.GetComponent<Animator>();
        achieveImage = goalAchieveInstance.transform.Find("goalimage").GetComponent<Image>();
        achieveImage.sprite = goalImage;

        // ��ʼ���԰��ı�
        dialoguesPreAnim1 = new string[] { "Dialogue 1", "Dialogue 2", "Dialogue 3" };
        dialoguesPostAnim1 = new string[] { "Dialogue 4", "Dialogue 5", "Dialogue 6" };
        dialoguesPostAnim2 = new string[] { "Dialogue 7", "Dialogue 8", "Dialogue 9" };

        currentStage = Stage.PreAnim1;
        SetCollidersActive(collidersPreAnim1);

        // ȷ���Ի����ı�һ��ʼ�����ص�
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

            // ����������һ��ʱ�������
            DOVirtual.DelayedCall(3f, () =>
            {
                dialogueText.gameObject.SetActive(false);
            });
        }
    }
    private void SetCollidersActive(Collider2D[] activeColliders)
    {
        // �Ƚ�������Collider
        foreach (var col in collidersPreAnim1) col.enabled = false;
        foreach (var col in collidersPostAnim1) col.enabled = false;
        foreach (var col in collidersPostAnim2) col.enabled = false;

        // ���õ�ǰ�׶ε�Collider
        foreach (var col in activeColliders) col.enabled = true;
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
        SetCollidersActive(collidersPostAnim1);
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

            // �л���anim2��Ľ׶�
            currentStage = Stage.PostAnim2;
            SetCollidersActive(collidersPostAnim2);
        }//��� mIsTriggered Ϊ true����ʲôҲ������
    }
 }
