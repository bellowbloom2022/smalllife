using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GoalSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class NamedAudioClip{
        public string name;//用于动画事件中调用的名称
        public AudioClip clip;// 对应的音效资源
    }

    [Header("Sound List")]
    public List<NamedAudioClip> namedClips = new List <NamedAudioClip>();

    private Dictionary<string, AudioClip> clipDict;
    private AudioSource audioSource;
    public SFXZone assignedZone; //手动指定自己属于哪个区域

    void Awake(){
        audioSource = GetComponent<AudioSource>();
        clipDict = new Dictionary<string, AudioClip>();

        foreach (var namedClip in namedClips){
            if (!clipDict.ContainsKey(namedClip.name) && namedClip.clip != null){
                clipDict.Add(namedClip.name, namedClip.clip);
            }
        }
    }

    void Start(){
        if (assignedZone != null){
            assignedZone.RegisterGoalSound(audioSource);
        }
        else{
            Debug.LogWarning($"GoalSoundPlayer on {gameObject.name} has no SFXZone assigned.");
        }
    }

    /// <summary>
    /// 在动画事件中调用，通过名字播放音效
    /// </summary>
    /// <param name="clipName">在 Inspector 中配置的音效名</param>
    public void PlaySoundByName(string clipName){
        if (clipDict == null){
            Debug.LogWarning($"[{name}] 音效字典未初始化");
            return;
        }

        if (clipDict.TryGetValue(clipName, out var clip)){
            audioSource.PlayOneShot(clip);
        }
        else{
            Debug.LogWarning($"[{name}]未找到名为{clipName}的音效");
        }
    }
}
