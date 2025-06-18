using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GoalSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class NamedAudioClip{
        public string name;//���ڶ����¼��е��õ�����
        public AudioClip clip;// ��Ӧ����Ч��Դ
    }

    [Header("Sound List")]
    public List<NamedAudioClip> namedClips = new List <NamedAudioClip>();

    private Dictionary<string, AudioClip> clipDict;
    private AudioSource audioSource;
    public SFXZone assignedZone; //�ֶ�ָ���Լ������ĸ�����

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
    /// �ڶ����¼��е��ã�ͨ�����ֲ�����Ч
    /// </summary>
    /// <param name="clipName">�� Inspector �����õ���Ч��</param>
    public void PlaySoundByName(string clipName){
        if (clipDict == null){
            Debug.LogWarning($"[{name}] ��Ч�ֵ�δ��ʼ��");
            return;
        }

        if (clipDict.TryGetValue(clipName, out var clip)){
            audioSource.PlayOneShot(clip);
        }
        else{
            Debug.LogWarning($"[{name}]δ�ҵ���Ϊ{clipName}����Ч");
        }
    }
}
