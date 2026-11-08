using UnityEngine;

public class ResearchProjectTerminal : MonoBehaviour, IInteractable
{
    public ResearchTrack track;

    public string InteractionPrompt
    {
        get
        {
            SecretLairManager lair = SecretLairManager.Instance;
            if (lair == null) return "Research unavailable";
            int level = lair.GetResearchLevel(track);
            return $"Research {FormatTrack()} L{level + 1} ({lair.GetResearchCost(track)} credits)";
        }
    }

    public void Interact(SpyPlayerController player)
    {
        SecretLairManager lair = SecretLairManager.Instance;
        if (lair == null) return;

        if (lair.GetRoomLevel(RoomType.ResearchLab) <= lair.GetResearchLevel(track))
        {
            player?.ShowActionMessage("UPGRADE THE RESEARCH LAB FIRST", 1.2f);
            return;
        }

        if (!lair.TryResearch(track))
        {
            player?.ShowActionMessage("INSUFFICIENT CREDITS", 1f);
            return;
        }

        player?.ShowActionMessage($"{FormatTrack().ToUpperInvariant()} RESEARCH COMPLETE", 1.3f);
    }

    private string FormatTrack()
    {
        return track == ResearchTrack.Tranquilizers ? "Tranquilizers" : track.ToString();
    }
}
