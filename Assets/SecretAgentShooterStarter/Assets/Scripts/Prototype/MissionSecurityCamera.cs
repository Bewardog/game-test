using UnityEngine;

[DisallowMultipleComponent]
public class MissionSecurityCamera : MonoBehaviour, IInteractable
{
    public Transform scanPivot;
    public Transform sightOrigin;
    public Renderer lensRenderer;
    public float scanArc = 58f;
    public float scanSpeed = 24f;
    public float viewRange = 20f;
    public float fieldOfView = 62f;
    public float confirmationSeconds = 1.35f;
    public float suspicionDecay = 0.72f;

    private SpyPlayerController player;
    private PrototypeAudioEmitter audioEmitter;
    private Quaternion pivotRestRotation = Quaternion.identity;
    private float suspicion;
    private bool disabled;
    private int warningStage;

    public bool IsDisabled => disabled;
    public float Suspicion => suspicion;
    public string InteractionPrompt => disabled ? string.Empty : "Sabotage security camera";

    private void Awake()
    {
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(1f);
    }

    public void Configure(Transform pivot, Transform origin, Renderer lens)
    {
        scanPivot = pivot;
        sightOrigin = origin;
        lensRenderer = lens;
        pivotRestRotation = scanPivot != null ? scanPivot.localRotation : Quaternion.identity;
        SetLensColor(new Color(0.2f, 0.95f, 0.78f));
    }

    private void Start()
    {
        if (scanPivot != null) pivotRestRotation = scanPivot.localRotation;
        player = FindAnyObjectByType<SpyPlayerController>();
    }

    private void Update()
    {
        if (disabled) return;
        if (player == null) player = FindAnyObjectByType<SpyPlayerController>();

        UpdateSweep();
        if (player == null || player.IsIncapacitated)
        {
            DecaySuspicion();
            return;
        }

        MissionManager mission = MissionManager.Instance;
        if (mission == null || mission.IsAlerted)
        {
            suspicion = 0f;
            SetLensColor(mission != null && mission.IsAlerted
                ? new Color(1f, 0.12f, 0.08f)
                : new Color(0.2f, 0.95f, 0.78f));
            return;
        }

        bool hasVisual = HasConfirmedLineOfSight();
        if (!hasVisual)
        {
            DecaySuspicion();
            return;
        }

        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        float confirmationRate = Mathf.Max(0.2f, difficulty.enemyAwarenessMultiplier) /
            Mathf.Max(0.25f, confirmationSeconds);
        suspicion = Mathf.Clamp01(suspicion + Time.deltaTime * confirmationRate);
        SetLensColor(Color.Lerp(
            new Color(1f, 0.72f, 0.08f),
            new Color(1f, 0.08f, 0.04f),
            suspicion));

        if (warningStage == 0)
        {
            warningStage = 1;
            player.ShowActionMessage("SECURITY CAMERA TRACKING", 0.75f);
        }
        else if (warningStage == 1 && suspicion >= 0.62f)
        {
            warningStage = 2;
            player.ShowActionMessage("CAMERA CONFIRMATION IMMINENT", 0.8f);
        }

        if (suspicion < 1f) return;

        audioEmitter?.PlaySecurityAlarm();
        mission.TriggerAlert($"{name} confirmed the agent");
        player.ShowActionMessage("SECURITY ALARM TRIGGERED", 1.1f);
    }

    public void Interact(SpyPlayerController interactingPlayer)
    {
        DisableCamera("CAMERA SABOTAGED", interactingPlayer);
    }

    public bool ReceiveWeaponHit(Vector3 hitPoint)
    {
        if (disabled) return false;
        DisableCamera("CAMERA DISABLED", player);
        return true;
    }

    public void DisableCamera(string message, SpyPlayerController interactingPlayer = null)
    {
        if (disabled) return;

        disabled = true;
        suspicion = 0f;
        warningStage = 0;
        SetLensColor(new Color(0.08f, 0.1f, 0.11f));
        audioEmitter?.PlaySecurityDisabled();
        (interactingPlayer != null ? interactingPlayer : player)?.ShowActionMessage(message, 0.9f);
    }

    private void UpdateSweep()
    {
        if (scanPivot == null) return;
        float phase = Mathf.PingPong(Time.time * scanSpeed, scanArc * 2f) - scanArc;
        scanPivot.localRotation = pivotRestRotation * Quaternion.Euler(0f, phase, 0f);
    }

    private bool HasConfirmedLineOfSight()
    {
        Transform originTransform = sightOrigin != null ? sightOrigin : scanPivot;
        if (originTransform == null) return false;

        Vector3 origin = originTransform.position + originTransform.forward * 0.18f;
        Vector3 target = player.transform.position + Vector3.up * 1.05f;
        Vector3 toPlayer = target - origin;
        float distance = toPlayer.magnitude;
        if (distance > viewRange || distance < 0.01f) return false;
        if (Vector3.Angle(originTransform.forward, toPlayer) > fieldOfView * 0.5f) return false;

        if (!Physics.Raycast(
                origin,
                toPlayer / distance,
                out RaycastHit hit,
                distance + 0.2f,
                ~0,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        SpyPlayerController visiblePlayer = hit.collider != null
            ? hit.collider.GetComponentInParent<SpyPlayerController>()
            : null;
        return visiblePlayer == player;
    }

    private void DecaySuspicion()
    {
        suspicion = Mathf.MoveTowards(suspicion, 0f, suspicionDecay * Time.deltaTime);
        if (suspicion <= 0.01f)
        {
            warningStage = 0;
            SetLensColor(new Color(0.2f, 0.95f, 0.78f));
        }
        else
        {
            SetLensColor(Color.Lerp(
                new Color(0.2f, 0.95f, 0.78f),
                new Color(1f, 0.72f, 0.08f),
                suspicion));
        }
    }

    private void SetLensColor(Color color)
    {
        if (lensRenderer != null) lensRenderer.material.color = color;
    }
}
