using UnityEngine;
using UnityEngine.UI;

public class ShowTextOnUI : MonoBehaviour
{
    public Text infoText;   // 用于显示信息的Text组件
    public Image backgroundImage;  //用于显示背景的Image组件

    // 显示信息（设置文本内容）
    public void ShowText(string text)
    {
        infoText.text = text;
        backgroundImage.gameObject.SetActive(true); //显示白色背景
        infoText.gameObject.SetActive(true);
    }

    // 显示信息（保持当前文本不变，仅激活 UI）
    public void ShowText()
    {
        backgroundImage.gameObject.SetActive(true);
        infoText.gameObject.SetActive(true);
    }

    // 隐藏信息
    public void HideText()
    {
        infoText.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false); //隐藏白色背景
    }
}