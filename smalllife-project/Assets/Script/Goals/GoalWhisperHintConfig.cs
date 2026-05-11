using UnityEngine;
using UnityEngine.Serialization;

public class GoalWhisperHintConfig : MonoBehaviour
{
    [Header("PreAnim1 Key：故事还没开始")]
    [FormerlySerializedAs("preAnim1Hints")]
    public string[] preAnim1HintKeys;

    [Header("PostAnim1 Key：Step1 后，故事还没结束")]
    [FormerlySerializedAs("postAnim1Hints")]
    public string[] postAnim1HintKeys;

    [Header("PostAnim2 Key：故事已完成")]
    [FormerlySerializedAs("postAnim2Hints")]
    public string[] postAnim2HintKeys;

    [Header("显示位置 Anchor")]
    [SerializeField] private Transform defaultHintAnchor;
    [SerializeField] private Transform preAnim1HintAnchor;
    [SerializeField] private Transform postAnim1HintAnchor;
    [SerializeField] private Transform postAnim2HintAnchor;

    public string GetHintKey(GoalHintStage stage, int variantSeed)
    {
        string[] hintKeys = GetHintKeys(stage);
        if (hintKeys == null || hintKeys.Length == 0)
            return string.Empty;

        int validCount = 0;
        for (int i = 0; i < hintKeys.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(hintKeys[i]))
                validCount++;
        }

        if (validCount == 0)
            return string.Empty;

        int target = Mathf.Abs(variantSeed) % validCount;
        for (int i = 0; i < hintKeys.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(hintKeys[i]))
                continue;

            if (target == 0)
                return hintKeys[i];

            target--;
        }

        return string.Empty;
    }

    public Transform GetHintAnchor(GoalHintStage stage)
    {
        Transform stageAnchor;
        switch (stage)
        {
            case GoalHintStage.PostAnim1:
                stageAnchor = postAnim1HintAnchor;
                break;
            case GoalHintStage.PostAnim2:
                stageAnchor = postAnim2HintAnchor;
                break;
            default:
                stageAnchor = preAnim1HintAnchor;
                break;
        }

        return stageAnchor != null ? stageAnchor : defaultHintAnchor;
    }

    private string[] GetHintKeys(GoalHintStage stage)
    {
        switch (stage)
        {
            case GoalHintStage.PostAnim1:
                return postAnim1HintKeys;
            case GoalHintStage.PostAnim2:
                return postAnim2HintKeys;
            default:
                return preAnim1HintKeys;
        }
    }
}
