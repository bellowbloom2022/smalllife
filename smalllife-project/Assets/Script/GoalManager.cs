using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public Goal[] goals;
    private float clickCoolDown = 0.1f; //100ms防抖时间
    private float lastClickTime = -1f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //防抖处理：点击间隔过短则忽略
            if (Time.time - lastClickTime < clickCoolDown) return;
            lastClickTime = Time.time;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hitColliders = Physics2D.OverlapPointAll(mousePosition);

            // 先检查是否有对话框 → 如果有，先隐藏
            if (DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.HideDialogue();
                return;
            }

            // 检查被点击的Collider，找到属于哪个Goal
            foreach (var hitCollider in hitColliders)
            {
                foreach (var goal in goals)
                {
                    if (goal.IsMyGoalCollider(hitCollider))
                    {
                        goal.HandleClick(hitCollider);
                        return; // 找到后就返回，避免多次触发
                    }
                }
            }
            //3.如果点击到的不是任何Goal ?? 点了空白 ?? 关掉对话
            DialogueManager.Instance.HideDialogue();
        }
    }
}
