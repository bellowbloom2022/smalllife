using UnityEngine;

public class BasePanel : MonoBehaviour
{
    public bool IsShown => gameObject.activeSelf;
    
    public virtual void Show()
    {
        gameObject.SetActive(true);
        AudioHub.Instance.PlayGlobal("click_confirm");
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        AudioHub.Instance.PlayGlobal("back_confirm");
    }
}
