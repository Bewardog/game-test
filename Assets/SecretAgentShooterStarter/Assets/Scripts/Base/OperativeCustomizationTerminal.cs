using UnityEngine;

public class OperativeCustomizationTerminal : MonoBehaviour, IInteractable
{
    private static readonly string[] Outfits = { "Field Suit", "Stealth Suit", "Heavy Armor", "Formal Cover" };
    private int selectedRecruitIndex;

    public string InteractionPrompt
    {
        get
        {
            RecruitRoster roster = RecruitRoster.Instance;
            if (roster == null || roster.recruits.Count == 0) return "Customization: no recruits";
            RecruitedOperativeData recruit = roster.GetRecruit(selectedRecruitIndex % roster.recruits.Count);
            return $"Change {recruit.displayName}'s outfit";
        }
    }

    public void Interact(SpyPlayerController player)
    {
        RecruitRoster roster = RecruitRoster.Instance;
        if (roster == null || roster.recruits.Count == 0)
        {
            player?.ShowActionMessage("NO RECRUITS AVAILABLE", 1f);
            return;
        }

        selectedRecruitIndex %= roster.recruits.Count;
        RecruitedOperativeData recruit = roster.GetRecruit(selectedRecruitIndex);
        int currentOutfit = System.Array.IndexOf(Outfits, recruit.activeOutfit);
        recruit.activeOutfit = Outfits[(currentOutfit + 1 + Outfits.Length) % Outfits.Length];
        selectedRecruitIndex = (selectedRecruitIndex + 1) % roster.recruits.Count;
        CampaignPersistence.SaveCurrent();
        player?.ShowActionMessage($"{recruit.displayName.ToUpperInvariant()}: {recruit.activeOutfit.ToUpperInvariant()}", 1.4f);
    }
}
