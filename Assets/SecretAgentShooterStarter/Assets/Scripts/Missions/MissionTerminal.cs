using UnityEngine;

public class MissionTerminal : MonoBehaviour, IInteractable
{
    private SpyPlayerController activePlayer;
    private int selectedMissionIndex;
    private int briefingPageIndex;
    private float inputReadyAt;
    private bool briefingOpen;
    private GUIStyle titleStyle;
    private GUIStyle rowStyle;
    private GUIStyle detailStyle;
    private GUIStyle briefingHeadingStyle;
    private GUIStyle briefingBodyStyle;
    private GUIStyle mapLabelStyle;
    private GUIStyle buttonStyle;
    private Vector2 missionScrollPosition;
    private int previousSelectedMissionIndex = -1;

    public bool IsOpen => activePlayer != null;
    public bool IsBriefingOpen => IsOpen && briefingOpen;
    public int BriefingPageIndex => briefingPageIndex;
    public string InteractionPrompt => IsOpen ? string.Empty : "Open mission directory";

    public void Interact(SpyPlayerController player)
    {
        if (player == null || GameFlowManager.Instance == null ||
            GameFlowManager.Instance.missions.Count == 0)
        {
            player?.ShowActionMessage("NO MISSIONS AVAILABLE", 1f);
            return;
        }

        activePlayer = player;
        selectedMissionIndex = FindFirstAvailableMission();
        briefingPageIndex = 0;
        briefingOpen = false;
        inputReadyAt = Time.unscaledTime + 0.18f;
        activePlayer.SetControlsLocked(true);
    }

    private void Update()
    {
        if (!IsOpen || Time.unscaledTime < inputReadyAt) return;

        int missionCount = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.missions.Count
            : 0;
        if (missionCount == 0)
        {
            Close();
            return;
        }

        if (briefingOpen)
        {
            UpdateBriefingInput();
            return;
        }

        if (SpyInput.MenuUpPressed)
        {
            selectedMissionIndex = (selectedMissionIndex - 1 + missionCount) % missionCount;
            KeepSelectedMissionVisible();
        }

        if (SpyInput.MenuDownPressed)
        {
            selectedMissionIndex = (selectedMissionIndex + 1) % missionCount;
            KeepSelectedMissionVisible();
        }

        if (SpyInput.MenuLeftPressed)
        {
            GameFlowManager.Instance.CycleDifficulty(-1);
        }

        if (SpyInput.MenuRightPressed)
        {
            GameFlowManager.Instance.CycleDifficulty(1);
        }

        if (SpyInput.CancelPressed)
        {
            Close();
            return;
        }

        if (SpyInput.ConfirmPressed)
        {
            TryDeploySelectedMission();
        }
    }

    private void OnGUI()
    {
        if (!IsOpen || GameFlowManager.Instance == null) return;

        EnsureStyles();
        if (briefingOpen)
        {
            DrawBriefing();
            return;
        }

        float width = Mathf.Min(820f, Screen.width - 64f);
        float height = Mathf.Min(560f, Screen.height - 64f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        GUI.color = new Color(0.035f, 0.055f, 0.07f, 0.98f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;

        GUI.Label(new Rect(panel.x + 28f, panel.y + 20f, panel.width - 56f, 44f), "MISSION DIRECTORY", titleStyle);

        float listWidth = panel.width * 0.45f;
        Rect listViewport = new Rect(panel.x + 28f, panel.y + 82f, listWidth - 36f, panel.height - 154f);
        float listContentHeight = Mathf.Max(listViewport.height - 4f, GameFlowManager.Instance.missions.Count * 54f);
        if (previousSelectedMissionIndex != selectedMissionIndex)
        {
            KeepSelectedMissionVisible(listViewport.height);
            previousSelectedMissionIndex = selectedMissionIndex;
        }

        missionScrollPosition = GUI.BeginScrollView(
            listViewport,
            missionScrollPosition,
            new Rect(0f, 0f, listViewport.width - 18f, listContentHeight),
            false,
            true);
        for (int i = 0; i < GameFlowManager.Instance.missions.Count; i++)
        {
            MissionDefinition mission = GameFlowManager.Instance.GetMission(i);
            bool unlocked = GameFlowManager.Instance.IsMissionUnlocked(mission);
            bool complete = GameFlowManager.Instance.IsMissionComplete(mission);
            bool selected = i == selectedMissionIndex;
            Rect row = new Rect(0f, i * 54f, listViewport.width - 20f, 46f);

            GUI.color = selected
                ? new Color(0.12f, 0.48f, 0.58f, 1f)
                : new Color(0.09f, 0.12f, 0.14f, 1f);
            GUI.Box(row, GUIContent.none);
            GUI.color = complete
                ? new Color(0.4f, 0.95f, 0.55f)
                : unlocked
                    ? Color.white
                    : new Color(0.42f, 0.46f, 0.48f);
            string status = complete ? "COMPLETE" : unlocked ? "AVAILABLE" : "LOCKED";
            MissionPerformanceRecord record = GameFlowManager.Instance.GetMissionRecord(mission.missionId);
            if (complete && record != null && !string.IsNullOrEmpty(record.bestGrade))
            {
                status += $"  BEST {record.bestGrade}";
            }
            GUI.Label(row, $"{mission.contractNumber:00}  {mission.displayName}\n{status}", rowStyle);
            GUI.color = Color.white;

            if (GUI.Button(row, GUIContent.none, GUIStyle.none))
            {
                selectedMissionIndex = i;
            }
        }
        GUI.EndScrollView();

        MissionDefinition selectedMission = GameFlowManager.Instance.GetMission(selectedMissionIndex);
        if (selectedMission != null)
        {
            float detailX = panel.x + listWidth;
            Rect detailRect = new Rect(detailX, panel.y + 82f, panel.width - listWidth - 28f, panel.height - 150f);
            string lockText = GameFlowManager.Instance.IsMissionUnlocked(selectedMission)
                ? string.Empty
                : $"\n\nLOCKED: COMPLETE CONTRACT {selectedMission.contractNumber - 1:00}";
            MissionPerformanceRecord selectedRecord =
                GameFlowManager.Instance.GetMissionRecord(selectedMission.missionId);
            string bestResult = selectedRecord != null
                ? $"\n\nBEST RESULT  GRADE {selectedRecord.bestGrade}  /  SCORE {selectedRecord.bestScore:000}\n" +
                  $"TIME  {FormatTime(selectedRecord.bestTimeSeconds)}  /  " +
                  $"ACCURACY  {Mathf.RoundToInt(selectedRecord.bestAccuracy * 100f)}%  /  " +
                  (selectedRecord.bestRunUndetected ? "UNDETECTED" : "DETECTED") +
                  $"\nRECORDED ON  {MissionDifficultyCatalog.Get(selectedRecord.bestDifficulty).displayName}"
                : string.Empty;
            MissionDifficultyProfile difficulty = GameFlowManager.Instance.GetDifficultyProfile();
            string details =
                $"CONTRACT {selectedMission.contractNumber:00}\n\n" +
                $"{selectedMission.displayName.ToUpperInvariant()}\n\n" +
                $"{selectedMission.briefing}\n\n" +
                $"DIFFICULTY  <  {difficulty.displayName}  >\n" +
                $"{difficulty.description}\n\n" +
                $"REWARD  {Mathf.RoundToInt(selectedMission.completionReward * difficulty.rewardMultiplier)} CREDITS" +
                bestResult +
                lockText;
            GUI.Label(detailRect, details, detailStyle);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = GameFlowManager.Instance.IsMissionUnlocked(selectedMission);
            if (GUI.Button(
                new Rect(detailX, panel.y + panel.height - 62f, detailRect.width, 38f),
                GUI.enabled ? "DEPLOY" : "LOCKED"))
            {
                TryDeploySelectedMission();
            }
            GUI.enabled = wasEnabled;
        }

        GUI.Label(
            new Rect(panel.x + 28f, panel.y + panel.height - 54f, listWidth - 36f, 34f),
            "W/S SELECT   A/D DIFFICULTY   ENTER DEPLOY   ESC CLOSE",
            detailStyle);
    }

    private int FindFirstAvailableMission()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        for (int i = 0; i < flow.missions.Count; i++)
        {
            MissionDefinition mission = flow.GetMission(i);
            if (flow.IsMissionUnlocked(mission) && !flow.IsMissionComplete(mission)) return i;
        }

        return 0;
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private void KeepSelectedMissionVisible(float viewportHeight = 380f)
    {
        const float rowHeight = 54f;
        float rowTop = selectedMissionIndex * rowHeight;
        float rowBottom = rowTop + 46f;
        if (rowTop < missionScrollPosition.y)
        {
            missionScrollPosition.y = rowTop;
        }
        else if (rowBottom > missionScrollPosition.y + viewportHeight)
        {
            missionScrollPosition.y = rowBottom - viewportHeight;
        }
        missionScrollPosition.y = Mathf.Max(0f, missionScrollPosition.y);
    }

    private void TryDeploySelectedMission()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        MissionDefinition mission = flow != null ? flow.GetMission(selectedMissionIndex) : null;
        if (mission == null || !flow.IsMissionUnlocked(mission))
        {
            activePlayer?.ShowActionMessage("CONTRACT LOCKED", 1f);
            return;
        }

        if (mission.briefingPages != null && mission.briefingPages.Count > 0)
        {
            briefingPageIndex = 0;
            briefingOpen = true;
            inputReadyAt = Time.unscaledTime + 0.18f;
            return;
        }

        LaunchSelectedMission(mission);
    }

    private void UpdateBriefingInput()
    {
        MissionDefinition mission = GameFlowManager.Instance?.GetMission(selectedMissionIndex);
        int pageCount = mission?.briefingPages?.Count ?? 0;
        if (pageCount == 0)
        {
            briefingOpen = false;
            return;
        }

        if (SpyInput.CancelPressed)
        {
            briefingOpen = false;
            inputReadyAt = Time.unscaledTime + 0.18f;
            return;
        }

        if (SpyInput.MenuUpPressed)
        {
            briefingPageIndex = Mathf.Max(0, briefingPageIndex - 1);
        }

        if (SpyInput.MenuDownPressed)
        {
            briefingPageIndex = Mathf.Min(pageCount - 1, briefingPageIndex + 1);
        }

        if (SpyInput.ConfirmPressed)
        {
            if (briefingPageIndex < pageCount - 1)
            {
                briefingPageIndex++;
            }
            else
            {
                LaunchSelectedMission(mission);
            }
        }
    }

    private void DrawBriefing()
    {
        MissionDefinition mission = GameFlowManager.Instance?.GetMission(selectedMissionIndex);
        int pageCount = mission?.briefingPages?.Count ?? 0;
        if (mission == null || pageCount == 0) return;

        briefingPageIndex = Mathf.Clamp(briefingPageIndex, 0, pageCount - 1);
        MissionBriefingPage page = mission.briefingPages[briefingPageIndex];
        DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.01f, 0.015f, 0.02f, 0.94f));

        float width = Mathf.Min(1080f, Screen.width - 48f);
        float height = Mathf.Min(700f, Screen.height - 48f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        DrawRect(panel, new Color(0.035f, 0.055f, 0.07f, 1f));
        DrawRect(new Rect(panel.x, panel.y, panel.width, 6f), new Color(0.12f, 0.68f, 0.7f));

        GUI.Label(
            new Rect(panel.x + 28f, panel.y + 20f, panel.width - 56f, 42f),
            $"CONTRACT {mission.contractNumber:00}  //  {mission.displayName.ToUpperInvariant()}",
            titleStyle);

        Rect schematic = new Rect(panel.x + 28f, panel.y + 82f, panel.width * 0.56f, panel.height - 190f);
        DrawBriefingSchematic(schematic, mission, briefingPageIndex);

        float textX = schematic.xMax + 26f;
        float textWidth = panel.xMax - textX - 28f;
        GUI.Label(new Rect(textX, panel.y + 92f, textWidth, 52f), page.heading.ToUpperInvariant(), briefingHeadingStyle);
        GUI.Label(new Rect(textX, panel.y + 154f, textWidth, panel.height - 290f), page.body, briefingBodyStyle);
        GUI.Label(
            new Rect(textX, panel.yMax - 150f, textWidth, 26f),
            $"BRIEFING PAGE {briefingPageIndex + 1} / {pageCount}",
            detailStyle);

        bool previousEnabled = GUI.enabled;
        GUI.enabled = briefingPageIndex > 0;
        if (GUI.Button(new Rect(textX, panel.yMax - 112f, textWidth, 34f), "PREVIOUS", buttonStyle))
        {
            briefingPageIndex--;
        }
        GUI.enabled = previousEnabled;

        string advanceLabel = briefingPageIndex < pageCount - 1 ? "NEXT" : "BEGIN OPERATION";
        if (GUI.Button(new Rect(textX, panel.yMax - 70f, textWidth, 38f), advanceLabel, buttonStyle))
        {
            if (briefingPageIndex < pageCount - 1)
            {
                briefingPageIndex++;
            }
            else
            {
                LaunchSelectedMission(mission);
            }
        }

        if (GUI.Button(new Rect(panel.x + 28f, panel.yMax - 58f, schematic.width, 30f), "RETURN TO MISSION DIRECTORY", buttonStyle))
        {
            briefingOpen = false;
            inputReadyAt = Time.unscaledTime + 0.18f;
        }
    }

    private void DrawBriefingSchematic(Rect area, MissionDefinition mission, int pageIndex)
    {
        DrawRect(area, new Color(0.055f, 0.085f, 0.095f));
        for (int i = 1; i < 8; i++)
        {
            float x = Mathf.Lerp(area.x, area.xMax, i / 8f);
            float y = Mathf.Lerp(area.y, area.yMax, i / 8f);
            DrawRect(new Rect(x, area.y, 1f, area.height), new Color(0.16f, 0.28f, 0.3f, 0.32f));
            DrawRect(new Rect(area.x, y, area.width, 1f), new Color(0.16f, 0.28f, 0.3f, 0.32f));
        }

        Color muted = new Color(0.18f, 0.3f, 0.32f);
        Color active = new Color(0.12f, 0.72f, 0.68f);
        Color objective = new Color(0.95f, 0.5f, 0.16f);
        bool showRoute = pageIndex == 0 || pageIndex >= mission.briefingPages.Count - 1;
        DrawContractRouteSchematic(area, mission, showRoute ? active : muted);

        Rect compound = new Rect(area.x + area.width * 0.12f, area.y + area.height * 0.08f, area.width * 0.76f, area.height * 0.56f);
        DrawOutline(compound, pageIndex > 0 ? active : muted, 3f);

        if (mission.environmentType == MissionEnvironmentType.WeaponsLaboratory)
        {
            DrawMapBuilding(new Rect(compound.x + 24f, compound.y + 28f, 92f, 66f), "COMMS", muted);
            DrawMapBuilding(new Rect(compound.xMax - 116f, compound.y + 28f, 92f, 66f), "FAB", muted);
            DrawMapBuilding(new Rect(compound.x + 24f, compound.yMax - 88f, 92f, 64f), "SEC", muted);
            DrawMapBuilding(new Rect(compound.xMax - 116f, compound.yMax - 88f, 92f, 64f), "STORE", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 72f, compound.center.y - 58f, 144f, 116f),
                "ARSENAL  //  3 FLOORS",
                pageIndex >= 2 ? objective : active);
        }
        else if (mission.environmentType == MissionEnvironmentType.Dockyard)
        {
            DrawMapBuilding(new Rect(compound.x + 18f, compound.y + 24f, 112f, 72f), "WEST CUSTOMS\n2 FLOORS", muted);
            DrawMapBuilding(new Rect(compound.xMax - 130f, compound.y + 24f, 112f, 72f), "EAST FREIGHT\n2 FLOORS", muted);
            DrawMapBuilding(new Rect(compound.x + 22f, compound.yMax - 82f, 104f, 58f), "HARBOR SECURITY", muted);
            DrawMapBuilding(new Rect(compound.xMax - 126f, compound.yMax - 82f, 104f, 58f), "MANIFEST OFFICE", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 67f, compound.center.y - 55f, 134f, 110f),
                "CONTROL TOWER\n3 FLOORS",
                pageIndex >= 2 ? objective : active);
        }
        else if (mission.environmentType == MissionEnvironmentType.CorporateAnnex)
        {
            Rect transit = new Rect(compound.center.x - 22f, compound.y + 34f, 44f, compound.height - 62f);
            DrawRect(transit, pageIndex > 0 ? active : muted);

            const float roomHeight = 42f;
            float leftX = compound.x + 20f;
            float rightX = compound.xMax - 116f;
            float firstY = compound.yMax - 62f;
            DrawMapBuilding(new Rect(leftX, firstY, 96f, roomHeight), "SECURITY", muted);
            DrawMapBuilding(new Rect(rightX, firstY, 96f, roomHeight), "LOCKERS", muted);
            DrawMapBuilding(new Rect(leftX, firstY - 52f, 96f, roomHeight), "RECORDS", muted);
            DrawMapBuilding(new Rect(rightX, firstY - 52f, 96f, roomHeight), "MEDICAL", muted);
            DrawMapBuilding(new Rect(leftX, firstY - 104f, 96f, roomHeight), "OPERATIONS", muted);
            DrawMapBuilding(new Rect(rightX, firstY - 104f, 96f, roomHeight), "STORAGE", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 84f, compound.y + 12f, 168f, 48f),
                "ARCHIVE",
                pageIndex >= 2 ? objective : active);
        }
        else if (mission.environmentType == MissionEnvironmentType.DataCenter)
        {
            float hallWidth = compound.width - 44f;
            DrawMapBuilding(new Rect(compound.x + 22f, compound.yMax - 82f, hallWidth, 54f), "DATA HALL A", muted);
            DrawMapBuilding(new Rect(compound.x + 22f, compound.center.y - 27f, hallWidth, 54f), "DATA HALL B", muted);
            DrawMapBuilding(new Rect(compound.x + 22f, compound.y + 28f, hallWidth, 54f), "DATA HALL C", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 88f, compound.y + 6f, 176f, 46f),
                mission.contractNumber >= 19 ? "GHOST NETWORK CORE" : "NETWORK VAULT",
                pageIndex >= 2 ? objective : active);
        }
        else if (mission.environmentType == MissionEnvironmentType.WoodlandAnnex)
        {
            DrawMapBuilding(new Rect(compound.x + 22f, compound.center.y - 48f, 102f, 96f), "BARRACKS\n2 FLOORS", muted);
            DrawMapBuilding(new Rect(compound.xMax - 124f, compound.center.y - 48f, 102f, 96f), "SECURITY\n2 FLOORS", muted);
            DrawMapBuilding(new Rect(compound.x + 28f, compound.y + 22f, 86f, 52f), "UTILITY", muted);
            DrawMapBuilding(new Rect(compound.xMax - 114f, compound.y + 22f, 86f, 52f), "LOGISTICS", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 76f, compound.y + 34f, 152f, 116f),
                "CENTRAL ARCHIVE\n2 FLOORS",
                pageIndex >= 2 ? objective : active);
        }
        else if (mission.environmentType == MissionEnvironmentType.ArcticRelay)
        {
            DrawMapBuilding(new Rect(compound.x + 18f, compound.center.y - 45f, 112f, 90f), "WEST GARRISON", muted);
            DrawMapBuilding(new Rect(compound.xMax - 130f, compound.center.y - 45f, 112f, 90f), "EAST POWER", muted);
            DrawMapBuilding(new Rect(compound.x + 28f, compound.y + 20f, 88f, 48f), "WEATHER", muted);
            DrawMapBuilding(new Rect(compound.xMax - 116f, compound.y + 20f, 88f, 48f), "GENERATOR", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 74f, compound.y + 34f, 148f, 112f),
                mission.contractNumber >= 16 ? "GARRISON CONTROL\n2 FLOORS" : "RELAY CONTROL\n2 FLOORS",
                pageIndex >= 2 ? objective : active);
        }
        else if (mission.environmentType == MissionEnvironmentType.Embassy)
        {
            DrawMapBuilding(new Rect(compound.x + 20f, compound.center.y - 42f, 100f, 84f), "CONSULAR", muted);
            DrawMapBuilding(new Rect(compound.xMax - 120f, compound.center.y - 42f, 100f, 84f), "SECURITY", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 96f, compound.y + 26f, 192f, 120f),
                mission.contractNumber >= 14 ? "SEALED CHANCERY\n2 FLOORS" : "CHANCERY\n2 FLOORS",
                pageIndex >= 2 ? objective : active);
        }
        else
        {
            DrawMapBuilding(new Rect(compound.x + 24f, compound.center.y - 46f, 102f, 92f), "GALLERY", muted);
            DrawMapBuilding(new Rect(compound.xMax - 126f, compound.center.y - 46f, 102f, 92f), "SECURITY", muted);
            DrawMapBuilding(
                new Rect(compound.center.x - 76f, compound.y + 30f, 152f, 116f),
                "COMMAND",
                pageIndex >= 2 ? objective : active);
        }

        GUI.Label(new Rect(area.center.x - 58f, area.yMax - 30f, 116f, 22f), "INSERTION", mapLabelStyle);
        string areaLabel;
        switch (mission.environmentType)
        {
            case MissionEnvironmentType.WeaponsLaboratory:
                areaLabel = "LABORATORY CAMPUS";
                break;
            case MissionEnvironmentType.Dockyard:
                areaLabel = "HARBOR TERMINAL";
                break;
            case MissionEnvironmentType.CorporateAnnex:
                areaLabel = "UNDERGROUND STATION";
                break;
            case MissionEnvironmentType.DataCenter:
                areaLabel = "DATA CENTER";
                break;
            case MissionEnvironmentType.WoodlandAnnex:
                areaLabel = "WOODLAND ANNEX";
                break;
            case MissionEnvironmentType.ArcticRelay:
                areaLabel = "POLAR RELAY";
                break;
            case MissionEnvironmentType.Embassy:
                areaLabel = "EMBASSY COMPOUND";
                break;
            default:
                areaLabel = "EXECUTIVE CITADEL";
                break;
        }
        GUI.Label(new Rect(compound.center.x - 74f, compound.y - 2f, 148f, 22f), areaLabel, mapLabelStyle);
        if (pageIndex >= mission.briefingPages.Count - 1)
        {
            GUI.Label(new Rect(area.center.x + 12f, area.yMax - 62f, 110f, 22f), "EXFIL", mapLabelStyle);
        }
    }

    private static void DrawContractRouteSchematic(
        Rect area,
        MissionDefinition mission,
        Color color)
    {
        float routeTop = area.yMax - area.height * 0.37f;
        float routeBottom = area.yMax - area.height * 0.06f;
        float centerX = area.center.x;
        if (mission.contractNumber <= 4)
        {
            DrawRect(new Rect(centerX - 5f, routeTop, 10f, routeBottom - routeTop), color);
            return;
        }

        float direction = mission.contractNumber % 2 == 0 ? -1f : 1f;
        int visitIndex = Mathf.Max(0, (mission.contractNumber - 5) / 4);
        float offset = direction * Mathf.Lerp(26f, 48f, Mathf.Clamp01(visitIndex / 3f));
        float lowerTurnY = Mathf.Lerp(routeTop, routeBottom, 0.63f);
        float upperTurnY = Mathf.Lerp(routeTop, routeBottom, 0.34f);

        DrawRect(new Rect(centerX - 5f, lowerTurnY, 10f, routeBottom - lowerTurnY), color);
        DrawRect(
            new Rect(
                Mathf.Min(centerX, centerX + offset),
                lowerTurnY - 5f,
                Mathf.Abs(offset),
                10f),
            color);
        DrawRect(new Rect(centerX + offset - 5f, upperTurnY, 10f, lowerTurnY - upperTurnY), color);

        if (visitIndex >= 1)
        {
            float reverseOffset = -offset * 0.72f;
            DrawRect(
                new Rect(
                    Mathf.Min(centerX + offset, centerX + reverseOffset),
                    upperTurnY - 5f,
                    Mathf.Abs(offset - reverseOffset),
                    10f),
                color);
            DrawRect(new Rect(centerX + reverseOffset - 5f, routeTop, 10f, upperTurnY - routeTop), color);
        }
        else
        {
            DrawRect(new Rect(centerX + offset - 5f, routeTop, 10f, upperTurnY - routeTop), color);
        }
    }

    private void DrawMapBuilding(Rect rect, string label, Color color)
    {
        DrawRect(rect, color);
        DrawOutline(rect, new Color(0.75f, 0.95f, 0.95f, 0.65f), 2f);
        GUI.Label(rect, label, mapLabelStyle);
    }

    private static void DrawOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private static void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void LaunchSelectedMission(MissionDefinition mission)
    {
        GameFlowManager flow = GameFlowManager.Instance;
        Close();
        flow.LaunchMission(mission);
    }

    private void Close()
    {
        SpyPlayerController player = activePlayer;
        activePlayer = null;
        briefingOpen = false;
        briefingPageIndex = 0;
        player?.SetControlsLocked(false);
    }

    private void OnDisable()
    {
        if (IsOpen) Close();
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        titleStyle.normal.textColor = Color.white;

        rowStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(14, 8, 4, 4)
        };

        detailStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(8, 8, 8, 8)
        };
        detailStyle.normal.textColor = new Color(0.88f, 0.93f, 0.95f);

        briefingHeadingStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft
        };
        briefingHeadingStyle.normal.textColor = new Color(0.35f, 0.95f, 0.9f);

        briefingBodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(0, 8, 8, 8)
        };
        briefingBodyStyle.normal.textColor = Color.white;

        mapLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        mapLabelStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
    }
}
