using UnityEngine;

public class DragCameraHandler : MonoBehaviour
{
    public float dragSpeed = 5f;
    public Vector2 moveBounds = new Vector2(10f, 10f);//调整数值的话，需要与camera controller的 move range 完全一致

    private void OnEnable()
    {
        InputRouter.Instance.OnDrag += HandleDrag;
    }

    private void OnDisable()
    {
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnDrag -= HandleDrag;
    }

    private void HandleDrag(Vector3 delta)
    {
        //将屏幕像素差转换为世界坐标差
        Camera cam = Camera.main;
        Vector3 screenDelta = new Vector3(-delta.x, -delta.y, 0f);

        // 注意：需要加上摄像机到画布的距离来获取准确差值（Z 轴）
        Vector3 worldPointBefore = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 worldPointAfter = cam.ScreenToWorldPoint(screenDelta + new Vector3(0, 0, cam.nearClipPlane));
        Vector3 worldDelta = worldPointAfter - worldPointBefore;

        // 应用世界坐标的差值，不再额外乘以 Time.deltaTime
        transform.position += worldDelta * dragSpeed;

        // 限制画布移动范围
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -moveBounds.x, moveBounds.x);
        pos.y = Mathf.Clamp(pos.y, -moveBounds.y, moveBounds.y);
        transform.position = pos;
    }
}
