using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudioEffect4 : MonoBehaviour
{
    public AudioClip mAudio;
    public bool mIsTriggered;

    public void onAnimTriggerAudioEffect4() 
    {
        if (!mIsTriggered) 
        {
            mIsTriggered = true;
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
    }

    public void onAnimTriggerAudioEffectCicle4()
    {
        AudioManager.ins.PlayAudioEffect(mAudio);
    }
}
