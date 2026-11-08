using UnityEngine;

public class ExtractionZone : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Extract captive";

    public void Interact(SpyPlayerController player)
    {
        if (player == null || !player.IsCarrying) return;
        player.ExtractCarriedOperative();
    }

    private void OnTriggerStay(Collider other)
    {
        SpyPlayerController player = other.GetComponentInParent<SpyPlayerController>();
        if (player != null && player.IsCarrying && SpyInput.InteractPressed)
        {
            player.ExtractCarriedOperative();
        }
    }
}
