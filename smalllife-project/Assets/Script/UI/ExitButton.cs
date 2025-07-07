using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // 这个方法将在按钮被点击时调用
    public void OnExitButtonClick()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
#if UNITY_EDITOR
        // 在Unity编辑器中，点击按钮会停止播放，类似于按下Stop按钮
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在发布的游戏中，点击按钮将退出程序
        Application.Quit();
#endif
    }
}
