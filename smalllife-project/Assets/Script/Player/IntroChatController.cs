using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroChatController : MonoBehaviour
{
    [Header("剧情步骤配置")]
    public NarrativeStep[] steps;

    [Header("UI 组件")]
    public Image imageUI;                          // 显示图片（可选）
    public LocalizedTypewriterEffect typewriter;   // 打字机（必填）
    public Button finishButton;                    // 结束按钮（最后出现）
    public Animator animator;                      // 可选动画器

    private int currentIndex;
    private Coroutine waitCo;

    private enum StepState { Idle, Typing, WaitingForClick, AutoWaiting, Finished }
    private StepState state = StepState.Idle;

    private void OnEnable()
    {
        InputRouter.OnBlankClick += HandleBlankClick; // 使用你的输入路由的“点空白”事件，自动排除 UI 点击
    }

    private void OnDisable()
    {
        InputRouter.OnBlankClick -= HandleBlankClick;

        if (waitCo != null)
        {
            StopCoroutine(waitCo);
            waitCo = null;
        }
    }

    private void Start()
    {
        if (finishButton != null) finishButton.gameObject.SetActive(false);
        currentIndex = 0;
        PlayStep(currentIndex);
    }

    private void HandleBlankClick()
    {
        switch (state)
        {
            case StepState.Typing:
                // 打字中 → 一键跳到全文
                typewriter.SkipToEnd();
                break;

            case StepState.WaitingForClick:
                // 本段已播完，等待点击推进
                NextStep();
                break;

            // AutoWaiting/Finished/Idle 时点击无效
        }
    }

    private void PlayStep(int index)
    {
        if (index >= (steps?.Length ?? 0))
        {
            ShowFinishButton();
            return;
        }

        var step = steps[index];

        // 切图
        if (imageUI != null && step.image != null)
            imageUI.sprite = step.image;

        // 播动画
        if (animator != null && !string.IsNullOrEmpty(step.animTrigger))
            animator.SetTrigger(step.animTrigger);

        state = StepState.Typing;

        // 播文字（播完后根据配置进入 等待/自动 等）
        typewriter.Play(step.textKey, onComplete: () =>
        {
            // 打字完成：
            if (step.autoWait > 0f)
            {
                state = StepState.AutoWaiting;
                if (waitCo != null) StopCoroutine(waitCo);
                waitCo = StartCoroutine(WaitThenContinue(step.autoWait));
            }
            else if (step.requireClick)
            {
                state = StepState.WaitingForClick; // 等玩家点击
            }
            else
            {
                NextStep(); // 直接下一个
            }
        });
    }

    private IEnumerator WaitThenContinue(float t)
    {
        yield return new WaitForSeconds(t);
        waitCo = null;
        NextStep();
    }

    private void NextStep()
    {
        if (waitCo != null)
        {
            StopCoroutine(waitCo);
            waitCo = null;
        }

        currentIndex++;
        PlayStep(currentIndex);
    }

    private void ShowFinishButton()
    {
        state = StepState.Finished;
        if (finishButton != null)
            finishButton.gameObject.SetActive(true);
    }
}
