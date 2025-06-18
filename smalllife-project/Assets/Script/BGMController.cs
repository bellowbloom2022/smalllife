using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;
    private float originalVolume;

    [Header("Fade Settings")]
    public bool playOnAwakeWithFadeIn = true;
    public float fadeInDuration = 5f;

    private Tween currentTween;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;

        if (playOnAwakeWithFadeIn)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            FadeIn(fadeInDuration);
        }
    }

    public void FadeIn(float duration)
    {
        if (audioSource == null) return;

        audioSource.volume = 0f;
        currentTween?.Kill(); //取消之前的淡入淡出
        currentTween = audioSource.DOFade(originalVolume, duration);
    }

    public void FadeOut(float duration)
    {
        if (audioSource == null) return;

        currentTween?.Kill();
        currentTween = audioSource.DOFade(0f, duration).OnComplete(() =>{
            audioSource.Stop();
            audioSource.volume = originalVolume;//还原音量，供下次用
        });
    }
}
