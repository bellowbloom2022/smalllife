using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudioEffect : MonoBehaviour
{
    public AudioClip mAudio;
    public bool mIsTriggered;

    public void onAnimTriggerAudioEffect() 
    {
        if (!mIsTriggered) 
        {
            mIsTriggered = true;
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
    }

    public void onAnimTriggerAudioEffectCicle()
    {
        if (mAudio != null)
        {
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
        else
        {
            Debug.LogError("mAudio is null! Please assign an AudioClip.");
        }
    }
}
