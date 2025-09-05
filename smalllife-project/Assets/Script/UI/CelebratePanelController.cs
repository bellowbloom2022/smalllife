using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class CelebratePanelController : MonoBehaviour
{
    public BasePanel basePanel;                 // 可选：预制体上有的话会被自动获取
    public TextMeshProUGUI goalText;
    public Button apartmentButton;

    [Header("Animation Settings")]
    public float panelStartDelay = 0.1f;
    public float stepDelay = 0.05f;
    public float scaleFactor = 1.5f;

    [Header("Apartment Highlight")]
    public Color apartmentHighlightColor = new Color(1f, 0.95f, 0.5f, 1f);
    private Coroutine apartmentHighlightCoroutine;
    [SerializeField] private UnityEngine.UI.Graphic apartmentGraphic; // 可在 Inspector 指定（更通用）
    private Color apartmentOriginalColor;

    private Coroutine playCoroutine;

    private void Awake()
    {
        if (basePanel == null) basePanel = GetComponent<BasePanel>();
        // 尝试自动查找子控件（允许在 Inspector 覆盖）
        if (goalText == null)
        {
            var gt = transform.Find("GoalText");
            if (gt != null) goalText = gt.GetComponent<TextMeshProUGUI>();
        }
        if (apartmentButton == null)
        {
            var ab = transform.Find("ApartmentButton");
            if (ab != null) apartmentButton = ab.GetComponent<Button>();
        }
        if (apartmentButton != null)
        {
            if (apartmentGraphic == null)
                apartmentGraphic = apartmentButton.targetGraphic;
            if (apartmentGraphic == null)
                apartmentGraphic = apartmentButton.GetComponentInChildren<UnityEngine.UI.Graphic>(true);
            if (apartmentGraphic != null)
                apartmentOriginalColor = apartmentGraphic.color;
            Debug.Log($"CelebratePanelController Awake: apartmentButton={(apartmentButton != null)}, apartmentGraphic={(apartmentGraphic != null)}");
        }
    }

    private void OnDestroy()
    {
        // 停止正在播放的计数协程
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        // 停止 apartment highlight
        StopApartmentHighlight();
        // 解绑 apartmentButton 的回调，避免悬挂引用
        if (apartmentButton != null)
            apartmentButton.onClick.RemoveAllListeners();
    }

    public void BindApartment(UnityAction action)
    {
        if (apartmentButton == null) return;
        apartmentButton.onClick.RemoveAllListeners();
        // 先执行外部行为（例如 Level.OnApartmentButtonClicked），再停止高亮并销毁面板
        apartmentButton.onClick.AddListener(() =>
        {
            action?.Invoke();
            StopApartmentHighlight();
            Destroy(gameObject);
        });
    }

    public void Show()
    {
        if (basePanel != null) basePanel.Show();
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (basePanel != null) basePanel.Hide();
        else gameObject.SetActive(false);
    }

    // 从 0 开始播放到 count，并在每次数字增加时播放动画（和按钮）
    public void ShowAndPlay(int count, int total)
    {
        if (goalText != null) goalText.text = $"0/{total}";
        if (playCoroutine != null) StopCoroutine(playCoroutine);
        playCoroutine = StartCoroutine(PlayCountSequence(count, total));
    }

    private IEnumerator PlayCountSequence(int count, int total)
    {
        yield return new WaitForSeconds(panelStartDelay);

        int displayed = 0;
        count = Mathf.Clamp(count, 0, total);

        while (displayed < count)
        {
            displayed++;
            if (goalText != null) goalText.text = $"{displayed}/{total}";

            // 播放 goalText 动画 + 音效
            if (goalText != null)
                yield return StartCoroutine(AnimateGoalText());

            // 播放 apartmentButton 动画（可选）
            if (apartmentButton != null)
                yield return StartCoroutine(AnimateButton(apartmentButton));

            yield return new WaitForSeconds(stepDelay);
        }

        playCoroutine = null;

        // 播放完计数后开始 apartment 的高亮提示
        StartApartmentHighlight();
    }

    private IEnumerator AnimateGoalText()
    {
        AudioHub.Instance.PlayGlobal("bubble");
        if (goalText == null) yield break;

        Vector3 original = goalText.transform.localScale;
        Vector3 target = original * scaleFactor;
        float dur = Mathf.Max(0.05f, stepDelay);

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            goalText.transform.localScale = Vector3.Lerp(original, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            goalText.transform.localScale = Vector3.Lerp(target, original, t / dur);
            yield return null;
        }
        goalText.transform.localScale = original;
    }

    private IEnumerator AnimateButton(Button button)
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
        if (button == null) yield break;

        RectTransform rt = button.transform as RectTransform;
        Vector3 original = rt.localScale;
        Vector3 target = original * scaleFactor;
        float dur = Mathf.Max(0.05f, stepDelay);

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(original, target, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(target, original, t / dur);
            yield return null;
        }
        rt.localScale = original;
    }

    public void StartApartmentHighlight()
    {
        Debug.Log("StartApartmentHighlight called");
        if (apartmentButton == null)
        {
            Debug.LogWarning("StartApartmentHighlight: apartmentButton is null");
            return;
        }
        if (apartmentGraphic == null)
        {
            Debug.LogWarning("StartApartmentHighlight: apartmentGraphic is null - trying GetComponentInChildren");
            apartmentGraphic = apartmentButton.GetComponentInChildren<UnityEngine.UI.Graphic>(true);
            if (apartmentGraphic != null) apartmentOriginalColor = apartmentGraphic.color;
        }
        // 立即设置一次颜色以便验证（可移除）
        if (apartmentGraphic != null)
            apartmentGraphic.color = apartmentHighlightColor;
        else
        {
            // 回退：修改 Button 的 colors.normalColor 以尽量可见
            var cb = apartmentButton.colors;
            cb.normalColor = apartmentHighlightColor;
            apartmentButton.colors = cb;
        }
        if (apartmentHighlightCoroutine != null) StopCoroutine(apartmentHighlightCoroutine);
        apartmentHighlightCoroutine = StartCoroutine(PulseApartment());
    }

    public void StopApartmentHighlight()
    {
        if (apartmentHighlightCoroutine != null)
        {
            StopCoroutine(apartmentHighlightCoroutine);
            apartmentHighlightCoroutine = null;
        }
        if (apartmentGraphic != null)
            apartmentGraphic.color = apartmentOriginalColor;
        else
        {
            var cb = apartmentButton.colors;
            cb.normalColor = apartmentOriginalColor;
            apartmentButton.colors = cb;
        }
    }
    private IEnumerator PulseApartment()
    {
        // 只做颜色脉冲（不缩放）使用 apartmentGraphic（若无则退出）
        var g = apartmentGraphic;
        if (g == null)
            yield break;
        while (true)
        {
            float p = (Mathf.Sin(Time.time * Mathf.PI) * 0.5f + 0.5f); // 0..1
            g.color = Color.Lerp(apartmentOriginalColor, apartmentHighlightColor, p * 0.5f);
            yield return null;
        }
    }
}