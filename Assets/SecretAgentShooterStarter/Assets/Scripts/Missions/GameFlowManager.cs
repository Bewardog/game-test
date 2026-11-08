using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameFlowState
{
    Hub,
    Mission,
    Transition
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public List<MissionDefinition> missions = new List<MissionDefinition>();
    public List<WeaponDefinition> weaponCatalog = new List<WeaponDefinition>();
    public List<string> completedMissionIds = new List<string>();
    public List<MissionPerformanceRecord> missionRecords = new List<MissionPerformanceRecord>();
    public MissionDifficulty SelectedDifficulty { get; private set; } = MissionDifficulty.SecretAgent;
    public GameFlowState State { get; private set; } = GameFlowState.Hub;
    public MissionDefinition CurrentMission { get; private set; }
    public float TransitionAlpha { get; private set; }

    private PrototypeSceneBuilder sceneBuilder;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (missions.Count == 0)
        {
            missions.AddRange(MissionDefinition.CreateCampaignMissions());
        }

        if (weaponCatalog.Count == 0)
        {
            weaponCatalog.Add(WeaponDefinition.CreateRuntime("tranq-pistol", "Tranquilizer Pistol", WeaponClass.Pistol, 35f, 0f, 0.45f, 80f, 12, 0));
            weaponCatalog.Add(WeaponDefinition.CreateRuntime("dart-smg", "Sedative SMG", WeaponClass.SubmachineGun, 18f, 0f, 0.14f, 55f, 30, 1));
            weaponCatalog.Add(WeaponDefinition.CreateRuntime("tranq-rifle", "Precision Tranquilizer Rifle", WeaponClass.Rifle, 70f, 0f, 1.05f, 140f, 8, 2));
            weaponCatalog.Add(WeaponDefinition.CreateRuntime("shock-pistol", "Shock Pistol", WeaponClass.Pistol, 48f, 0f, 0.7f, 65f, 10, 3));
        }

        CampaignPersistence.LoadInto(this);
    }

    public void Configure(PrototypeSceneBuilder builder)
    {
        sceneBuilder = builder;
    }

    public MissionDefinition GetMission(int index)
    {
        if (missions.Count == 0) return null;
        return missions[Mathf.Clamp(index, 0, missions.Count - 1)];
    }

    public bool IsMissionUnlocked(MissionDefinition mission)
    {
        if (mission == null) return false;
        return string.IsNullOrEmpty(mission.prerequisiteMissionId) || completedMissionIds.Contains(mission.prerequisiteMissionId);
    }

    public bool IsMissionComplete(MissionDefinition mission)
    {
        return mission != null && completedMissionIds.Contains(mission.missionId);
    }

    public MissionDifficultyProfile GetDifficultyProfile()
    {
        return MissionDifficultyCatalog.Get(SelectedDifficulty);
    }

    public void SetDifficulty(MissionDifficulty difficulty)
    {
        SelectedDifficulty = difficulty;
        CampaignPersistence.SaveCurrent();
    }

    public void CycleDifficulty(int direction)
    {
        if (direction == 0) return;
        int count = System.Enum.GetValues(typeof(MissionDifficulty)).Length;
        int next = ((int)SelectedDifficulty + (direction > 0 ? 1 : -1) + count) % count;
        SetDifficulty((MissionDifficulty)next);
    }

    public void LoadDifficulty(MissionDifficulty difficulty)
    {
        SelectedDifficulty = difficulty;
    }

    public void MarkMissionCompleted(MissionDefinition mission)
    {
        if (mission == null || string.IsNullOrEmpty(mission.missionId)) return;
        if (!completedMissionIds.Contains(mission.missionId))
        {
            completedMissionIds.Add(mission.missionId);
            CampaignPersistence.SaveCurrent();
        }
    }

    public MissionPerformanceRecord GetMissionRecord(string missionId)
    {
        if (string.IsNullOrEmpty(missionId)) return null;
        return missionRecords.Find(record => record != null && record.missionId == missionId);
    }

    public void RecordMissionPerformance(MissionPerformanceReport report)
    {
        if (report == null || string.IsNullOrEmpty(report.missionId)) return;

        MissionPerformanceRecord existing = GetMissionRecord(report.missionId);
        bool isBetter = existing == null ||
            report.score > existing.bestScore ||
            (report.score == existing.bestScore &&
             (existing.bestTimeSeconds <= 0f || report.elapsedSeconds < existing.bestTimeSeconds));
        if (!isBetter) return;

        if (existing == null)
        {
            existing = new MissionPerformanceRecord { missionId = report.missionId };
            missionRecords.Add(existing);
        }

        existing.bestScore = report.score;
        existing.bestGrade = report.grade;
        existing.bestTimeSeconds = report.elapsedSeconds;
        existing.bestAccuracy = report.accuracy;
        existing.bestRunUndetected = !report.wasAlerted;
        existing.bestKnockouts = report.knockedOut;
        existing.bestKills = report.killed;
        existing.bestRecruits = report.recruited;
        existing.bestDifficulty = report.difficulty;
        CampaignPersistence.SaveCurrent();
    }

    public WeaponDefinition GetWeapon(string weaponId)
    {
        return weaponCatalog.Find(weapon => weapon != null && weapon.weaponId == weaponId);
    }

    public WeaponDefinition GetEquippedWeapon()
    {
        SecretLairManager lair = SecretLairManager.Instance;
        string equippedId = lair != null ? lair.equippedWeaponId : "tranq-pistol";
        return GetWeapon(equippedId) ?? (weaponCatalog.Count > 0 ? weaponCatalog[0] : null);
    }

    public List<WeaponDefinition> GetArmoryAvailableWeapons()
    {
        SecretLairManager lair = SecretLairManager.Instance;
        int armoryLevel = lair != null ? lair.GetRoomLevel(RoomType.Armory) : 0;
        return weaponCatalog.FindAll(weapon => weapon != null &&
            (weapon.unlockedAtArmoryLevel <= armoryLevel || (lair != null && lair.IsWeaponUnlocked(weapon.weaponId))));
    }

    public void EnterHubImmediate()
    {
        CurrentMission = null;
        State = GameFlowState.Hub;
        TransitionAlpha = 0f;
        sceneBuilder?.BuildSecretLair();
    }

    public void EnterMissionImmediate(MissionDefinition mission)
    {
        CurrentMission = mission ?? GetMission(0);
        State = GameFlowState.Mission;
        TransitionAlpha = 0f;
        sceneBuilder?.BuildMission(CurrentMission);
    }

    public void LaunchMission(MissionDefinition mission)
    {
        if (State == GameFlowState.Transition || mission == null || !IsMissionUnlocked(mission)) return;
        StartTransition(false, mission);
    }

    public void ReturnToHub()
    {
        if (State == GameFlowState.Transition) return;
        StartTransition(true, null);
    }

    private void StartTransition(bool toHub, MissionDefinition mission)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionRoutine(toHub, mission));
    }

    private IEnumerator TransitionRoutine(bool toHub, MissionDefinition mission)
    {
        State = GameFlowState.Transition;

        for (float elapsed = 0f; elapsed < 0.25f; elapsed += Time.unscaledDeltaTime)
        {
            TransitionAlpha = Mathf.Clamp01(elapsed / 0.25f);
            yield return null;
        }

        TransitionAlpha = 1f;
        if (toHub)
        {
            CurrentMission = null;
            sceneBuilder?.BuildSecretLair();
            State = GameFlowState.Hub;
        }
        else
        {
            CurrentMission = mission;
            sceneBuilder?.BuildMission(CurrentMission);
            State = GameFlowState.Mission;
        }

        for (float elapsed = 0f; elapsed < 0.25f; elapsed += Time.unscaledDeltaTime)
        {
            TransitionAlpha = 1f - Mathf.Clamp01(elapsed / 0.25f);
            yield return null;
        }

        TransitionAlpha = 0f;
        transitionRoutine = null;
    }
}

[System.Serializable]
public class MissionPerformanceRecord
{
    public string missionId;
    public int bestScore;
    public string bestGrade;
    public float bestTimeSeconds;
    public float bestAccuracy;
    public bool bestRunUndetected;
    public int bestKnockouts;
    public int bestKills;
    public int bestRecruits;
    public MissionDifficulty bestDifficulty;
}
