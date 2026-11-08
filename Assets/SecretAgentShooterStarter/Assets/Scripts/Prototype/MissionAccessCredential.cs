using UnityEngine;

[DisallowMultipleComponent]
public class MissionAccessCredential : MonoBehaviour, IInteractable
{
    public int clearanceLevel = 1;
    public float hoverHeight = 0.12f;
    public float hoverSpeed = 2.2f;
    public float rotationSpeed = 42f;

    private Vector3 basePosition;
    private PrototypeAudioEmitter audioEmitter;

    public string InteractionPrompt => $"Take clearance {clearanceLevel} keycard";

    public void Configure(int level)
    {
        clearanceLevel = Mathf.Clamp(level, 1, 3);
    }

    private void Start()
    {
        basePosition = transform.position;
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(1f);
    }

    private void Update()
    {
        transform.position = basePosition +
            Vector3.up * (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    public void Interact(SpyPlayerController player)
    {
        if (player == null) return;

        player.GrantMissionClearance(clearanceLevel);
        audioEmitter?.PlaySecurityDisabled();
        Destroy(gameObject);
    }
}
