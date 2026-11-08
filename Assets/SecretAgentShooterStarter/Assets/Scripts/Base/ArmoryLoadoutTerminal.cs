using System.Collections.Generic;
using UnityEngine;

public class ArmoryLoadoutTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Cycle equipped weapon";

    public void Interact(SpyPlayerController player)
    {
        if (player == null || GameFlowManager.Instance == null || SecretLairManager.Instance == null) return;

        List<WeaponDefinition> available = GameFlowManager.Instance.GetArmoryAvailableWeapons();
        if (available.Count == 0)
        {
            player.ShowActionMessage("NO WEAPONS AVAILABLE", 1f);
            return;
        }

        int currentIndex = available.FindIndex(weapon => weapon.weaponId == SecretLairManager.Instance.equippedWeaponId);
        WeaponDefinition nextWeapon = available[(currentIndex + 1 + available.Count) % available.Count];
        SecretLairManager.Instance.EquipWeapon(nextWeapon);
        player.WeaponController?.SetWeapon(nextWeapon, false);
        player.ShowActionMessage($"EQUIPPED: {nextWeapon.weaponName.ToUpperInvariant()}", 1.25f);
    }
}
