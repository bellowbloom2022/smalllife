using System.Collections.Generic;
using UnityEngine;

public class SFXZone : MonoBehaviour
{
    [Header("Zone Sound control")]
    public Transform zoneCenter; // 区域中心，可设置为空物体
    public float audibleRange = 20f;// 距离超过此值则音量为0
    public float minVolume = 0.2f;   // 最低音量，避免完全静音
    public float maxVolume = 1.0f;   // 区域最近时音量上限

    [Header("Sound List")]
    public List<AudioSource> ambientSources = new List<AudioSource>();// 持续环境音效
    public List<AudioSource> goalInteractionSources = new List<AudioSource>(); // 交互音效（goal）

    void Update(){
        if (Camera.main == null || zoneCenter == null) return;

        // 距离衰减 
        float dist = Vector3.Distance(Camera.main.transform.position, zoneCenter.position);
        float t = Mathf.InverseLerp(audibleRange, 0f, dist);// 距离越近，t 越接近1
        float distanceVolume = Mathf.Lerp(minVolume, maxVolume, t);

        // 叠加 zoom 衰减（如果存在控制器）
        float zoomVolume = 1f;
        if (SFXVolumeController.Instance != null)
        {
            zoomVolume = SFXVolumeController.Instance.GetZoomVolumeMultiplier();
        }

        float finalVolume = distanceVolume * zoomVolume;

        // 应用音量到所有 AudioSource
        foreach (var s in ambientSources){
            if (s != null) s.volume = finalVolume;
        }

        foreach (var g in goalInteractionSources){
            if (g != null) g.volume = finalVolume;
        }
    }

    // 用于动态注册 goal 的音效
    public void RegisterGoalSound(AudioSource src){
        if (src != null && !goalInteractionSources.Contains(src)){
            goalInteractionSources.Add(src);
        }
    }
}
