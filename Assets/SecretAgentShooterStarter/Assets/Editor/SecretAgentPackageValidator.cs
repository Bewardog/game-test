#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SecretAgentPackageValidator
{
    private static readonly string[] RequiredModelResources =
    {
        "Models/Player/SecretAgent_Animated",
        "Models/Player/SecretAgent_FirstPersonArms",
        "Models/RiggedGrunts/HandToHandGrunt01_Animated",
        "Models/RiggedGrunts/HandToHandGrunt02_Animated",
        "Models/RiggedGrunts/HandToHandGrunt03_Animated",
        "Models/RiggedGrunts/HandToHandGrunt04_Animated",
        "Models/RiggedGrunts/HandToHandGrunt05_Animated",
        "Models/RiggedGrunts/HandToHandGrunt06_Animated",
        "Models/RiggedGrunts/HandToHandGrunt07_Animated",
        "Models/RiggedGrunts/HandToHandGrunt08_Animated",
        "Models/RiggedGrunts/HandToHandGrunt09_Animated",
        "Models/RiggedGrunts/HandToHandGrunt10_Animated",
        "Models/Weapons/TranquilizerPistol",
        "Models/Weapons/SedativeSMG",
        "Models/Weapons/PrecisionRifle",
        "Models/Weapons/ShockPistol",
        "Models/EnemyGear/HandToHandBracer",
        "Models/EnemyGear/StealthBlade",
        "Models/EnemyGear/TankArmorPack",
        "Models/EnemyGear/MarksmanRifle",
        "Models/EnemyGear/HackerSignalPack",
        "Models/EnemyGear/MedicTraumaPack",
        "Models/Environment/FloorPanel_A",
        "Models/Environment/WallPanel_A",
        "Models/Environment/DoorFrame_A",
        "Models/Environment/CeilingLight_A",
        "Models/Environment/SupportColumn_A",
        "Models/Environment/CargoContainer_A",
        "Models/Environment/ServerRack_A",
        "Models/Environment/PipeCluster_A",
        "Models/Environment/VentUnit_A",
        "Models/Environment/RelayAntenna_A",
        "Models/Environment/EmbassyPortal_A",
        "Models/Environment/DockCrane_A",
        "Models/Environment/GuardBooth_A",
        "Models/Environment/LabConsole_A",
        "Models/Environment/CryoCore_A",
        "Models/Environment/EmbassyFountain_A",
        "Models/Environment/ArcticBunker_A",
        "Models/Environment/PineTree_A",
        "Models/Environment/RockFormation_A",
        "Models/Environment/SupplyCrate_A",
        "Models/Environment/AmmoBox_A",
        "Models/Environment/FieldMedkit_A",
        "Models/Environment/EncryptedCredit_A",
        "Models/Environment/AlarmBeacon_A",
        "Models/Environment/IntelDatapad_A",
        "Models/Environment/PrototypePedestal_A",
        "Models/Environment/TrainingTarget_A",
        "Models/Environment/SecurityCamera_A",
        "Models/Environment/AccessPanel_A",
        "Models/Environment/Keycard_A",
        "Models/Environment/OfficeDesk_A",
        "Models/Environment/LockerBank_A",
        "Models/Environment/FacilityBench_A",
        "Models/Environment/WeaponRack_A",
        "Models/Environment/MedicalStation_A",
        "Models/Environment/BriefingTable_A",
        "Models/Environment/StairFlight_A",
        "Models/Environment/CatwalkSection_A",
        "Models/Environment/SafetyRailing_A",
        "Models/Environment/ElevatorFront_A",
        "Models/Environment/GarageDoor_A",
        "Models/Environment/RoofServiceCluster_A",
        "Models/Environment/RoomBulkhead_A",
        "Models/Environment/WindowFacade_A",
        "Models/Environment/FacilityRoomShell_A",
        "Models/Environment/MechServiceBay_A"
    };

    private static readonly string[] RequiredTextureResources =
    {
        "Models/RiggedGrunts/Textures/Grunt01_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt02_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt03_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt04_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt05_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt06_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt07_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt08_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt09_BaseColor",
        "Models/RiggedGrunts/Textures/Grunt10_BaseColor"
    };

    static SecretAgentPackageValidator()
    {
        EditorApplication.delayCall += ValidateAfterImport;
    }

    [MenuItem("Secret Agent/Validate Durable Package")]
    public static void ValidateFromMenu()
    {
        Validate(true);
    }

    private static void ValidateAfterImport()
    {
        Validate(false);
    }

    private static bool Validate(bool verbose)
    {
        List<string> failures = new List<string>();
        ValidateCampaign(failures);
        ValidateResources(failures);

        if (failures.Count > 0)
        {
            Debug.LogError(
                "Secret Agent package validation failed:\n- " +
                string.Join("\n- ", failures));
            return false;
        }

        if (verbose)
        {
            Debug.Log(
                "Secret Agent package validation passed: 20 sequential contracts and " +
                $"{RequiredModelResources.Length} required model resources are available.");
        }
        return true;
    }

    private static void ValidateCampaign(List<string> failures)
    {
        List<MissionDefinition> missions = MissionDefinition.CreateCampaignMissions();
        if (missions.Count != 20)
        {
            failures.Add($"Expected 20 campaign contracts but found {missions.Count}.");
        }

        HashSet<string> ids = new HashSet<string>();
        for (int index = 0; index < missions.Count; index++)
        {
            MissionDefinition mission = missions[index];
            if (mission == null)
            {
                failures.Add($"Contract slot {index + 1} is null.");
                continue;
            }

            if (mission.contractNumber != index + 1)
            {
                failures.Add(
                    $"{mission.displayName} is numbered {mission.contractNumber}, expected {index + 1}.");
            }
            if (string.IsNullOrEmpty(mission.missionId) || !ids.Add(mission.missionId))
            {
                failures.Add($"{mission.displayName} has a missing or duplicate mission ID.");
            }
            if (index == 0 && !string.IsNullOrEmpty(mission.prerequisiteMissionId))
            {
                failures.Add("Contract 01 must not have a prerequisite.");
            }
            if (index > 0 && mission.prerequisiteMissionId != missions[index - 1].missionId)
            {
                failures.Add($"{mission.displayName} does not require the previous contract.");
            }
            if (mission.objectives == null || mission.objectives.Count == 0)
            {
                failures.Add($"{mission.displayName} has no objectives.");
            }
            if (mission.enemySpawns == null || mission.enemySpawns.Count == 0)
            {
                failures.Add($"{mission.displayName} has no enemy roster.");
            }
        }
    }

    private static void ValidateResources(List<string> failures)
    {
        foreach (string resourcePath in RequiredModelResources)
        {
            if (Resources.Load<GameObject>(resourcePath) == null)
            {
                failures.Add($"Missing model resource: Resources/{resourcePath}");
            }
        }

        foreach (string resourcePath in RequiredTextureResources)
        {
            if (Resources.Load<Texture2D>(resourcePath) == null)
            {
                failures.Add($"Missing texture resource: Resources/{resourcePath}");
            }
        }
    }
}
#endif
