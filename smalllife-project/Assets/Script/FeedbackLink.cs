using UnityEngine;
using Lean.Localization;
using UnityEngine.SceneManagement;

public class FeedbackLink : MonoBehaviour
{
    [Header("Links")]
    public string feedbackUrl = "https://docs.google.com/forms/d/e/1FAIpQLSfYESa-4SKHDTAcArWJDhHzwpHckBBPd8mBCF3GUIS_BCz8-A/viewform?usp=header";
    public string steamUrl;
    public string qqUrl;
    public string discordUrl;

    [Header("Buttons")]
    public GameObject steamButton;
    public GameObject qqButton;
    public GameObject discordButton;

    private const string ChineseLanguage = "Chinese";
    private const string SteamButtonObjectName = "button_Steam";
    private const string QQButtonObjectName = "button_QQ";
    private const string DiscordButtonObjectName = "button_Discord";
    private static bool runtimeHooksInitialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeRuntimeHooks()
    {
        if (runtimeHooksInitialized)
        {
            return;
        }

        runtimeHooksInitialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        LeanLocalization.OnLocalizationChanged += RefreshButtonsByCurrentLanguage;
        RefreshButtonsByCurrentLanguage();
    }

    private void OnEnable()
    {
        LeanLocalization.OnLocalizationChanged += RefreshButtonsByLanguage;
        RefreshButtonsByLanguage();
    }

    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= RefreshButtonsByLanguage;
    }

    public void OpenSteam()
    {
        OpenUrl(steamUrl);
    }

    public void OpenQQ()
    {
        OpenUrl(qqUrl);
    }

    public void OpenDiscord()
    {
        OpenUrl(discordUrl);
    }

    public void OpenFeedbackForm()
    {
        OpenUrl(feedbackUrl);
    }

    private void RefreshButtonsByLanguage()
    {
        string language = LeanLocalization.GetFirstCurrentLanguage();
        bool handledByInspectorReferences = steamButton != null || qqButton != null || discordButton != null;

        if (!handledByInspectorReferences)
        {
            RefreshButtonsByCurrentLanguage(language);
            return;
        }

        bool isChinese = IsChinese(language);

        if (steamButton != null)
        {
            steamButton.SetActive(true);
        }

        if (qqButton != null)
        {
            qqButton.SetActive(isChinese);
        }

        if (discordButton != null)
        {
            discordButton.SetActive(!isChinese);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshButtonsByCurrentLanguage();
    }

    private static void RefreshButtonsByCurrentLanguage()
    {
        RefreshButtonsByCurrentLanguage(LeanLocalization.GetFirstCurrentLanguage());
    }

    private static void RefreshButtonsByCurrentLanguage(string language)
    {
        bool isChinese = IsChinese(language);

        GameObject steam = FindSceneObjectByName(SteamButtonObjectName);
        GameObject qq = FindSceneObjectByName(QQButtonObjectName);
        GameObject discord = FindSceneObjectByName(DiscordButtonObjectName);

        if (steam != null)
        {
            steam.SetActive(true);
        }

        if (qq != null)
        {
            qq.SetActive(isChinese);
        }

        if (discord != null)
        {
            discord.SetActive(!isChinese);
        }
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
#if UNITY_2023_1_OR_NEWER
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        Transform[] allTransforms = Object.FindObjectsOfType<Transform>(true);
#endif

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (t != null && t.name == objectName)
            {
                return t.gameObject;
            }
        }

        return null;
    }

    private static bool IsChinese(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return false;
        }

        if (string.Equals(language, ChineseLanguage, System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return language.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase);
    }

    private static void OpenUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }
}
