using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimSoundEventPlayer : MonoBehaviour
{
    public void PlayAnimSound(string tag)
    {
        var src = GetComponent<AudioSource>();
        if (src != null)
        {
            AudioHub.Instance.PlayLocal(src, tag);
        }
    }
}
