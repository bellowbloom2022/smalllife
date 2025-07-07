using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 1f;
    public Vector2 zoomRange = new Vector2(0.1f, 10f);
    public Vector2 moveRange = new Vector2(10f, 10f);

    private Camera mainCamera;
    private Vector3 initialPosition;
    private bool isDraggingUI = false;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.CheckGuiRaycastObject())
            return;

        // WASD 控制画布移动
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.position += new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;

        // 滚轮控制缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * zoomSpeed, zoomRange.x, zoomRange.y);

        // R键重置
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = initialPosition;
            mainCamera.orthographicSize = (zoomRange.x + zoomRange.y) / 2f;
        }

        // 限制画布移动范围
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -moveRange.x, moveRange.x);
        pos.y = Mathf.Clamp(pos.y, -moveRange.y, moveRange.y);
        transform.position = pos;
    }

    public void MoveCameraToPosition(Vector3 targetPos, float speed)
    {
        StopAllCoroutines();
        StartCoroutine(MoveCameraCoroutine(targetPos, speed));
    }

    IEnumerator MoveCameraCoroutine(Vector3 targetPos, float speed)
    {
        Vector3 start = transform.position;
        float journey = Vector3.Distance(start, targetPos);
        float startTime = Time.time;

        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.5f, 1),
            new Keyframe(1, 0)
        );

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            float distance = (Time.time - startTime) * speed;
            float frac = distance / journey;
            float eased = curve.Evaluate(frac);
            transform.position = Vector3.Lerp(start, targetPos, eased);
            yield return null;
        }

        transform.position = targetPos;
    }
}
