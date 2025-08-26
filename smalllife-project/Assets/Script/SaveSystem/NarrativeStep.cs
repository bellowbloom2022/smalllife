using UnityEngine;

[System.Serializable]
public class NarrativeStep
{
    [Tooltip("LeanLocalization 的词条 key")]
    public string textKey;

    [Tooltip("对应显示的图像（可选）")]
    public Sprite image;

    [Tooltip("如果 >0，则自动等待指定时间后进入下一步")]
    public float autoWait = 0f;

    [Tooltip("是否必须等待玩家点击才能进入下一步")]
    public bool requireClick = true;

    [Tooltip("Animator 的触发参数（可选）")]
    public string animTrigger;
}
