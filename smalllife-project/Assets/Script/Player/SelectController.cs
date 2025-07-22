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
        // ��Ļ����ת����
        cameraRay = Camera.main.ScreenPointToRay(screenPos);
        Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 10);

        if (Physics.Raycast(cameraRay, out cameraHit, 1000))
        {
            GameObject go = cameraHit.transform.gameObject;
            Animator anim = go.GetComponent<Animator>();
            if (anim)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("A0_lunchbox_loop"))
                {
                    Debug.Log(go.name + "click1");
                    anim.SetTrigger("click1");
                }
                else
                {
                    Debug.Log(go.name + "click");
                    anim.SetTrigger("click");
                }

                interactAnimationsClickedCount++;
                if (interactAnimationsClickedCount >= 2)
                    interactAnimationsClickedCount = 2;

                HintMarkController hintMark = go.GetComponentInChildren<HintMarkController>();
                if (hintMark != null) hintMark.HideHint();
            }
        }
    }

    public bool InteractAnimationsClicked
    {
        get { return interactAnimationsClickedCount == 2; }
    }
}
