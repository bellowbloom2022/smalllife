using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PhoneButtonController : MonoBehaviour
{
    [Header("References")]
    public Button phoneButton;
    [SerializeField] private RedDotController redDot;
    public PhoneAlbumPanelController phoneAlbumPanel; // 直接在 Inspector 里拖进来

    [Header("Animation Settings")]
    public float redDotScaleDuration = 0.3f;
    public float clickFeedbackScale = 0.9f;
    public float clickFeedbackDuration = 0.1f;

    private bool redDotVisible = false;
    private bool hasNewContent = false; // 用于控制动画触发


    void Start()
    {
        if (phoneButton != null)
            phoneButton.onClick.AddListener(OnPhoneButtonClick);

        RefreshRedDot();
    }

    void OnEnable()
    {
        RefreshRedDot();
    }

    /// <summary>
    /// 点击 Phone 按钮
    /// </summary>
    void OnPhoneButtonClick()
    {
        // 按钮反馈动画
        phoneButton.transform.DOScale(clickFeedbackScale, clickFeedbackDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad);

        // 打开相册面板
        phoneAlbumPanel?.Show();

        // 清除红点（标记所有解锁的照片为已查看）
        var album = SaveSystem.GameData.phoneAlbum;
        if (album != null)
        {
            foreach (var photoID in album.unlockedPhotos)
            {
                album.MarkPhotoViewed(photoID);
            }
            SaveSystem.SaveGame();
        }

        HideRedDot();
    }

    /// <summary>
    /// 刷新红点状态（检测是否有未查看照片）
    /// </summary>
    public void RefreshRedDot()
    {
        var album = SaveSystem.GameData?.phoneAlbum;
        // 若无存档或数据为空，直接隐藏红点
        if (album == null || album.unlockedPhotos == null || album.unlockedPhotos.Count == 0)
        {
            HideRedDot();
            return;
        }

        bool hasNew = false;

        // 检查是否存在“已解锁但未查看”的照片
        foreach (var photoID in album.unlockedPhotos)
        {
            if (!album.IsPhotoViewed(photoID))
            {
                hasNew = true;
                break;
            }
        }

        if (hasNew != redDotVisible) // 状态变化时才更新
        {
            if (hasNew) ShowRedDot();
            else HideRedDot();
        }
        // 更新PhoneButton的动画，暗示新内容
        if (hasNew && !hasNewContent)
        {
            hasNewContent = true;
            TriggerButtonAnimation();
        }
    }

    void ShowRedDot()
    {
        if (redDotVisible) return;
        redDotVisible = true;

        if (redDot == null) return;

        redDot.gameObject.SetActive(true);
        redDot.transform.localScale = Vector3.zero;
        redDot.transform.DOScale(1f, redDotScaleDuration)
            .SetEase(Ease.OutBack);
    }

    void HideRedDot()
    {
        if (!redDotVisible) return;
        redDotVisible = false;
        if (redDot == null) return;

        redDot.transform.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // 确保动画完成后再隐藏红点
                redDot.gameObject.SetActive(false);
            });
    }

    // PhoneButton 动画：按钮缩放动画
    private void TriggerButtonAnimation()
    {
        phoneButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 10, 1f).SetEase(Ease.OutBounce);
    }
}
