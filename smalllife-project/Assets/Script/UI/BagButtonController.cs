using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BagButtonController : MonoBehaviour
{
    public static BagButtonController Instance;

    [SerializeField] private GameObject highlightRing; // bag 外圈光效
    [SerializeField] private RectTransform sidebar;    // ScrollView-sidebar
    [SerializeField] private float animDuration = 0.3f;

    private bool isOpen = false;
    private Vector2 sidebarHiddenPos;
    private Vector2 sidebarShownPos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 假设 sidebar 是锚在右侧的，可以用 anchoredPosition 控制
        sidebarShownPos = sidebar.anchoredPosition;
        sidebarHiddenPos = sidebarShownPos + new Vector2(sidebar.rect.width, 0);
        sidebar.anchoredPosition = sidebarHiddenPos;

        SetHighlight(false);

        GetComponent<Button>().onClick.AddListener(ToggleSidebar);
    }

    public void SetHighlight(bool val)
    {
        if (highlightRing != null)
            highlightRing.SetActive(val);
    }

    private void ToggleSidebar()
    {
        if (isOpen)
        {
            AudioHub.Instance.PlayGlobal("click_confirm");
            sidebar.DOAnchorPos(sidebarHiddenPos, animDuration).SetEase(Ease.InOutQuad);
            isOpen = false;
        }
        else
        {
            AudioHub.Instance.PlayGlobal("click_confirm");
            sidebar.DOAnchorPos(sidebarShownPos, animDuration).SetEase(Ease.InOutQuad);
            isOpen = true;
        }
    }
}
