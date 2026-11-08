using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CampaignPersistence
{
    private const int CurrentVersion = 2;
    private const string SaveFileName = "secret-agent-campaign.json";

    private static CampaignSaveData cachedData;
    private static bool loadAttempted;

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static void LoadInto(SecretLairManager lair)
    {
        if (lair == null) return;

        CampaignSaveData data = GetData();
        if (data == null) return;

        lair.currency = Mathf.Max(0, data.currency);
        lair.unlockedWeaponIds = new List<string>(data.unlockedWeaponIds ?? new List<string>());
        if (!lair.unlockedWeaponIds.Contains("tranq-pistol")) lair.unlockedWeaponIds.Add("tranq-pistol");
        lair.equippedWeaponId = string.IsNullOrEmpty(data.equippedWeaponId) ? "tranq-pistol" : data.equippedWeaponId;
        lair.tranquilizerResearchLevel = Mathf.Max(0, data.tranquilizerResearchLevel);
        lair.armorResearchLevel = Mathf.Max(0, data.armorResearchLevel);
        lair.weaponsResearchLevel = Mathf.Max(0, data.weaponsResearchLevel);

        foreach (RoomSaveData savedRoom in data.rooms)
        {
            RoomState room = lair.rooms.Find(candidate => candidate.type == savedRoom.type);
            if (room != null)
            {
                room.level = Mathf.Clamp(savedRoom.level, 0, room.maxLevel);
            }
        }
    }

    public static void LoadInto(RecruitRoster roster)
    {
        if (roster == null) return;

        CampaignSaveData data = GetData();
        if (data == null) return;

        roster.recruits.Clear();
        foreach (RecruitSaveData savedRecruit in data.recruits)
        {
            roster.recruits.Add(new RecruitedOperativeData
            {
                displayName = savedRecruit.displayName,
                rank = savedRecruit.rank,
                specialization = savedRecruit.specialization,
                recruitmentValue = savedRecruit.recruitmentValue,
                activeOutfit = string.IsNullOrEmpty(savedRecruit.activeOutfit) ? "Field Suit" : savedRecruit.activeOutfit,
                visualResourcePath = savedRecruit.visualResourcePath
            });
        }
    }

    public static void LoadInto(GameFlowManager flow)
    {
        if (flow == null) return;

        CampaignSaveData data = GetData();
        if (data == null) return;
        flow.completedMissionIds = new List<string>(data.completedMissionIds ?? new List<string>());
        flow.missionRecords = new List<MissionPerformanceRecord>(
            data.missionRecords ?? new List<MissionPerformanceRecord>());
        flow.LoadDifficulty(data.version >= 2
            ? data.selectedDifficulty
            : MissionDifficulty.SecretAgent);
    }

    public static void SaveCurrent()
    {
        CampaignSaveData data = GetData() ?? new CampaignSaveData();
        data.version = CurrentVersion;

        SecretLairManager lair = SecretLairManager.Instance;
        if (lair != null)
        {
            data.currency = lair.currency;
            data.equippedWeaponId = lair.equippedWeaponId;
            data.unlockedWeaponIds = new List<string>(lair.unlockedWeaponIds);
            data.tranquilizerResearchLevel = lair.tranquilizerResearchLevel;
            data.armorResearchLevel = lair.armorResearchLevel;
            data.weaponsResearchLevel = lair.weaponsResearchLevel;
            data.rooms.Clear();
            foreach (RoomState room in lair.rooms)
            {
                data.rooms.Add(new RoomSaveData { type = room.type, level = room.level });
            }
        }

        RecruitRoster roster = RecruitRoster.Instance;
        if (roster != null)
        {
            data.recruits.Clear();
            foreach (RecruitedOperativeData recruit in roster.recruits)
            {
                data.recruits.Add(new RecruitSaveData
                {
                    displayName = recruit.displayName,
                    rank = recruit.rank,
                    specialization = recruit.specialization,
                    recruitmentValue = recruit.recruitmentValue,
                    activeOutfit = recruit.activeOutfit,
                    visualResourcePath = recruit.visualResourcePath
                });
            }
        }

        GameFlowManager flow = GameFlowManager.Instance;
        if (flow != null)
        {
            data.completedMissionIds = new List<string>(flow.completedMissionIds);
            data.missionRecords = new List<MissionPerformanceRecord>(flow.missionRecords);
            data.selectedDifficulty = flow.SelectedDifficulty;
        }

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            cachedData = data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Campaign save failed: {exception.Message}");
        }
    }

    public static void ResetRuntimeCache()
    {
        cachedData = null;
        loadAttempted = false;
    }

    private static CampaignSaveData GetData()
    {
        if (loadAttempted) return cachedData;
        loadAttempted = true;

        if (!File.Exists(SavePath)) return null;

        try
        {
            cachedData = JsonUtility.FromJson<CampaignSaveData>(File.ReadAllText(SavePath));
            return cachedData;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Campaign save could not be loaded: {exception.Message}");
            return null;
        }
    }
}

[Serializable]
public class CampaignSaveData
{
    public int version = 1;
    public int currency;
    public List<RoomSaveData> rooms = new List<RoomSaveData>();
    public List<string> unlockedWeaponIds = new List<string> { "tranq-pistol" };
    public string equippedWeaponId = "tranq-pistol";
    public int tranquilizerResearchLevel;
    public int armorResearchLevel;
    public int weaponsResearchLevel;
    public List<RecruitSaveData> recruits = new List<RecruitSaveData>();
    public List<string> completedMissionIds = new List<string>();
    public List<MissionPerformanceRecord> missionRecords = new List<MissionPerformanceRecord>();
    public MissionDifficulty selectedDifficulty = MissionDifficulty.SecretAgent;
}

[Serializable]
public class RoomSaveData
{
    public RoomType type;
    public int level;
}

[Serializable]
public class RecruitSaveData
{
    public string displayName;
    public EnemyRank rank;
    public OperativeSpecialization specialization;
    public int recruitmentValue;
    public string activeOutfit;
    public string visualResourcePath;
}
