using UnityEngine;

public class HintAnimator3 : MonoBehaviour
{
    public GameObject coverUI; // ���� UI Ԫ��

    public void GoalAchieved()
    {
        if (coverUI != null)
        {
            coverUI.SetActive(false); // ��������
            Debug.Log("�������Ƴ������Ե�� nextButton");
        }
        else
        {
            Debug.LogError("Cover UI reference is not set in the HintAnimator3 script");
        }
    }
}
