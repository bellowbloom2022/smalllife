using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GoalHintZone : MonoBehaviour
{
    public static readonly List<GoalHintZone> ActiveZones = new List<GoalHintZone>();

    [Header("Goal")]
    [SerializeField] private Goal goal;

    [Header("启用阶段")]
    [SerializeField] private bool activeInPreAnim1 = true;
    [SerializeField] private bool activeInPostAnim1 = true;
    [SerializeField] private bool activeInPostAnim2 = false;

    [Header("注意力权重")]
    [SerializeField] private float attentionMultiplier = 1f;

    private Collider2D cachedCollider2D;
    private Collider cachedCollider3D;

    public Goal Goal => goal;
    public float AttentionMultiplier => Mathf.Max(0f, attentionMultiplier);

    private void Awake()
    {
        if (goal == null)
            goal = GetComponentInParent<Goal>();

        cachedCollider2D = GetComponent<Collider2D>();
        cachedCollider3D = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        if (!ActiveZones.Contains(this))
            ActiveZones.Add(this);
    }

    private void OnDisable()
    {
        ActiveZones.Remove(this);
    }

    public bool IsAvailable()
    {
        if (goal == null || goal.IsHintCompleted())
            return false;

        GoalHintStage stage = goal.GetHintStage();
        switch (stage)
        {
            case GoalHintStage.PostAnim1:
                return activeInPostAnim1;
            case GoalHintStage.PostAnim2:
                return activeInPostAnim2;
            default:
                return activeInPreAnim1;
        }
    }

    public bool ContainsScreenPoint(Camera camera, Vector3 screenPoint)
    {
        if (!isActiveAndEnabled || camera == null || !IsAvailable())
            return false;

        if (cachedCollider2D != null && cachedCollider2D.enabled)
        {
            Vector3 world = camera.ScreenToWorldPoint(screenPoint);
            return cachedCollider2D.OverlapPoint(new Vector2(world.x, world.y));
        }

        if (cachedCollider3D != null && cachedCollider3D.enabled)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            return cachedCollider3D.Raycast(ray, out _, camera.farClipPlane);
        }

        return false;
    }
}
