using UnityEngine;

public class FeedbackLink : MonoBehaviour
{
    // 表单链接
    public string feedbackUrl = "https://docs.google.com/forms/d/e/1FAIpQLSfYESa-4SKHDTAcArWJDhHzwpHckBBPd8mBCF3GUIS_BCz8-A/viewform?usp=header";

    // 点击按钮时调用
    public void OpenFeedbackForm()
    {
        Application.OpenURL(feedbackUrl);
    }
}
