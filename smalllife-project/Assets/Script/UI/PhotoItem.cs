using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class PhotoItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private RedDotController redDot;
    [SerializeField] private GameObject lockMask;

    private Button button;
    private string goalKey;
    public event Action<string> OnPhotoClicked;

    private void Awake()
    {
        // 在子物体中查找 Button（包括未激活的）
        button = GetComponentInChildren<Button>(true);
        if (button != null)
            button.onClick.AddListener(OnClick);
        else
            Debug.LogWarning($"{name} 没有找到 Button 组件！");
    }
    
    public void Setup(string key, Sprite sprite, bool unlocked, bool showRedDot)
    {
        goalKey = key;
        thumbnail.sprite = sprite;
        lockMask.SetActive(!unlocked);
        button.interactable = unlocked;
        // 设置红点显示状态并播放动画
        SetRedDot(showRedDot);
    }

    public void SetRedDot(bool state)
    {
        if (redDot == null)
        {
            Debug.LogWarning("⚠️ RedDotController 未绑定！");
            return;
        }
        
        if (state)
        {
            redDot.Show();  // 显示红点动画
        }
        else
        {
            redDot.Hide();  // 隐藏红点动画
        }
    }

    public void OnClick()
    {
        // 如果被锁定（lockMask激活），不触发点击
        if (lockMask != null && lockMask.activeSelf)
            return;
        
        // ✅ 缩略图点击时添加缩放动画效果（模拟 iPhone 照片放大效果）
        thumbnail.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            thumbnail.transform.DOScale(1f, 0.2f).SetEase(Ease.InBack);  // 恢复原大小
        });

        OnPhotoClicked?.Invoke(goalKey);
        redDot.Hide();
    }
}
