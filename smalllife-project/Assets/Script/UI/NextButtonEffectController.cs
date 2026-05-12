using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// NextButton 出现时的动态效果控制器：
/// - 红圈从小放大到包裹按钮，然后持续旋转（Loop）
/// - 三颗星星分别从圈圈周围向左上、右上、右下方向飞出并消失（Loop）
/// - 点击按钮后，红圈和星星以按钮为中心缩小消失
/// </summary>
public class NextButtonEffectController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("红圈图片 (hint-mark.png)")]
    public Image hintCircle;

    [Tooltip("三颗星星图片 (new-star-white.png)，需要3个")]
    public List<Image> stars = new List<Image>();

    [Header("Animation Settings")]
    [Tooltip("红圈从小放大的持续时间")]
    public float circleScaleDuration = 0.4f;

    [Tooltip("红圈旋转一圈的时间")]
    public float circleRotateDuration = 3f;

    [Tooltip("星星单次飞出的持续时间")]
    public float starMoveDuration = 1f;

    [Tooltip("星星飞出的距离")]
    public float starMoveDistance = 60f;

    [Tooltip("星星出现的间隔")]
    public float starSpawnInterval = 0.3f;

    [Tooltip("星星颜色 #F99999")]
    public Color starColor = new Color(0.976f, 0.6f, 0.6f);

    // 星星的三个飞出方向（左上、右上、右下）
    private readonly Vector3[] starDirections = new Vector3[]
    {
        new Vector3(-1, 1, 0),   // 左上
        new Vector3(1, 1, 0),    // 右上
        new Vector3(1, -1, 0)    // 右下
    };

    private Vector3 circleOriginalScale;
    private List<Vector3> starOriginalScales = new List<Vector3>();
    private List<Vector3> starOriginalPositions = new List<Vector3>();
    private Tween circleRotateTween;
    private List<Tween> starTweens = new List<Tween>();
    private bool isPlaying = false;

    private void Awake()
    {
        // 缓存原始状态（先记录，再归零）
        if (hintCircle != null)
        {
            circleOriginalScale = hintCircle.transform.localScale;
            // 如果原始 scale 为零，使用默认 scale
            if (circleOriginalScale.sqrMagnitude < 0.0001f)
                circleOriginalScale = Vector3.one;
            hintCircle.transform.localScale = Vector3.zero;
            hintCircle.gameObject.SetActive(false);
        }

        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] != null)
            {
                Vector3 originalScale = stars[i].transform.localScale;
                if (originalScale.sqrMagnitude < 0.0001f)
                    originalScale = Vector3.one;
                starOriginalScales.Add(originalScale);
                starOriginalPositions.Add(stars[i].transform.localPosition);
                stars[i].color = starColor;
                stars[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 播放出现动画：红圈放大 + 旋转，星星循环飞出
    /// </summary>
    public void PlayAppearEffect()
    {
        if (isPlaying) return;
        isPlaying = true;

        // 红圈：先出现，从小放大到原始大小
        if (hintCircle != null)
        {
            hintCircle.gameObject.SetActive(true);
            hintCircle.transform.localScale = Vector3.zero;
            hintCircle.transform.DOScale(circleOriginalScale, circleScaleDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // 放大完成后开始持续旋转
                    circleRotateTween = hintCircle.transform
                        .DORotate(new Vector3(0, 0, -360), circleRotateDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Restart);
                });
        }

        // 星星：依次开始循环飞出动画
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] == null) continue;

            int index = i;
            float delay = i * starSpawnInterval;

            DOVirtual.DelayedCall(delay + circleScaleDuration * 0.5f, () =>
            {
                PlayStarLoop(index);
            });
        }
    }

    /// <summary>
    /// 单颗星星的循环飞出动画
    /// </summary>
    private void PlayStarLoop(int index)
    {
        if (index < 0 || index >= stars.Count || stars[index] == null) return;
        if (!isPlaying) return;

        Image star = stars[index];
        Vector3 startPos = starOriginalPositions[index];
        Vector3 dir = starDirections[index % starDirections.Length].normalized;
        Vector3 endPos = startPos + dir * starMoveDistance;

        // 重置星星状态
        star.gameObject.SetActive(true);
        star.transform.localPosition = startPos;
        star.transform.localScale = Vector3.zero;
        star.color = new Color(starColor.r, starColor.g, starColor.b, 1f);

        // 创建飞出序列：出现 → 移动+淡出 → 隐藏 → 延迟后重新开始
        Sequence seq = DOTween.Sequence();

        // 0. 快速放大出现
        seq.Append(star.transform.DOScale(starOriginalScales[index], 0.15f).SetEase(Ease.OutBack));

        // 1. 同时移动和淡出
        seq.Join(star.transform.DOLocalMove(endPos, starMoveDuration).SetEase(Ease.OutQuad));
        seq.Join(star.DOFade(0f, starMoveDuration).SetEase(Ease.InQuad));

        // 2. 完成后隐藏并延迟重新开始
        seq.AppendCallback(() =>
        {
            star.gameObject.SetActive(false);
        });

        seq.AppendInterval(starSpawnInterval * (stars.Count - 1));

        seq.AppendCallback(() =>
        {
            if (isPlaying)
            {
                PlayStarLoop(index);
            }
        });

        seq.SetTarget(star.gameObject);
        starTweens.Add(seq);
    }

    /// <summary>
    /// 点击按钮后播放消失动画：所有效果以按钮为中心缩小消失
    /// </summary>
    /// <param name="duration">缩小消失的持续时间</param>
    /// <param name="onComplete">消失完成后的回调</param>
    public void PlayDisappearEffect(float duration, System.Action onComplete)
    {
        isPlaying = false;

        // 停止所有循环动画
        if (circleRotateTween != null && circleRotateTween.IsActive())
        {
            circleRotateTween.Kill();
            circleRotateTween = null;
        }

        foreach (var tween in starTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
        starTweens.Clear();

        DOTween.Kill(hintCircle?.gameObject);
        foreach (var star in stars)
        {
            if (star != null)
                DOTween.Kill(star.gameObject);
        }

        // 红圈缩小消失
        if (hintCircle != null)
        {
            hintCircle.transform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack);
        }

        // 所有星星向中心缩小消失
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] == null) continue;

            stars[i].gameObject.SetActive(true);
            stars[i].transform.DOLocalMove(Vector3.zero, duration)
                .SetEase(Ease.InBack);
            stars[i].transform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack);
        }

        // 延迟后执行回调
        DOVirtual.DelayedCall(duration, () =>
        {
            // 隐藏所有元素
            if (hintCircle != null)
                hintCircle.gameObject.SetActive(false);
            foreach (var star in stars)
            {
                if (star != null)
                    star.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        });
    }

    private void OnDestroy()
    {
        // 清理所有 DOTween 动画
        if (circleRotateTween != null && circleRotateTween.IsActive())
            circleRotateTween.Kill();

        foreach (var tween in starTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
        starTweens.Clear();

        DOTween.Kill(hintCircle?.gameObject);
        foreach (var star in stars)
        {
            if (star != null)
                DOTween.Kill(star.gameObject);
        }
    }
}
