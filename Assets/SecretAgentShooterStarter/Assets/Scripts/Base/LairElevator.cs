using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A real elevator car that travels up and down the lair shaft.
/// Each stop has a pair of sliding landing doors; only the doors at the floor
/// where the car is parked open. A player standing inside the car rides along.
/// </summary>
public class LairElevator : MonoBehaviour
{
    private enum ElevatorState
    {
        Idle,
        Closing,
        Moving,
        Opening
    }

    private class Landing
    {
        public string name;
        public float floorY;
        public Transform leftDoor;
        public Transform rightDoor;
        public Vector3 leftClosed;
        public Vector3 rightClosed;
    }

    public float travelSpeed = 3.2f;
    public float doorSlideDistance = 1.95f;
    public float doorSpeed = 2.4f;
    [Tooltip("Half-size of the car's interior, used to tell whether the player is aboard.")]
    public Vector3 interiorHalfExtents = new Vector3(2.2f, 4.1f, 2.4f);

    private readonly List<Landing> landings = new List<Landing>();
    private ElevatorState state = ElevatorState.Idle;
    private int currentStop;
    private int targetStop = -1;
    private float doorOpenAmount = 1f;
    private SpyPlayerController rider;
    private SpyPlayerController cachedPlayer;
    private PrototypeAudioEmitter audioEmitter;

    public int CurrentStop => currentStop;
    public bool IsBusy => state != ElevatorState.Idle;

    private void Awake()
    {
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(1f);
    }

    public void AddLanding(string landingName, float floorY, Transform leftDoor, Transform rightDoor)
    {
        landings.Add(new Landing
        {
            name = landingName,
            floorY = floorY,
            leftDoor = leftDoor,
            rightDoor = rightDoor,
            leftClosed = leftDoor != null ? leftDoor.position : Vector3.zero,
            rightClosed = rightDoor != null ? rightDoor.position : Vector3.zero
        });
        ApplyDoors();
    }

    public string GetStopName(int stopIndex)
    {
        return stopIndex >= 0 && stopIndex < landings.Count ? landings[stopIndex].name : "Unknown";
    }

    public void RequestStop(int stopIndex, SpyPlayerController player)
    {
        if (stopIndex < 0 || stopIndex >= landings.Count) return;

        if (state != ElevatorState.Idle)
        {
            player?.ShowActionMessage("ELEVATOR IN TRANSIT", 0.8f);
            return;
        }

        if (stopIndex == currentStop)
        {
            state = ElevatorState.Opening;
            return;
        }

        targetStop = stopIndex;
        state = ElevatorState.Closing;
        audioEmitter?.PlayDoor(false);
    }

    private void Update()
    {
        switch (state)
        {
            case ElevatorState.Closing:
                doorOpenAmount = Mathf.MoveTowards(doorOpenAmount, 0f, doorSpeed / doorSlideDistance * Time.deltaTime);
                if (doorOpenAmount <= 0f)
                {
                    SpyPlayerController player = FindPlayer();
                    if (player != null && IsInsideCar(player.transform.position))
                    {
                        rider = player;
                        rider.BeginPlatformRide();
                    }

                    state = ElevatorState.Moving;
                }
                break;

            case ElevatorState.Moving:
                float targetY = landings[targetStop].floorY;
                Vector3 position = transform.position;
                float newY = Mathf.MoveTowards(position.y, targetY, travelSpeed * Time.deltaTime);
                Vector3 delta = new Vector3(0f, newY - position.y, 0f);
                transform.position = position + delta;
                if (rider != null) rider.MoveWithPlatform(delta);

                if (Mathf.Approximately(newY, targetY))
                {
                    currentStop = targetStop;
                    targetStop = -1;
                    if (rider != null)
                    {
                        rider.EndPlatformRide();
                        rider.ShowActionMessage($"ARRIVED: {landings[currentStop].name.ToUpperInvariant()}", 1f);
                        rider = null;
                    }

                    state = ElevatorState.Opening;
                    audioEmitter?.PlayDoor(true);
                }
                break;

            case ElevatorState.Opening:
                doorOpenAmount = Mathf.MoveTowards(doorOpenAmount, 1f, doorSpeed / doorSlideDistance * Time.deltaTime);
                if (doorOpenAmount >= 1f) state = ElevatorState.Idle;
                break;
        }

        ApplyDoors();
    }

    private void ApplyDoors()
    {
        for (int i = 0; i < landings.Count; i++)
        {
            Landing landing = landings[i];
            float open = i == currentStop && state != ElevatorState.Moving ? doorOpenAmount : 0f;
            float slide = open * doorSlideDistance;
            if (landing.leftDoor != null) landing.leftDoor.position = landing.leftClosed + Vector3.left * slide;
            if (landing.rightDoor != null) landing.rightDoor.position = landing.rightClosed + Vector3.right * slide;
        }
    }

    private bool IsInsideCar(Vector3 worldPosition)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        return Mathf.Abs(local.x) <= interiorHalfExtents.x
            && Mathf.Abs(local.z) <= interiorHalfExtents.z
            && local.y >= -0.6f
            && local.y <= interiorHalfExtents.y;
    }

    private SpyPlayerController FindPlayer()
    {
        if (cachedPlayer == null) cachedPlayer = FindAnyObjectByType<SpyPlayerController>();
        return cachedPlayer;
    }

    private void OnDisable()
    {
        if (rider != null)
        {
            rider.EndPlatformRide();
            rider = null;
        }
    }
}
