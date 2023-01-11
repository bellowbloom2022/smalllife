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
    // Start is called before the first frame update
    void Start()
    {
        if (mAudioSource == null) {
            mAudioSource = GetComponent<AudioSource>();
        }
        ins = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGetTrigger1AudioEffect() {
        this.mAudioSource.PlayOneShot(mAudioClip1);
    }
    public void PlayGetTrigger2AudioEffect()
    {
        this.mAudioSource.PlayOneShot(mAudioClip2);
    }

    public void PlayAudioEffect(AudioClip audio) {
        this.mAudioSource.PlayOneShot(audio);
    }
}
