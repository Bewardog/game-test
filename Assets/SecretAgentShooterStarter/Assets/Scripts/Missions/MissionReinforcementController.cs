using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MissionReinforcementController : MonoBehaviour
{
    public List<GameObject> reserveOperatives = new List<GameObject>();
    public float initialDelay = 3.5f;
    public float deploymentInterval = 1.25f;

    private bool responseQueued;
    private float nextDeploymentTime;
    private int nextReserveIndex;
    private SpyPlayerController player;
    private PrototypeAudioEmitter audioEmitter;

    public void Configure(List<GameObject> reserves, float delay, float interval)
    {
        reserveOperatives = reserves ?? new List<GameObject>();
        initialDelay = Mathf.Max(0f, delay);
        deploymentInterval = Mathf.Max(0.2f, interval);
    }

    private void Awake()
    {
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(0f);
    }

    private void Update()
    {
        if (nextReserveIndex >= reserveOperatives.Count) return;

        MissionManager mission = MissionManager.Instance;
        if (mission == null || !mission.IsAlerted) return;
        if (player == null) player = FindAnyObjectByType<SpyPlayerController>();

        if (!responseQueued)
        {
            responseQueued = true;
            nextDeploymentTime = Time.time + initialDelay;
            player?.ShowActionMessage("SYNDICATE RESPONSE TEAM INBOUND", 1.2f);
            audioEmitter?.PlaySecurityAlarm();
            return;
        }

        if (Time.time < nextDeploymentTime) return;

        GameObject reserve = reserveOperatives[nextReserveIndex++];
        if (reserve != null) reserve.SetActive(true);
        nextDeploymentTime = Time.time + deploymentInterval;

        if (nextReserveIndex >= reserveOperatives.Count)
        {
            player?.ShowActionMessage("RESPONSE TEAM DEPLOYED", 0.9f);
        }
    }
}
