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
        //isDragging = false;//初始化为false
    }
    public void OnBeginDrag(PointerEventData evenData)
    {
        if (dropZone != null && dropZone.isDropZoneOccupied)
        {
            Debug.Log("已经匹配过了");
            enabled = false;
            return;
        }
        //如果没有匹配，执行正常的拖拽逻辑
        GetComponent<Image>().color = highlightColor;//设置高亮颜色
        cameraController.SetDraggingUI(true);//通知cameracontroller正在拖动UI
        startPosition = transform.position;//记录物品的起始位置
        startParent = transform.parent;//记录物品的初始父级
        transform.SetParent(canvas.transform);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        enabled = true;
    }
    void Update()
    {
        if (isDragging && dropZone != null && dropZone.isDropZoneOccupied)
        {
            Debug.Log("已经匹配过了");
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
        GetComponent<Image>().color = originalColor;//重置材质为原始材质
        cameraController.SetDraggingUI(false);//通知cameracontroller停止拖动UI了
        //transform.SetParent(startParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (eventData.pointerEnter != null)//检查指针进入的物体是否是一个DropZone
        {
            if (eventData.pointerEnter.transform.parent == canvas.transform)//判断指针进入的物体是否是画布
            {
                transform.SetParent(canvas.transform);//将父级设置为画布
                transform.position = startPosition;//将位置设置为初始位置
            }
            else if (eventData.pointerEnter.GetComponentInParent<Canvas>() != null)//判断指针进入的物体是否是UI
            {
                transform.SetParent(startParent);//将父级设置为初始父级
                transform.position = startPosition;//将位置设置为初始位置
            }
            else//检查指针进入的物体是否是一个DropZone
            {
                DropZone dropZone = eventData.pointerEnter.GetComponent<DropZone>();
                if (dropZone != null)
                {
                    //把物件放置到drop zone
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