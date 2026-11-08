public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(SpyPlayerController player);
}

