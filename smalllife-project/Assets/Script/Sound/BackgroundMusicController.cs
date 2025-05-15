using System.Collections;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public AudioSource musicSource;

    void Start()
    {
        // 在每个场景中都能访问这个背景音乐对象
        DontDestroyOnLoad(gameObject);

        // 播放背景音乐
        musicSource.Play();
    }

    void Update()
    {
        // 如果按下 ESC 键，暂停音乐
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            musicSource.Pause();
        }
        // 如果再次按下 ESC 键，继续播放音乐
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            musicSource.UnPause();
        }
    }

    // 渐变音乐结束
    public void FadeOutMusic(float fadeDuration)
    {
        StartCoroutine(FadeOutCoroutine(fadeDuration));
    }

    IEnumerator FadeOutCoroutine(float fadeDuration)
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;

            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }
}
