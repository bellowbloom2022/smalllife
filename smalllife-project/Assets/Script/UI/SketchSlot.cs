using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sketch 显示槽：继承 BaseDiarySlot
/// Setup(key, sketchSprite)
/// </summary>
public class SketchSlot : BaseDiarySlot
{
    [Header("UI")]
    public Image sketchImage;
    public GameObject lockMask; // optional mask to show locked state
    private Sprite sketchSprite;
    private bool isPlaced = false;  // UI 内部状态

    public void Setup(string key, Sprite sketch)
    {
        sketchSprite = sketch;
        base.Setup(key);
    }

    protected override void Refresh()
    {
        if (lockMask != null) lockMask.SetActive(!isPlaced);
        if (sketchImage != null)
        {
            sketchImage.sprite = isPlaced ? sketchSprite : null;
        }
    }

    // Force show (used on restore)
    public void ForceUnlocked(Sprite s = null)
    {
        if (s != null) sketchSprite = s;
        unlocked = true;
        Refresh();
    }

    public void Clear()
    {
        sketchSprite = null;
        if (sketchImage != null) sketchImage.sprite = null;
        if (lockMask != null) lockMask.SetActive(true);
        unlocked = false;
        goalKey = null;
    }
}
