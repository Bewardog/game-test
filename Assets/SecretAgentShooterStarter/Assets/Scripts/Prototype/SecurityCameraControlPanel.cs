using UnityEngine;

[DisallowMultipleComponent]
public class SecurityCameraControlPanel : MonoBehaviour, IInteractable
{
    public MissionSecurityCamera linkedCamera;
    public Renderer statusRenderer;

    private bool feedLooped;

    public string InteractionPrompt =>
        feedLooped || linkedCamera == null || linkedCamera.IsDisabled
            ? string.Empty
            : "Loop camera feed";

    public void Configure(MissionSecurityCamera camera, Renderer renderer)
    {
        linkedCamera = camera;
        statusRenderer = renderer;
        SetStatusColor(new Color(0.12f, 0.9f, 0.74f));
    }

    public void Interact(SpyPlayerController player)
    {
        if (feedLooped || linkedCamera == null || linkedCamera.IsDisabled) return;

        feedLooped = true;
        linkedCamera.DisableCamera("SECURITY FEED LOOPED", player);
        SetStatusColor(new Color(0.12f, 0.24f, 0.22f));
    }

    private void SetStatusColor(Color color)
    {
        if (statusRenderer != null) statusRenderer.material.color = color;
    }
}
