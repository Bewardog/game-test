using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Day and night cycle. Moves the sun across the sky, switches to dim blue
/// moonlight at night, and blends the sky, ambient light and fog between day,
/// sunset and night.
///
/// In the lair it uses a fixed daytime palette and keeps the clock running
/// between visits. In missions it captures the mission's own lighting as its
/// daytime look and starts at the mission's designed time of day, so each
/// environment keeps its character as the day turns.
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

    [Tooltip("Fog thickness. 0 keeps whatever the scene already uses.")]
    public float fogDensity;

    [Tooltip("Use the scene's existing lighting as the daytime look (missions).")]
    public bool captureScenePalette;

    [Tooltip("Carry the time of day over between visits (the lair).")]
    public bool continueSharedTime;

    private static float sharedTime = -1f;

    private static readonly Color SunsetSun = new Color(1f, 0.55f, 0.3f);
    private static readonly Color MoonLight = new Color(0.45f, 0.55f, 0.82f);
    private const float MoonIntensity = 0.16f;
    private static readonly Color NightAmbientSky = new Color(0.035f, 0.05f, 0.09f);
    private static readonly Color NightAmbientEquator = new Color(0.03f, 0.04f, 0.06f);
    private static readonly Color NightAmbientGround = new Color(0.015f, 0.02f, 0.02f);
    private static readonly Color SunsetFog = new Color(0.62f, 0.44f, 0.36f);
    private static readonly Color NightFog = new Color(0.02f, 0.03f, 0.05f);
    private static readonly Color NightSkyTint = new Color(0.12f, 0.15f, 0.25f);

    // Daytime palette: defaults, optionally replaced by the scene's own lighting.
    private Color daySun = new Color(1f, 0.95f, 0.86f);
    private float daySunIntensity = 1.25f;
    private Color dayAmbientSky = new Color(0.42f, 0.5f, 0.6f);
    private Color dayAmbientEquator = new Color(0.32f, 0.36f, 0.4f);
    private Color dayAmbientGround = new Color(0.14f, 0.16f, 0.14f);
    private Color dayFog = new Color(0.56f, 0.64f, 0.72f);
    private Color daySkyTint = new Color(0.5f, 0.55f, 0.65f);
    private float dayExposure = 1f;
    private float dayAtmosphere = 1f;

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
        if (captureScenePalette) CaptureScenePalette();
        if (continueSharedTime && sharedTime >= 0f) timeOfDay = sharedTime;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.fog = true;
        if (fogDensity > 0f)
        {
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
        }

        Apply(true);
    }

    private void CaptureScenePalette()
    {
        bool nightScene = sun != null && sun.intensity < 0.6f;
        if (!nightScene)
        {
            if (sun != null)
            {
                daySun = sun.color;
                daySunIntensity = sun.intensity;
                // Start at the time of day the mission was designed for.
                float pitch = Mathf.DeltaAngle(0f, sun.transform.eulerAngles.x);
                timeOfDay = Mathf.Repeat(0.25f + Mathf.Clamp(pitch, 5f, 175f) / 360f, 1f);
                sunYaw = sun.transform.eulerAngles.y;
            }

            dayAmbientSky = RenderSettings.ambientSkyColor;
            dayAmbientEquator = RenderSettings.ambientEquatorColor;
            dayAmbientGround = RenderSettings.ambientGroundColor;
            dayFog = RenderSettings.fogColor;
            if (sky != null)
            {
                if (sky.HasProperty("_SkyTint")) daySkyTint = sky.GetColor("_SkyTint");
                if (sky.HasProperty("_Exposure")) dayExposure = Mathf.Max(0.6f, sky.GetFloat("_Exposure"));
                if (sky.HasProperty("_AtmosphereThickness")) dayAtmosphere = Mathf.Max(0.6f, sky.GetFloat("_AtmosphereThickness"));
            }
        }
        else
        {
            // Night missions keep the default daytime look and start at night.
            timeOfDay = 0.92f;
        }
    }

    private void Update()
    {
        if (!paused && dayLengthMinutes > 0.01f)
        {
            timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime / (dayLengthMinutes * 60f), 1f);
        }

        if (continueSharedTime) sharedTime = timeOfDay;
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
            sun.color = moon ? MoonLight : Color.Lerp(daySun, SunsetSun, horizonGlow);
            sun.intensity = moon
                ? MoonIntensity * Mathf.Clamp01(-elevation * 4f)
                : Mathf.Lerp(0.05f, daySunIntensity, daylight);
        }

        RenderSettings.ambientSkyColor = Color.Lerp(NightAmbientSky, dayAmbientSky, daylight);
        RenderSettings.ambientEquatorColor = Color.Lerp(
            Color.Lerp(NightAmbientEquator, dayAmbientEquator, daylight),
            SunsetFog * 0.6f,
            horizonGlow * 0.4f);
        RenderSettings.ambientGroundColor = Color.Lerp(NightAmbientGround, dayAmbientGround, daylight);
        RenderSettings.fogColor = Color.Lerp(Color.Lerp(NightFog, dayFog, daylight), SunsetFog, horizonGlow * 0.6f);

        if (sky != null)
        {
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", Mathf.Lerp(0.12f, dayExposure, daylight));
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", Mathf.Lerp(0.35f, dayAtmosphere, daylight) + horizonGlow * 0.4f);
            if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", Color.Lerp(NightSkyTint, daySkyTint, daylight));
        }

        // Refreshing the environment lighting is costly, so only do it now and then.
        if (force || Time.time >= nextEnvironmentUpdate)
        {
            nextEnvironmentUpdate = Time.time + 1f;
            DynamicGI.UpdateEnvironment();
        }
    }
}
