using System.Collections.Generic;
using UnityEngine;

public class SFXZone : MonoBehaviour
{
    [Header("Zone Sound control")]
    public Transform zoneCenter; // �������ģ�������Ϊ������
    public float audibleRange = 20f;// ���볬����ֵ������Ϊ0
    public float minVolume = 0.2f;   // ���������������ȫ����
    public float maxVolume = 1.0f;   // �������ʱ��������

    [Header("Sound List")]
    public List<AudioSource> ambientSources = new List<AudioSource>();// ����������Ч
    public List<AudioSource> goalInteractionSources = new List<AudioSource>(); // ������Ч��goal��

    void Update()
    {
        if (Camera.main == null || zoneCenter == null) return;

        // ����˥�� 
        float dist = Vector3.Distance(Camera.main.transform.position, zoneCenter.position);
        float t = Mathf.InverseLerp(audibleRange, 0f, dist);// ����Խ����t Խ�ӽ�1
        float distanceVolume = Mathf.Lerp(minVolume, maxVolume, t);

        // ���� zoom ˥����������ڿ�������
        float zoomVolume = 1f;
        if (SFXVolumeController.Instance != null)
        {
            zoomVolume = SFXVolumeController.Instance.GetZoomVolumeMultiplier();
        }

        float finalVolume = distanceVolume * zoomVolume;

        // Ӧ������������ AudioSource
        foreach (var s in ambientSources)
        {
            if (s != null) s.volume = finalVolume;
        }

        foreach (var g in goalInteractionSources)
        {
            if (g != null) g.volume = finalVolume;
        }
    }

    // ���ڶ�̬ע�� goal ����Ч
    public void RegisterGoalSound(AudioSource src)
    {
        if (src != null && !goalInteractionSources.Contains(src))
        {
            goalInteractionSources.Add(src);
        }
    }
    //�����Դע�ᾲ̬�������������������
    public static void TryRegister(AudioSource src)
    {
        if (src == null) return;
        var zones = FindObjectsOfType<SFXZone>();
        if (zones.Length == 0) return;

        float minDist = float.MaxValue;
        SFXZone nearest = null;
        foreach (var z in zones)
        {
            if (z.zoneCenter == null) continue;
            float dist = Vector3.Distance(src.transform.position, z.zoneCenter.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = z;
            }
        }
        if (nearest != null) nearest.RegisterGoalSound(src);
    }
}
