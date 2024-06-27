using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudioEffect3 : MonoBehaviour
{
    public AudioClip mAudio;
    public bool mIsTriggered;

    public void onAnimTriggerAudioEffect3() {
        if (!mIsTriggered) {
            mIsTriggered = true;
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
    }

    public void onAnimTriggerAudioEffectCicle3()
    {
        AudioManager.ins.PlayAudioEffect(mAudio);
    }
}
