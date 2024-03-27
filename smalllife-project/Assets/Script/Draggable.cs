using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isDragging = true;
    private RectTransform rectTransfrom;
    private Vector3 startPosition;
    private Transform startParent;
    private CameraController cameraController;
    private Canvas canvas;

    private Color originalColor;
    private Color highlightColor = new Color(204f / 255f, 102f / 255f, 102f / 255f);

    private Animator animator;
    public DropZone dropZone;

    private void Awake()
    {
        rectTransfrom = GetComponent<RectTransform>();
        startParent = transform.parent;
        cameraController = FindObjectOfType<CameraController>();
        canvas = GetComponentInParent<Canvas>();
        originalColor = GetComponent<Image>().color;
        animator = GetComponent<Animator>();
        //isDragging = false;//��ʼ��Ϊfalse
    }
    public void OnBeginDrag(PointerEventData evenData)
    {
        if (dropZone != null && dropZone.isDropZoneOccupied)
        {
            Debug.Log("�Ѿ�ƥ�����");
            enabled = false;
            return;
        }
        //���û��ƥ�䣬ִ����������ק�߼�
        GetComponent<Image>().color = highlightColor;//���ø�����ɫ
        cameraController.SetDraggingUI(true);//֪ͨcameracontroller�����϶�UI
        startPosition = transform.position;//��¼��Ʒ����ʼλ��
        startParent = transform.parent;//��¼��Ʒ�ĳ�ʼ����
        transform.SetParent(canvas.transform);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        enabled = true;
    }
    void Update()
    {
        if (isDragging && dropZone != null && dropZone.isDropZoneOccupied)
        {
            Debug.Log("�Ѿ�ƥ�����");
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            enabled = false;
        }
        else if (isDragging && dropZone != null && !dropZone.isDropZoneOccupied)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            transform.position = canvas.transform.TransformPoint(localPoint);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<Image>().color = originalColor;//���ò���Ϊԭʼ����
        cameraController.SetDraggingUI(false);//֪ͨcameracontrollerֹͣ�϶�UI��
        //transform.SetParent(startParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (eventData.pointerEnter != null)//���ָ�����������Ƿ���һ��DropZone
        {
            if (eventData.pointerEnter.transform.parent == canvas.transform)//�ж�ָ�����������Ƿ��ǻ���
            {
                transform.SetParent(canvas.transform);//����������Ϊ����
                transform.position = startPosition;//��λ������Ϊ��ʼλ��
            }
            else if (eventData.pointerEnter.GetComponentInParent<Canvas>() != null)//�ж�ָ�����������Ƿ���UI
            {
                transform.SetParent(startParent);//����������Ϊ��ʼ����
                transform.position = startPosition;//��λ������Ϊ��ʼλ��
            }
            else//���ָ�����������Ƿ���һ��DropZone
            {
                DropZone dropZone = eventData.pointerEnter.GetComponent<DropZone>();
                if (dropZone != null)
                {
                    //��������õ�drop zone
                    transform.SetParent(dropZone.transform);
                    transform.position = dropZone.GetDropPosition();
                }
                else
                {
                    transform.SetParent(startParent);
                    transform.position = startPosition;
                }
            }
        }
        else
        {
            transform.SetParent(startParent);
            transform.position = startPosition;
        }
        //enabled = false;
    }
    public Vector3 GetStartPosition()
    {
        return startPosition;
    }
    public Transform GetStartParent()
    {
        return startParent;
    }
    public void PlayCorrectAnimation()
    {
        animator.Play("correct");
    }
}