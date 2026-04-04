using UnityEngine;
using UnityEngine.UI;

public partial class DisplaySettingsController
{
    public void SyncOverlayFromSettings()
    {
        EnsureOverlayToggleListeners();

        int savedIndex = Mathf.Clamp(SaveSystem.GameData.settings.overlayColorIndex, 0, 3);
        ApplyOverlaySelectionByIndex(savedIndex, false);
    }

    private void LoadSavedOverlayColor()
    {
        SyncOverlayFromSettings();
    }

    public void SetOverlayColor(Toggle toggle)
    {
        if (suppressOverlayToggleCallbacks)
            return;

        EnsureOverlayToggleListeners();

        if (toggle == null) return;
        int index = GetToggleIndex(toggle);
        if (index < 0)
        {
            return;
        }

        ApplyOverlaySelectionByIndex(index, true);
    }

    private int GetToggleIndex(Toggle toggle)
    {
        if (toggle == toggleWhite) return 0;
        if (toggle == toggleBeige) return 1;
        if (toggle == toggleGreen) return 2;
        if (toggle == toggleBrown) return 3;
        return -1;
    }

    public Toggle GetToggleByIndex(int index)
    {
        switch (index)
        {
            case 0: return toggleWhite;
            case 1: return toggleBeige;
            case 2: return toggleGreen;
            case 3: return toggleBrown;
            default: return toggleWhite != null ? toggleWhite : toggleBeige;
        }
    }

    private void ApplyOverlaySelectionByIndex(int index, bool save)
    {
        if (index < 0)
            return;

        index = Mathf.Clamp(index, 0, 3);

        UpdateOverlayToggles(index);

        Color selectedColor = GetOverlayColorByIndex(index);

        ApplyOverlayVisualColor(selectedColor);

        if (save && SaveSystem.GameData.settings.overlayColorIndex != index)
        {
            SaveSystem.GameData.settings.overlayColorIndex = index;
            SaveSystem.SaveGame();
        }
    }

    private Color GetOverlayColorByIndex(int index)
    {
        switch (index)
        {
            case 0: return new Color(1f, 1f, 1f, 0f);
            case 1: return new Color(1f, 1f, 0.9f, 0.3f);
            case 2: return new Color(0.85f, 1f, 0.85f, 0.3f);
            case 3: return new Color(0.9f, 0.8f, 0.7f, 0.3f);
            default: return new Color(1f, 1f, 1f, 0f);
        }
    }

    private void UpdateOverlayToggles(int selectedIndex)
    {
        if (toggleWhite != null) toggleWhite.SetIsOnWithoutNotify(selectedIndex == 0);
        if (toggleBeige != null) toggleBeige.SetIsOnWithoutNotify(selectedIndex == 1);
        if (toggleGreen != null) toggleGreen.SetIsOnWithoutNotify(selectedIndex == 2);
        if (toggleBrown != null) toggleBrown.SetIsOnWithoutNotify(selectedIndex == 3);
    }

    private void EnsureOverlayToggleListeners()
    {
        if (colorListenersBound)
            return;

        RegisterOverlayToggleListener(toggleWhite);
        RegisterOverlayToggleListener(toggleBeige);
        RegisterOverlayToggleListener(toggleGreen);
        RegisterOverlayToggleListener(toggleBrown);

        colorListenersBound = true;
    }

    private void RegisterOverlayToggleListener(Toggle toggle)
    {
        if (toggle == null)
            return;

        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn && !suppressOverlayToggleCallbacks) SetOverlayColor(toggle);
        });
    }

    public void BeginDisplayTabActivationSync()
    {
        suppressOverlayToggleCallbacks = true;
        SyncOverlayFromSettings();
    }

    public void EndDisplayTabActivationSync()
    {
        SyncOverlayFromSettings();
        suppressOverlayToggleCallbacks = false;
    }

    private Canvas EnsureRuntimeOverlayCanvas()
    {
        if (runtimeOverlayCanvas != null)
        {
            EnsureOverlayOnTop(runtimeOverlayCanvas);
            return runtimeOverlayCanvas;
        }

        GameObject existingCanvas = GameObject.Find(runtimeOverlayCanvasName);
        if (existingCanvas != null)
        {
            runtimeOverlayCanvas = existingCanvas.GetComponent<Canvas>();
            if (runtimeOverlayCanvas != null)
            {
                GraphicRaycaster existingRaycaster = existingCanvas.GetComponent<GraphicRaycaster>();
                if (existingRaycaster != null)
                {
                    Destroy(existingRaycaster);
                }

                runtimeOverlayImage = existingCanvas.GetComponentInChildren<Image>(true);
                ApplyMultiplyMaterial(runtimeOverlayImage);
                colorOverlayImage = runtimeOverlayImage;
                EnsureOverlayOnTop(runtimeOverlayCanvas);
                return runtimeOverlayCanvas;
            }
        }

        GameObject canvasObject = new GameObject(runtimeOverlayCanvasName, typeof(RectTransform), typeof(Canvas));
        runtimeOverlayCanvas = canvasObject.GetComponent<Canvas>();
        runtimeOverlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.layer = gameObject.layer;
        DontDestroyOnLoad(canvasObject);

        EnsureOverlayOnTop(runtimeOverlayCanvas);
        return runtimeOverlayCanvas;
    }

    private Image RebuildRuntimeOverlayImage(Color selectedColor)
    {
        Canvas overlayCanvas = EnsureRuntimeOverlayCanvas();
        if (overlayCanvas == null)
            return null;

        if (runtimeOverlayImage == null)
        {
            runtimeOverlayImage = overlayCanvas.GetComponentInChildren<Image>(true);
            if (runtimeOverlayImage != null && runtimeOverlayImage.gameObject.name != runtimeOverlayImageName)
            {
                runtimeOverlayImage = null;
            }
        }

        if (runtimeOverlayImage == null)
        {
            GameObject imageObject = new GameObject(runtimeOverlayImageName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(overlayCanvas.transform, false);
            imageObject.layer = overlayCanvas.gameObject.layer;

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            runtimeOverlayImage = imageObject.GetComponent<Image>();
            imageObject.transform.SetAsLastSibling();
        }

        ConfigureRuntimeOverlayImage(runtimeOverlayImage);
        ApplyMultiplyMaterial(runtimeOverlayImage);
        runtimeOverlayImage.color = selectedColor;
        runtimeOverlayImage.enabled = selectedColor.a > 0f;
        runtimeOverlayImage.transform.SetAsLastSibling();

        colorOverlayImage = runtimeOverlayImage;
        return runtimeOverlayImage;
    }

    private void ApplyOverlayVisualColor(Color selectedColor)
    {
        Image overlay = RebuildRuntimeOverlayImage(selectedColor);
        if (overlay == null)
            return;

        ConfigureRuntimeOverlayImage(overlay);
        ApplyMultiplyMaterial(overlay);
        overlay.color = selectedColor;
        overlay.enabled = selectedColor.a > 0f;
    }

    private void ConfigureRuntimeOverlayImage(Image overlay)
    {
        if (overlay == null)
            return;

        overlay.raycastTarget = false;
        overlay.maskable = false;
        overlay.type = Image.Type.Simple;
        overlay.sprite = GetRuntimeOverlaySprite();
    }

    private Sprite GetRuntimeOverlaySprite()
    {
        if (sharedRuntimeOverlaySprite != null)
            return sharedRuntimeOverlaySprite;

        if (sharedRuntimeOverlayTexture == null)
        {
            sharedRuntimeOverlayTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            sharedRuntimeOverlayTexture.SetPixel(0, 0, Color.white);
            sharedRuntimeOverlayTexture.Apply();
            sharedRuntimeOverlayTexture.wrapMode = TextureWrapMode.Clamp;
            sharedRuntimeOverlayTexture.filterMode = FilterMode.Bilinear;
        }

        sharedRuntimeOverlaySprite = Sprite.Create(
            sharedRuntimeOverlayTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );

        return sharedRuntimeOverlaySprite;
    }

    private void ApplyMultiplyMaterial(Image overlay)
    {
        if (overlay == null)
            return;

        if (runtimeOverlayMaterial == null)
        {
            Shader multiplyShader = Shader.Find("UI/MultiplyOverlay");
            if (multiplyShader == null)
                return;

            runtimeOverlayMaterial = new Material(multiplyShader);
            runtimeOverlayMaterial.name = "__OverlayMultiplyRuntimeMat";
        }

        overlay.material = runtimeOverlayMaterial;
    }

    private void EnsureOverlayOnTop(Canvas overlayCanvas)
    {
        if (overlayCanvas == null)
            return;

        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = Mathf.Max(overlayCanvas.sortingOrder, overlayFallbackSortingOrder);
        overlayCanvas.transform.SetAsLastSibling();

        OverlayOrderLocker locker = overlayCanvas.GetComponent<OverlayOrderLocker>();
        if (locker == null)
        {
            locker = overlayCanvas.gameObject.AddComponent<OverlayOrderLocker>();
        }
        locker.Configure(overlayCanvas, overlayFallbackSortingOrder, "WhiteFadePanel");
    }

    private void OnDestroy()
    {
        // Runtime overlay canvas is marked DontDestroyOnLoad, so its material must not be
        // destroyed with a scene-local DisplaySettingsController instance.
    }
}
