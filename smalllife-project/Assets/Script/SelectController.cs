using UnityEngine;
using UnityEngine.EventSystems;

public class SelectController : MonoBehaviour
{
    Ray cameraRay;                      //����һ������
    Vector3 mousePos = new Vector3();   //��¼����꣨��Ϊ��Ļ����û��z�����������ǽ�z��Ϊ0��
    RaycastHit cameraHit;

    //public GameObject mPrefabMouseClick;
    private GameManager gameManager;
    private int interactAnimationsClickedCount;

    void Start()
    {
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();    
    }

    void Update()
    {
        CheckMouseClick();
    }
    void CheckMouseClick()
    {
        if (Input.GetMouseButtonDown(0))  //˼·���������������ʱ����������������Ļλ�÷���һ�����߽��м��
        {
            //����GameManager�ű��е�CheckGuiRaycastObjects����
            if (GameManager.instance.CheckGuiRaycastObject())
            {
                //�����ǰ�����λ����GUI�����ϣ���ֱ�ӷ��أ���ִ�к����Ĵ���
                return;
            }
            //���ｫ��Ļ��������λ�ô���һ��vector3����
            mousePos.x = Input.mousePosition.x;
            mousePos.y = Input.mousePosition.y;
            mousePos.z = 0;
            //Ray ray=Camera.main.ScreenPointToRay(Vector3 Pos):����һ������������������淢�侭��Pos�����ߡ�
            cameraRay = Camera.main.ScreenPointToRay(mousePos);
            Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 10);
            //public static bool Raycast(Ray ray, RaycastHit hitInfo, float distance, int layerMask);
            //���������ߣ�outһ��RaycastHit���͵� hitInfo ��Ϣ��float distance�����߳��ȣ�int layerMask��Ҫת�������ƣ����������²���
            if (Physics.Raycast(cameraRay, out cameraHit, 1000))
            {
                GameObject go = cameraHit.transform.gameObject; //���Ǽ�⵽������
                Animator anim = go.GetComponent<Animator>();
                if (anim)
                {
                    if(anim.GetCurrentAnimatorStateInfo(0).IsName("A0_lunchbox_loop"))
                    {
                        Debug.Log(go.name + "click1");
                        anim.SetTrigger("click1");
                        interactAnimationsClickedCount++; // ��ҵ����һ����������
                    }
                    else
                    {
                        Debug.Log(go.name + "click");
                        anim.SetTrigger("click");
                        interactAnimationsClickedCount++; // ��ҵ����һ����������
                    }
                    if (interactAnimationsClickedCount >= 2)
                    {
                        interactAnimationsClickedCount = 2;//��ֹ����������2
                    }
                    //Hide HintMark
                    HintMarkController hintMark = go.GetComponentInChildren<HintMarkController>();
                    if(hintMark != null)
                    {
                        hintMark.HideHint();
                    }
                }
            }
            //��ʾ�����Ч
            //if (mPrefabMouseClick)
            //{
            //mPrefabMouseClick.GetComponent<Transform>().position = mousePos;
            //mPrefabMouseClick.GetComponent<Animator>().SetTrigger("click");
            //}
        }
    }
    public bool InteractAnimationsClicked
    {
        get { return interactAnimationsClickedCount == 2; }
    }
}
