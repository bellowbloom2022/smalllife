using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudioEffect5 : MonoBehaviour
{
    public AudioClip mAudio;
    public bool mIsTriggered;

    public void onAnimTriggerAudioEffect5() 
    {
        if (!mIsTriggered) 
        {
            mIsTriggered = true;
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
    }

    public void onAnimTriggerAudioEffectCicle5()
    {
        AudioManager.ins.PlayAudioEffect(mAudio);
    }
}
