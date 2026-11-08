using UnityEngine;

/// <summary>
/// Console in the mission room that starts a base defense: the syndicate
/// sets up a camp out in the expanse and starts sending attack waves.
/// </summary>
public class LairDefenseConsole : MonoBehaviour, IInteractable
{
    public string InteractionPrompt
    {
        get
        {
            LairDefenseController defense = LairDefenseController.Instance;
            if (defense == null) return string.Empty;
            return defense.State == LairDefenseController.DefenseState.Active
                ? "Base under attack - capture the enemy camp"
                : "Begin base defense";
        }
    }

    public void Interact(SpyPlayerController player)
    {
        LairDefenseController defense = LairDefenseController.Instance;
        if (defense == null || !defense.CanStart) return;
        defense.StartDefense();
    }
}
