using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance { get; private set; }

    [Header("Managers")]
    public GameManager gameManager;

    [Header("Global UI References")]
    public PhoneButtonController phoneButtonController;
    public PhoneAlbumPanelController phoneAlbumPanel;
    public DiaryStickerPageController diaryStickerPage;
    // 将来还可以加：PausePanel, SettingsPanel, CongratulatePanel 等

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // ? 注册场景加载事件监听
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
        AutoBindUI();// 初次加载 TitlePage 也要绑定一次
    }
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"? 场景加载完成：{scene.name}，重新绑定 UI");
        AutoBindUI();
    }
    private void AutoBindUI()
    {
        // 自动找场景里的 PhoneButtonController
        if (phoneButtonController == null)
        {
            phoneButtonController = FindObjectOfType<PhoneButtonController>(true);
            if (phoneButtonController != null)
                Debug.Log($"? 自动绑定 PhoneButtonController：{phoneButtonController.gameObject.name}");
            else
                Debug.LogWarning("?? 未找到 PhoneButtonController，请确认场景中有该对象。");
        }
    }

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();
    }

    // ? 提供给其他脚本调用的统一入口
    public void RefreshPhoneRedDot()
    {
        phoneButtonController?.RefreshRedDot();
    }

    public void ShowAlbumPanel()
    {
        phoneAlbumPanel?.Show();
    }
    void OnDestroy()
    {
        // ? 记得移除监听，避免重复注册
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
