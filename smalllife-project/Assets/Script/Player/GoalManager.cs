using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public Goal[] goals;
    [Header("Click Raycast")]
    [SerializeField] private LayerMask stepClickLayerMask = ~0;
    [SerializeField] private LayerMask dialogueClickLayerMask = ~0;
    [SerializeField] private int max3DHits = 16;
    [SerializeField] private int max2DHits = 16;

    private InputRouter subscribedRouter;
    private Camera cachedMainCamera;
    private RaycastHit[] raycastHitsBuffer;
    private Collider2D[] overlap2DBuffer;

    private void OnEnable()
    {
        EnsureBuffers();
        TryRefreshMainCamera();
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
        if (!TryRefreshMainCamera()) return false;

        EnsureBuffers();

        Ray ray = cachedMainCamera.ScreenPointToRay(screenPosition);
        int hitCount = Physics.RaycastNonAlloc(ray, raycastHitsBuffer, 1000f, stepClickLayerMask);
        if (hitCount <= 0)
            return false;

        Goal goal = FindNearestGoalFromHits(raycastHitsBuffer, hitCount);
        if (goal == null)
            return false;

        goal.OnClicked();
        return true;
    }

    private bool TryHandleGoalDialogueClick(Vector3 screenPosition)
    {
        if (!TryRefreshMainCamera()) return false;

        EnsureBuffers();

        Vector3 worldPos = cachedMainCamera.ScreenToWorldPoint(screenPosition);
        Vector2 worldPoint = new Vector2(worldPos.x, worldPos.y);
        int colliderCount = Physics2D.OverlapPointNonAlloc(worldPoint, overlap2DBuffer, dialogueClickLayerMask);
        if (colliderCount <= 0)
            return false;

        for (int i = 0; i < colliderCount; i++)
        {
            Collider2D hitCollider = overlap2DBuffer[i];
            if (hitCollider == null)
                continue;

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

    private Goal FindNearestGoalFromHits(RaycastHit[] hits, int hitCount)
    {
        Goal nearestGoal = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
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

    private void EnsureBuffers()
    {
        int safe3DSize = Mathf.Max(1, max3DHits);
        if (raycastHitsBuffer == null || raycastHitsBuffer.Length != safe3DSize)
            raycastHitsBuffer = new RaycastHit[safe3DSize];

        int safe2DSize = Mathf.Max(1, max2DHits);
        if (overlap2DBuffer == null || overlap2DBuffer.Length != safe2DSize)
            overlap2DBuffer = new Collider2D[safe2DSize];
    }

    private bool TryRefreshMainCamera()
    {
        if (cachedMainCamera == null)
            cachedMainCamera = Camera.main;

        return cachedMainCamera != null;
    }
}
