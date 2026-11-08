using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class SlidingSecurityDoor : MonoBehaviour, IInteractable
{
    public Transform leftPanel;
    public Transform rightPanel;
    public float slideDistance = 2.45f;
    public float slideSpeed = 5.5f;
    public float closeDelay = 1.25f;
    public int requiredClearance;

    private readonly HashSet<Transform> nearbyActors = new HashSet<Transform>();
    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;
    private float closeAt;
    private float manualOpenUntil;
    private bool initialized;
    private bool wasOpen;
    private bool unlocked;
    private PrototypeAudioEmitter audioEmitter;

    public bool IsLocked => requiredClearance > 0 && !unlocked;
    public string InteractionPrompt => IsOpenRequested
        ? string.Empty
        : IsLocked
            ? $"Use clearance {requiredClearance}"
            : "Open security door";
    private bool IsOpenRequested =>
        HasAuthorizedNearbyActor() ||
        Time.time < manualOpenUntil ||
        (!IsLocked && Time.time < closeAt);

    private void Awake()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(1f);
    }

    public void Initialize(Transform left, Transform right, float distance)
    {
        leftPanel = left;
        rightPanel = right;
        slideDistance = Mathf.Max(0.25f, distance);
        leftClosedPosition = leftPanel != null ? leftPanel.localPosition : Vector3.zero;
        rightClosedPosition = rightPanel != null ? rightPanel.localPosition : Vector3.zero;
        initialized = leftPanel != null && rightPanel != null;
    }

    public void ConfigureLock(int clearanceLevel)
    {
        requiredClearance = Mathf.Clamp(clearanceLevel, 0, 3);
        unlocked = requiredClearance <= 0;
    }

    public void Interact(SpyPlayerController player)
    {
        if (IsLocked)
        {
            if (player == null || !player.HasMissionClearance(requiredClearance))
            {
                player?.ShowActionMessage($"CLEARANCE {requiredClearance} REQUIRED", 0.9f);
                audioEmitter?.PlayAccessDenied();
                return;
            }

            unlocked = true;
            player.ShowActionMessage($"CLEARANCE {requiredClearance} ACCEPTED", 0.8f);
        }

        manualOpenUntil = Time.time + 4f;
        closeAt = manualOpenUntil;
    }

    private void Update()
    {
        if (!initialized) return;

        nearbyActors.RemoveWhere(actor => actor == null);
        bool shouldOpen = IsOpenRequested;
        Vector3 leftTarget = leftClosedPosition + Vector3.left * (shouldOpen ? slideDistance : 0f);
        Vector3 rightTarget = rightClosedPosition + Vector3.right * (shouldOpen ? slideDistance : 0f);
        leftPanel.localPosition = Vector3.MoveTowards(
            leftPanel.localPosition,
            leftTarget,
            slideSpeed * Time.deltaTime);
        rightPanel.localPosition = Vector3.MoveTowards(
            rightPanel.localPosition,
            rightTarget,
            slideSpeed * Time.deltaTime);

        if (shouldOpen != wasOpen)
        {
            wasOpen = shouldOpen;
            audioEmitter?.PlayDoor(shouldOpen);
        }
    }

    private bool HasAuthorizedNearbyActor()
    {
        foreach (Transform actor in nearbyActors)
        {
            if (actor == null) continue;
            if (actor.GetComponent<EnemyOperative>() != null) return true;
            if (!IsLocked && actor.GetComponent<SpyPlayerController>() != null) return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform actor = FindActorRoot(other);
        if (actor == null) return;
        nearbyActors.Add(actor);
        closeAt = Time.time + closeDelay;
    }

    private void OnTriggerStay(Collider other)
    {
        Transform actor = FindActorRoot(other);
        if (actor == null) return;
        nearbyActors.Add(actor);
        closeAt = Time.time + closeDelay;
    }

    private void OnTriggerExit(Collider other)
    {
        Transform actor = FindActorRoot(other);
        if (actor == null) return;
        nearbyActors.Remove(actor);
        closeAt = Time.time + closeDelay;
    }

    private static Transform FindActorRoot(Collider other)
    {
        if (other == null) return null;
        SpyPlayerController player = other.GetComponentInParent<SpyPlayerController>();
        if (player != null) return player.transform;
        EnemyOperative operative = other.GetComponentInParent<EnemyOperative>();
        return operative != null ? operative.transform : null;
    }
}
