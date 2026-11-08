using System.Collections.Generic;
using UnityEngine;

public class PrototypeHUD : MonoBehaviour
{
    private static readonly Color PanelColor = new Color(0.015f, 0.025f, 0.035f, 0.82f);
    private static readonly Color PanelBorder = new Color(0.2f, 0.72f, 0.7f, 0.78f);
    private static readonly Color TextColor = new Color(0.86f, 0.92f, 0.94f);
    private static readonly Color MutedTextColor = new Color(0.58f, 0.67f, 0.7f);
    private static readonly Color HealthColor = new Color(0.2f, 0.82f, 0.46f);
    private static readonly Color ArmorColor = new Color(0.22f, 0.62f, 0.9f);
    private static readonly Color ObjectiveColor = new Color(1f, 0.58f, 0.16f);

    public SpyPlayerController player;
    public SpyWeaponController weapon;

    private GUIStyle tinyStyle;
    private GUIStyle smallStyle;
    private GUIStyle smallBoldStyle;
    private GUIStyle valueStyle;
    private GUIStyle centeredStyle;
    private GUIStyle mapTitleStyle;
    private GUIStyle mapLabelStyle;
    private bool mapVisible;
    private Bounds mapWorldBounds;
    private readonly List<MapGeometry> mapGeometry = new List<MapGeometry>();

    public bool MapVisible => mapVisible;

    private void Update()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        bool missionActive = flow != null && flow.State == GameFlowState.Mission;

        if (mapVisible && !missionActive)
        {
            SetMapVisible(false);
            return;
        }

        if (SpyInput.MapPressed && missionActive && player != null && (!player.ControlsLocked || mapVisible))
        {
            SetMapVisible(!mapVisible);
        }
    }

    private void OnDisable()
    {
        if (mapVisible && player != null)
        {
            mapVisible = false;
            player.SetControlsLocked(false);
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        if (mapVisible)
        {
            DrawMissionMap();
            DrawTransition();
            return;
        }

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        bool enemyHit = weapon != null && weapon.EnemyHitRecently && Time.time <= weapon.FeedbackUntil;
        Color crosshairColor = enemyHit ? new Color(1f, 0.25f, 0.7f) : Color.white;

        DrawRect(new Rect(centerX - 7f, centerY - 1f, 14f, 2f), Color.black);
        DrawRect(new Rect(centerX - 1f, centerY - 7f, 2f, 14f), Color.black);
        DrawRect(new Rect(centerX - 5f, centerY - 1f, 10f, 2f), crosshairColor);
        DrawRect(new Rect(centerX - 1f, centerY - 5f, 2f, 10f), crosshairColor);

        DrawCampaignStatus();
        DrawVitals();
        DrawAmmo();
        DrawCarryStatus();
        DrawFeedback(centerX, centerY);
        DrawDamageDirection(centerX, centerY);
        DrawInteractionPrompt(centerX);
        DrawControls();

        if (player != null && Time.time <= player.DamageFlashUntil)
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.65f, 0f, 0f, 0.18f));
        }

        DrawTransition();
    }

    private void DrawCampaignStatus()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        if (flow == null) return;

        string location = flow.State == GameFlowState.Mission && flow.CurrentMission != null
            ? flow.CurrentMission.displayName.ToUpperInvariant()
            : "SECRET LAIR";
        string details = location;

        if (flow.State == GameFlowState.Mission && MissionManager.Instance != null)
        {
            details += "\n" + flow.GetDifficultyProfile().displayName;
            details += "\n" + MissionManager.Instance.GetAlertSummary();
            string observedThreats = GetObservedThreatSummary();
            if (!string.IsNullOrEmpty(observedThreats))
            {
                details += "\n" + observedThreats;
            }
            details += "\n" + MissionManager.Instance.GetObjectiveSummary();
        }
        else
        {
            int currency = SecretLairManager.Instance != null ? SecretLairManager.Instance.currency : 0;
            details += $"\nCURRENCY  {currency}";
            string defense = LairDefenseController.Instance != null ? LairDefenseController.Instance.StatusText : string.Empty;
            if (!string.IsNullOrEmpty(defense)) details += "\n" + defense;
        }

        int lineCount = details.Split('\n').Length;
        Rect panel = new Rect(16f, 16f, 270f, 14f + lineCount * 16f);
        DrawPanel(panel);
        GUI.Label(new Rect(panel.x + 9f, panel.y + 6f, panel.width - 18f, panel.height - 12f), details, tinyStyle);
    }

    private void DrawVitals()
    {
        if (player == null) return;

        Rect panel = new Rect(16f, Screen.height - 76f, 222f, 58f);
        DrawPanel(panel);

        float health = Mathf.Ceil(player.CurrentHealth);
        GUI.Label(new Rect(panel.x + 10f, panel.y + 5f, 96f, 22f), $"HEALTH  {health:0}", smallBoldStyle);
        GUI.Label(new Rect(panel.x + 116f, panel.y + 5f, 96f, 22f), $"ARMOR  {player.Armor}", smallBoldStyle);

        DrawMeter(
            new Rect(panel.x + 10f, panel.y + 34f, 96f, 9f),
            player.CurrentHealth / Mathf.Max(1f, player.maxHealth),
            HealthColor);
        DrawMeter(
            new Rect(panel.x + 116f, panel.y + 34f, 96f, 9f),
            player.Armor / Mathf.Max(1f, player.maxArmor),
            ArmorColor);
    }

    private void DrawAmmo()
    {
        string weaponName = weapon != null && weapon.currentWeapon != null
            ? weapon.currentWeapon.weaponName.ToUpperInvariant()
            : "TRANQUILIZER PISTOL";

        Rect panel = new Rect(Screen.width - 260f, Screen.height - 82f, 244f, 64f);
        DrawPanel(panel);
        GUI.Label(new Rect(panel.x + 10f, panel.y + 5f, panel.width - 20f, 18f), weaponName, tinyStyle);

        string magazine = weapon != null ? weapon.magazineAmmo.ToString("00") : "--";
        string reserve = weapon != null ? weapon.reserveAmmo.ToString("000") : "---";
        GUI.Label(new Rect(panel.x + 10f, panel.y + 22f, 84f, 34f), magazine, valueStyle);
        GUI.Label(new Rect(panel.x + 87f, panel.y + 31f, 18f, 22f), "/", smallBoldStyle);
        GUI.Label(new Rect(panel.x + 104f, panel.y + 31f, 72f, 22f), reserve, smallBoldStyle);

        string status = weapon != null && weapon.IsReloading ? "RELOADING" : "MAG / RESERVE";
        GUI.Label(new Rect(panel.x + 166f, panel.y + 29f, 70f, 25f), status, tinyStyle);
    }

    private void DrawCarryStatus()
    {
        if (player == null) return;
        if (!player.IsCarrying && !player.IsCrouching) return;

        Rect panel = new Rect(Screen.width - 260f, 16f, 244f, 34f);
        DrawPanel(panel);
        string status = player.IsCarrying && player.CarriedOperative != null
            ? $"CARRYING  {player.CarriedOperative.name.ToUpperInvariant()}"
            : "CROUCHED // LOW VISIBILITY";
        GUI.Label(
            new Rect(panel.x + 8f, panel.y + 5f, panel.width - 16f, panel.height - 10f),
            status,
            tinyStyle);
    }

    private void DrawFeedback(float centerX, float centerY)
    {
        string feedback = string.Empty;
        if (player != null && Time.time <= player.ActionMessageUntil)
        {
            feedback = player.ActionMessage;
        }
        else if (weapon != null && Time.time <= weapon.FeedbackUntil)
        {
            feedback = weapon.FeedbackMessage;
        }

        if (string.IsNullOrEmpty(feedback)) return;

        Rect panel = new Rect(centerX - 145f, centerY + 24f, 290f, 28f);
        DrawRect(panel, new Color(0f, 0f, 0f, 0.64f));
        GUI.Label(panel, feedback, centeredStyle);
    }

    private void DrawInteractionPrompt(float centerX)
    {
        if (player == null || string.IsNullOrEmpty(player.CurrentInteractionPrompt)) return;

        Rect panel = new Rect(centerX - 155f, Screen.height - 120f, 310f, 32f);
        DrawPanel(panel);
        GUI.Label(panel, player.CurrentInteractionPrompt, centeredStyle);
    }

    private void DrawDamageDirection(float centerX, float centerY)
    {
        if (player == null || Time.time > player.DamageDirectionUntil) return;

        Vector3 localDirection = player.transform.InverseTransformDirection(player.LastDamageDirection);
        float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float fade = Mathf.Clamp01((player.DamageDirectionUntil - Time.time) / 0.85f);
        Color indicatorColor = new Color(1f, 0.18f, 0.12f, 0.35f + fade * 0.65f);
        Vector2 pivot = new Vector2(centerX, centerY);
        Matrix4x4 previousMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, pivot);
        DrawRect(new Rect(centerX - 20f, centerY - 92f, 40f, 4f), Color.black);
        DrawRect(new Rect(centerX - 16f, centerY - 91f, 32f, 2f), indicatorColor);
        DrawRect(new Rect(centerX - 2f, centerY - 101f, 4f, 12f), indicatorColor);
        GUI.matrix = previousMatrix;
    }

    private void DrawControls()
    {
        string controls = "CTRL  CROUCH    1-4 / WHEEL  WEAPONS    R  RELOAD    F  PUNCH    Q  KNIFE    M  MAP    V  CAMERA    B  CARRY";
        GUI.Label(new Rect(252f, Screen.height - 25f, Mathf.Max(0f, Screen.width - 528f), 18f), controls, tinyStyle);
    }

    private void SetMapVisible(bool visible)
    {
        mapVisible = visible;
        if (mapVisible)
        {
            CacheMapGeometry();
        }

        player?.SetControlsLocked(mapVisible);
    }

    private void CacheMapGeometry()
    {
        mapGeometry.Clear();
        GameObject missionFloor = GameObject.Find("Mission Floor");
        Renderer floorRenderer = missionFloor != null ? missionFloor.GetComponent<Renderer>() : null;
        mapWorldBounds = floorRenderer != null
            ? floorRenderer.bounds
            : new Bounds(player != null ? player.transform.position : Vector3.zero, new Vector3(80f, 1f, 120f));

        GameObject runtimeRoot = GameObject.Find("Mission Runtime World");
        if (runtimeRoot == null) return;

        Renderer[] renderers = runtimeRoot.GetComponentsInChildren<Renderer>(false);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !ShouldDrawMapGeometry(renderer.gameObject.name)) continue;
            Bounds bounds = renderer.bounds;
            if (bounds.size.x < 0.2f || bounds.size.z < 0.2f) continue;

            string objectName = renderer.gameObject.name;
            Color color = objectName.Contains("Route") || objectName.Contains("Trail")
                ? new Color(0.2f, 0.42f, 0.44f, 0.75f)
                : objectName.Contains("Cover") || objectName.Contains("Cache")
                    ? new Color(0.38f, 0.46f, 0.48f, 0.9f)
                    : new Color(0.2f, 0.28f, 0.31f, 0.96f);
            string label = objectName.EndsWith(" Foundation")
                ? objectName.Substring(0, objectName.Length - " Foundation".Length)
                : string.Empty;
            mapGeometry.Add(new MapGeometry(bounds, color, label));
        }
    }

    private static bool ShouldDrawMapGeometry(string objectName)
    {
        if (string.IsNullOrEmpty(objectName) || objectName == "Mission Floor") return false;
        return objectName.EndsWith(" Foundation") ||
            objectName.Contains("Perimeter") ||
            objectName.Contains("Fence") ||
            objectName.Contains("Main Gate") ||
            objectName.Contains("Gatehouse") ||
            objectName.Contains("Cover") ||
            objectName.Contains("Cache") ||
            objectName.Contains("Route") ||
            objectName.Contains("Trail") ||
            objectName.Contains("Transit Corridor") ||
            objectName.Contains("Wing Partition") ||
            objectName.Contains("Security Gate");
    }

    private void DrawMissionMap()
    {
        DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.01f, 0.015f, 0.02f, 0.96f));

        Rect frame = new Rect(28f, 26f, Screen.width - 56f, Screen.height - 52f);
        DrawPanel(frame, new Color(0.02f, 0.045f, 0.055f, 0.98f));

        MissionDefinition mission = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentMission : null;
        string title = mission != null ? mission.displayName.ToUpperInvariant() : "TACTICAL MAP";
        int currentFloor = player != null ? Mathf.Max(1, Mathf.FloorToInt(player.transform.position.y / 3.4f) + 1) : 1;
        GUI.Label(new Rect(frame.x + 18f, frame.y + 12f, frame.width - 36f, 32f), $"TACTICAL MAP  //  {title}  //  FLOOR {currentFloor}", mapTitleStyle);
        GUI.Label(
            new Rect(frame.xMax - 180f, frame.y + 17f, 150f, 22f),
            "M  CLOSE",
            mapLabelStyle);

        Rect availableMapArea = new Rect(frame.x + 22f, frame.y + 54f, frame.width - 260f, frame.height - 76f);
        Rect mapRect = FitMapRect(availableMapArea);
        DrawRect(mapRect, new Color(0.045f, 0.07f, 0.075f, 1f));
        DrawOutline(mapRect, PanelBorder, 2f);

        foreach (MapGeometry geometry in mapGeometry)
        {
            Rect geometryRect = WorldBoundsToMapRect(geometry.bounds, mapRect);
            DrawRect(geometryRect, geometry.color);
            if (!string.IsNullOrEmpty(geometry.label) && geometryRect.width >= 42f && geometryRect.height >= 18f)
            {
                GUI.Label(geometryRect, geometry.label.ToUpperInvariant(), mapLabelStyle);
            }
        }

        DrawLiveMapMarkers(mapRect);
        DrawMapLegend(new Rect(frame.xMax - 212f, frame.y + 70f, 178f, 398f));

        if (MissionManager.Instance != null)
        {
            Rect objectivePanel = new Rect(frame.xMax - 224f, frame.yMax - 180f, 194f, 132f);
            DrawPanel(objectivePanel, new Color(0.015f, 0.025f, 0.03f, 0.96f));
            GUI.Label(new Rect(objectivePanel.x + 10f, objectivePanel.y + 8f, objectivePanel.width - 20f, 18f), "OBJECTIVES", smallBoldStyle);
            GUI.Label(
                new Rect(objectivePanel.x + 10f, objectivePanel.y + 29f, objectivePanel.width - 20f, objectivePanel.height - 38f),
                MissionManager.Instance.GetObjectiveSummary(),
                tinyStyle);
        }
    }

    private Rect FitMapRect(Rect available)
    {
        float worldWidth = Mathf.Max(1f, mapWorldBounds.size.x);
        float worldDepth = Mathf.Max(1f, mapWorldBounds.size.z);
        float worldAspect = worldWidth / worldDepth;
        float areaAspect = available.width / Mathf.Max(1f, available.height);

        if (areaAspect > worldAspect)
        {
            float proportionalWidth = available.height * worldAspect;
            float minimumReadableWidth = available.width * 0.38f;
            float width = Mathf.Min(available.width, Mathf.Max(proportionalWidth, minimumReadableWidth));
            return new Rect(available.center.x - width * 0.5f, available.y, width, available.height);
        }

        float height = available.width / worldAspect;
        return new Rect(available.x, available.center.y - height * 0.5f, available.width, height);
    }

    private void DrawLiveMapMarkers(Rect mapRect)
    {
        EnemyOperative[] enemies = FindObjectsByType<EnemyOperative>();
        foreach (EnemyOperative enemy in enemies)
        {
            if (enemy == null || enemy.isDead || enemy.isRecruited) continue;
            EnemyCombatController awareness = enemy.GetComponent<EnemyCombatController>();
            if (awareness != null && !awareness.HasBeenObserved) continue;
            Color enemyColor = enemy.isKnockedOut
                ? new Color(0.48f, 0.52f, 0.54f)
                : EnemyCombatController.GetSpecializationColor(enemy.Specialization);
            DrawEnemyMapMarker(
                WorldToMap(enemy.transform.position, mapRect),
                enemyColor,
                enemy.isKnockedOut ? EnemyAwarenessState.Calm : awareness != null
                    ? awareness.AwarenessState
                    : EnemyAwarenessState.Calm,
                enemy.Rank == EnemyRank.Commander ? 10f : 7f);
        }

        MissionObjectiveInteractable[] objectives = FindObjectsByType<MissionObjectiveInteractable>();
        foreach (MissionObjectiveInteractable objective in objectives)
        {
            if (objective != null)
            {
                DrawMapMarker(WorldToMap(objective.transform.position, mapRect), ObjectiveColor, 10f);
            }
        }

        FieldPickup[] pickups = FindObjectsByType<FieldPickup>();
        foreach (FieldPickup pickup in pickups)
        {
            if (pickup == null) continue;
            Color pickupColor = pickup.pickupType == PickupType.Health
                ? HealthColor
                : pickup.pickupType == PickupType.Weapon
                    ? new Color(0.72f, 0.36f, 0.95f)
                    : new Color(0.25f, 0.65f, 0.95f);
            DrawMapMarker(WorldToMap(pickup.transform.position, mapRect), pickupColor, 5f);
        }

        GameObject extraction = GameObject.Find("Extraction Zone");
        if (extraction != null)
        {
            DrawMapMarker(WorldToMap(extraction.transform.position, mapRect), new Color(0.15f, 0.95f, 0.45f), 11f);
        }

        if (player != null)
        {
            Vector2 playerPoint = WorldToMap(player.transform.position, mapRect);
            DrawMapMarker(playerPoint, new Color(0.1f, 0.9f, 1f), 11f);
            Vector3 facingWorld = player.transform.position + player.transform.forward * (mapWorldBounds.size.z * 0.035f);
            DrawLine(playerPoint, WorldToMap(facingWorld, mapRect), Color.white, 2f);
        }
    }

    private void DrawMapLegend(Rect rect)
    {
        DrawPanel(rect, new Color(0.015f, 0.025f, 0.03f, 0.96f));
        GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 18f), "LEGEND", smallBoldStyle);
        DrawLegendRow(rect, 34f, new Color(0.1f, 0.9f, 1f), "AGENT");
        DrawLegendRow(rect, 60f, ObjectiveColor, "OBJECTIVE");
        DrawLegendRow(rect, 86f, new Color(0.15f, 0.95f, 0.45f), "EXTRACTION");
        DrawLegendRow(rect, 112f, EnemyCombatController.GetSpecializationColor(OperativeSpecialization.HandToHand), "HAND TO HAND");
        DrawLegendRow(rect, 138f, EnemyCombatController.GetSpecializationColor(OperativeSpecialization.Stealth), "STEALTH");
        DrawLegendRow(rect, 164f, EnemyCombatController.GetSpecializationColor(OperativeSpecialization.Tank), "TANK");
        DrawLegendRow(rect, 190f, EnemyCombatController.GetSpecializationColor(OperativeSpecialization.Marksman), "MARKSMAN");
        DrawLegendRow(rect, 216f, EnemyCombatController.GetSpecializationColor(OperativeSpecialization.Hacker), "HACKER");
        DrawLegendRow(rect, 242f, EnemyCombatController.GetSpecializationColor(OperativeSpecialization.Medic), "MEDIC");
        DrawLegendRow(rect, 268f, new Color(0.72f, 0.36f, 0.95f), "WEAPON");
        DrawLegendRow(rect, 294f, HealthColor, "SUPPLY / HEALTH");
        DrawAwarenessLegendRow(rect, 320f, new Color(1f, 0.72f, 0.12f), "SUSPICIOUS");
        DrawAwarenessLegendRow(rect, 346f, new Color(1f, 0.42f, 0.08f), "SEARCHING");
        DrawAwarenessLegendRow(rect, 372f, new Color(1f, 0.12f, 0.1f), "ALERTED");
    }

    private void DrawLegendRow(Rect parent, float yOffset, Color color, string label)
    {
        DrawMapMarker(new Vector2(parent.x + 17f, parent.y + yOffset + 7f), color, 8f);
        GUI.Label(new Rect(parent.x + 30f, parent.y + yOffset, parent.width - 38f, 18f), label, tinyStyle);
    }

    private void DrawAwarenessLegendRow(Rect parent, float yOffset, Color borderColor, string label)
    {
        Vector2 center = new Vector2(parent.x + 17f, parent.y + yOffset + 7f);
        DrawRect(new Rect(center.x - 6f, center.y - 6f, 12f, 12f), borderColor);
        DrawRect(
            new Rect(center.x - 3f, center.y - 3f, 6f, 6f),
            EnemyCombatController.GetSpecializationColor(OperativeSpecialization.HandToHand));
        GUI.Label(new Rect(parent.x + 30f, parent.y + yOffset, parent.width - 38f, 18f), label, tinyStyle);
    }

    private Vector2 WorldToMap(Vector3 world, Rect mapRect)
    {
        float x = Mathf.InverseLerp(mapWorldBounds.min.x, mapWorldBounds.max.x, world.x);
        float z = Mathf.InverseLerp(mapWorldBounds.min.z, mapWorldBounds.max.z, world.z);
        return new Vector2(
            Mathf.Lerp(mapRect.x, mapRect.xMax, x),
            Mathf.Lerp(mapRect.yMax, mapRect.y, z));
    }

    private Rect WorldBoundsToMapRect(Bounds bounds, Rect mapRect)
    {
        Vector2 min = WorldToMap(new Vector3(bounds.min.x, 0f, bounds.max.z), mapRect);
        Vector2 max = WorldToMap(new Vector3(bounds.max.x, 0f, bounds.min.z), mapRect);
        return new Rect(
            min.x,
            min.y,
            Mathf.Max(1.5f, max.x - min.x),
            Mathf.Max(1.5f, max.y - min.y));
    }

    private void DrawMapMarker(Vector2 center, Color color, float size)
    {
        DrawRect(new Rect(center.x - size * 0.5f - 1f, center.y - size * 0.5f - 1f, size + 2f, size + 2f), Color.black);
        DrawRect(new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size), color);
    }

    private void DrawEnemyMapMarker(
        Vector2 center,
        Color roleColor,
        EnemyAwarenessState awareness,
        float size)
    {
        Color borderColor;
        switch (awareness)
        {
            case EnemyAwarenessState.Suspicious:
                borderColor = new Color(1f, 0.72f, 0.12f);
                break;
            case EnemyAwarenessState.Searching:
                borderColor = new Color(1f, 0.42f, 0.08f);
                break;
            case EnemyAwarenessState.Alerted:
                borderColor = new Color(1f, 0.12f, 0.1f);
                break;
            default:
                borderColor = Color.black;
                break;
        }

        float border = awareness == EnemyAwarenessState.Calm ? 1f : 3f;
        DrawRect(
            new Rect(
                center.x - size * 0.5f - border,
                center.y - size * 0.5f - border,
                size + border * 2f,
                size + border * 2f),
            borderColor);
        DrawRect(
            new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size),
            roleColor);
    }

    private static string GetObservedThreatSummary()
    {
        EnemyCombatController[] contacts = FindObjectsByType<EnemyCombatController>();
        int suspicious = 0;
        int searching = 0;
        int alerted = 0;
        foreach (EnemyCombatController contact in contacts)
        {
            if (contact == null || !contact.HasBeenObserved) continue;
            switch (contact.AwarenessState)
            {
                case EnemyAwarenessState.Suspicious:
                    suspicious++;
                    break;
                case EnemyAwarenessState.Searching:
                    searching++;
                    break;
                case EnemyAwarenessState.Alerted:
                    alerted++;
                    break;
            }
        }

        if (alerted > 0) return $"OBSERVED CONTACTS  ALERTED {alerted}";
        if (searching > 0) return $"OBSERVED CONTACTS  SEARCHING {searching}";
        if (suspicious > 0) return $"OBSERVED CONTACTS  SUSPICIOUS {suspicious}";
        return string.Empty;
    }

    private void DrawMeter(Rect rect, float normalized, Color color)
    {
        DrawRect(rect, new Color(0.08f, 0.11f, 0.12f, 1f));
        DrawRect(new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * Mathf.Clamp01(normalized), rect.height - 2f), color);
    }

    private void DrawPanel(Rect rect, Color? overrideColor = null)
    {
        DrawRect(rect, overrideColor ?? PanelColor);
        DrawRect(new Rect(rect.x, rect.y, 2f, rect.height), PanelBorder);
    }

    private void DrawOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private void DrawLine(Vector2 from, Vector2 to, Color color, float width)
    {
        Vector2 delta = to - from;
        if (delta.sqrMagnitude < 0.01f) return;

        Matrix4x4 previousMatrix = GUI.matrix;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        GUIUtility.RotateAroundPivot(angle, from);
        DrawRect(new Rect(from.x, from.y - width * 0.5f, delta.magnitude, width), color);
        GUI.matrix = previousMatrix;
    }

    private void DrawTransition()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.TransitionAlpha > 0f)
        {
            DrawRect(
                new Rect(0f, 0f, Screen.width, Screen.height),
                new Color(0f, 0f, 0f, GameFlowManager.Instance.TransitionAlpha));
        }
    }

    private void EnsureStyles()
    {
        if (tinyStyle != null) return;

        tinyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            wordWrap = true
        };
        tinyStyle.normal.textColor = TextColor;

        smallStyle = new GUIStyle(tinyStyle)
        {
            fontSize = 13
        };

        smallBoldStyle = new GUIStyle(smallStyle)
        {
            fontStyle = FontStyle.Bold
        };

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 25,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        valueStyle.normal.textColor = Color.white;

        centeredStyle = new GUIStyle(smallBoldStyle)
        {
            alignment = TextAnchor.MiddleCenter
        };

        mapTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        mapTitleStyle.normal.textColor = Color.white;

        mapLabelStyle = new GUIStyle(tinyStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10
        };
        mapLabelStyle.normal.textColor = MutedTextColor;
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private readonly struct MapGeometry
    {
        public readonly Bounds bounds;
        public readonly Color color;
        public readonly string label;

        public MapGeometry(Bounds bounds, Color color, string label)
        {
            this.bounds = bounds;
            this.color = color;
            this.label = label;
        }
    }
}
