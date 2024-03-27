using UnityEngine;
using UnityEngine.UI;

public class NotebookOpener : MonoBehaviour
{
    public GameObject notebookPanel;
    public Button closeButton; //πÿ±’∞¥≈•

    void Start()
    {
        notebookPanel.SetActive(false);

        //…Ë÷√µØ¥∞πÿ±’
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void ToggleNotebookPanel()
    {
        notebookPanel.SetActive(!notebookPanel.activeSelf);
    }
    
    private void ClosePanel()
    {
        //“˛≤ÿpanel
        notebookPanel.SetActive(false);
    }
}