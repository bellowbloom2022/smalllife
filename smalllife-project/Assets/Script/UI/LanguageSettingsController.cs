using UnityEngine;
using Lean.Localization;
using UnityEngine.UI;
using System.Collections.Generic;

public class LanguageSettingsController : MonoBehaviour
{
    private enum MarkSide
    {
        Left,
        Right
    }

    [Header("Selected State Marks")]
    [SerializeField] private bool createRuntimeCheckmarks = true;
    [SerializeField] private Sprite selectedMarkSprite;
    [SerializeField] private Color checkmarkColor = new Color(0.92f, 0.34f, 0.31f, 1f);
    [SerializeField] private Vector2 markSize = new Vector2(26f, 26f);
    [SerializeField] private float markEdgePadding = 32f;
    [SerializeField] private MarkSide markSide = MarkSide.Right;
    [SerializeField] private bool showBothSides = false;
    [SerializeField] private bool mirrorRightMark = false;

    private const string ChineseCode = "Chinese";
    private const string EnglishCode = "English";
    private const string JapaneseCode = "Japanese";

    private readonly Dictionary<string, RuntimeMarkPair> runtimeMarks = new Dictionary<string, RuntimeMarkPair>();
    private bool hasLoggedMissingSprite = false;

    private struct RuntimeMarkPair
    {
        public Image Left;
        public Image Right;
    }

    private void OnEnable()
    {
        EnsureRuntimeCheckmarks();
        RefreshSelectedLanguageVisual();
    }

    /// <summary>
    /// 点击语言按钮时调用。会自动设置语言并保存。
    /// </summary>
    public void SetLanguage(string langCode)
    {
        if (SaveSystem.GameData.settings.language == langCode)
        {
            RefreshSelectedLanguageVisual();
            return;
        }

        SaveSystem.GameData.settings.language = langCode;
        GameSettingsApplier.ApplyLanguage(langCode, true);
        RefreshSelectedLanguageVisual();
    }

    /// <summary>
    /// 在启动或从存档恢复时调用，自动设置语言
    /// </summary>
    public void ApplySavedLanguage()
    {
        string savedLang = SaveSystem.GameData.settings.language;
        if (!string.IsNullOrEmpty(savedLang))
        {
            GameSettingsApplier.ApplyLanguage(savedLang, false);
        }

        RefreshSelectedLanguageVisual();
    }

    private void RefreshSelectedLanguageVisual()
    {
        string selectedLanguage = SaveSystem.GameData.settings.language;
        if (string.IsNullOrEmpty(selectedLanguage))
            return;

        SetMarkActive(ChineseCode, selectedLanguage == ChineseCode);
        SetMarkActive(EnglishCode, selectedLanguage == EnglishCode);
        SetMarkActive(JapaneseCode, selectedLanguage == JapaneseCode);
    }

    private void EnsureRuntimeCheckmarks()
    {
        if (!createRuntimeCheckmarks)
            return;

        if (selectedMarkSprite == null)
        {
            if (!hasLoggedMissingSprite)
            {
                Debug.LogWarning("LanguageSettingsController: selectedMarkSprite is not assigned.");
                hasLoggedMissingSprite = true;
            }
            return;
        }

        EnsureLanguageMark(ChineseCode);
        EnsureLanguageMark(EnglishCode);
        EnsureLanguageMark(JapaneseCode);
    }

    private void EnsureLanguageMark(string languageCode)
    {
        if (runtimeMarks.ContainsKey(languageCode))
            return;

        Transform option = FindChildRecursive(transform, languageCode);
        if (option == null)
        {
            GameObject fallback = GameObject.Find(languageCode);
            if (fallback != null)
                option = fallback.transform;
        }

        if (option == null)
            return;

        RuntimeMarkPair pair = new RuntimeMarkPair
        {
            Left = showBothSides || markSide == MarkSide.Left
                ? EnsureMark(option, "__SelectedMarkLeft", true)
                : null,
            Right = showBothSides || markSide == MarkSide.Right
                ? EnsureMark(option, "__SelectedMarkRight", false)
                : null
        };

        runtimeMarks.Add(languageCode, pair);
    }

    private Image EnsureMark(Transform parent, string markName, bool isLeft)
    {
        Transform existing = parent.Find(markName);
        if (existing != null)
        {
            Image existingImage = existing.GetComponent<Image>();
            if (existingImage != null)
                return existingImage;
        }

        GameObject markGo = new GameObject(markName, typeof(RectTransform), typeof(Image));
        markGo.transform.SetParent(parent, false);
        markGo.transform.SetAsLastSibling();

        RectTransform rect = markGo.GetComponent<RectTransform>();
        rect.anchorMin = isLeft ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = markSize;
        rect.anchoredPosition = isLeft ? new Vector2(markEdgePadding, 0f) : new Vector2(-markEdgePadding, 0f);

        if (!isLeft && mirrorRightMark)
        {
            rect.localScale = new Vector3(-1f, 1f, 1f);
        }

        Image image = markGo.GetComponent<Image>();
        image.sprite = selectedMarkSprite;
        image.color = checkmarkColor;
        image.preserveAspect = true;
        image.raycastTarget = false;

        return image;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildRecursive(root.GetChild(i), childName);
            if (result != null)
                return result;
        }

        return null;
    }

    private void SetMarkActive(string languageCode, bool active)
    {
        if (!runtimeMarks.TryGetValue(languageCode, out RuntimeMarkPair pair))
            return;

        if (pair.Left != null)
            pair.Left.gameObject.SetActive(active);
        if (pair.Right != null)
            pair.Right.gameObject.SetActive(active);
    }
}
