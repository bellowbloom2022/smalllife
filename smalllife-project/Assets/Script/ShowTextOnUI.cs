using UnityEngine;
using UnityEngine.UI;

public class ShowTextOnUI : MonoBehaviour
{
    public Text infoText;   // ������ʾ��Ϣ��Text���
    public Image backgroundImage;  //������ʾ������Image���

    // ��ʾ��Ϣ
    public void ShowText(string text)
    {
        infoText.text = text;
        backgroundImage.gameObject.SetActive(true); //��ʾ��ɫ����
        infoText.gameObject.SetActive(true);
    }

    // ������Ϣ
    public void HideText()
    {
        infoText.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false); //���ذ�ɫ����
    }
}