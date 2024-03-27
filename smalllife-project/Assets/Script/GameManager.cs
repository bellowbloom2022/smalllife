using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    public static GameManager instance;

    public List<Sprite> photoImages = new List<Sprite>();//所有的照片图片 
    public List<string> photoKeys = new List<string>();//所有的照片键值

    private Dictionary<string, Sprite> photoKeyToImageMap;//照片键值和正确图片的映射

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        //初始化photoKeyToImageMap字典
        photoKeyToImageMap = new Dictionary<string, Sprite>();
    }
    public Sprite GetImageForKey(string key)
    {
        return photoKeyToImageMap.ContainsKey(key) ? photoKeyToImageMap[key] : null;
    }

    // 添加照片键值和正确图片的映射
    public void AddPhotoKeyToImageMapping(string photoKey, Sprite correctImage)
    {
        if (!photoKeyToImageMap.ContainsKey(photoKey))
        {
            photoKeyToImageMap.Add(photoKey, correctImage);
            photoImages.Add(correctImage);//添加照片到列表
            photoKeys.Add(photoKey);//添加键值到列表
            Debug.Log("gamemanager添加key和照片");
            //添加照片的文本信息到字典
            //photoKeyToTextMap.Add(photoKey, photoText);
        }
    }
    // 更新照片键值和正确图片的映射
    public void UpdatePhotoKey(string photoFrameName, Sprite newPhotoKey)
    {
        if (string.IsNullOrEmpty(photoFrameName))
        {
            Debug.LogError("Invalid photoFrameName!");
            return;
        }

        if (photoKeyToImageMap.ContainsKey(photoFrameName))
        {
            photoKeyToImageMap[photoFrameName] = newPhotoKey;
            Debug.Log("更新photokey");
        }
    }

    //公共方法用来执行GUI射线检测
    public bool CheckGuiRaycastObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())//检测当前鼠标指针是否悬停在GUI对象上
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void ResetGame()
    {
        //删除所有储存的键值对数据
        PlayerPrefs.DeleteAll();
    }
}
