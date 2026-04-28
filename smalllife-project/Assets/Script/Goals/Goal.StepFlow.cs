using UnityEngine;
using DG.Tweening;

public partial class Goal : MonoBehaviour
{
    protected void ExecuteStep(StepConfig config)
    {
        if (config == null) return;

        stepCutsceneActive = true;
        stepCutsceneStartTime = Time.time;
        ScheduleInputUnlockFallback();
        if (InputRouter.Instance != null)
            InputRouter.Instance.LockInput($"Goal#{goalID}.ExecuteStep");

        if (config.useFocus && config.focusTarget != null)
        {
            FocusMaskController.Instance.Show(
                config.focusTarget,
                config.focusRadius,
                config.focusShowDuration
            );
        }

        if (config.moveCamera && config.cameraTarget != null)
        {
            DOVirtual.DelayedCall(config.cameraDelay, () =>
            {
                if (Camera.main == null) return;
                CameraController controller = Camera.main.GetComponent<CameraController>();
                if (controller == null) return;
                controller.MoveCameraToPositionByDuration(
                    config.cameraTarget.position,
                    config.cameraDuration
                );
            });
        }
    }

    private void ScheduleInputUnlockFallback()
    {
        int lockVersion = ++inputLockVersion;
        float fallbackDelay = FallbackUnlockTimeout;

        DOVirtual.DelayedCall(fallbackDelay, () =>
        {
            if (lockVersion != inputLockVersion)
                return;

            if (!stepCutsceneActive)
                return;

            if (InputRouter.Instance == null || !InputRouter.Instance.InputLocked)
                return;

            float elapsed = Time.time - stepCutsceneStartTime;
            Debug.LogWarning($"[Goal {goalID}] Fallback unlock triggered after timeout {elapsed:F2}s.");
            InputRouter.Instance.UnlockInput($"Goal#{goalID}.Fallback");
        });
    }

    private void CancelInputUnlockFallback()
    {
        inputLockVersion++;
    }

    protected void EndStep(StepConfig config)
    {
        if (config == null) return;

        stepCutsceneActive = false;

        if (config.useFocus)
            FocusMaskController.Instance.Hide(config.focusHideDuration, config.focusHideMode);

        CancelInputUnlockFallback();
        if (InputRouter.Instance != null)
            InputRouter.Instance.UnlockInput($"Goal#{goalID}.EndStep");
    }
}
