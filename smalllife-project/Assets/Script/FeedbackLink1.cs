using UnityEngine;

public class FeedbackLink1 : MonoBehaviour
{
    // ������
    public string feedbackUrl = "https://www.wjx.cn/vm/eTnoS6H.aspx# ";

    // �����ťʱ����
    public void OpenFeedbackForm()
    {
        Application.OpenURL(feedbackUrl);
    }
}
