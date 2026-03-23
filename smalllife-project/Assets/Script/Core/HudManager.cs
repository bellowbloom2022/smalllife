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

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"? 场景加载完成：{scene.name}，重新绑定 UI");
    }

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();
    }

    void OnDestroy()
    {
        // ? 记得移除监听，避免重复注册
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
