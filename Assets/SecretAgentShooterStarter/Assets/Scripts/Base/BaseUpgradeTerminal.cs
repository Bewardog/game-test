using UnityEngine;

public class BaseUpgradeTerminal : MonoBehaviour, IInteractable
{
    public SecretLairManager lairManager;
    public RoomType roomToUpgrade = RoomType.Armory;

    public string InteractionPrompt
    {
        get
        {
            if (lairManager == null) return $"Build {roomToUpgrade}";
            int level = lairManager.GetRoomLevel(roomToUpgrade);
            RoomState room = lairManager.rooms.Find(candidate => candidate.type == roomToUpgrade);
            int cost = room != null ? room.CurrentUpgradeCost : 0;
            return level == 0
                ? $"Build {roomToUpgrade} ({cost})"
                : $"Upgrade {roomToUpgrade} L{level + 1} ({cost})";
        }
    }

    private void Awake()
    {
        if (lairManager == null)
        {
            lairManager = FindAnyObjectByType<SecretLairManager>();
        }
    }

    public void Interact(SpyPlayerController player)
    {
        if (lairManager == null) return;

        bool upgraded = lairManager.TryUpgrade(roomToUpgrade);
        player?.ShowActionMessage(upgraded
            ? $"{roomToUpgrade.ToString().ToUpperInvariant()} LEVEL {lairManager.GetRoomLevel(roomToUpgrade)}"
            : "INSUFFICIENT CURRENCY OR MAX LEVEL", 1.25f);
        Debug.Log(upgraded
            ? $"Upgraded {roomToUpgrade} to level {lairManager.GetRoomLevel(roomToUpgrade)}."
            : $"Cannot upgrade {roomToUpgrade}. Need more currency or room is maxed.");
    }
}
