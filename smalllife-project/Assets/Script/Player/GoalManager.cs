using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public Goal[] goals;
    private float clickCoolDown = 0.1f; // 100ms����ʱ��
    private float lastClickTime = -1f;

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

    private void HandleClick(Vector3 screenPosition)
    {
        if (Time.time - lastClickTime < clickCoolDown) return;
        lastClickTime = Time.time;

        // ����жԻ����ȹر�
        if (DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.HideDialogue();
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPos);

        foreach (var hitCollider in hitColliders)
        {
            foreach (var goal in goals)
            {
                if (goal.IsMyGoalCollider(hitCollider))
                {
                    goal.HandleClick(hitCollider);
                    return;
                }
            }
        }
        // û�е���κ�Ŀ��ʱ�رնԻ������У�
        DialogueManager.Instance.HideDialogue();
    }
}
