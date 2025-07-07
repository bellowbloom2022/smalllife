using UnityEngine;

/// <summary>
/// ȫ�ֿ����������ŵĿ���������������� zoom ���Ż��߼�����ľ�����ơ�
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
    /// ��ȡ��ǰ zoom ��Ӧ���������ӣ�����ԽԶ����ԽС��
    /// </summary>
    public float GetZoomVolumeMultiplier()
    {
        if (mainCamera == null) return 1f;

        float zoom = mainCamera.orthographicSize;
        float t = Mathf.InverseLerp(maxZoom, minZoom, zoom); // zoom ԽСԽ�ӽ� 1
        return Mathf.Lerp(minVolume, maxVolume, t);
    }

    /// <summary>
    /// ���ݾ����ȡ����˥����������ʹ�ã�
    /// </summary>
    public float GetDistanceVolumeMultiplier(Vector3 listenerPos, Vector3 soundPos, float audibleRange = 20f)
    {
        float dist = Vector3.Distance(listenerPos, soundPos);
        float t = Mathf.InverseLerp(audibleRange, 0f, dist);
        return Mathf.Lerp(minVolume, maxVolume, t);
    }
}
