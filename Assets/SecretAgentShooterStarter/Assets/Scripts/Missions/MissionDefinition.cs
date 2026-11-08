using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Secret Agent/Mission Definition")]
public class MissionDefinition : ScriptableObject
{
    public string missionId = "velvet-key";
    public int contractNumber = 1;
    public string displayName = "Operation Velvet Key";
    [TextArea] public string briefing = "Steal the syndicate files and extract an operative.";
    public string prerequisiteMissionId;
    public MissionEnvironmentType environmentType = MissionEnvironmentType.CorporateAnnex;
    public int completionReward = 450;
    public List<MissionBriefingPage> briefingPages = new List<MissionBriefingPage>();
    public List<MissionObjectiveTemplate> objectives = new List<MissionObjectiveTemplate>();
    public List<EnemySpawnDefinition> enemySpawns = new List<EnemySpawnDefinition>();
    public List<MissionItemDefinition> missionItems = new List<MissionItemDefinition>();

    public static MissionDefinition CreatePrototypeMission()
    {
        return CreateCampaignMissions()[0];
    }

    public static List<MissionDefinition> CreateCampaignMissions()
    {
        List<MissionDefinition> missions = new List<MissionDefinition>();

        MissionDefinition mission = CreateRuntimeMission(
            "velvet-key",
            1,
            "Operation Velvet Key",
            "Enter the syndicate's underground records station, steal its personnel files, and extract an operative.",
            string.Empty,
            MissionEnvironmentType.CorporateAnnex,
            450);
        mission.briefingPages.Add(CreateBriefingPage(
            "INSERTION",
            "A maintenance elevator will place you inside the station's arrival hall. The captive drop-off and mission exit remain beside the elevator, giving you a clear extraction point from the moment the operation begins."));
        mission.briefingPages.Add(CreateBriefingPage(
            "THE FACILITY",
            "The station is large but straightforward: a long central transit corridor links security, records, medical, operations, storage, and locker rooms. The side rooms also connect to one another, providing simple alternate routes."));
        mission.briefingPages.Add(CreateBriefingPage(
            "OBJECTIVES",
            "Follow the transit corridor to the archive chamber and steal the personnel records. Patrols are spaced through the halls and rooms so you can practice observation, sedation, carrying, and direct combat."));
        mission.briefingPages.Add(CreateBriefingPage(
            "EXTRACTION",
            "Recruit at least one sedated operative by carrying her back to the green captive drop-off in the arrival hall. Use the mission exit console beside the elevator once every objective is complete."));
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.StealIntel,
            requiredAmount = 1
        });
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.RecruitTarget,
            requiredAmount = 1
        });
        mission.enemySpawns.Add(CreateEnemy("Transit Guard", new Vector3(0f, 0f, -50f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, new Color(0.8f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Security Room Operative", new Vector3(-17f, 0f, -44f), EnemyRank.Grunt, OperativeSpecialization.Stealth, new Color(0.52f, 0.16f, 0.68f)));
        mission.enemySpawns.Add(CreateEnemy("Locker Bay Patrol", new Vector3(17f, 0f, -44f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, new Color(0.8f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Records Hall Scout", new Vector3(0f, 0f, -17f), EnemyRank.Grunt, OperativeSpecialization.Stealth, new Color(0.55f, 0.1f, 0.9f)));
        mission.enemySpawns.Add(CreateEnemy("Medical Guard", new Vector3(17f, 0f, -14f), EnemyRank.Grunt, OperativeSpecialization.Medic, new Color(0.18f, 0.62f, 0.48f)));
        mission.enemySpawns.Add(CreateEnemy("Operations Sergeant", new Vector3(-17f, 0f, 16f), EnemyRank.Sergeant, OperativeSpecialization.HandToHand, new Color(0.72f, 0.1f, 0.15f)));
        mission.enemySpawns.Add(CreateEnemy("Archive Lieutenant", new Vector3(0f, 0f, 42f), EnemyRank.Lieutenant, OperativeSpecialization.Marksman, new Color(0.14f, 0.46f, 0.7f)));
        mission.missionItems.Add(new MissionItemDefinition
        {
            displayName = "Personnel Archive",
            pickupType = PickupType.Intel,
            position = new Vector3(0f, 1.15f, 47f),
            accentColor = new Color(0.2f, 0.9f, 0.85f)
        });
        missions.Add(mission);

        mission = CreateRuntimeMission(
            "black-glass",
            2,
            "The Black Glass Arsenal",
            "Raid the weapons laboratory, neutralize its security detail, and recover the prototype.",
            "velvet-key",
            MissionEnvironmentType.WeaponsLaboratory,
            700);
        mission.briefingPages.Add(CreateBriefingPage(
            "INSERTION",
            "Insertion is on a maintenance route far south of the Black Glass complex. The first stretch should be quiet, but scouts patrol the final approach and will investigate movement at long range."));
        mission.briefingPages.Add(CreateBriefingPage(
            "OUTER COMPLEX",
            "The laboratory is a campus, not a single room. Communications, fabrication, security, and storage buildings surround the central arsenal. Their upper floors and courtyards are actively patrolled."));
        mission.briefingPages.Add(CreateBriefingPage(
            "CENTRAL ARSENAL",
            "The Arc Prototype is stored on the third floor of the central arsenal. Expect progressively heavier resistance at the perimeter, inside the satellite buildings, and on each floor of the arsenal."));
        mission.briefingPages.Add(CreateBriefingPage(
            "EXTRACTION",
            "Recover the prototype, then retrace the marked maintenance route. Captives can be recruited at the green drop-off beside the insertion point before you activate the mission exit console."));
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.NeutralizeAll,
            requiredAmount = 6
        });
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.ExtractWeaponPrototype,
            requiredAmount = 1
        });
        mission.enemySpawns.Add(CreateEnemy("Distant Glass Scout", new Vector3(-10f, 0f, -68f), EnemyRank.Grunt, OperativeSpecialization.Stealth, new Color(0.42f, 0.18f, 0.62f)));
        mission.enemySpawns.Add(CreateEnemy("Maintenance Patrol", new Vector3(14f, 0f, -45f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, new Color(0.78f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Crimson Gate Guard", new Vector3(-7f, 0f, -14f), EnemyRank.Sergeant, OperativeSpecialization.HandToHand, new Color(0.78f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Glass Gate Scout", new Vector3(8f, 0f, -8f), EnemyRank.Grunt, OperativeSpecialization.Stealth, new Color(0.42f, 0.18f, 0.62f)));
        mission.enemySpawns.Add(CreateEnemy("Comms Technician", new Vector3(-25f, 0f, 20f), EnemyRank.Grunt, OperativeSpecialization.Hacker, new Color(0.25f, 0.62f, 0.72f)));
        mission.enemySpawns.Add(CreateEnemy("Comms Marksman", new Vector3(-24f, 3.4f, 22f), EnemyRank.SpecialOperative, OperativeSpecialization.Marksman, new Color(0.15f, 0.48f, 0.72f)));
        mission.enemySpawns.Add(CreateEnemy("Fabrication Guard", new Vector3(25f, 0f, 20f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, new Color(0.78f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Fabrication Heavy", new Vector3(24f, 3.4f, 22f), EnemyRank.Lieutenant, OperativeSpecialization.Tank, new Color(0.78f, 0.3f, 0.12f)));
        mission.enemySpawns.Add(CreateEnemy("Courtyard Sergeant", new Vector3(-8f, 0f, 43f), EnemyRank.Sergeant, OperativeSpecialization.HandToHand, new Color(0.72f, 0.1f, 0.15f)));
        mission.enemySpawns.Add(CreateEnemy("Storage Medic", new Vector3(25f, 3.4f, 54f), EnemyRank.Lieutenant, OperativeSpecialization.Medic, new Color(0.78f, 0.82f, 0.86f)));
        mission.enemySpawns.Add(CreateEnemy("Arsenal Guard", new Vector3(-5f, 0f, 63f), EnemyRank.Sergeant, OperativeSpecialization.HandToHand, new Color(0.72f, 0.1f, 0.15f)));
        mission.enemySpawns.Add(CreateEnemy("Glass Marksman", new Vector3(6f, 3.4f, 63f), EnemyRank.SpecialOperative, OperativeSpecialization.Marksman, new Color(0.15f, 0.48f, 0.72f)));
        mission.enemySpawns.Add(CreateEnemy("Syndicate Medic", new Vector3(0f, 6.8f, 64f), EnemyRank.Lieutenant, OperativeSpecialization.Medic, new Color(0.78f, 0.82f, 0.86f)));
        mission.missionItems.Add(new MissionItemDefinition
        {
            displayName = "Arc Prototype",
            pickupType = PickupType.PrototypeWeapon,
            position = new Vector3(0f, 7.35f, 66f),
            accentColor = new Color(0.95f, 0.35f, 0.12f)
        });
        missions.Add(mission);

        mission = CreateRuntimeMission(
            "glasshouse",
            3,
            "Operation Glasshouse",
            "Infiltrate the syndicate's woodland annex, steal its command roster, and extract an operative.",
            "black-glass",
            MissionEnvironmentType.WoodlandAnnex,
            850);
        mission.briefingPages.Add(CreateBriefingPage(
            "INSERTION",
            "Insertion is on an isolated woodland service trail south of the annex. Use the long approach to observe the first patrols before entering the compound."));
        mission.briefingPages.Add(CreateBriefingPage(
            "THE COMPOUND",
            "The annex contains a central archive, barracks, security offices, and utility buildings around an open courtyard. Several buildings have occupied upper floors and internal stairs."));
        mission.briefingPages.Add(CreateBriefingPage(
            "OBJECTIVES",
            "The syndicate command roster is kept on the second floor of the central archive. Reach it by crossing the compound, then sedate and extract at least one operative for recruitment."));
        mission.briefingPages.Add(CreateBriefingPage(
            "EXTRACTION",
            "Carry a captive back to the green drop-off beside the woodland insertion point. Once the roster and recruit are secured, activate the mission exit console there."));
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.StealIntel,
            requiredAmount = 1
        });
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.RecruitTarget,
            requiredAmount = 1
        });
        mission.enemySpawns.Add(CreateEnemy("Trail Scout", new Vector3(-8f, 0f, -42f), EnemyRank.Grunt, OperativeSpecialization.Stealth, new Color(0.52f, 0.16f, 0.68f)));
        mission.enemySpawns.Add(CreateEnemy("Road Patrol", new Vector3(10f, 0f, -24f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, new Color(0.8f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Annex Gate Guard", new Vector3(-5f, 0f, -7f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, new Color(0.8f, 0.12f, 0.16f), true));
        mission.enemySpawns.Add(CreateEnemy("Courtyard Patrol", new Vector3(10f, 0f, 7f), EnemyRank.Grunt, OperativeSpecialization.Stealth, new Color(0.55f, 0.1f, 0.9f)));
        mission.enemySpawns.Add(CreateEnemy("Barracks Sergeant", new Vector3(-19f, 3.4f, 15f), EnemyRank.Sergeant, OperativeSpecialization.HandToHand, new Color(0.72f, 0.1f, 0.15f)));
        mission.enemySpawns.Add(CreateEnemy("Security Officer", new Vector3(19f, 0f, 14f), EnemyRank.Lieutenant, OperativeSpecialization.Marksman, new Color(0.14f, 0.46f, 0.7f)));
        mission.enemySpawns.Add(CreateEnemy("Archive Custodian", new Vector3(-4f, 0f, 29f), EnemyRank.Grunt, OperativeSpecialization.Tank, new Color(0.78f, 0.3f, 0.12f)));
        mission.enemySpawns.Add(CreateEnemy("Ghost Lieutenant", new Vector3(3f, 3.4f, 30f), EnemyRank.Lieutenant, OperativeSpecialization.Stealth, new Color(0.4f, 0.12f, 0.58f)));
        mission.missionItems.Add(new MissionItemDefinition
        {
            displayName = "Annex Command Roster",
            pickupType = PickupType.Intel,
            position = new Vector3(0f, 3.95f, 31f),
            accentColor = new Color(0.2f, 0.9f, 0.85f)
        });
        missions.Add(mission);

        mission = CreateRuntimeMission(
            "crownfall",
            4,
            "Operation Crownfall",
            "Breach the executive stronghold, seize the command ledger, and extract Lady Cipher alive.",
            "glasshouse",
            MissionEnvironmentType.ExecutiveStronghold,
            1100);
        mission.briefingPages.Add(CreateBriefingPage(
            "INSERTION",
            "Approach the executive district through the service level. Patrols are light near insertion and increase toward the stronghold gates."));
        mission.briefingPages.Add(CreateBriefingPage(
            "PRIMARY OBJECTIVE",
            "Lady Cipher commands the upper stronghold. Capture her alive and recover the command ledger from the executive level."));
        mission.briefingPages.Add(CreateBriefingPage(
            "EXTRACTION",
            "Carry Lady Cipher to the captive drop-off, then return through the service route and activate the mission exit console."));
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.CaptureCommander,
            requiredAmount = 1,
            requiresSpecificRank = true,
            requiredRank = EnemyRank.Commander
        });
        mission.objectives.Add(new MissionObjectiveTemplate
        {
            type = MissionObjectiveType.StealIntel,
            requiredAmount = 1
        });
        mission.enemySpawns.Add(CreateEnemy("Velvet Enforcer", new Vector3(-7f, 0f, 3f), EnemyRank.Sergeant, OperativeSpecialization.HandToHand, new Color(0.72f, 0.08f, 0.14f), true));
        mission.enemySpawns.Add(CreateEnemy("Silent Knife", new Vector3(6f, 0f, 5f), EnemyRank.SpecialOperative, OperativeSpecialization.Stealth, new Color(0.38f, 0.12f, 0.58f)));
        mission.enemySpawns.Add(CreateEnemy("Royal Guard", new Vector3(-6f, 0f, 13f), EnemyRank.SpecialOperative, OperativeSpecialization.Tank, new Color(0.68f, 0.2f, 0.16f)));
        mission.enemySpawns.Add(CreateEnemy("Cipher Marksman", new Vector3(5f, 0f, 15f), EnemyRank.SpecialOperative, OperativeSpecialization.Marksman, new Color(0.12f, 0.42f, 0.68f)));
        mission.enemySpawns.Add(CreateEnemy("Lady Cipher", new Vector3(0f, 0f, 19f), EnemyRank.Commander, OperativeSpecialization.Hacker, new Color(0.95f, 0.72f, 0.14f)));
        mission.missionItems.Add(new MissionItemDefinition
        {
            displayName = "Command Ledger",
            pickupType = PickupType.Intel,
            position = new Vector3(0f, 0.9f, 21f),
            accentColor = new Color(0.95f, 0.75f, 0.2f)
        });
        missions.Add(mission);

        AddExtendedCampaign(missions);
        return missions;
    }

    private static void AddExtendedCampaign(List<MissionDefinition> missions)
    {
        AddExtendedMission(missions, "night-freight", 5, "Operation Night Freight",
            "Board a syndicate freight terminal, steal its shipping manifest, and neutralize the dock security team.",
            MissionEnvironmentType.Dockyard, 1250, MissionObjectiveType.StealIntel, MissionObjectiveType.NeutralizeAll);
        AddExtendedMission(missions, "paper-crown", 6, "Operation Paper Crown",
            "Infiltrate a diplomatic reception, capture the station chief, and recover her encrypted guest ledger.",
            MissionEnvironmentType.Embassy, 1400, MissionObjectiveType.CaptureCommander, MissionObjectiveType.StealIntel);
        AddExtendedMission(missions, "blue-vein", 7, "Operation Blue Vein",
            "Descend into the syndicate data center and copy the personnel network before its guards can purge it.",
            MissionEnvironmentType.DataCenter, 1550, MissionObjectiveType.StealIntel, MissionObjectiveType.NeutralizeAll);
        AddExtendedMission(missions, "white-signal", 8, "Operation White Signal",
            "Reach an arctic relay, recover a field prototype, and extract an operative who knows the transmission codes.",
            MissionEnvironmentType.ArcticRelay, 1700, MissionObjectiveType.ExtractWeaponPrototype, MissionObjectiveType.RecruitTarget);
        AddExtendedMission(missions, "iron-current", 9, "Operation Iron Current",
            "Sweep the armored cargo docks and seize the coil weapon waiting for overseas shipment.",
            MissionEnvironmentType.Dockyard, 1850, MissionObjectiveType.NeutralizeAll, MissionObjectiveType.ExtractWeaponPrototype);
        AddExtendedMission(missions, "velvet-reception", 10, "Operation Velvet Reception",
            "Return to the embassy under alert conditions, steal the security registry, and recruit an inside operative.",
            MissionEnvironmentType.Embassy, 2000, MissionObjectiveType.StealIntel, MissionObjectiveType.RecruitTarget);
        AddExtendedMission(missions, "dead-channel", 11, "Operation Dead Channel",
            "Penetrate the communications vault, copy its routing table, and recover the experimental shock sidearm.",
            MissionEnvironmentType.DataCenter, 2200, MissionObjectiveType.StealIntel, MissionObjectiveType.ExtractWeaponPrototype);
        AddExtendedMission(missions, "snowblind", 12, "Operation Snowblind",
            "Cross the frozen relay perimeter, capture its commander alive, and extract her through the insertion route.",
            MissionEnvironmentType.ArcticRelay, 2400, MissionObjectiveType.CaptureCommander, MissionObjectiveType.RecruitTarget);
        AddExtendedMission(missions, "harbor-ghost", 13, "Operation Harbor Ghost",
            "Slip through a nighttime container yard, recover the contraband ledger, and recruit a dockside specialist.",
            MissionEnvironmentType.Dockyard, 2600, MissionObjectiveType.RecruitTarget, MissionObjectiveType.StealIntel);
        AddExtendedMission(missions, "crystal-ballroom", 14, "Operation Crystal Ballroom",
            "Fight through the embassy's sealed reception wing and recover its master intelligence index.",
            MissionEnvironmentType.Embassy, 2800, MissionObjectiveType.NeutralizeAll, MissionObjectiveType.StealIntel);
        AddExtendedMission(missions, "cold-storage", 15, "Operation Cold Storage",
            "Clear the underground server farm and remove the cryogenic weapon core before it is relocated.",
            MissionEnvironmentType.DataCenter, 3000, MissionObjectiveType.NeutralizeAll, MissionObjectiveType.ExtractWeaponPrototype);
        AddExtendedMission(missions, "polar-eclipse", 16, "Operation Polar Eclipse",
            "Break the relay garrison, capture its field commander, and leave no active security behind.",
            MissionEnvironmentType.ArcticRelay, 3250, MissionObjectiveType.NeutralizeAll, MissionObjectiveType.CaptureCommander);
        AddExtendedMission(missions, "black-tide", 17, "Operation Black Tide",
            "Raid the syndicate flagship terminal, seize the naval prototype, and capture the harbor commander.",
            MissionEnvironmentType.Dockyard, 3500, MissionObjectiveType.ExtractWeaponPrototype, MissionObjectiveType.CaptureCommander);
        AddExtendedMission(missions, "crimson-embassy", 18, "Operation Crimson Embassy",
            "Capture the syndicate ambassador and extract a second operative from the fortified diplomatic wing.",
            MissionEnvironmentType.Embassy, 3800, MissionObjectiveType.CaptureCommander, MissionObjectiveType.RecruitTarget);
        AddExtendedMission(missions, "ghost-network", 19, "Operation Ghost Network",
            "Erase the syndicate's command network after copying its final archive and neutralizing every defender.",
            MissionEnvironmentType.DataCenter, 4200, MissionObjectiveType.NeutralizeAll, MissionObjectiveType.StealIntel);
        AddExtendedMission(missions, "last-light", 20, "Operation Last Light",
            "Storm the executive citadel, capture the syndicate director alive, and secure the final weapons prototype.",
            MissionEnvironmentType.ExecutiveStronghold, 5000, MissionObjectiveType.CaptureCommander, MissionObjectiveType.ExtractWeaponPrototype);
    }

    private static void AddExtendedMission(
        List<MissionDefinition> missions,
        string id,
        int number,
        string title,
        string description,
        MissionEnvironmentType environment,
        int reward,
        MissionObjectiveType primaryObjective,
        MissionObjectiveType secondaryObjective)
    {
        string prerequisite = missions.Count > 0 ? missions[missions.Count - 1].missionId : string.Empty;
        MissionDefinition mission = CreateRuntimeMission(
            id,
            number,
            title,
            description,
            prerequisite,
            environment,
            reward);

        mission.briefingPages.Add(CreateBriefingPage(
            "INSERTION",
            $"Insertion is outside the {GetEnvironmentLabel(environment)} perimeter. The opening approach is deliberately quiet so you can observe patrol routes before entering the defended zone."));
        mission.briefingPages.Add(CreateBriefingPage(
            "THE OPERATION",
            description));
        mission.briefingPages.Add(CreateBriefingPage(
            "THREAT PROFILE",
            $"Contract {number:00} includes mixed hand-to-hand, stealth, tank, marksman, hacker, and medic teams. Resistance becomes denser around the primary objective."));
        mission.briefingPages.Add(CreateBriefingPage(
            "EXTRACTION",
            "The captive drop-off and exit console remain at insertion. Complete every objective, return any recruit alive, and retrace the marked approach to exfiltrate."));

        int enemyCount = Mathf.Clamp(7 + (number - 5) / 2, 7, 14);
        Vector3[] positions = GetEnvironmentEnemyPositions(environment);
        bool needsCommander = primaryObjective == MissionObjectiveType.CaptureCommander ||
            secondaryObjective == MissionObjectiveType.CaptureCommander;
        int standardEnemyCount = needsCommander ? enemyCount - 1 : enemyCount;
        for (int i = 0; i < standardEnemyCount; i++)
        {
            OperativeSpecialization specialization = GetEncounterSpecialization(
                number,
                i,
                standardEnemyCount,
                environment);
            EnemyRank rank = GetGeneratedRank(number, i);
            Vector3 position = positions[i % positions.Length];
            if (i >= positions.Length)
            {
                int repeat = i / positions.Length;
                position += new Vector3((repeat % 2 == 0 ? -1f : 1f) * 2.1f, 0f, repeat * 1.8f);
            }

            string zoneLabel = GetEncounterZoneLabel(i, standardEnemyCount);
            mission.enemySpawns.Add(CreateEnemy(
                $"{zoneLabel} {specialization} {i + 1:00}",
                position,
                rank,
                specialization,
                GetSpecializationColor(specialization),
                specialization == OperativeSpecialization.HandToHand));
        }

        if (needsCommander)
        {
            Vector3 commanderPosition = GetEnvironmentCommanderPosition(environment);
            OperativeSpecialization commanderRole = GetCommanderSpecialization(environment, number);
            mission.enemySpawns.Add(CreateEnemy(
                $"{title.Replace("Operation ", string.Empty)} Commander",
                commanderPosition,
                EnemyRank.Commander,
                commanderRole,
                GetSpecializationColor(commanderRole)));
        }

        AddGeneratedObjective(mission, primaryObjective, enemyCount);
        if (secondaryObjective != primaryObjective)
        {
            AddGeneratedObjective(mission, secondaryObjective, enemyCount);
        }

        Vector3 objectivePosition = GetEnvironmentObjectivePosition(environment);
        if (primaryObjective == MissionObjectiveType.StealIntel || secondaryObjective == MissionObjectiveType.StealIntel)
        {
            mission.missionItems.Add(new MissionItemDefinition
            {
                displayName = $"{title.Replace("Operation ", string.Empty)} Intelligence",
                pickupType = PickupType.Intel,
                position = objectivePosition,
                accentColor = new Color(0.15f, 0.95f, 0.82f)
            });
        }
        if (primaryObjective == MissionObjectiveType.ExtractWeaponPrototype ||
            secondaryObjective == MissionObjectiveType.ExtractWeaponPrototype)
        {
            mission.missionItems.Add(new MissionItemDefinition
            {
                displayName = $"{title.Replace("Operation ", string.Empty)} Prototype",
                pickupType = PickupType.PrototypeWeapon,
                position = objectivePosition + new Vector3(-1.35f, 0f, 0f),
                accentColor = new Color(1f, 0.38f, 0.12f)
            });
        }

        missions.Add(mission);
    }

    private static OperativeSpecialization GetEncounterSpecialization(
        int missionNumber,
        int spawnIndex,
        int enemyCount,
        MissionEnvironmentType environment)
    {
        float progress = enemyCount <= 1
            ? 1f
            : spawnIndex / (float)(enemyCount - 1);
        OperativeSpecialization[] pool;

        if (progress < 0.27f)
        {
            pool = new[]
            {
                OperativeSpecialization.Stealth,
                OperativeSpecialization.HandToHand,
                OperativeSpecialization.HandToHand,
                environment == MissionEnvironmentType.ArcticRelay
                    ? OperativeSpecialization.Marksman
                    : OperativeSpecialization.Stealth
            };
        }
        else if (progress < 0.68f)
        {
            switch (environment)
            {
                case MissionEnvironmentType.Dockyard:
                    pool = new[]
                    {
                        OperativeSpecialization.Tank,
                        OperativeSpecialization.Marksman,
                        OperativeSpecialization.HandToHand,
                        OperativeSpecialization.Medic
                    };
                    break;
                case MissionEnvironmentType.Embassy:
                    pool = new[]
                    {
                        OperativeSpecialization.Stealth,
                        OperativeSpecialization.Hacker,
                        OperativeSpecialization.Marksman,
                        OperativeSpecialization.HandToHand
                    };
                    break;
                case MissionEnvironmentType.DataCenter:
                    pool = new[]
                    {
                        OperativeSpecialization.Hacker,
                        OperativeSpecialization.Tank,
                        OperativeSpecialization.Medic,
                        OperativeSpecialization.Marksman
                    };
                    break;
                case MissionEnvironmentType.ArcticRelay:
                    pool = new[]
                    {
                        OperativeSpecialization.Marksman,
                        OperativeSpecialization.Tank,
                        OperativeSpecialization.Medic,
                        OperativeSpecialization.Stealth
                    };
                    break;
                default:
                    pool = new[]
                    {
                        OperativeSpecialization.HandToHand,
                        OperativeSpecialization.Marksman,
                        OperativeSpecialization.Hacker,
                        OperativeSpecialization.Tank
                    };
                    break;
            }
        }
        else
        {
            pool = missionNumber >= 13
                ? new[]
                {
                    OperativeSpecialization.Tank,
                    OperativeSpecialization.Medic,
                    OperativeSpecialization.Marksman,
                    OperativeSpecialization.Hacker,
                    OperativeSpecialization.Stealth,
                    OperativeSpecialization.HandToHand
                }
                : new[]
                {
                    OperativeSpecialization.Tank,
                    OperativeSpecialization.Marksman,
                    OperativeSpecialization.Medic,
                    OperativeSpecialization.HandToHand
                };
        }

        return pool[(spawnIndex + missionNumber) % pool.Length];
    }

    private static string GetEncounterZoneLabel(int spawnIndex, int enemyCount)
    {
        float progress = enemyCount <= 1
            ? 1f
            : spawnIndex / (float)(enemyCount - 1);
        if (progress < 0.27f) return "Approach";
        if (progress < 0.5f) return "Perimeter";
        if (progress < 0.76f) return "Interior";
        return "Objective";
    }

    private static OperativeSpecialization GetCommanderSpecialization(
        MissionEnvironmentType environment,
        int missionNumber)
    {
        switch (environment)
        {
            case MissionEnvironmentType.Dockyard:
                return missionNumber >= 17
                    ? OperativeSpecialization.Tank
                    : OperativeSpecialization.Marksman;
            case MissionEnvironmentType.Embassy:
                return missionNumber >= 18
                    ? OperativeSpecialization.Hacker
                    : OperativeSpecialization.Stealth;
            case MissionEnvironmentType.DataCenter:
                return OperativeSpecialization.Hacker;
            case MissionEnvironmentType.ArcticRelay:
                return missionNumber >= 16
                    ? OperativeSpecialization.Tank
                    : OperativeSpecialization.Marksman;
            case MissionEnvironmentType.ExecutiveStronghold:
                return OperativeSpecialization.Hacker;
            default:
                return OperativeSpecialization.HandToHand;
        }
    }

    private static void AddGeneratedObjective(
        MissionDefinition mission,
        MissionObjectiveType type,
        int enemyCount)
    {
        MissionObjectiveTemplate objective = new MissionObjectiveTemplate
        {
            type = type,
            requiredAmount = type == MissionObjectiveType.NeutralizeAll ? enemyCount : 1
        };
        if (type == MissionObjectiveType.CaptureCommander)
        {
            objective.requiresSpecificRank = true;
            objective.requiredRank = EnemyRank.Commander;
        }
        mission.objectives.Add(objective);
    }

    private static EnemyRank GetGeneratedRank(int missionNumber, int spawnIndex)
    {
        if (spawnIndex > 0 && spawnIndex % 7 == 0) return EnemyRank.SpecialOperative;
        if (spawnIndex > 0 && spawnIndex % 4 == 0) return EnemyRank.Lieutenant;
        if (spawnIndex > 0 && spawnIndex % 2 == 0) return EnemyRank.Sergeant;
        return missionNumber >= 14 && spawnIndex % 3 == 0 ? EnemyRank.Lieutenant : EnemyRank.Grunt;
    }

    private static Vector3[] GetEnvironmentEnemyPositions(MissionEnvironmentType environment)
    {
        if (environment == MissionEnvironmentType.Dockyard || environment == MissionEnvironmentType.WeaponsLaboratory)
        {
            return new[]
            {
                new Vector3(-10f, 0f, -68f), new Vector3(14f, 0f, -45f),
                new Vector3(-7f, 0f, -14f), new Vector3(8f, 0f, -8f),
                new Vector3(-25f, 0f, 20f), new Vector3(-24f, 3.4f, 22f),
                new Vector3(25f, 0f, 20f), new Vector3(24f, 3.4f, 22f),
                new Vector3(-8f, 0f, 43f), new Vector3(25f, 3.4f, 54f),
                new Vector3(-5f, 0f, 63f), new Vector3(6f, 3.4f, 63f),
                new Vector3(0f, 6.8f, 64f), new Vector3(12f, 0f, 78f)
            };
        }
        if (environment == MissionEnvironmentType.WoodlandAnnex || environment == MissionEnvironmentType.ArcticRelay)
        {
            return new[]
            {
                new Vector3(-8f, 0f, -42f), new Vector3(10f, 0f, -24f),
                new Vector3(-5f, 0f, -7f), new Vector3(10f, 0f, 7f),
                new Vector3(-19f, 3.4f, 15f), new Vector3(19f, 0f, 14f),
                new Vector3(-4f, 0f, 29f), new Vector3(3f, 3.4f, 30f),
                new Vector3(-22f, 0f, 38f), new Vector3(21f, 3.4f, 39f),
                new Vector3(0f, 0f, 44f), new Vector3(13f, 0f, 47f)
            };
        }
        if (environment == MissionEnvironmentType.CorporateAnnex || environment == MissionEnvironmentType.DataCenter)
        {
            return new[]
            {
                new Vector3(0f, 0f, -50f), new Vector3(-17f, 0f, -44f),
                new Vector3(17f, 0f, -44f), new Vector3(0f, 0f, -17f),
                new Vector3(17f, 0f, -14f), new Vector3(-17f, 0f, 16f),
                new Vector3(17f, 0f, 16f), new Vector3(0f, 0f, 32f),
                new Vector3(-14f, 0f, 40f), new Vector3(14f, 0f, 40f),
                new Vector3(0f, 0f, 45f)
            };
        }

        return new[]
        {
            new Vector3(-7f, 0f, 3f), new Vector3(6f, 0f, 5f),
            new Vector3(-6f, 0f, 13f), new Vector3(5f, 0f, 15f),
            new Vector3(-12f, 0f, 20f), new Vector3(12f, 0f, 20f),
            new Vector3(-8f, 0f, 28f), new Vector3(8f, 0f, 28f),
            new Vector3(0f, 0f, 35f), new Vector3(-14f, 0f, 39f),
            new Vector3(14f, 0f, 39f)
        };
    }

    private static Vector3 GetEnvironmentObjectivePosition(MissionEnvironmentType environment)
    {
        if (environment == MissionEnvironmentType.Dockyard || environment == MissionEnvironmentType.WeaponsLaboratory)
        {
            return new Vector3(0f, 7.35f, 66f);
        }
        if (environment == MissionEnvironmentType.WoodlandAnnex || environment == MissionEnvironmentType.ArcticRelay)
        {
            return new Vector3(0f, 3.95f, 31f);
        }
        if (environment == MissionEnvironmentType.CorporateAnnex || environment == MissionEnvironmentType.DataCenter)
        {
            return new Vector3(0f, 1.15f, 47f);
        }
        return new Vector3(0f, 0.9f, 21f);
    }

    private static Vector3 GetEnvironmentCommanderPosition(MissionEnvironmentType environment)
    {
        if (environment == MissionEnvironmentType.Dockyard || environment == MissionEnvironmentType.WeaponsLaboratory)
        {
            return new Vector3(2.5f, 6.8f, 64f);
        }
        if (environment == MissionEnvironmentType.WoodlandAnnex || environment == MissionEnvironmentType.ArcticRelay)
        {
            return new Vector3(2.5f, 3.4f, 29f);
        }
        if (environment == MissionEnvironmentType.CorporateAnnex || environment == MissionEnvironmentType.DataCenter)
        {
            return new Vector3(2.5f, 0f, 45f);
        }
        return new Vector3(2.5f, 0f, 19f);
    }

    private static Color GetSpecializationColor(OperativeSpecialization specialization)
    {
        switch (specialization)
        {
            case OperativeSpecialization.HandToHand: return new Color(0.85f, 0.12f, 0.16f);
            case OperativeSpecialization.Stealth: return new Color(0.5f, 0.14f, 0.68f);
            case OperativeSpecialization.Tank: return new Color(0.8f, 0.32f, 0.12f);
            case OperativeSpecialization.Marksman: return new Color(0.12f, 0.5f, 0.75f);
            case OperativeSpecialization.Hacker: return new Color(0.12f, 0.75f, 0.62f);
            case OperativeSpecialization.Medic: return new Color(0.35f, 0.82f, 0.5f);
            default: return Color.white;
        }
    }

    private static string GetEnvironmentLabel(MissionEnvironmentType environment)
    {
        switch (environment)
        {
            case MissionEnvironmentType.Dockyard: return "container dock";
            case MissionEnvironmentType.Embassy: return "diplomatic compound";
            case MissionEnvironmentType.DataCenter: return "underground data center";
            case MissionEnvironmentType.ArcticRelay: return "arctic relay";
            case MissionEnvironmentType.WeaponsLaboratory: return "weapons laboratory";
            case MissionEnvironmentType.WoodlandAnnex: return "woodland annex";
            case MissionEnvironmentType.ExecutiveStronghold: return "executive stronghold";
            default: return "corporate annex";
        }
    }

    private static MissionDefinition CreateRuntimeMission(
        string id,
        int number,
        string title,
        string description,
        string prerequisite,
        MissionEnvironmentType environment,
        int reward)
    {
        MissionDefinition mission = CreateInstance<MissionDefinition>();
        mission.name = title;
        mission.hideFlags = HideFlags.HideAndDontSave;
        mission.missionId = id;
        mission.contractNumber = number;
        mission.displayName = title;
        mission.briefing = description;
        mission.prerequisiteMissionId = prerequisite;
        mission.environmentType = environment;
        mission.completionReward = reward;
        return mission;
    }

    private static MissionBriefingPage CreateBriefingPage(string heading, string body)
    {
        return new MissionBriefingPage
        {
            heading = heading,
            body = body
        };
    }

    private static EnemySpawnDefinition CreateEnemy(
        string displayName,
        Vector3 position,
        EnemyRank rank,
        OperativeSpecialization specialization,
        Color fallbackColor,
        bool usePrimaryModel = false)
    {
        return new EnemySpawnDefinition
        {
            displayName = displayName,
            position = position,
            rank = rank,
            specialization = specialization,
            fallbackColor = fallbackColor,
            usePrimaryGruntModel = usePrimaryModel
        };
    }
}

[Serializable]
public class MissionObjectiveTemplate
{
    public MissionObjectiveType type;
    public int requiredAmount = 1;
    public bool requiresSpecificRank;
    public EnemyRank requiredRank;
    public bool requiresSpecificSpecialization;
    public OperativeSpecialization requiredSpecialization;

    public MissionObjective CreateRuntimeObjective()
    {
        return new MissionObjective
        {
            type = type,
            requiredAmount = Mathf.Max(1, requiredAmount),
            requiresSpecificRank = requiresSpecificRank,
            requiredRank = requiredRank,
            requiresSpecificSpecialization = requiresSpecificSpecialization,
            requiredSpecialization = requiredSpecialization
        };
    }
}

[Serializable]
public class EnemySpawnDefinition
{
    public string displayName = "Syndicate Operative";
    public Vector3 position;
    public EnemyRank rank = EnemyRank.Grunt;
    public OperativeSpecialization specialization = OperativeSpecialization.HandToHand;
    public Color fallbackColor = Color.red;
    public string modelResourcePath;
    public bool usePrimaryGruntModel;
}

[Serializable]
public class MissionItemDefinition
{
    public string displayName = "Mission Item";
    public PickupType pickupType = PickupType.Intel;
    public Vector3 position;
    public Color accentColor = Color.cyan;
}

[Serializable]
public class MissionBriefingPage
{
    public string heading = "MISSION BRIEFING";
    [TextArea] public string body;
}
