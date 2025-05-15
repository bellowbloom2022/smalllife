using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public RectTransform panel; // �������� RectTransform
    public Button closeButton;  // �ر��������İ�ť

    public GameObject[] backgroundUIElements; // ��Ҫ�ƶ��ı��� UI Ԫ����
    private bool isPanelOpen = false; // ������¼����Ƿ��

    void Start()
    {   
        // ���رհ�ť��Ӽ�����
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        // ȷ����ʼ��ʱ��������ǹرյ�
        panel.gameObject.SetActive(false);
    }

    // ��/�ر�������岢�ƶ����� UI
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;

        if (isPanelOpen)
        {
            // �����
            panel.gameObject.SetActive(true);

            // ����UI��ʧ
            foreach(GameObject bgUI in backgroundUIElements)
            {
                bgUI.SetActive(false);
            }
        }
        else
        {
            // �ر��������
            panel.gameObject.SetActive(false);

            foreach (GameObject bgUI in backgroundUIElements)
            {
                bgUI.SetActive(true);
            }
        }
    }

    private void OnCloseButtonClicked()
    {
        TogglePanel();
    }
}

