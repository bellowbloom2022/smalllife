using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudioEffect1 : MonoBehaviour
{
    public AudioClip mAudio;
    public bool mIsTriggered;

    public void onAnimTriggerAudioEffect1() 
    {
        if (!mIsTriggered) 
        {
            mIsTriggered = true;
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
    }

    public void onAnimTriggerAudioEffectCicle1()
    {
        AudioManager.ins.PlayAudioEffect(mAudio);
    }
}
