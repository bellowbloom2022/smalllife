using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class Goal : MonoBehaviour
{
    protected void TriggerCollectAnimation(bool markStep2 = true)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out Vector2 uiPos);
        RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
        rectPenZai.anchoredPosition = uiPos;

        screenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

        mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.3f);
        rectPenZai.DOLocalMove(uiPos, 0.4f).onComplete = () =>
        {
            mGameObjectNovel.transform.DOScale(Vector3.one, 0.3f);

            // 在第二段飞行前先把 bar 对齐到目标，避免“落点按旧视口计算，随后 bar 才移动”的体感偏移。
            if (levelData != null)
                GoalIconBarController.Instance?.TryFocusToGoal(levelData.levelID, goalID, false);

            Canvas.ForceUpdateCanvases();
            Transform finalTarget = iconController != null ? iconController.transform : mNovelPos.transform;
            // 用世界坐标转换，无论目标是否在 Content 内都能落到正确槽位
            Vector3 finalLocalPos = rectPenZai.parent.InverseTransformPoint(finalTarget.position);
            finalLocalPos.z = 0f;
            rectPenZai.DOLocalMove(finalLocalPos, 0.4f).SetEase(Ease.InBack);
            mGameObjectNovel.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack);
            rectPenZai.DOLocalMove(finalLocalPos, 0.4f).onComplete = () =>
            {
                rectPenZai.GetComponent<Animator>().SetTrigger("click");
                Level.ins.AddCount();
                isFound = true;
                step1Completed = true;
                if (markStep2) step2Completed = true;

                Debug.Log($"Goal ID {goalID} marked as found.");
                GameDataUtils.SetGoalStep(SaveSystem.GameData, Level.ins.currentLevelIndex, goalID, true, markStep2);

                int completedGoals = GameDataUtils.GetCompletedGoalCount(SaveSystem.GameData, Level.ins.currentLevelIndex);
                SaveSystem.UpdateLevelStar(Level.ins.currentLevelIndex, completedGoals);

                if (markStep2)
                {
                    SaveSystem.SaveGame();
                    GoalNoteEvents.RaiseGoalCompleted(levelData.levelID, goalID, GoalNoteStep.Step2);
                }
                else
                {
                    // SingleGoal 单步完成：SaveGame 并通知 UI
                    SaveSystem.SaveGame();
                    GoalNoteEvents.RaiseGoalCompleted(levelData.levelID, goalID, GoalNoteStep.Step1);
                }

                if (levelData != null)
                    GoalIconBarController.Instance?.TryFocusToGoal(levelData.levelID, goalID, false);

                CleanupCollectedNovelObject();
            };
        };

        AudioHub.Instance.PlayGlobal("goal_found");
        currentStage = markStep2 ? Stage.PostAnim2 : Stage.PostAnim1;
        ShowFirstDialogueOfCurrentStage();
    }

    private void CleanupCollectedNovelObject()
    {
        if (!cleanupNovelAfterCollect || mGameObjectNovel == null)
            return;

        float delay = Mathf.Max(0f, cleanupDelayAfterCollect);
        DOVirtual.DelayedCall(delay, () =>
        {
            if (mGameObjectNovel == null)
                return;

            if (destroyNovelObjectAfterCollect)
                Destroy(mGameObjectNovel);
            else
                mGameObjectNovel.SetActive(false);
        });
    }
}
