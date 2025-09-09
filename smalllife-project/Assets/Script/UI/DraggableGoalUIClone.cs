using UnityEngine;
using UnityEngine.UI;

public class DraggableGoalUIClone : MonoBehaviour
{
    public Image icon;
    public Text label;

    private int goalID;
    private ApartmentController controller;

    public void Init(int id, Sprite sprite, string text, ApartmentController ctrl)
    {
        goalID = id;
        controller = ctrl;

        if (icon != null) icon.sprite = sprite;
        if (label != null) label.text = text;
    }
}
