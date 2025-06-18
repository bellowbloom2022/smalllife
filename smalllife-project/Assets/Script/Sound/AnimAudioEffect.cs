using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AnimAudioEffect : MonoBehaviour
{
    public AudioClip mAudio;           // 需要播放的音效
    public bool mIsTriggered;          // 是否只播放一次
    public bool autoRegisterToSFXZone = true; // 是否自动注册到 SFXZone
    private AudioSource audioSource;   // 本地音频播放器

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (mAudio != null)
        {
            audioSource.clip = mAudio;
        }

        if (autoRegisterToSFXZone)
        {
            RegisterToNearestSFXZone();
        }
    }

    // 播放一次音效（只触发一次）
    public void onAnimTriggerAudioEffect()
    {
        if (!mIsTriggered && mAudio != null)
        {
            mIsTriggered = true;
            audioSource.Play();
        }
    }

    // 每次调用都播放一次音效（可循环用）
    public void onAnimTriggerAudioEffectCicle()
    {
        if (mAudio != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"{name}: mAudio is null, please assign an AudioClip.");
        }
    }

    // 注册到最近的 SFXZone（用于统一音量控制）
    void RegisterToNearestSFXZone()
    {
        SFXZone[] zones = FindObjectsOfType<SFXZone>();
        if (zones.Length == 0) return;

        float minDist = float.MaxValue;
        SFXZone nearestZone = null;
        Vector3 selfPos = transform.position;

        foreach (var zone in zones)
        {
            if (zone.zoneCenter == null) continue;
            float dist = Vector3.Distance(selfPos, zone.zoneCenter.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestZone = zone;
            }
        }

        if (nearestZone != null)
        {
            nearestZone.RegisterGoalSound(audioSource);
        }
    }
}
