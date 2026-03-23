using UnityEngine;

/// <summary>
/// 负责 Goal Note 面板相关的镜头移动和 focus target 查询
/// 职责: 镜头聚焦、focus target 定位、camera controller 交互
/// </summary>
public class GoalNoteCameraFocusController : MonoBehaviour
{
    private const float DefaultCameraMoveDuration = 0.8f;

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

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[GoalNoteCameraFocus] Main camera not found.");
            return;
        }

        CameraController controller = mainCam.GetComponent<CameraController>();
        if (controller == null)
        {
            Debug.LogWarning("[GoalNoteCameraFocus] CameraController missing on main camera.");
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
        Goal[] goals = FindObjectsOfType<Goal>();
        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i] != null && goals[i].GoalID == goalID)
                return goals[i];
        }

        return null;
    }
}
