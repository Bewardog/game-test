using UnityEngine;

public class MissionReturnTerminal : MonoBehaviour, IInteractable
{
    public string InteractionPrompt
    {
        get
        {
            MissionManager mission = MissionManager.Instance;
            return mission != null && mission.IsComplete()
                ? "Review mission report"
                : "Objectives incomplete";
        }
    }

    public void Interact(SpyPlayerController player)
    {
        MissionManager mission = MissionManager.Instance;
        if (mission == null || !mission.IsComplete())
        {
            player?.ShowActionMessage("COMPLETE THE MISSION FIRST", 1f);
            return;
        }

        if (player == null) return;

        MissionDebriefOverlay overlay = player.GetComponent<MissionDebriefOverlay>();
        if (overlay == null)
        {
            overlay = player.gameObject.AddComponent<MissionDebriefOverlay>();
        }

        overlay.Open(player, mission, player.WeaponController);
    }
}
