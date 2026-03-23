using UnityEngine;
using UnityEngine.UI;

public class NotebookButton : MonoBehaviour
{
    [SerializeField] private GoalNotePanelController notePanel;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnNotebookButtonClicked);
            button.onClick.AddListener(OnNotebookButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnNotebookButtonClicked);
    }

    private void OnNotebookButtonClicked()
    {
        if (notePanel == null)
        {
            Debug.LogWarning("[NotebookButton] notePanel is not assigned.");
            return;
        }

        if (notePanel.IsShown)
            notePanel.Hide();
        else
            notePanel.Show();
    }
}
