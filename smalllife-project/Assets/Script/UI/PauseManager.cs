using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public PausePanel pausePanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (pausePanel == null) return;

        if (pausePanel.IsShown)
        {
            pausePanel.Hide();
        }
        else
        {
            pausePanel.Show();
        }
    }
}