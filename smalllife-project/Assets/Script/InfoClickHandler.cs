using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoClickHandler : MonoBehaviour
{
    public GameObject InfoPopup; //��������
    public Image popupImage;  //����ͼƬ
    public Sprite popupSprite;  //����ͼƬ��sprite
    public Button closeButton;  //�رհ�ť

    private void OnMouseUp()
    {
        //���ص�������
        popupImage.sprite = popupSprite;

        //��ʾ����
        InfoPopup.SetActive(true);
    }
    private void Start()
    {
        //���õ����Ĺر�ʱ�䴦�����
        closeButton.onClick.AddListener(ClosePopup);
    }

    private void ClosePopup()
    {
        //���ص���
        InfoPopup.SetActive(false);
    }
}
