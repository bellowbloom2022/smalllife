using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public Goal[] goals;
    private InputRouter subscribedRouter;

    private void OnEnable()
    {
        InputRouter.InstanceReady += HandleInputRouterReady;
        TrySubscribeToRouter(InputRouter.Instance);
    }

    private void OnDisable()
    {
        InputRouter.InstanceReady -= HandleInputRouterReady;
        UnsubscribeFromRouter();
    }

    private void HandleInputRouterReady(InputRouter router)
    {
        TrySubscribeToRouter(router);
    }

    private void TrySubscribeToRouter(InputRouter router)
    {
        if (router == null || router == subscribedRouter)
            return;

        UnsubscribeFromRouter();
        router.OnClick += HandleClick;
        subscribedRouter = router;
    }

    private void UnsubscribeFromRouter()
    {
        if (subscribedRouter == null)
            return;

        subscribedRouter.OnClick -= HandleClick;
        subscribedRouter = null;
    }

    private void HandleClick(Vector3 screenPosition)
    {
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.ConsumeSuppressedSceneClick())
            return;

        // 单通道：先处理 Goal 的 3D 点击（步骤推进）
        if (TryHandleGoalStepClick(screenPosition))
            return;

        // 再处理 Goal 的 2D 对话热点点击
        if (TryHandleGoalDialogueClick(screenPosition))
            return;

        // 没有点击任何 Goal 时关闭对话框（如有）
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            DialogueManager.Instance.HideDialogue();
    }

    private bool TryHandleGoalStepClick(Vector3 screenPosition)
    {
        if (Camera.main == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
        if (hits == null || hits.Length == 0)
            return false;

        Goal goal = FindNearestGoalFromHits(hits);
        if (goal == null)
            return false;

        goal.OnClicked();
        return true;
    }

    private bool TryHandleGoalDialogueClick(Vector3 screenPosition)
    {
        if (Camera.main == null) return false;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPos);

        foreach (var hitCollider in hitColliders)
        {
            foreach (var goal in goals)
            {
                if (goal.IsMyGoalCollider(hitCollider))
                {
                    goal.HandleClick(hitCollider);
                    return true;
                }
            }
        }

        return false;
    }

    private Goal FindNearestGoalFromHits(RaycastHit[] hits)
    {
        Goal nearestGoal = null;
        float nearestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            Goal goal = hit.transform.GetComponentInParent<Goal>();
            if (goal == null)
                continue;

            if (hit.distance >= nearestDistance)
                continue;

            nearestDistance = hit.distance;
            nearestGoal = goal;
        }

        return nearestGoal;
    }
}
