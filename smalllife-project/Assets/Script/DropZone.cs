using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite correctImage;
    public GameObject correctItem;//��ȷ�����

    public bool isDropZoneOccupied;//dropzone�Ƿ��Ѿ���ռ��
    private Draggable draggableItem;
    public Image dropImage;
    public NotebookController notebookController;

    public string photoKey; //��unity�༭����ָ������Ƭ��ֵ
    private GameManager gameManager;//��ȡGameManager������


    private void Awake()
    {
        //��ȡ��Ƭ���Image���
        dropImage = GetComponent<Image>();
        Debug.Log("dropImage: " + dropImage);
    }
    private void Start()
    {
        notebookController = FindObjectOfType<NotebookController>();
        //��ȡGameManagerʵ��
        gameManager = GameManager.instance;
    }
    public void SetPhoto(Sprite photo)
    {
        Debug.Log("Setting Photo.dropImage: " + dropImage + ", photo: " + photo);
        if (photo != null && dropImage != null)
        {
            // ������Ƭ����ʾΪ�������Ƭ
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
        //����ͼƬ�Ƿ��Ѿ���ռ��
        if (isDropZoneOccupied)
        {
            //����Ѿ���ռ�ݣ���ִ���κβ���
            return;
        }
        //��ȡ�϶���Ʒ��Draggable���
        draggableItem = eventData.pointerDrag.GetComponent<Draggable>();
        if (draggableItem != null)
        {
            //��ȡ�϶���Ʒ���ڵ�dropzone
            DropZone dropZone = GetComponent<DropZone>();
            if(dropZone != null && !dropZone.isDropZoneOccupied)
            {
                if(draggableItem.gameObject == correctItem)
                {
                    //�����ק���������ȷ���������ʾ��ȷ��ͼƬ
                    SetPhoto(correctImage);
                    Debug.Log("��ʾ��ȷ��ƬcorrectImage");
                    //���š��̡�����
                    draggableItem.PlayCorrectAnimation();

                    //����dropzoneΪ��ռ��״̬
                    isDropZoneOccupied = true;
                    Debug.Log("��ռ��");

                    //ʹ��GameManager������ȷ��Ƭ��ӳ��,ͬʱ����ƥ�䵽����Ƭ���ݵ�PlayerPrefs
                    gameManager.AddPhotoKeyToImageMapping(photoKey, correctImage);
                    PlayerPrefs.SetInt(photoKey, 1);
                    PlayerPrefs.Save();
                    Debug.Log("����Ƭ�浽gamemanager");

                    Level.ins.AddCount();
                }
                else
                {
                    //�����ק�����������ȷ�������������Ʒ��
                    draggableItem.transform.SetParent(draggableItem.GetStartParent());
                    draggableItem.transform.position = draggableItem.GetStartPosition();
                }
                //����Ʒ�ƶ�����Ƭ��
                draggableItem.transform.SetParent(transform);
                draggableItem.transform.position = GetDropPosition();
            }
            else
            {
                //���dropzone�Ѿ���ռ�ݻ���û���ҵ�dropzone��������Ʒ��
                draggableItem.transform.SetParent(draggableItem.GetStartParent());
                draggableItem.transform.position = draggableItem.GetStartPosition();
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //�����Ƭ���Ƿ��ѱ�ռ��
        if (isDropZoneOccupied)
        {
            return;
        }
        //�����Ƭ��û�б�ռ�ݲ�������ק��Ʒ
        if (!isDropZoneOccupied && eventData.pointerDrag != null)
        {
            //������Ƭ��
            StartCoroutine(LerpColor(dropImage, Color.gray, 0.1f));
            Debug.Log("����ѡ����Ƭ��");
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //����Ƿ�����ק��Ʒ
        if(eventData.pointerDrag != null)
        {
            //û�еĻ���Ƭ���ذ�ɫ
            StartCoroutine(LerpColor(dropImage, Color.white, 0.1f));
            Debug.Log("�뿪��Ƭ��");
        }
        else
        {
            //������Ƭ���Ǳ�ذ�ɫ
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