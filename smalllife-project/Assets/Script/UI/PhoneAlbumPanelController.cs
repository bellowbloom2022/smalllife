using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;
using DG.Tweening;

public class PhoneAlbumPanelController : BasePanel
{
    [Header("Data")]
    public LevelDataAsset currentLevel;

    [Header("UI References")]
    public GameObject panel;               // 整个面板
    public Transform photoGridParent;   // ScrollView Content
    public GameObject photoItemPrefab;  // 缩略图 prefab
    public Button closeButton;
    public GameObject photoViewerPanel; // 大图面板
    public Image largePhotoImage;       // 大图展示 Image
    public Text monologueText;      // 碎碎念文本
    public GameObject monologueBox;         // Monologue box（碎碎念框）
    [SerializeField] private Button backButton;
    
    [Header("Animation Settings")]
    public float photoViewerScaleDuration = 0.25f; // ✅ 新增动画时长
    public Ease photoViewerEaseIn = Ease.OutCubic;
    public Ease photoViewerEaseOut = Ease.InCubic;
    private bool isAnimating = false;

    [Header("Settings")]
    public int maxPhotosPerLevel = 10;  // 可选，防止溢出
    public float photoAppearDuration = 0.5f;  // Photo显示的动画时长
    public float buttonDelay = 1f;            // 延迟显示BackButton的时间
    public float monologueDelay = 2f;         // 延迟显示MonologueBox的时间

    private PhoneAlbumData albumData;
    private Dictionary<string, PhotoItem> photoItems = new();
    // 防止重复生成
    private bool hasInitialized = false;

    private void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(ClosePhotoViewer);
        backButton.gameObject.SetActive(false);      // 初始时BackButton不可见
        monologueBox.SetActive(false);               // 初始时MonologueBox不可见
    }

    public override void Show()
    {
        if (isAnimating) return; // 防止多次调用
        isAnimating = true;

        base.Show();
        LoadAlbumData();
        RefreshPhotoLibrary();

        if (!hasInitialized)
        {
            hasInitialized = true;
            SetupCloseButton();
        }
        // 从下往上滑入动画
        panel.transform.localPosition = new Vector3(0, -Screen.height, 0); // 设置初始位置在屏幕下方
        panel.SetActive(true);
        panel.transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutSine).OnComplete(() => isAnimating = false);
    }
    
    public override void Hide()
    {
        if (isAnimating) return; // 防止多次调用
        isAnimating = true;
        // 从上往下滑出动画
        panel.transform.DOLocalMoveY(Screen.height, 0.3f).SetEase(Ease.InSine).OnComplete(() =>
        {
            panel.SetActive(false);
            isAnimating = false;
        });
        base.Hide();
    }

    private void LoadAlbumData()
    {
        if (SaveSystem.GameData.phoneAlbum == null)
            SaveSystem.GameData.phoneAlbum = new PhoneAlbumData();

        albumData = SaveSystem.GameData.phoneAlbum;
    }
    private void SetupCloseButton()
    {
        if (closeButton == null)
        {
            Debug.LogWarning("⚠️ PhoneAlbumPanelController: CloseButton 未绑定。");
            return;
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Hide();

            // ✅ 安全刷新红点
            if (HudManager.Instance != null && HudManager.Instance.phoneButtonController != null)
            {
                HudManager.Instance.phoneButtonController.RefreshRedDot();
            }
        });
    }

    /// <summary>
    /// 动态生成相册缩略图列表
    /// </summary>
    private void RefreshPhotoLibrary()
    {
        if (photoGridParent == null || photoItemPrefab == null)
        {
            Debug.LogError("❌ PhotoGridParent 或 PhotoItemPrefab 未赋值！");
            return;
        }

        // 清空旧内容
        for (int i = photoGridParent.childCount - 1; i >= 0; i--)
            Destroy(photoGridParent.GetChild(i).gameObject);

        photoItems.Clear();

        int count = Mathf.Min(currentLevel.goalTotal, maxPhotosPerLevel);

        for (int i = 0; i < count; i++)
        {
            string goalKey = $"{currentLevel.levelID}_{currentLevel.goalIDs[i]}";
            Sprite photoSprite = (i < currentLevel.photoImages.Length) ? currentLevel.photoImages[i] : null;

            if (photoSprite == null) continue;

            GameObject go = Instantiate(photoItemPrefab, photoGridParent);
            PhotoItem item = go.GetComponent<PhotoItem>();

            bool unlocked = albumData.IsPhotoUnlocked(goalKey);
            bool viewed = albumData.IsPhotoViewed(goalKey);

            Debug.Log($"📸 生成照片: {goalKey}, unlocked={unlocked}, viewed={viewed}");
            item.Setup(goalKey, photoSprite, unlocked, !viewed);
            item.OnPhotoClicked += OnPhotoClicked;

            photoItems[goalKey] = item;
        }
        Debug.Log($"📸 相册刷新完毕：生成 {photoItems.Count} 张照片");
    }

    /// <summary>
    /// 点击缩略图 → 打开大图 + 碎碎念
    /// </summary>
    private void OnPhotoClicked(string goalKey)
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
        if (!photoItems.ContainsKey(goalKey)) return;

        var (photoSprite, monologueKey) = GetPhotoContent(goalKey);
        if (photoSprite == null) return;

        largePhotoImage.sprite = photoSprite;// 设置大图
        monologueText.text = LeanLocalization.GetTranslationText(monologueKey);// 设置碎碎念文本
        // ✅ 播放放大动画（模拟 iPhone 相册）
        photoViewerPanel.SetActive(true);
        photoViewerPanel.transform.localScale = Vector3.one * 0.85f;
        photoViewerPanel.transform.DOScale(1f, photoViewerScaleDuration).SetEase(photoViewerEaseIn);
        // 延迟1秒显示BackButton
        Invoke("ShowBackButton", buttonDelay);

        // 延迟2秒显示MonologueBox
        Invoke("ShowMonologueBox", monologueDelay);

        // 清除红点 + 保存 viewed 状态
        if (!albumData.viewedPhotos.Contains(goalKey))
        {
            albumData.viewedPhotos.Add(goalKey);
            SaveSystem.SaveGame();

            if (photoItems.TryGetValue(goalKey, out var item))
                item.SetRedDot(false);
        }
    }
    private void ShowBackButton()
    {
        backButton.gameObject.SetActive(true);
    }
    private void ShowMonologueBox()
    {
        monologueBox.SetActive(true);
    }

    public void ClosePhotoViewer()
    {
        if (photoViewerPanel == null) return;
        backButton.gameObject.SetActive(false);
        monologueBox.SetActive(false);

        photoViewerPanel.transform.DOScale(0.85f, 0.2f)
            .SetEase(photoViewerEaseOut)
            .OnComplete(() => photoViewerPanel.SetActive(false));
    }

    /// <summary>
    /// 根据 goalKey 获取对应照片与文本
    /// </summary>
    private (Sprite, string) GetPhotoContent(string goalKey)
    {
        for (int i = 0; i < currentLevel.goalTotal; i++)
        {
            string currentKey = $"{currentLevel.levelID}_{currentLevel.goalIDs[i]}";
            if (currentKey == goalKey)
            {
                Sprite sprite = (i < currentLevel.photoImages.Length) ? currentLevel.photoImages[i] : null;
                string monoKey = (i < currentLevel.photoMonologueKeys.Length) ? currentLevel.photoMonologueKeys[i] : "";
                return (sprite, monoKey);
            }
        }
        return (null, "");
    }
}
