using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Goal2 : MonoBehaviour
{
    // Goal �ĺ�������
    public int goalID; // Ψһ��ʶ��
    public bool isFound; // �Ƿ����ҵ�
    public GameObject mGameObjectNovel; // С˵����
    public Collider2D step1Collider; // Step1 ��Ӧ�� BoxCollider
    public Collider2D step2Collider; // Step2 ��Ӧ�� BoxCollider

    // ������λ����Ϣ
    private Animator animator;
    public GameObject mNovelPosStart; // С˵��ʼλ��
    public GameObject mNovelPosMid; // С˵�м�λ��
    public GameObject mNovelPos; // С˵����λ��
    public GameObject mCamPosA; // �����Ŀ��λ�� A
    public GameObject mCamPosB; // �����Ŀ��λ�� B
    public Canvas mCanvas;

    // ������ƶ�����
    private CameraController cameraController;
    public bool mMoveCamera; // �Ƿ��ƶ������
    public float mCamMoveSpeedA = 3f; // �ƶ��� A ����ٶ�
    public float mCamMoveSpeedB = 3f; // �ƶ��� B ����ٶ�

    // UI �ɾͶ���
    public GameObject goalAchievePrefab; // �ɾͶ���Ԥ����
    public Sprite goalImage; // �ɾ�ͼƬ
    private Animator goalAchieveAnimator;
    private Image achieveImage;

    // �׶ο���
    private enum Stage { PreAnim1, PostAnim1, PostAnim2 }
    private Stage currentStage;

    // �Ի������
    public Collider2D[] collidersPreAnim1; // Anim1 ǰ�Ĵ�����
    public Collider2D[] collidersPostAnim1; // Anim1 ��Ĵ�����
    public Collider2D[] collidersPostAnim2; // Anim2 ��Ĵ�����
    public GameObject[] dialogueSpritesPreAnim1; // Anim1 ǰ�ĶԻ���
    public GameObject[] dialogueSpritesPostAnim1; // Anim1 ��ĶԻ���
    public GameObject[] dialogueSpritesPostAnim2; // Anim2 ��ĶԻ���
    public GameObject dialogueTextBox; // �Ի�����ʾ����
    private GameObject activeDialogueSprite;

    private bool mIsTriggered; // �Ƿ��Ѿ���������

    private void Start()
    {
        animator = GetComponent<Animator>();
        cameraController = FindObjectOfType<CameraController>();

        // ��ʼ�� UI �Ͷ���
        GameObject goalAchieveInstance = Instantiate(goalAchievePrefab, mCanvas.transform);
        goalAchieveAnimator = goalAchieveInstance.GetComponent<Animator>();
        achieveImage = goalAchieveInstance.transform.Find("goalimage").GetComponent<Image>();
        achieveImage.sprite = goalImage;

        currentStage = Stage.PreAnim1;
        HideAllDialogueSprites();
        DisableAllColliders();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

            if (activeDialogueSprite != null)
            {
                HideActiveDialogueSprite();
            }
            else if (hitCollider != null)
            {
                HandleStageDialogue(hitCollider);
            }
        }
    }
    public void EnableStep1Collider()
    {
        if (step1Collider != null)
            step1Collider.enabled = true;
    }

    public void DisableStep1Collider()
    {
        if (step1Collider != null)
            step1Collider.enabled = false;
    }
    private void HandleStageDialogue(Collider2D hitCollider)
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
        dialogueTextBox.SetActive(true);
        activeDialogueSprite = dialogueSprite;
        activeDialogueSprite.SetActive(true);
    }

    private void HideActiveDialogueSprite()
    {
        if (activeDialogueSprite != null)
        {
            activeDialogueSprite.SetActive(false);
            activeDialogueSprite = null;
            dialogueTextBox.SetActive(false);
        }
    }

    private void HideAllDialogueSprites()
    {
        foreach (var sprite in dialogueSpritesPreAnim1) sprite.SetActive(false);
        foreach (var sprite in dialogueSpritesPostAnim1) sprite.SetActive(false);
        foreach (var sprite in dialogueSpritesPostAnim2) sprite.SetActive(false);
        activeDialogueSprite = null;
        dialogueTextBox.SetActive(false);
    }

    private void DisableAllColliders()
    {
        if (step1Collider != null) step1Collider.enabled = false;
        if (step2Collider != null) step2Collider.enabled = false;
    }

    public void SetStep1ColliderState(bool state)
    {
        if (step1Collider != null) step1Collider.enabled = state;
    }

    public void SetStep2ColliderState(bool state)
    {
        if (step2Collider != null) step2Collider.enabled = state;
    }

    private void OnAnim1End()
    {
        DisableAllColliders();

        if (goalAchieveAnimator != null)
        {
            goalAchieveAnimator.SetTrigger("goal_step1achieve");
        }

        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosA.transform.position, mCamMoveSpeedA);
        }

        currentStage = Stage.PostAnim1;
    }

    private void OnAnim2End()
    {
        if (mIsTriggered) return;

        if (goalAchieveAnimator != null)
        {
            goalAchieveAnimator.SetTrigger("goal_step2achieve");
        }

        if (cameraController != null && mMoveCamera)
        {
            cameraController.MoveCameraToPosition(mCamPosB.transform.position, mCamMoveSpeedB);
        }

        PlayNovelMovementSequence();
        currentStage = Stage.PostAnim2;
    }

    private void PlayNovelMovementSequence()
    {
        Vector3 startScreenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, startScreenPos, null, out var startPos);

        RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
        rectPenZai.anchoredPosition = startPos;

        Vector3 midScreenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, midScreenPos, null, out var midPos);

        mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.3f);
        rectPenZai.DOLocalMove(midPos, 0.4f).onComplete = () =>
        {
            mGameObjectNovel.transform.DOScale(Vector3.one, 0.3f);
            RectTransform r = mNovelPos.transform as RectTransform;
            Vector2 endPos = r.anchoredPosition;

            rectPenZai.DOLocalMove(endPos, 0.4f).onComplete = () =>
            {
                rectPenZai.GetComponent<Animator>().SetTrigger("click");
                Level.ins.AddCount();
                isFound = true;
            };
        };
    }
}
