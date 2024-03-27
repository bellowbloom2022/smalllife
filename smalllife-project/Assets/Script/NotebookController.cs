using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NotebookController : MonoBehaviour
{
    public int currentPage = 1;//��ǰҳ�룻
    public int totalPages;//��ҳ����

    public List<DropZone> dropZones = new List<DropZone>();//dropzones�б�
    public Button previousButton;//��߰�ť
    public Button nextButton;//�ұ߰�ť
    public Text currentPageLeft;//���ҳ���ı�
    public Text currentPageRight;//�ұ�ҳ���ı�

    private int dropZoneCount;
    private int itemsPerPage = 2;

    private GameManager gameManager;

    private void Start()
    {
        dropZoneCount = dropZones.Count;
        //totalpages��dropzones������intemsPerPage�ı���
        totalPages = Mathf.CeilToInt((float)dropZoneCount / (itemsPerPage));
        //��ʼ��ҳ���ı�
        currentPageLeft.text = ((currentPage - 1) * itemsPerPage + 1).ToString();
        currentPageRight.text = ((currentPage - 1) * itemsPerPage + itemsPerPage).ToString();
        ShowPage(currentPage);
        gameManager = GameManager.instance;
        LoadSavedData();//��start�����м����ѱ��ֵ�����
    }
    public void ShowPage(int page)
    {
        int startIndex = (page - 1) * itemsPerPage * 2;
        int endIndex = Mathf.Min(startIndex + itemsPerPage * 2, dropZoneCount);

        for(int i = 0; i < dropZoneCount; i++)
        {
            if(i >= startIndex && i < endIndex)
            {
                dropZones[i].gameObject.SetActive(true);// ʹ�� gameObject.SetActive ������Ϸ����
            }
            else
            {
                dropZones[i].gameObject.SetActive(false);// ʹ�� gameObject.SetActive ������Ϸ����
            }
        }
        currentPage = page;
        currentPageLeft.text = ((currentPage - 1) * 2 + 1).ToString();
        currentPageRight.text = ((currentPage - 1) * 2 + 2).ToString();

        //���°�ť״̬
        previousButton.interactable = currentPage > 1;
        nextButton.interactable = currentPage < totalPages;
    }

    public void OnPreviousButtonClick()
    {
        //����߰�ť�����ʱ������ҳ�벢�����������ߵ�����
        if(currentPage > 1)
        {
            ShowPage(currentPage - 1);
        }
    }
    public void OnNextButtonClick()
    {
        //���ұ߰�ť�����ʱ������ҳ�벢�����������ߵ�����
        if(currentPage < totalPages)
        {
            ShowPage(currentPage + 1);
        }
    }
    private void LoadSavedData()
    {
        Debug.Log("Start LoadSavedData");
        //�������е���Ƭ��
        foreach (DropZone dropZone in dropZones)
        {
            //��ȡ��Ƭ���Ӧ��photoKey
            string photoKey = dropZone.photoKey;
            Debug.Log("��������photokey��dropzone: " + photoKey);

            //���PlayerPrefs���Ƿ���ڶ�Ӧ�ļ�ֵ
            if (PlayerPrefs.HasKey(photoKey))
            {
                //������ڣ����ݼ�ֵ�ж��Ƿ��ѽ�����ƥ����Ƭ
                int value = PlayerPrefs.GetInt(photoKey);
                Debug.Log("PlayerPrefs value for " + photoKey + ": " + value);
                if (value == 1)
                {
                    //�ѽ�����ƥ��
                    Sprite correctImage = dropZone.correctImage;
                    Debug.Log("Setting correctImage for DropZone: " + correctImage); // ��Ӵ����Լ����ȷ����Ƭ·��
                    //������Ƭ�����ʾ
                    dropZone.SetPhoto(correctImage);
                    dropZone.isDropZoneOccupied = true;
                    Debug.Log("DropZone updated. isDropZoneOccupied:" + dropZone.isDropZoneOccupied);
                }
            }
            else
            {
                //�����û���κδ洢�ļ�ֵ������null��״̬
                dropZone.SetPhoto(null);
                dropZone.isDropZoneOccupied = false;
                Debug.Log("No PlayerPrefs value for" + photoKey + ". Setting to default.");
            }
        }
        Debug.Log("End LoadSavedData");
    }
}
