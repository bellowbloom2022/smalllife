using System.Collections;
using UnityEngine;

public class HintAnimator : MonoBehaviour
{
    public GameObject hint1;
    public GameObject hint2;
    public GameObject hint3;
    public GameObject nextButton;

    private Camera mainCamera;

    void Start()
    {
        // 开始显示第一个提示动画
        StartCoroutine(ShowHint1());
        mainCamera = Camera.main;
    }

    IEnumerator ShowHint1()
    {
        hint1.SetActive(true);
        yield return new WaitUntil(() => Input.GetMouseButton(1)); // 等待右键拖拽画布
        yield return new WaitForSeconds(2);
        hint1.SetActive(false);
        StartCoroutine(ShowHint2());
    }

    IEnumerator ShowHint2()
    {
        hint2.SetActive(true);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)); // 等待WASD拖拽画布
        yield return new WaitForSeconds(2);
        hint2.SetActive(false);
        StartCoroutine(ShowHint3());
    }

    IEnumerator ShowHint3()
    {
        hint3.SetActive(true);
        yield return new WaitUntil(() => Input.mouseScrollDelta.y != 0); // 等待鼠标滚轮开始滚动
        yield return new WaitUntil(() => Input.mouseScrollDelta.y == 0); // 等待鼠标滚轮停止滚动
        // 检查玩家是否已经看到了 hint3，如果是，才继续显示 nextButton
        if (hint3.activeSelf)
        {
            yield return new WaitForSeconds(2);
            hint3.SetActive(false);
            nextButton.SetActive(true);
        }
        else
        {
            // 如果玩家在 hint3 出现前就滚动了鼠标滚轮，跳过提示3并直接显示 nextButton
            nextButton.SetActive(true);
        }
    }
}
