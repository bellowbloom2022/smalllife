using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [HideInInspector]
    public AudioSource mAudioSource;

    public AudioClip mAudioClip1;
    public AudioClip mAudioClip2;
    public static  AudioManager ins;

    void Start()
    {
        if (mAudioSource == null) 
        {
            mAudioSource = GetComponent<AudioSource>();
        }
        ins = this;
    }

    public void PlayGetTrigger1AudioEffect() 
    {
        this.mAudioSource.PlayOneShot(mAudioClip1);
    }
    public void PlayGetTrigger2AudioEffect()
    {
        this.mAudioSource.PlayOneShot(mAudioClip2);
    }

    public void PlayGetTrigger3AudioEffect()
    {
        this.mAudioSource.PlayOneShot(mAudioClip2);
    }

    public void PlayAudioEffect(AudioClip audio) 
    {
        this.mAudioSource.PlayOneShot(audio);
    }
}
