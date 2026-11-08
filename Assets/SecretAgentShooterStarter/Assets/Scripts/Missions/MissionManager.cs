using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }
    public static event Action<MissionAlertState> AlertStateChanged;

    public string missionName = "Infiltrate The Velvet Annex";
    public int completionReward = 600;
    public MissionDefinition activeDefinition;
    public List<MissionObjective> objectives = new List<MissionObjective>();

    private readonly HashSet<EnemyOperative> spawnedEnemies = new HashSet<EnemyOperative>();
    private readonly HashSet<EnemyOperative> neutralizedEnemies = new HashSet<EnemyOperative>();
    private readonly HashSet<EnemyOperative> knockedOutEnemies = new HashSet<EnemyOperative>();
    private readonly HashSet<EnemyOperative> killedEnemies = new HashSet<EnemyOperative>();
    private readonly HashSet<EnemyOperative> recruitedEnemies = new HashSet<EnemyOperative>();
    private bool rewardPaid;
    private bool missionClockStarted;
    private float missionStartTime;
    private string activeCheckpointId;
    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation = Quaternion.identity;
    private int checkpointRecoveriesRemaining;

    public MissionAlertState AlertState { get; private set; } = MissionAlertState.Infiltration;
    public bool IsAlerted => AlertState == MissionAlertState.Alerted;
    public float ElapsedTime => missionClockStarted ? Mathf.Max(0f, Time.time - missionStartTime) : 0f;
    public int KnockoutCount => knockedOutEnemies.Count;
    public int KillCount => killedEnemies.Count;
    public int RecruitCount => recruitedEnemies.Count;
    public int NeutralizedCount => neutralizedEnemies.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        EnemyOperative.AnySpawned += RegisterEnemy;
        EnemyOperative.AnyKnockedOut += HandleEnemyKnockedOut;
        EnemyOperative.AnyKilled += HandleEnemyKilled;
        EnemyOperative.AnyRecruited += HandleEnemyRecruited;
    }

    private void OnDisable()
    {
        EnemyOperative.AnySpawned -= RegisterEnemy;
        EnemyOperative.AnyKnockedOut -= HandleEnemyKnockedOut;
        EnemyOperative.AnyKilled -= HandleEnemyKilled;
        EnemyOperative.AnyRecruited -= HandleEnemyRecruited;
    }

    private void Update()
    {
        if (missionClockStarted || activeDefinition == null) return;

        SpyPlayerController player = FindAnyObjectByType<SpyPlayerController>();
        if (player != null && !player.ControlsLocked)
        {
            missionClockStarted = true;
            missionStartTime = Time.time;
        }
    }

    public void NotifyIntelStolen()
    {
        IncrementObjective(MissionObjectiveType.StealIntel, null);
    }

    public void NotifyPrototypeRecovered()
    {
        IncrementObjective(MissionObjectiveType.ExtractWeaponPrototype, null);
    }

    public void TriggerAlert(string reason)
    {
        if (IsAlerted) return;

        AlertState = MissionAlertState.Alerted;
        AlertStateChanged?.Invoke(AlertState);
        Debug.Log($"Syndicate alert triggered: {reason}.");
    }

    public string GetAlertSummary()
    {
        return IsAlerted ? "STATUS  SYNDICATE ALERTED" : "STATUS  INFILTRATION";
    }

    public void Configure(MissionDefinition definition)
    {
        activeDefinition = definition;
        missionName = definition != null ? definition.displayName : "Prototype Mission";
        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        completionReward = definition != null
            ? Mathf.RoundToInt(definition.completionReward * difficulty.rewardMultiplier)
            : 0;
        rewardPaid = false;
        missionClockStarted = false;
        missionStartTime = Time.time;
        AlertState = MissionAlertState.Infiltration;
        AlertStateChanged?.Invoke(AlertState);
        spawnedEnemies.Clear();
        neutralizedEnemies.Clear();
        knockedOutEnemies.Clear();
        killedEnemies.Clear();
        recruitedEnemies.Clear();
        objectives.Clear();

        SpyPlayerController player = FindAnyObjectByType<SpyPlayerController>();
        player?.WeaponController?.ResetMissionPerformance();
        activeCheckpointId = "deployment";
        checkpointPosition = player != null ? player.transform.position : Vector3.zero;
        checkpointRotation = player != null ? player.transform.rotation : Quaternion.identity;
        checkpointRecoveriesRemaining = difficulty.checkpointRecoveriesPerSector;

        if (definition == null) return;
        foreach (MissionObjectiveTemplate template in definition.objectives)
        {
            if (template != null)
            {
                objectives.Add(template.CreateRuntimeObjective());
            }
        }
    }

    public string GetObjectiveSummary()
    {
        if (objectives.Count == 0) return "NO ACTIVE OBJECTIVES";

        List<string> lines = new List<string>();
        foreach (MissionObjective objective in objectives)
        {
            string status = objective.IsComplete ? "COMPLETE" : $"{objective.currentAmount}/{objective.requiredAmount}";
            lines.Add($"{GetObjectiveLabel(objective.type)}  {status}");
        }

        return string.Join("\n", lines);
    }

    public bool IsComplete()
    {
        return objectives.Count > 0 && objectives.TrueForAll(objective => objective.IsComplete);
    }

    public void ActivateCheckpoint(
        string checkpointId,
        Vector3 respawnPosition,
        Quaternion respawnRotation,
        SpyPlayerController player)
    {
        if (string.IsNullOrEmpty(checkpointId) || checkpointId == activeCheckpointId) return;

        activeCheckpointId = checkpointId;
        checkpointPosition = respawnPosition;
        checkpointRotation = respawnRotation;
        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        checkpointRecoveriesRemaining = difficulty.checkpointRecoveriesPerSector;
        player?.ShowActionMessage("FIELD CHECKPOINT SECURED", 1.2f);
    }

    public bool TryRecoverPlayer(SpyPlayerController player)
    {
        if (player == null || checkpointRecoveriesRemaining <= 0) return false;

        checkpointRecoveriesRemaining--;
        player.RestoreFromCheckpoint(checkpointPosition, checkpointRotation);
        return true;
    }

    private void RegisterEnemy(EnemyOperative enemy)
    {
        if (enemy != null)
        {
            spawnedEnemies.Add(enemy);
        }
    }

    private void HandleEnemyKnockedOut(EnemyOperative enemy)
    {
        if (enemy == null) return;
        knockedOutEnemies.Add(enemy);
        HandleEnemyNeutralized(enemy);
    }

    private void HandleEnemyKilled(EnemyOperative enemy)
    {
        if (enemy == null) return;
        killedEnemies.Add(enemy);
        HandleEnemyNeutralized(enemy);
    }

    private void HandleEnemyNeutralized(EnemyOperative enemy)
    {
        if (enemy == null || neutralizedEnemies.Contains(enemy)) return;

        neutralizedEnemies.Add(enemy);

        IncrementObjective(MissionObjectiveType.NeutralizeAll, enemy);

        TryPayReward();
    }

    private void HandleEnemyRecruited(EnemyOperative enemy)
    {
        if (enemy == null) return;

        recruitedEnemies.Add(enemy);
        IncrementObjective(MissionObjectiveType.RecruitTarget, enemy);

        if (enemy.Rank == EnemyRank.Commander)
        {
            IncrementObjective(MissionObjectiveType.CaptureCommander, enemy);
        }

        TryPayReward();
    }

    private void IncrementObjective(MissionObjectiveType type, EnemyOperative operative)
    {
        foreach (MissionObjective objective in objectives)
        {
            if (objective.type != type || objective.IsComplete) continue;

            if (objective.requiresSpecificRank && operative != null && operative.Rank != objective.requiredRank)
            {
                continue;
            }

            if (objective.requiresSpecificSpecialization && operative != null && operative.Specialization != objective.requiredSpecialization)
            {
                continue;
            }

            objective.currentAmount++;
        }

        TryPayReward();
    }

    private void TryPayReward()
    {
        if (rewardPaid || !IsComplete()) return;

        rewardPaid = true;

        if (SecretLairManager.Instance != null)
        {
            SecretLairManager.Instance.AddCurrency(completionReward);
        }

        GameFlowManager.Instance?.MarkMissionCompleted(activeDefinition);

        Debug.Log($"Mission complete: {missionName}. Reward: {completionReward}");
    }

    public MissionPerformanceReport CreatePerformanceReport(SpyWeaponController weapon)
    {
        int shotsFired = weapon != null ? weapon.MissionShotsFired : 0;
        int shotsHit = weapon != null ? weapon.MissionShotsHit : 0;
        float accuracy = shotsFired > 0 ? shotsHit / (float)shotsFired : 1f;
        float elapsed = ElapsedTime;
        int contract = activeDefinition != null ? activeDefinition.contractNumber : 1;
        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        float parTime = 105f + contract * 14f;

        int score = 65;
        score += difficulty.performanceScoreModifier;
        score += IsAlerted ? -8 : 18;
        score += Mathf.Clamp(KnockoutCount * 2, 0, 12);
        score -= Mathf.Clamp(KillCount * 5, 0, 25);
        score += Mathf.Clamp(RecruitCount * 5, 0, 15);

        if (shotsFired > 0)
        {
            if (accuracy >= 0.8f) score += 10;
            else if (accuracy >= 0.55f) score += 6;
            else if (accuracy >= 0.35f) score += 2;
            else score -= 6;
        }

        if (elapsed > 0f && elapsed <= parTime) score += 8;
        else if (elapsed > parTime * 1.75f) score -= 5;

        score = Mathf.Clamp(score, 0, 100);
        return new MissionPerformanceReport
        {
            missionId = activeDefinition != null ? activeDefinition.missionId : string.Empty,
            missionName = missionName,
            difficulty = difficulty.difficulty,
            difficultyName = difficulty.displayName,
            contractNumber = contract,
            elapsedSeconds = elapsed,
            wasAlerted = IsAlerted,
            neutralized = NeutralizedCount,
            knockedOut = KnockoutCount,
            killed = KillCount,
            recruited = RecruitCount,
            shotsFired = shotsFired,
            shotsHit = shotsHit,
            accuracy = accuracy,
            reward = completionReward,
            score = score,
            grade = GetGrade(score)
        };
    }

    private static string GetGrade(int score)
    {
        if (score >= 95) return "S";
        if (score >= 85) return "A";
        if (score >= 72) return "B";
        if (score >= 58) return "C";
        return "D";
    }

    private static string GetObjectiveLabel(MissionObjectiveType type)
    {
        switch (type)
        {
            case MissionObjectiveType.StealIntel: return "STEAL INTEL";
            case MissionObjectiveType.NeutralizeAll: return "NEUTRALIZE ENEMIES";
            case MissionObjectiveType.CaptureCommander: return "CAPTURE COMMANDER";
            case MissionObjectiveType.ExtractWeaponPrototype: return "RECOVER PROTOTYPE";
            case MissionObjectiveType.RecruitTarget: return "EXTRACT RECRUIT";
            default: return type.ToString().ToUpperInvariant();
        }
    }
}

[Serializable]
public class MissionPerformanceReport
{
    public string missionId;
    public string missionName;
    public MissionDifficulty difficulty;
    public string difficultyName;
    public int contractNumber;
    public float elapsedSeconds;
    public bool wasAlerted;
    public int neutralized;
    public int knockedOut;
    public int killed;
    public int recruited;
    public int shotsFired;
    public int shotsHit;
    public float accuracy;
    public int reward;
    public int score;
    public string grade;
}

[Serializable]
public class MissionObjective
{
    public MissionObjectiveType type;
    public int requiredAmount = 1;
    public int currentAmount;
    public bool requiresSpecificRank;
    public EnemyRank requiredRank;
    public bool requiresSpecificSpecialization;
    public OperativeSpecialization requiredSpecialization;

    public bool IsComplete => currentAmount >= requiredAmount;
}
