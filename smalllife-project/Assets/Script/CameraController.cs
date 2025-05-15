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
        StopAllCoroutines();//��ѡ����ֹ��ε���ʱ��ͻ
        StartCoroutine(MoveCameraCoroutine(position,speed));
    }

    IEnumerator MoveCameraCoroutine(Vector3 targetPos,float speed)
    {
        Vector3 startPos = transform.position;
        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;

        //ʹ��AnimationCurve���ƻ��뻺��
        AnimationCurve easeInOutCurve = new AnimationCurve(
           new Keyframe(0, 0),      // ��ʼ��
           new Keyframe(0.5f, 1),   // �м���ٵ�
           new Keyframe(1, 0)       // ������
        );

        //ֱ���������Ŀ��λ��
        while(Vector3.Distance(transform.position, targetPos) > 0.01f){
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;

            //ʹ�������������ƶ��ļ��ٶ�
            float easedFraction = easeInOutCurve.Evaluate(fractionOfJourney);
            transform.position = Vector3.Lerp(startPos, targetPos, easedFraction);

            yield return null;
        }
        //����ǿ����ΪĿ��λ�ã��������
        transform.position = targetPos;
    }

    public void SetDraggingUI(bool value)
    {
        isDraggingUI = value;
    }
}
