using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public Goal[] goals;
    private float clickCoolDown = 0.1f; //100ms����ʱ��
    private float lastClickTime = -1f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //�����������������������
            if (Time.time - lastClickTime < clickCoolDown) return;
            lastClickTime = Time.time;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hitColliders = Physics2D.OverlapPointAll(mousePosition);

            // �ȼ���Ƿ��жԻ��� �� ����У�������
            if (DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.HideDialogue();
                return;
            }

            // ��鱻�����Collider���ҵ������ĸ�Goal
            foreach (var hitCollider in hitColliders)
            {
                foreach (var goal in goals)
                {
                    if (goal.IsMyGoalCollider(hitCollider))
                    {
                        goal.HandleClick(hitCollider);
                        return; // �ҵ���ͷ��أ������δ���
                    }
                }
            }
            //3.���������Ĳ����κ�Goal ?? ���˿հ� ?? �ص��Ի�
            DialogueManager.Instance.HideDialogue();
        }
    }
}
