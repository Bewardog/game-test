using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MissionAlarmLightingController : MonoBehaviour
{
    public Color alarmColor = new Color(1f, 0.035f, 0.018f);
    public float pulseSpeed = 6f;
    public float minimumAlarmIntensity = 0.45f;
    public float maximumAlarmIntensity = 2.8f;
    [Range(0.25f, 1f)] public float alertedAmbientMultiplier = 0.58f;

    private Light[] ambientLights = System.Array.Empty<Light>();
    private Light[] alarmLights = System.Array.Empty<Light>();
    private Renderer[] beaconRenderers = System.Array.Empty<Renderer>();
    private float[] ambientBaseIntensities = System.Array.Empty<float>();
    private Color[] ambientBaseColors = System.Array.Empty<Color>();
    private bool alerted;

    private void OnEnable()
    {
        MissionManager.AlertStateChanged += HandleAlertStateChanged;
    }

    private void OnDisable()
    {
        MissionManager.AlertStateChanged -= HandleAlertStateChanged;
    }

    public void Configure(
        Transform missionRoot,
        List<Light> configuredAlarmLights,
        List<Renderer> configuredBeaconRenderers)
    {
        alarmLights = configuredAlarmLights != null
            ? configuredAlarmLights.ToArray()
            : System.Array.Empty<Light>();
        beaconRenderers = configuredBeaconRenderers != null
            ? configuredBeaconRenderers.ToArray()
            : System.Array.Empty<Renderer>();

        HashSet<Light> alarmSet = new HashSet<Light>(alarmLights);
        List<Light> ambient = new List<Light>();
        if (missionRoot != null)
        {
            foreach (Light candidate in missionRoot.GetComponentsInChildren<Light>(true))
            {
                if (candidate != null && !alarmSet.Contains(candidate))
                {
                    ambient.Add(candidate);
                }
            }
        }

        ambientLights = ambient.ToArray();
        ambientBaseIntensities = new float[ambientLights.Length];
        ambientBaseColors = new Color[ambientLights.Length];
        for (int index = 0; index < ambientLights.Length; index++)
        {
            ambientBaseIntensities[index] = ambientLights[index].intensity;
            ambientBaseColors[index] = ambientLights[index].color;
        }

        SetAlerted(MissionManager.Instance != null && MissionManager.Instance.IsAlerted);
    }

    private void Update()
    {
        if (!alerted) return;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minimumAlarmIntensity, maximumAlarmIntensity, pulse);
        foreach (Light alarmLight in alarmLights)
        {
            if (alarmLight == null) continue;
            alarmLight.intensity = intensity;
            alarmLight.color = alarmColor;
        }

        bool visiblePulse = pulse > 0.18f;
        foreach (Renderer beaconRenderer in beaconRenderers)
        {
            if (beaconRenderer != null)
            {
                beaconRenderer.enabled = visiblePulse;
            }
        }
    }

    private void HandleAlertStateChanged(MissionAlertState state)
    {
        SetAlerted(state == MissionAlertState.Alerted);
    }

    private void SetAlerted(bool value)
    {
        alerted = value;
        for (int index = 0; index < ambientLights.Length; index++)
        {
            Light ambient = ambientLights[index];
            if (ambient == null) continue;
            ambient.intensity = ambientBaseIntensities[index] *
                (alerted ? alertedAmbientMultiplier : 1f);
            ambient.color = alerted
                ? Color.Lerp(ambientBaseColors[index], alarmColor, 0.22f)
                : ambientBaseColors[index];
        }

        foreach (Light alarmLight in alarmLights)
        {
            if (alarmLight == null) continue;
            alarmLight.enabled = alerted;
            alarmLight.intensity = alerted ? minimumAlarmIntensity : 0f;
        }
        foreach (Renderer beaconRenderer in beaconRenderers)
        {
            if (beaconRenderer != null)
            {
                beaconRenderer.enabled = alerted;
            }
        }
    }
}
