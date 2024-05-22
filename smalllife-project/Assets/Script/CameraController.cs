using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;  //相机移动速度
    public float zoomSpeed = 1f;   //相机缩放速度
    public Vector2 zoomRange = new Vector2(0.1f, 10f);

    private Vector3 initialPosition;
    private Camera mainCamera;
    private bool isDraggingUI = false;
    public Vector2 moveRange = new Vector2(10f, 10f);

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
    }

    void Update()
    {
        CheckCameraMove();
    }

    void CheckCameraMove()
    {
        if (isDraggingUI)
        {
            return;
        }
        if (GameManager.instance.CheckGuiRaycastObject())
        {
            //如果当前点击的位置在GUI对象上，则直接返回，不执行后续的代码
            return;
        }

        // 如果玩家按下鼠标右键，可以拖动画布
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f) * 4f;
            transform.position += mouseDelta * moveSpeed * Time.deltaTime;
        }

        // WASD控制画布的上下左右移动
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;

        // 鼠标滚轮控制画布的放大和缩小
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * zoomSpeed, zoomRange.x, zoomRange.y);

        // 如果玩家按下R键，可以重置画布位置和缩放
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = initialPosition;
            mainCamera.orthographicSize = (zoomRange.x + zoomRange.y) / 2f;
        }

        // 限制画布的移动范围
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -moveRange.x, moveRange.x);
        pos.y = Mathf.Clamp(pos.y, -moveRange.y, moveRange.y);
        transform.position = pos;
    }

    public void MoveCameraToPosition(Vector3 position,float speed)
    {
        //在这里编写摄像机移动的代码，使其移动到position位置
        StartCoroutine(MoveCameraCoroutine(position,speed));
    }

    IEnumerator MoveCameraCoroutine(Vector3 targetPos,float speed)
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0.0f;
        float moveTime = Vector3.Distance(startPos, targetPos) / speed;     //移动时间为1秒，可以根据需要修改

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveTime);
            yield return null;
        }
    }
    public void SetDraggingUI(bool value)
    {
        isDraggingUI = value;
    }
}
