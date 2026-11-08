using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Day and night cycle for the lair grounds. Moves the sun across the sky,
/// switches to dim blue moonlight at night, and blends the sky, ambient light
/// and fog between day, sunset and night palettes.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Real-time minutes for one full day.")]
    public float dayLengthMinutes = 20f;

    [Tooltip("0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset.")]
    [Range(0f, 1f)] public float timeOfDay = 0.33f;

    [Tooltip("Compass direction the sun travels along.")]
    public float sunYaw = 35f;

    public bool paused;

    [Tooltip("Fog thickness. The grounds run on into the fog, so this sets how far you can see.")]
    public float fogDensity = 0.006f;

    private static readonly Color DaySun = new Color(1f, 0.95f, 0.86f);
    private static readonly Color SunsetSun = new Color(1f, 0.55f, 0.3f);
    private static readonly Color MoonLight = new Color(0.45f, 0.55f, 0.82f);
    private const float DaySunIntensity = 1.25f;
    private const float MoonIntensity = 0.16f;

    private static readonly Color DayAmbientSky = new Color(0.42f, 0.5f, 0.6f);
    private static readonly Color DayAmbientEquator = new Color(0.32f, 0.36f, 0.4f);
    private static readonly Color DayAmbientGround = new Color(0.14f, 0.16f, 0.14f);
    private static readonly Color NightAmbientSky = new Color(0.035f, 0.05f, 0.09f);
    private static readonly Color NightAmbientEquator = new Color(0.03f, 0.04f, 0.06f);
    private static readonly Color NightAmbientGround = new Color(0.015f, 0.02f, 0.02f);

    private static readonly Color DayFog = new Color(0.56f, 0.64f, 0.72f);
    private static readonly Color SunsetFog = new Color(0.62f, 0.44f, 0.36f);
    private static readonly Color NightFog = new Color(0.02f, 0.03f, 0.05f);

    private static readonly Color DaySkyTint = new Color(0.5f, 0.55f, 0.65f);
    private static readonly Color NightSkyTint = new Color(0.12f, 0.15f, 0.25f);

    private Light sun;
    private Material sky;
    private float nextEnvironmentUpdate;

    private void Start()
    {
        sun = RenderSettings.sun;
        if (sun == null)
        {
            foreach (Light candidate in FindObjectsByType<Light>())
            {
                if (candidate.type != LightType.Directional) continue;
                sun = candidate;
                break;
            }
        }

        sky = RenderSettings.skybox;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        Apply(true);
    }

    private void Update()
    {
        if (!paused && dayLengthMinutes > 0.01f)
        {
            timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime / (dayLengthMinutes * 60f), 1f);
        }

        Apply(false);
    }

    private void Apply(bool force)
    {
        // Elevation runs from -1 at midnight through 0 at sunrise/sunset to 1 at noon.
        float elevation = Mathf.Sin((timeOfDay - 0.25f) * Mathf.PI * 2f);
        float daylight = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elevation * 3f + 0.25f));
        float horizonGlow = Mathf.Clamp01(1f - Mathf.Abs(elevation) * 4f) * Mathf.Clamp01(elevation * 6f + 1f);

        if (sun != null)
        {
            float sunPitch = (timeOfDay - 0.25f) * 360f;
            bool moon = elevation < -0.05f;
            // At night the light comes from the moon, opposite the sun.
            float pitch = moon ? sunPitch - 180f : sunPitch;
            sun.transform.rotation = Quaternion.Euler(pitch, sunYaw, 0f);

            Color dayColor = Color.Lerp(DaySun, SunsetSun, horizonGlow);
            sun.color = moon ? MoonLight : dayColor;
            sun.intensity = moon
                ? MoonIntensity * Mathf.Clamp01(-elevation * 4f)
                : Mathf.Lerp(0.05f, DaySunIntensity, daylight);
        }

        RenderSettings.ambientSkyColor = Color.Lerp(NightAmbientSky, DayAmbientSky, daylight);
        RenderSettings.ambientEquatorColor = Color.Lerp(
            Color.Lerp(NightAmbientEquator, DayAmbientEquator, daylight),
            SunsetFog * 0.6f,
            horizonGlow * 0.4f);
        RenderSettings.ambientGroundColor = Color.Lerp(NightAmbientGround, DayAmbientGround, daylight);
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = Color.Lerp(Color.Lerp(NightFog, DayFog, daylight), SunsetFog, horizonGlow * 0.6f);

        if (sky != null)
        {
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", Mathf.Lerp(0.12f, 1f, daylight));
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", Mathf.Lerp(0.35f, 1f, daylight) + horizonGlow * 0.4f);
            if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", Color.Lerp(NightSkyTint, DaySkyTint, daylight));
        }

        // Refreshing the environment lighting is costly, so only do it now and then.
        if (force || Time.time >= nextEnvironmentUpdate)
        {
            nextEnvironmentUpdate = Time.time + 1f;
            DynamicGI.UpdateEnvironment();
        }
    }
}
