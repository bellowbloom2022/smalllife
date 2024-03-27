using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NotebookController : MonoBehaviour
{
    public int currentPage = 1;//当前页码；
    public int totalPages;//总页数；

    public List<DropZone> dropZones = new List<DropZone>();//dropzones列表
    public Button previousButton;//左边按钮
    public Button nextButton;//右边按钮
    public Text currentPageLeft;//左边页码文本
    public Text currentPageRight;//右边页码文本

    private int dropZoneCount;
    private int itemsPerPage = 2;

    private GameManager gameManager;

    private void Start()
    {
        dropZoneCount = dropZones.Count;
        //totalpages是dropzones数量的intemsPerPage的倍数
        totalPages = Mathf.CeilToInt((float)dropZoneCount / (itemsPerPage));
        //初始化页码文本
        currentPageLeft.text = ((currentPage - 1) * itemsPerPage + 1).ToString();
        currentPageRight.text = ((currentPage - 1) * itemsPerPage + itemsPerPage).ToString();
        ShowPage(currentPage);
        gameManager = GameManager.instance;
        LoadSavedData();//在start方法中加载已保持的数据
    }
    public void ShowPage(int page)
    {
        int startIndex = (page - 1) * itemsPerPage * 2;
        int endIndex = Mathf.Min(startIndex + itemsPerPage * 2, dropZoneCount);

        for(int i = 0; i < dropZoneCount; i++)
        {
            if(i >= startIndex && i < endIndex)
            {
                dropZones[i].gameObject.SetActive(true);// 使用 gameObject.SetActive 激活游戏对象
            }
            else
            {
                dropZones[i].gameObject.SetActive(false);// 使用 gameObject.SetActive 激活游戏对象
            }
        }
        currentPage = page;
        currentPageLeft.text = ((currentPage - 1) * 2 + 1).ToString();
        currentPageRight.text = ((currentPage - 1) * 2 + 2).ToString();

        //更新按钮状态
        previousButton.interactable = currentPage > 1;
        nextButton.interactable = currentPage < totalPages;
    }

    public void OnPreviousButtonClick()
    {
        //当左边按钮被点击时，更新页码并更新左右两边的内容
        if(currentPage > 1)
        {
            ShowPage(currentPage - 1);
        }
    }
    public void OnNextButtonClick()
    {
        //当右边按钮被点击时，更新页码并更新左右两边的内容
        if(currentPage < totalPages)
        {
            ShowPage(currentPage + 1);
        }
    }
    private void LoadSavedData()
    {
        Debug.Log("Start LoadSavedData");
        //遍历所有的照片框
        foreach (DropZone dropZone in dropZones)
        {
            //获取照片框对应的photoKey
            string photoKey = dropZone.photoKey;
            Debug.Log("正在填入photokey到dropzone: " + photoKey);

            //检查PlayerPrefs中是否存在对应的键值
            if (PlayerPrefs.HasKey(photoKey))
            {
                //如果存在，根据键值判断是否已解锁或匹配照片
                int value = PlayerPrefs.GetInt(photoKey);
                Debug.Log("PlayerPrefs value for " + photoKey + ": " + value);
                if (value == 1)
                {
                    //已解锁或匹配
                    Sprite correctImage = dropZone.correctImage;
                    Debug.Log("Setting correctImage for DropZone: " + correctImage); // 添加此行以检查正确的照片路径
                    //更新照片框的显示
                    dropZone.SetPhoto(correctImage);
                    dropZone.isDropZoneOccupied = true;
                    Debug.Log("DropZone updated. isDropZoneOccupied:" + dropZone.isDropZoneOccupied);
                }
            }
            else
            {
                //如果还没有任何存储的键值，保持null的状态
                dropZone.SetPhoto(null);
                dropZone.isDropZoneOccupied = false;
                Debug.Log("No PlayerPrefs value for" + photoKey + ". Setting to default.");
            }
        }
        Debug.Log("End LoadSavedData");
    }
}
