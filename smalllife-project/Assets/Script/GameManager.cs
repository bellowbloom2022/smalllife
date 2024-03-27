using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    public static GameManager instance;

    public List<Sprite> photoImages = new List<Sprite>();//���е���ƬͼƬ 
    public List<string> photoKeys = new List<string>();//���е���Ƭ��ֵ

    private Dictionary<string, Sprite> photoKeyToImageMap;//��Ƭ��ֵ����ȷͼƬ��ӳ��

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
        //��ʼ��photoKeyToImageMap�ֵ�
        photoKeyToImageMap = new Dictionary<string, Sprite>();
    }
    public Sprite GetImageForKey(string key)
    {
        return photoKeyToImageMap.ContainsKey(key) ? photoKeyToImageMap[key] : null;
    }

    // �����Ƭ��ֵ����ȷͼƬ��ӳ��
    public void AddPhotoKeyToImageMapping(string photoKey, Sprite correctImage)
    {
        if (!photoKeyToImageMap.ContainsKey(photoKey))
        {
            photoKeyToImageMap.Add(photoKey, correctImage);
            photoImages.Add(correctImage);//�����Ƭ���б�
            photoKeys.Add(photoKey);//��Ӽ�ֵ���б�
            Debug.Log("gamemanager���key����Ƭ");
            //�����Ƭ���ı���Ϣ���ֵ�
            //photoKeyToTextMap.Add(photoKey, photoText);
        }
    }
    // ������Ƭ��ֵ����ȷͼƬ��ӳ��
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
            Debug.Log("����photokey");
        }
    }

    //������������ִ��GUI���߼��
    public bool CheckGuiRaycastObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())//��⵱ǰ���ָ���Ƿ���ͣ��GUI������
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
        //ɾ�����д���ļ�ֵ������
        PlayerPrefs.DeleteAll();
    }
}
