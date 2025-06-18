using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarDisplayController : MonoBehaviour
{
    public GameObject starPrefab;     // ����������� prefab
    public bool useLayoutGroup = true; // �Ƿ�ʹ�� Horizontal Layout Group
    public float manualSpacing = 50f; // �����ʹ�� Layout Group����������ֶ����ü��

    private List<GameObject> currentStars = new List<GameObject>();

    /// <summary>
    /// �������� UI��
    /// </summary>
    /// <param name="completed">����ɵ�Ŀ������</param>
    /// <param name="total">��Ŀ������</param>
    public void UpdateStars(int completed, int total)
    {
        // ��վ�����
        foreach (GameObject star in currentStars)
        {
            Destroy(star);
        }
        currentStars.Clear();

        // ��̬��������
        for (int i = 0; i < total; i++)
        {
            GameObject star = Instantiate(starPrefab, transform);
            star.SetActive(true);

            if (!useLayoutGroup)
            {
                // �ֶ�����λ�ã���ر� Layout Group��
                RectTransform rt = star.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * manualSpacing, 0);
            }

            Animator animator = star.GetComponent<Animator>();

            if (i < completed)
            {
                animator?.SetTrigger("FillStar");
            }
            else
            {
                animator?.Play("Default", 0, 0);
            }

            currentStars.Add(star);
        }
    }
}
