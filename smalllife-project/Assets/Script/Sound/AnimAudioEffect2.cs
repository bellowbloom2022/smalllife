using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudioEffect2 : MonoBehaviour
{
    public AudioClip mAudio;
    public bool mIsTriggered;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void onAnimTriggerAudioEffect2() {
        if (!mIsTriggered) {
            mIsTriggered = true;
            AudioManager.ins.PlayAudioEffect(mAudio);
        }
    }

    public void onAnimTriggerAudioEffectCicle2()
    {
        AudioManager.ins.PlayAudioEffect(mAudio);
    }
}
