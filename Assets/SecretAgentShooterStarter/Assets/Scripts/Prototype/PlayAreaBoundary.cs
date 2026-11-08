using UnityEngine;

/// <summary>
/// An invisible limit around the lair grounds that only applies to the player.
/// There is no wall: if the player walks past the radius they are held at the
/// edge and told to head back. Enemies and other characters are unaffected, so
/// attack waves can approach from beyond it.
/// </summary>
public class PlayAreaBoundary : MonoBehaviour
{
    public Vector3 center;
    public float radius = 220f;

    private SpyPlayerController player;
    private float nextMessageTime;

    private void LateUpdate()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<SpyPlayerController>();
            if (player == null) return;
        }

        Vector3 position = player.transform.position;
        Vector3 offset = position - center;
        offset.y = 0f;
        float distance = offset.magnitude;
        if (distance <= radius) return;

        Vector3 clamped = center + offset / distance * radius;
        clamped.y = position.y;
        player.Teleport(clamped);

        if (Time.time >= nextMessageTime)
        {
            nextMessageTime = Time.time + 2f;
            player.ShowActionMessage("RETURN TO THE BASE", 1.2f);
        }
    }
}
