using UnityEngine;

public class MissionDebriefOverlay : MonoBehaviour
{
    private SpyPlayerController player;
    private MissionPerformanceReport report;
    private float openedAt;
    private bool isOpen;

    private GUIStyle titleStyle;
    private GUIStyle gradeStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle valueStyle;
    private GUIStyle buttonStyle;

    public void Open(
        SpyPlayerController playerController,
        MissionManager mission,
        SpyWeaponController weapon)
    {
        if (isOpen || playerController == null || mission == null) return;

        player = playerController;
        report = mission.CreatePerformanceReport(weapon);
        GameFlowManager.Instance?.RecordMissionPerformance(report);
        openedAt = Time.unscaledTime;
        isOpen = true;
        player.SetControlsLocked(true);
    }

    private void Update()
    {
        if (!isOpen || Time.unscaledTime < openedAt + 0.25f) return;
        if (SpyInput.ConfirmPressed)
        {
            ReturnToLair();
        }
    }

    private void OnGUI()
    {
        if (!isOpen || report == null) return;

        EnsureStyles();
        GUI.color = new Color(0.015f, 0.025f, 0.04f, 0.97f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float width = Mathf.Min(780f, Screen.width - 64f);
        float height = Mathf.Min(600f, Screen.height - 56f);
        Rect panel = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height);

        GUI.color = new Color(0.06f, 0.085f, 0.12f, 1f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(
            new Rect(panel.x + 34f, panel.y + 24f, panel.width - 68f, 28f),
            $"CONTRACT {report.contractNumber:00} COMPLETE  /  {report.difficultyName}",
            headingStyle);
        GUI.Label(
            new Rect(panel.x + 34f, panel.y + 55f, panel.width - 190f, 50f),
            report.missionName.ToUpperInvariant(),
            titleStyle);

        GUI.Label(
            new Rect(panel.xMax - 156f, panel.y + 45f, 110f, 90f),
            report.grade,
            gradeStyle);
        GUI.Label(
            new Rect(panel.xMax - 164f, panel.y + 126f, 128f, 24f),
            $"SCORE {report.score:000}",
            valueStyle);

        float left = panel.x + 42f;
        float right = panel.x + panel.width * 0.54f;
        float rowY = panel.y + 170f;
        DrawStat(left, rowY, "MISSION TIME", FormatTime(report.elapsedSeconds));
        DrawStat(right, rowY, "ALERT STATUS", report.wasAlerted ? "DETECTED" : "UNDETECTED");
        rowY += 52f;
        DrawStat(left, rowY, "TARGETS NEUTRALIZED", report.neutralized.ToString());
        DrawStat(right, rowY, "NONLETHAL TAKEDOWNS", report.knockedOut.ToString());
        rowY += 52f;
        DrawStat(left, rowY, "LETHAL TAKEDOWNS", report.killed.ToString());
        DrawStat(right, rowY, "OPERATIVES RECRUITED", report.recruited.ToString());
        rowY += 52f;
        DrawStat(left, rowY, "SHOTS FIRED", report.shotsFired.ToString());
        DrawStat(right, rowY, "SHOTS ON TARGET", report.shotsHit.ToString());
        rowY += 52f;
        DrawStat(left, rowY, "ACCURACY", $"{Mathf.RoundToInt(report.accuracy * 100f)}%");
        DrawStat(right, rowY, "CONTRACT REWARD", $"{report.reward} CREDITS");

        Rect actionRect = new Rect(
            panel.x + 42f,
            panel.yMax - 70f,
            panel.width - 84f,
            40f);
        if (GUI.Button(actionRect, "RETURN TO SECRET LAIR   [ENTER / E]", buttonStyle))
        {
            ReturnToLair();
        }
    }

    private void DrawStat(float x, float y, string label, string value)
    {
        float columnWidth = Mathf.Max(210f, Screen.width * 0.18f);
        GUI.Label(new Rect(x, y, columnWidth, 22f), label, bodyStyle);
        GUI.Label(new Rect(x, y + 21f, columnWidth, 26f), value, valueStyle);
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 27,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.96f, 1f) }
        };
        gradeStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 72,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.15f, 0.95f, 0.82f) }
        };
        headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            normal = { textColor = new Color(0.35f, 0.85f, 0.94f) }
        };
        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.56f, 0.66f, 0.74f) }
        };
        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            hover = { textColor = new Color(0.15f, 0.95f, 0.82f) }
        };
    }

    private void ReturnToLair()
    {
        if (!isOpen) return;
        isOpen = false;
        GameFlowManager.Instance?.ReturnToHub();
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}
