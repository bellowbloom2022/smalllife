using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责 Goal Note 面板相关的镜头移动和 focus target 查询
/// 职责: 镜头聚焦、focus target 定位、camera controller 交互
/// </summary>
public class GoalNoteCameraFocusController : MonoBehaviour
{
    private const float DefaultCameraMoveDuration = 0.8f;
    private readonly Dictionary<int, Goal> goalById = new Dictionary<int, Goal>();

    private Camera cachedMainCamera;
    private CameraController cachedCameraController;

    /// <summary>
    /// 将镜头移动到指定 Goal 的 step focus target
    /// </summary>
    public void MoveCameraToGoalFocusTarget(int goalID, GoalNoteStep step)
    {
        Goal targetGoal = FindGoalById(goalID);
        if (targetGoal == null)
        {
            Debug.LogWarning("[GoalNoteCameraFocus] Goal not found in scene. goalID=" + goalID);
            return;
        }

        StepConfig config = step == GoalNoteStep.Step1 ? targetGoal.step1Config : targetGoal.step2Config;
        Transform focusTarget = config != null ? config.focusTarget : null;
        if (focusTarget == null)
        {
            Debug.LogWarning("[GoalNoteCameraFocus] Focus target missing. goalID=" + goalID + " step=" + step);
            return;
        }

        if (!TryGetCameraController(out var controller))
        {
            return;
        }

        float duration = config != null && config.cameraDuration > 0f
            ? config.cameraDuration
            : DefaultCameraMoveDuration;

        controller.MoveCameraToPositionByDuration(focusTarget.position, duration);
    }

    /// <summary>
    /// 在场景中查找指定 GoalID 的 Goal 对象
    /// </summary>
    private Goal FindGoalById(int goalID)
    {
        if (goalById.TryGetValue(goalID, out var cachedGoal) && cachedGoal != null)
            return cachedGoal;

        RebuildGoalCache();
        if (goalById.TryGetValue(goalID, out var rebuiltGoal) && rebuiltGoal != null)
            return rebuiltGoal;

        return null;
    }

    private void RebuildGoalCache()
    {
        goalById.Clear();

        Goal[] goals = FindObjectsOfType<Goal>();
        for (int i = 0; i < goals.Length; i++)
        {
            Goal goal = goals[i];
            if (goal == null)
                continue;

            if (!goalById.ContainsKey(goal.GoalID))
                goalById.Add(goal.GoalID, goal);
        }
    }

    private bool TryGetCameraController(out CameraController controller)
    {
        if (cachedCameraController != null)
        {
            controller = cachedCameraController;
            return true;
        }

        if (cachedMainCamera == null)
            cachedMainCamera = Camera.main;

        if (cachedMainCamera == null)
        {
            Debug.LogWarning("[GoalNoteCameraFocus] Main camera not found.");
            controller = null;
            return false;
        }

        cachedCameraController = cachedMainCamera.GetComponent<CameraController>();
        if (cachedCameraController == null)
        {
            Debug.LogWarning("[GoalNoteCameraFocus] CameraController missing on main camera.");
            controller = null;
            return false;
        }

        controller = cachedCameraController;
        return true;
    }
}
