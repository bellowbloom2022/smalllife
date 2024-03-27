using UnityEngine;
using UnityEngine.UI;

public class NotebookOpener : MonoBehaviour
{
    public GameObject notebookPanel;
    public Button closeButton; //�رհ�ť

    void Start()
    {
        notebookPanel.SetActive(false);

        //���õ����ر�
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void ToggleNotebookPanel()
    {
        notebookPanel.SetActive(!notebookPanel.activeSelf);
    }
    
    private void ClosePanel()
    {
        //����panel
        notebookPanel.SetActive(false);
    }
}