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
        // ��ʼ��ʾ��һ����ʾ����
        StartCoroutine(ShowHint1());
        mainCamera = Camera.main;
    }

    IEnumerator ShowHint1()
    {
        hint1.SetActive(true);
        yield return new WaitUntil(() => Input.GetMouseButton(1)); // �ȴ��Ҽ���ק����
        yield return new WaitForSeconds(2);
        hint1.SetActive(false);
        StartCoroutine(ShowHint2());
    }

    IEnumerator ShowHint2()
    {
        hint2.SetActive(true);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)); // �ȴ�WASD��ק����
        yield return new WaitForSeconds(2);
        hint2.SetActive(false);
        StartCoroutine(ShowHint3());
    }

    IEnumerator ShowHint3()
    {
        hint3.SetActive(true);
        yield return new WaitUntil(() => Input.mouseScrollDelta.y != 0); // �ȴ������ֿ�ʼ����
        yield return new WaitUntil(() => Input.mouseScrollDelta.y == 0); // �ȴ�������ֹͣ����
        // �������Ƿ��Ѿ������� hint3������ǣ��ż�����ʾ nextButton
        if (hint3.activeSelf)
        {
            yield return new WaitForSeconds(2);
            hint3.SetActive(false);
            nextButton.SetActive(true);
        }
        else
        {
            // �������� hint3 ����ǰ�͹����������֣�������ʾ3��ֱ����ʾ nextButton
            nextButton.SetActive(true);
        }
    }
}
