using UnityEngine;
using UnityEngine.UI;

public class PopupHandler : MonoBehaviour
{
    public GameObject Popup; // ��������
    public Button openButton;  // �򿪵����İ�ť
    public Button closeButton; // �رյ����İ�ť

    private void Start()
    {
        // ���ð�ť�ĵ���¼��������
        openButton.onClick.AddListener(OpenPopup);
        closeButton.onClick.AddListener(ClosePopup);
        Popup.SetActive(false); // Ĭ�����ص���
    }

    private void OpenPopup()
    {
        // ��ʾ����
        Popup.SetActive(true);
    }

    private void ClosePopup()
    {
        // ���ص���
        Popup.SetActive(false);
    }
}
