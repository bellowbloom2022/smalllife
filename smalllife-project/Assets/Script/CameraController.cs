using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;  //����ƶ��ٶ�
    public float zoomSpeed = 1f;   //��������ٶ�
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
            //�����ǰ�����λ����GUI�����ϣ���ֱ�ӷ��أ���ִ�к����Ĵ���
            return;
        }

        // �����Ұ�������Ҽ��������϶�����
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f) * 4f;
            transform.position += mouseDelta * moveSpeed * Time.deltaTime;
        }

        // WASD���ƻ��������������ƶ�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;

        // �����ֿ��ƻ����ķŴ����С
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * zoomSpeed, zoomRange.x, zoomRange.y);

        // �����Ұ���R�����������û���λ�ú�����
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = initialPosition;
            mainCamera.orthographicSize = (zoomRange.x + zoomRange.y) / 2f;
        }

        // ���ƻ������ƶ���Χ
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -moveRange.x, moveRange.x);
        pos.y = Mathf.Clamp(pos.y, -moveRange.y, moveRange.y);
        transform.position = pos;
    }

    public void MoveCameraToPosition(Vector3 position,float speed)
    {
        //�������д������ƶ��Ĵ��룬ʹ���ƶ���positionλ��
        StartCoroutine(MoveCameraCoroutine(position,speed));
    }

    IEnumerator MoveCameraCoroutine(Vector3 targetPos,float speed)
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0.0f;
        float moveTime = Vector3.Distance(startPos, targetPos) / speed;     //�ƶ�ʱ��Ϊ1�룬���Ը�����Ҫ�޸�

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
