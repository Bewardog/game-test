using UnityEngine;

public class MissionObjectiveInteractable : MonoBehaviour, IInteractable
{
    public string displayName = "Mission Item";
    public PickupType objectiveType = PickupType.Intel;

    private bool collected;

    public string InteractionPrompt
    {
        get
        {
            if (collected) return string.Empty;
            return objectiveType == PickupType.PrototypeWeapon
                ? $"Recover {displayName}"
                : $"Steal {displayName}";
        }
    }

    public void Interact(SpyPlayerController player)
    {
        if (collected || player == null) return;
        collected = true;
        MissionManager.Instance?.TriggerAlert($"{displayName} compromised");

        if (objectiveType == PickupType.PrototypeWeapon)
        {
            MissionManager.Instance?.NotifyPrototypeRecovered();
            WeaponDefinition prototype = GameFlowManager.Instance?.GetWeapon("tranq-crossbow");
            if (prototype != null)
            {
                SecretLairManager.Instance?.UnlockWeapon(prototype);
                player.WeaponController?.SetWeapon(prototype);
            }
            player.ShowActionMessage($"PROTOTYPE SECURED: {displayName.ToUpperInvariant()}", 1.5f);
        }
        else
        {
            MissionManager.Instance?.NotifyIntelStolen();
            player.ShowActionMessage($"INTEL ACQUIRED: {displayName.ToUpperInvariant()}", 1.5f);
        }

        gameObject.SetActive(false);
    }
}
