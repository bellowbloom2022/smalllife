using UnityEngine;

public class FeedbackLink : MonoBehaviour
{
    // ������
    public string feedbackUrl = "https://docs.google.com/forms/d/e/1FAIpQLSfYESa-4SKHDTAcArWJDhHzwpHckBBPd8mBCF3GUIS_BCz8-A/viewform?usp=header";

    // �����ťʱ����
    public void OpenFeedbackForm()
    {
        Application.OpenURL(feedbackUrl);
    }
}
