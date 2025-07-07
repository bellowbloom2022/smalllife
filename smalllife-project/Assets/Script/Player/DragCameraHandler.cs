using UnityEngine;

public class DragCameraHandler : MonoBehaviour
{
    public float dragSpeed = 10f;
    public Vector2 moveBounds = new Vector2(10f, 10f);

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
        Vector3 movement = new Vector3(-delta.x, -delta.y, 0f) * dragSpeed * Time.deltaTime;
        transform.position += movement;

        // 限制画布移动范围
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -moveBounds.x, moveBounds.x);
        pos.y = Mathf.Clamp(pos.y, -moveBounds.y, moveBounds.y);
        transform.position = pos;
    }
}
