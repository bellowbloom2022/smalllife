using System.Collections.Generic;
using UnityEngine;

public class GoalAttentionTracker : MonoBehaviour
{
    private class FocusState
    {
        public string levelID;
        public int goalID;
        public Goal goal;
        public float score;
        public float lastFocusedTime;
    }

    public static GoalAttentionTracker Instance { get; private set; }

    [Header("采样")]
    [SerializeField] private float sampleInterval = 0.1f;
    [SerializeField] private float maxStableMouseSpeed = 260f;

    [Header("缩放权重")]
    [SerializeField] private float fallbackNearOrthoSize = 2f;
    [SerializeField] private float fallbackFarOrthoSize = 10f;
    [SerializeField, Range(0f, 1f)] private float minZoomedOutWeight = 0.15f;
    [SerializeField] private float dwellWhenZoomedIn = 0.3f;
    [SerializeField] private float dwellWhenZoomedOut = 1.0f;

    [Header("注意力分数")]
    [SerializeField] private float zoneScorePerSecond = 1f;
    [SerializeField] private float goalIconHoverScore = 3.5f;
    [SerializeField] private float step1ProgressScore = 5f;
    [SerializeField] private float scoreDecayPerSecond = 0.02f;

    private readonly Dictionary<string, FocusState> focusStates = new Dictionary<string, FocusState>();
    private Camera cachedCamera;
    private GoalHintZone currentZone;
    private float currentZoneDwell;
    private float nextSampleTime;
    private float lastSampleTime;
    private Vector3 lastMousePosition;
    private bool hasLastMousePosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GoalNoteEvents.GoalCompleted += HandleGoalCompleted;
    }

    private void OnDisable()
    {
        GoalNoteEvents.GoalCompleted -= HandleGoalCompleted;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (Time.time < nextSampleTime)
            return;

        float now = Time.time;
        float dt = lastSampleTime > 0f ? now - lastSampleTime : sampleInterval;
        lastSampleTime = now;
        nextSampleTime = now + Mathf.Max(0.02f, sampleInterval);

        DecayScores(dt);
        SampleSceneAttention(dt);
    }

    public void ReportGoalIconHover(string levelID, int goalID)
    {
        if (goalID <= 0)
            return;

        Goal goal = ResolveGoal(levelID, goalID);
        AddScore(levelID, goalID, goal, goalIconHoverScore);
    }

    public void ReportGoalFocus(Goal goal, float amount)
    {
        if (goal == null || amount <= 0f || goal.IsHintCompleted())
            return;

        AddScore(goal.GetHintLevelID(), goal.GoalID, goal, amount);
    }

    public bool TryGetBestGoal(float minScore, out Goal goal, out GoalHintStage stage, out float score)
    {
        goal = null;
        stage = GoalHintStage.PreAnim1;
        score = 0f;

        foreach (FocusState state in focusStates.Values)
        {
            Goal candidate = state.goal != null ? state.goal : ResolveGoal(state.levelID, state.goalID);
            if (candidate == null || candidate.IsHintCompleted())
                continue;

            if (!IsCurrentLevel(state.levelID, candidate))
                continue;

            if (state.score < minScore || state.score <= score)
                continue;

            goal = candidate;
            stage = candidate.GetHintStage();
            score = state.score;
        }

        return goal != null;
    }

    public void ClearScores()
    {
        focusStates.Clear();
        currentZone = null;
        currentZoneDwell = 0f;
    }

    private void SampleSceneAttention(float dt)
    {
        if (InputRouter.Instance != null && InputRouter.Instance.InputLocked)
            return;

        if (UIBlockChecker.IsPointerOverUI() || BasePanel.IsPointerOverAnyShownPanel(Input.mousePosition))
        {
            currentZone = null;
            currentZoneDwell = 0f;
            return;
        }

        Camera camera = GetCamera();
        if (camera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;
        bool stable = IsMouseStable(mousePosition, dt);
        lastMousePosition = mousePosition;
        hasLastMousePosition = true;

        if (!stable)
        {
            currentZone = null;
            currentZoneDwell = 0f;
            return;
        }

        GoalHintZone zone = FindPointedZone(camera, mousePosition);
        if (zone == null)
        {
            currentZone = null;
            currentZoneDwell = 0f;
            return;
        }

        if (zone == currentZone)
            currentZoneDwell += dt;
        else
        {
            currentZone = zone;
            currentZoneDwell = dt;
        }

        float zoomWeight = GetZoomWeight(camera);
        float requiredDwell = Mathf.Lerp(dwellWhenZoomedOut, dwellWhenZoomedIn, zoomWeight);
        if (currentZoneDwell < requiredDwell)
            return;

        ReportGoalFocus(zone.Goal, zoneScorePerSecond * dt * zoomWeight * zone.AttentionMultiplier);
    }

    private bool IsMouseStable(Vector3 mousePosition, float dt)
    {
        if (!hasLastMousePosition || dt <= 0f)
            return true;

        float speed = Vector3.Distance(mousePosition, lastMousePosition) / dt;
        return speed <= maxStableMouseSpeed;
    }

    private GoalHintZone FindPointedZone(Camera camera, Vector3 screenPoint)
    {
        GoalHintZone bestZone = null;
        float bestWeight = -1f;
        List<GoalHintZone> zones = GoalHintZone.ActiveZones;

        for (int i = 0; i < zones.Count; i++)
        {
            GoalHintZone zone = zones[i];
            if (zone == null || !zone.ContainsScreenPoint(camera, screenPoint))
                continue;

            float weight = zone.AttentionMultiplier;
            if (weight <= bestWeight)
                continue;

            bestWeight = weight;
            bestZone = zone;
        }

        return bestZone;
    }

    private float GetZoomWeight(Camera camera)
    {
        if (camera == null || !camera.orthographic)
            return 1f;

        float nearSize = fallbackNearOrthoSize;
        float farSize = fallbackFarOrthoSize;
        CameraController controller = camera.GetComponent<CameraController>();
        if (controller != null)
        {
            nearSize = controller.zoomRange.x;
            farSize = controller.zoomRange.y;
        }

        if (farSize <= nearSize)
            return 1f;

        float normalized = Mathf.InverseLerp(farSize, nearSize, camera.orthographicSize);
        return Mathf.Lerp(minZoomedOutWeight, 1f, normalized);
    }

    private void AddScore(string levelID, int goalID, Goal goal, float amount)
    {
        if (goalID <= 0 || amount <= 0f)
            return;

        string normalizedLevelID = NormalizeLevelID(levelID, goal);
        string key = BuildKey(normalizedLevelID, goalID);
        FocusState state = GetOrCreateState(key, normalizedLevelID, goalID);
        if (goal != null)
            state.goal = goal;

        state.score += amount;
        state.lastFocusedTime = Time.time;
    }

    private FocusState GetOrCreateState(string key, string levelID, int goalID)
    {
        if (focusStates.TryGetValue(key, out FocusState state))
            return state;

        state = new FocusState
        {
            levelID = levelID,
            goalID = goalID,
            score = 0f,
            lastFocusedTime = Time.time
        };
        focusStates[key] = state;
        return state;
    }

    private void DecayScores(float dt)
    {
        if (dt <= 0f || focusStates.Count == 0)
            return;

        float decay = scoreDecayPerSecond * dt;
        foreach (FocusState state in focusStates.Values)
            state.score = Mathf.Max(0f, state.score - decay);
    }

    private void HandleGoalCompleted(string levelID, int goalID, GoalNoteStep completedStep)
    {
        Goal goal = ResolveGoal(levelID, goalID);
        if (completedStep == GoalNoteStep.Step1 && goal != null && !goal.IsHintCompleted())
            AddScore(levelID, goalID, goal, step1ProgressScore);
    }

    private Camera GetCamera()
    {
        if (cachedCamera == null)
            cachedCamera = Camera.main;

        return cachedCamera;
    }

    private Goal ResolveGoal(string levelID, int goalID)
    {
        if (goalID <= 0 || Level.ins == null || Level.ins.goals == null)
            return null;

        for (int i = 0; i < Level.ins.goals.Count; i++)
        {
            GameObject goalObject = Level.ins.goals[i];
            if (goalObject == null)
                continue;

            Goal goal = goalObject.GetComponent<Goal>();
            if (goal == null || goal.GoalID != goalID)
                continue;

            if (string.IsNullOrEmpty(levelID) || IsSameLevel(levelID, goal.GetHintLevelID()))
                return goal;
        }

        return null;
    }

    private bool IsCurrentLevel(string levelID, Goal goal)
    {
        if (goal == null)
            return false;

        string goalLevelID = goal.GetHintLevelID();
        if (!string.IsNullOrEmpty(levelID) && !IsSameLevel(levelID, goalLevelID))
            return false;

        if (Level.ins == null || Level.ins.levelDataAsset == null)
            return true;

        return IsSameLevel(goalLevelID, Level.ins.levelDataAsset.levelID);
    }

    private static string NormalizeLevelID(string levelID, Goal goal)
    {
        if (!string.IsNullOrEmpty(levelID))
            return levelID;

        if (goal != null)
            return goal.GetHintLevelID();

        if (Level.ins != null && Level.ins.levelDataAsset != null)
            return Level.ins.levelDataAsset.levelID;

        if (Level.ins != null)
            return Level.ins.currentLevelIndex.ToString();

        return string.Empty;
    }

    private static bool IsSameLevel(string lhs, string rhs)
    {
        if (string.IsNullOrEmpty(lhs) || string.IsNullOrEmpty(rhs))
            return true;

        return GoalProgressRules.IsSameLevelID(lhs, rhs);
    }

    private static string BuildKey(string levelID, int goalID)
    {
        return string.Concat(levelID ?? string.Empty, "#", goalID.ToString());
    }
}
