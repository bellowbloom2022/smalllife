using UnityEngine;

/// <summary>
/// 全局控制音量缩放的控制器，基于摄像机 zoom 缩放或逻辑区域的距离控制。
/// </summary>
public class SFXVolumeController : MonoBehaviour
{
    public static SFXVolumeController Instance;

    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float minVolume = 0.2f;
    public float maxVolume = 1f;

    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// 获取当前 zoom 对应的音量因子（缩放越远音量越小）
    /// </summary>
    public float GetZoomVolumeMultiplier()
    {
        if (mainCamera == null) return 1f;

        float zoom = mainCamera.orthographicSize;
        float t = Mathf.InverseLerp(maxZoom, minZoom, zoom); // zoom 越小越接近 1
        return Mathf.Lerp(minVolume, maxVolume, t);
    }

    /// <summary>
    /// 根据距离获取音量衰减（区域类使用）
    /// </summary>
    public float GetDistanceVolumeMultiplier(Vector3 listenerPos, Vector3 soundPos, float audibleRange = 20f)
    {
        float dist = Vector3.Distance(listenerPos, soundPos);
        float t = Mathf.InverseLerp(audibleRange, 0f, dist);
        return Mathf.Lerp(minVolume, maxVolume, t);
    }
}
