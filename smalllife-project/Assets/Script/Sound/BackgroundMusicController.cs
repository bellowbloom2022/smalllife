using System.Collections;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public AudioSource musicSource;

    void Start()
    {
        // ��ÿ�������ж��ܷ�������������ֶ���
        DontDestroyOnLoad(gameObject);

        // ���ű�������
        musicSource.Play();
    }

    void Update()
    {
        // ������� ESC ������ͣ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            musicSource.Pause();
        }
        // ����ٴΰ��� ESC ����������������
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            musicSource.UnPause();
        }
    }

    // �������ֽ���
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
