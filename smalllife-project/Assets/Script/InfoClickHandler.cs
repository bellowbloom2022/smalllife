using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoClickHandler : MonoBehaviour
{
    public GameObject InfoPopup; //弹窗对象
    public Image popupImage;  //弹窗图片
    public Sprite popupSprite;  //弹窗图片的sprite
    public Button closeButton;  //关闭按钮

    private void OnMouseUp()
    {
        //加载弹窗内容
        popupImage.sprite = popupSprite;

        //显示弹窗
        InfoPopup.SetActive(true);
    }
    private void Start()
    {
        //设置弹窗的关闭时间处理程序
        closeButton.onClick.AddListener(ClosePopup);
    }

    private void ClosePopup()
    {
        //隐藏弹窗
        InfoPopup.SetActive(false);
    }
}
