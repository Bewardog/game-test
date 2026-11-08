using UnityEngine;

[DisallowMultipleComponent]
public class AbilityPulse : MonoBehaviour
{
    private float duration = 0.75f;
    private float targetScale = 3.5f;
    private float startedAt;
    private Vector3 initialScale;
    private Renderer pulseRenderer;
    private Color initialColor;

    public void Configure(float lifetime, float scale)
    {
        duration = Mathf.Max(0.1f, lifetime);
        targetScale = Mathf.Max(0.5f, scale);
        startedAt = Time.time;
        initialScale = transform.localScale;
        pulseRenderer = GetComponent<Renderer>();
        if (pulseRenderer != null) initialColor = pulseRenderer.material.color;
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        float t = Mathf.Clamp01((Time.time - startedAt) / duration);
        transform.localScale = Vector3.Lerp(initialScale, Vector3.one * targetScale, t);
        if (pulseRenderer != null)
        {
            Color faded = initialColor;
            faded.a = 1f - t;
            pulseRenderer.material.color = faded;
        }
    }
}
