using UnityEngine;
using UnityEngine.EventSystems;

public class SelectController : MonoBehaviour
{
    Ray cameraRay;                      //声明一个射线
    Vector3 mousePos = new Vector3();   //记录将鼠标（因为屏幕坐标没有z，所以下面是将z设为0）
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
        if (Input.GetMouseButtonDown(0))  //思路：当点击鼠标左键的时候，以鼠标在摄像机屏幕位置发射一个射线进行检测
        {
            //调用GameManager脚本中的CheckGuiRaycastObjects方法
            if (GameManager.instance.CheckGuiRaycastObject())
            {
                //如果当前点击的位置在GUI对象上，则直接返回，不执行后续的代码
                return;
            }
            //这里将屏幕坐标的鼠标位置存入一个vector3里面
            mousePos.x = Input.mousePosition.x;
            mousePos.y = Input.mousePosition.y;
            mousePos.z = 0;
            //Ray ray=Camera.main.ScreenPointToRay(Vector3 Pos):返回一条射线由摄像机近裁面发射经过Pos的射线。
            cameraRay = Camera.main.ScreenPointToRay(mousePos);
            Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 10);
            //public static bool Raycast(Ray ray, RaycastHit hitInfo, float distance, int layerMask);
            //物理检测射线，out一个RaycastHit类型的 hitInfo 信息，float distance是射线长度，int layerMask需要转换二进制，所以有如下操作
            if (Physics.Raycast(cameraRay, out cameraHit, 1000))
            {
                GameObject go = cameraHit.transform.gameObject; //这是检测到的物体
                Animator anim = go.GetComponent<Animator>();
                if (anim)
                {
                    if(anim.GetCurrentAnimatorStateInfo(0).IsName("A0_lunchbox_loop"))
                    {
                        Debug.Log(go.name + "click1");
                        anim.SetTrigger("click1");
                        interactAnimationsClickedCount++; // 玩家点击了一个互动动画
                    }
                    else
                    {
                        Debug.Log(go.name + "click");
                        anim.SetTrigger("click");
                        interactAnimationsClickedCount++; // 玩家点击了一个互动动画
                    }
                    if (interactAnimationsClickedCount >= 2)
                    {
                        interactAnimationsClickedCount = 2;//防止计数器超过2
                    }
                    //Hide HintMark
                    HintMarkController hintMark = go.GetComponentInChildren<HintMarkController>();
                    if(hintMark != null)
                    {
                        hintMark.HideHint();
                    }
                }
            }
            //显示鼠标特效
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
