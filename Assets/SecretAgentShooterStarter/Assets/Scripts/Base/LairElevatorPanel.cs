using UnityEngine;

public class LairElevatorPanel : MonoBehaviour, IInteractable
{
    public string floorLabel = "Floor 2";
    public Vector3 destination;

    public string InteractionPrompt => $"Take elevator to {floorLabel}";

    public void Interact(SpyPlayerController player)
    {
        if (player == null) return;
        player.Teleport(destination);
        player.ShowActionMessage($"ARRIVED: {floorLabel.ToUpperInvariant()}", 1f);
    }
}
