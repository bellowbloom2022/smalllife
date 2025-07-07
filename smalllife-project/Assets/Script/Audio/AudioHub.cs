using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AudioHub : MonoBehaviour
{
    private float sfxVolume = 1f;
    public static AudioHub Instance { get; private set; }

    [System.Serializable]
    public class TaggedAudioClip
    {
        public string tag;
        public AudioClip clip;
    }

    [Header("Global sound effects clip")]
    public List<TaggedAudioClip> audioClips = new List<TaggedAudioClip>();

    private Dictionary<string, AudioClip> clipDict = new();
    private AudioSource globalSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        globalSource = gameObject.AddComponent<AudioSource>();
        globalSource.playOnAwake = false;

        foreach (var tagged in audioClips)
        {
            if (!string.IsNullOrEmpty(tagged.tag) && tagged.clip != null && !clipDict.ContainsKey(tagged.tag))
            {
                clipDict.Add(tagged.tag, tagged.clip);
            }
        }
        //从 PlayerPrefs 读取音效音量
        sfxVolume = PlayerPrefs.GetFloat("Volume_SFX", 1f);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
    }

    public void PlayGlobal(string tag, float volume = 1f)
    {
        if (clipDict.TryGetValue(tag, out var clip))
        {
            globalSource.PlayOneShot(clip, volume * sfxVolume);
        }
    }

    public void PlayLocal(AudioSource source, string tag, float volume = 1f)
    {
        if (source == null) return;
        if (clipDict.TryGetValue(tag, out var clip))
        {
            source.PlayOneShot(clip, volume * sfxVolume);
        }
        else
        {
            Debug.LogWarning($"[AudioHub] not found{tag}");
        }
    }

    public AudioClip GetClip(string tag)
    {
        clipDict.TryGetValue(tag, out var clip);
        return clip;
    }
}
