using UnityEngine;
using UnityEngine.EventSystems;

public class SelectController : MonoBehaviour
{
    Ray cameraRay;                      //����һ������
    Vector3 mousePos = new Vector3();   //��¼����꣨��Ϊ��Ļ����û��z�����������ǽ�z��Ϊ0��
    RaycastHit cameraHit;

    private int interactAnimationsClickedCount;

    private void OnEnable()
    {
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnClick += HandleClick;
    }

    private void OnDisable()
    {
        if (InputRouter.Instance != null)
            InputRouter.Instance.OnClick -= HandleClick;
    }
    
    void HandleClick(Vector3 screenPos)
    {
        cameraRay = Camera.main.ScreenPointToRay(screenPos);
        Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 10);

        if (Physics.Raycast(cameraRay, out cameraHit, 1000))
        {
            GameObject hitObj = cameraHit.transform.gameObject;

            // ===== ① 优先处理 Goal（新增）=====
            Goal goal = hitObj.GetComponentInParent<Goal>();
            if (goal != null)
            {
                Debug.Log(hitObj.name + " Goal Click");
                goal.OnClicked();

                // 👉 保留你原有的点击计数逻辑
                interactAnimationsClickedCount++;
                if (interactAnimationsClickedCount >= 2)
                    interactAnimationsClickedCount = 2;

                // 👉 保留 HintMark 隐藏
                HintMarkController hintMark = hitObj.GetComponentInChildren<HintMarkController>();
                if (hintMark != null) hintMark.HideHint();

                return; // ⚠️ 非常重要：Goal 不再往下走 Animator
            }

            // ===== ② 普通物体（原逻辑，基本不动）=====
            Animator anim = hitObj.GetComponent<Animator>();
            if (anim)
            {
                Debug.Log(hitObj.name + " click");
                anim.SetTrigger("click");

                interactAnimationsClickedCount++;
                if (interactAnimationsClickedCount >= 2)
                    interactAnimationsClickedCount = 2;

                HintMarkController hintMark = hitObj.GetComponentInChildren<HintMarkController>();
                if (hintMark != null) hintMark.HideHint();
            }
        }
    }


    public bool InteractAnimationsClicked
    {
        get { return interactAnimationsClickedCount == 2; }
    }
}
