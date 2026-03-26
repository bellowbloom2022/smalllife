using UnityEngine;

public class SelectController : MonoBehaviour
{
    Ray cameraRay;                      //����һ������
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

            // Goal 点击统一由 GoalManager 处理，这里只处理普通物体。
            Animator anim = hitObj.GetComponent<Animator>();
            if (anim)
            {
                if (!HasTrigger(anim, "click"))
                    return;

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

    private bool HasTrigger(Animator anim, string triggerName)
    {
        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                return true;
        }

        return false;
    }
}
