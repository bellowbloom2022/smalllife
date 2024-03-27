using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite correctImage;
    public GameObject correctItem;//正确的物件

    public bool isDropZoneOccupied;//dropzone是否已经被占据
    private Draggable draggableItem;
    public Image dropImage;
    public NotebookController notebookController;

    public string photoKey; //在unity编辑器中指定的照片键值
    private GameManager gameManager;//获取GameManager的引用


    private void Awake()
    {
        //获取照片框的Image组件
        dropImage = GetComponent<Image>();
        Debug.Log("dropImage: " + dropImage);
    }
    private void Start()
    {
        notebookController = FindObjectOfType<NotebookController>();
        //获取GameManager实例
        gameManager = GameManager.instance;
    }
    public void SetPhoto(Sprite photo)
    {
        Debug.Log("Setting Photo.dropImage: " + dropImage + ", photo: " + photo);
        if (photo != null && dropImage != null)
        {
            // 设置照片框显示为传入的照片
            dropImage.sprite = photo;
            Debug.Log("Setting Photo for DropZone: " + photo);
            Debug.Log("dropImage: " + dropImage);
        }
    }
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }
    public void OnDrop(PointerEventData eventData)
    {
        //检查该图片是否已经被占据
        if (isDropZoneOccupied)
        {
            //如果已经被占据，不执行任何操作
            return;
        }
        //获取拖动物品的Draggable组件
        draggableItem = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggableItem != null)
        {
            //获取拖动物品所在的dropzone
            DropZone dropZone = GetComponent<DropZone>();
            if(dropZone != null && !dropZone.isDropZoneOccupied)
            {
                if(draggableItem.gameObject == correctItem)
                {
                    //如果拖拽的物件是正确的物件，显示正确的图片
                    SetPhoto(correctImage);
                    Debug.Log("显示正确照片correctImage");
                    //播放“√”动画
                    draggableItem.PlayCorrectAnimation();

                    //设置dropzone为被占据状态
                    isDropZoneOccupied = true;
                    Debug.Log("被占据");

                    //使用GameManager保存正确照片的映射,同时保存匹配到的照片数据到PlayerPrefs
                    gameManager.AddPhotoKeyToImageMapping(photoKey, correctImage);
                    PlayerPrefs.SetInt(photoKey, 1);
                    PlayerPrefs.Save();
                    Debug.Log("把照片存到gamemanager");

                    Level.ins.AddCount();
                }
                else
                {
                    //如果拖拽的物件不是正确的物件，弹回物品栏
                    draggableItem.transform.SetParent(draggableItem.GetStartParent());
                    draggableItem.transform.position = draggableItem.GetStartPosition();
                }
                //将物品移动到照片框
                draggableItem.transform.SetParent(transform);
                draggableItem.transform.position = GetDropPosition();
            }
            else
            {
                //如果dropzone已经被占据或者没有找到dropzone，弹回物品栏
                draggableItem.transform.SetParent(draggableItem.GetStartParent());
                draggableItem.transform.position = draggableItem.GetStartPosition();
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //检查照片框是否已被占据
        if (isDropZoneOccupied)
        {
            return;
        }
        //如果照片框没有被占据并且有拖拽物品
        if (!isDropZoneOccupied && eventData.pointerDrag != null)
        {
            //高亮照片框
            StartCoroutine(LerpColor(dropImage, Color.gray, 0.1f));
            Debug.Log("正在选择照片框");
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //检查是否有拖拽物品
        if(eventData.pointerDrag != null)
        {
            //没有的话照片框变回白色
            StartCoroutine(LerpColor(dropImage, Color.white, 0.1f));
            Debug.Log("离开照片框");
        }
        else
        {
            //否则照片框还是变回白色
            StartCoroutine(LerpColor(dropImage, Color.white, 0.1f));
        }
    }
    public Vector3 GetDropPosition()
    {
        return transform.position;
    }
    IEnumerator LerpColor(Image image, Color targetColor,float duration)
    {
        float timeElapsed = 0f;
        Color startColor = image.color;
        while (timeElapsed < duration)
        {
            image.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        image.color = targetColor;
    }
}