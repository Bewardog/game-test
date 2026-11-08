using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MissionCheckpoint : MonoBehaviour
{
    public string checkpointId = "checkpoint";
    public Vector3 respawnPosition;
    public Vector3 respawnEuler;

    private bool activated;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || other == null) return;

        SpyPlayerController player = other.GetComponentInParent<SpyPlayerController>();
        MissionManager mission = MissionManager.Instance;
        if (player == null || mission == null) return;

        activated = true;
        mission.ActivateCheckpoint(
            checkpointId,
            player.transform.position + Vector3.up * 0.08f,
            player.transform.rotation,
            player);
    }
}
