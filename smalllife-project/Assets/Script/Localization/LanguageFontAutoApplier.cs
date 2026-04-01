using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LanguageFontAutoApplier : MonoBehaviour
{
    private const string ChineseLanguage = "Chinese";
    private const string JapaneseLanguage = "Japanese";

    private const string ChineseFontResourcePath = "Fonts/NotoSansSC-Light";
    private const string JapaneseFontResourcePath = "Fonts/NotoSansCJKjp-Light";



    private static LanguageFontAutoApplier instance;
    private static Font chineseFont;
    private static Font japaneseFont;
    private static TMP_FontAsset chineseTmpFont;
    private static TMP_FontAsset japaneseTmpFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) 
        {
            return;
        }

        var existing = FindObjectOfType<LanguageFontAutoApplier>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        var go = new GameObject("LanguageFontAutoApplier");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<LanguageFontAutoApplier>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        LeanLocalization.OnLocalizationChanged += ApplyCurrentLanguageFont;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyCurrentLanguageFont();
    }

    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= ApplyCurrentLanguageFont;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCurrentLanguageFont();
    }

    private void ApplyCurrentLanguageFont()
    {
        string language = LeanLocalization.GetFirstCurrentLanguage();
        if (string.IsNullOrEmpty(language))
        {
            return;
        }

        var uiFont = ResolveUiFont(language);
        var tmpFont = ResolveTmpFont(language);
        if (uiFont == null || tmpFont == null)
        {
            return;
        }

        var tmpTexts = FindAll<TMP_Text>();
        for (int i = 0; i < tmpTexts.Length; i++)
        {
            var text = tmpTexts[i];
            if (text != null)
            {
                text.font = tmpFont;
            }
        }

        var legacyTexts = FindAll<Text>();
        for (int i = 0; i < legacyTexts.Length; i++)
        {
            var text = legacyTexts[i];
            if (text != null)
            {
                text.font = uiFont;
            }
        }
    }

    private static Font ResolveUiFont(string language)
    {
        EnsureFontsLoaded();
        return IsJapanese(language) ? japaneseFont : chineseFont;
    }

    private static TMP_FontAsset ResolveTmpFont(string language)
    {
        EnsureFontsLoaded();
        return IsJapanese(language) ? japaneseTmpFont : chineseTmpFont;
    }

    private static bool IsJapanese(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            return false;
        }

        if (string.Equals(language, JapaneseLanguage, System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return language.StartsWith("ja", System.StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureFontsLoaded()
    {
        if (chineseFont == null)
        {
            chineseFont = Resources.Load<Font>(ChineseFontResourcePath);
        }

        if (japaneseFont == null)
        {
            japaneseFont = Resources.Load<Font>(JapaneseFontResourcePath);
        }

        if (chineseFont != null && chineseTmpFont == null)
        {
            chineseTmpFont = TMP_FontAsset.CreateFontAsset(chineseFont);
        }

        if (japaneseFont != null && japaneseTmpFont == null)
        {
            japaneseTmpFont = TMP_FontAsset.CreateFontAsset(japaneseFont);
        }

        if (chineseFont == null)
        {
            Debug.LogWarning("[LanguageFontAutoApplier] Missing Chinese font in Resources/Fonts/NotoSansSC-Light.");
        }

        if (japaneseFont == null)
        {
            Debug.LogWarning("[LanguageFontAutoApplier] Missing Japanese font in Resources/Fonts/NotoSansCJKjp-Light.");
        }
    }

    private static T[] FindAll<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        return FindObjectsOfType<T>(true);
#endif
    }
}
