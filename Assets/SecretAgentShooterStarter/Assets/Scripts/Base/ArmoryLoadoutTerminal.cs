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
        // Weapons the armory has reached are unlocked when picked, so they also
        // appear on the number keys and the scroll wheel.
        SecretLairManager.Instance.UnlockWeapon(nextWeapon);
        player.WeaponController?.SetWeapon(nextWeapon, false);
        player.ShowActionMessage($"EQUIPPED: {nextWeapon.weaponName.ToUpperInvariant()}", 1.25f);
    }
}
