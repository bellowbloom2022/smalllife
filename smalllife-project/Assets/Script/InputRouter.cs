using UnityEngine;
using System;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance;

    public event Action<Vector3> OnDrag;   // 拖拽事件（delta）
    public event Action<Vector3> OnClick;  // 点击事件（屏幕位置）

    private Vector3 lastMousePos;
    private string dragMode = "right"; // "left" or "right"

    private int DragMouseButton => (dragMode == "left") ? 0 : 1;
    private int ClickMouseButton => 0; // 总是使用左键作为点击（交互）

    void Awake()
    {
        Instance = this;
        dragMode = PlayerPrefs.GetString("Control_DragMode", "right");
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //if (GameManager.instance != null && GameManager.instance.IsPaused)
            //return;

        if (GameManager.instance != null && GameManager.instance.CheckGuiRaycastObject())
            return;

        // 拖拽逻辑
        if (Input.GetMouseButtonDown(DragMouseButton))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(DragMouseButton))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;
            OnDrag?.Invoke(delta);
        }

        // 点击逻辑（左键点击）
        if (Input.GetMouseButtonDown(ClickMouseButton))
        {
            OnClick?.Invoke(Input.mousePosition);
        }
    }

    public void SetDragMode(string mode)
    {
        dragMode = mode;
        PlayerPrefs.SetString("Control_DragMode", mode);
    }

    public string GetDragMode() => dragMode;
}
