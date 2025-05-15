using UnityEngine;

public class FeedbackLink1 : MonoBehaviour
{
    // 表单链接
    public string feedbackUrl = "https://www.wjx.cn/vm/eTnoS6H.aspx# ";

    // 点击按钮时调用
    public void OpenFeedbackForm()
    {
        Application.OpenURL(feedbackUrl);
    }
}
