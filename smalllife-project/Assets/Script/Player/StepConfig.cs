using UnityEngine;

[System.Serializable]
public class StepConfig
{
    [Header("Input")]
    public bool lockInput = true;

    [Header("Focus Mask")]
    public bool useFocus = true;
    public Transform focusTarget;
    public float focusRadius = 0.25f;
    public float focusShowDuration = 0.4f;
    public float focusHideDuration = 0.3f;

    [Header("Camera")]
    public bool moveCamera = true;
    public Transform cameraTarget;
    public float cameraDelay = 0.2f;
    public float cameraDuration = 1.0f;
}
