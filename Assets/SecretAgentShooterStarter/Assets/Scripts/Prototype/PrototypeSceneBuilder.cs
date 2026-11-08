using System.Collections.Generic;
using UnityEngine;

public class PrototypeSceneBuilder : MonoBehaviour
{
    private const int CurrentSettingsVersion = 1;
    private const string RiggedGrunt01Model = "Models/RiggedGrunts/HandToHandGrunt01_Animated";
    private const string LegacyRiggedGrunt01Model = "Models/RiggedGrunts/HandToHandGrunt01_Rigged";
    private const string RiggedGrunt01BaseColor = "Models/RiggedGrunts/Textures/Grunt01_BaseColor";
    private const string RiggedGrunt02Model = "Models/RiggedGrunts/HandToHandGrunt02_Animated";
    private const string RiggedGrunt02BaseColor = "Models/RiggedGrunts/Textures/Grunt02_BaseColor";
    private const string RiggedGrunt03Model = "Models/RiggedGrunts/HandToHandGrunt03_Animated";
    private const string RiggedGrunt03BaseColor = "Models/RiggedGrunts/Textures/Grunt03_BaseColor";
    private const string RiggedGrunt04Model = "Models/RiggedGrunts/HandToHandGrunt04_Animated";
    private const string RiggedGrunt04BaseColor = "Models/RiggedGrunts/Textures/Grunt04_BaseColor";
    private const string RiggedGrunt05Model = "Models/RiggedGrunts/HandToHandGrunt05_Animated";
    private const string RiggedGrunt05BaseColor = "Models/RiggedGrunts/Textures/Grunt05_BaseColor";
    private const string RiggedGrunt06Model = "Models/RiggedGrunts/HandToHandGrunt06_Animated";
    private const string RiggedGrunt06BaseColor = "Models/RiggedGrunts/Textures/Grunt06_BaseColor";
    private const string RiggedGrunt07Model = "Models/RiggedGrunts/HandToHandGrunt07_Animated";
    private const string RiggedGrunt07BaseColor = "Models/RiggedGrunts/Textures/Grunt07_BaseColor";
    private const string RiggedGrunt08Model = "Models/RiggedGrunts/HandToHandGrunt08_Animated";
    private const string RiggedGrunt08BaseColor = "Models/RiggedGrunts/Textures/Grunt08_BaseColor";
    private const string RiggedGrunt09Model = "Models/RiggedGrunts/HandToHandGrunt09_Animated";
    private const string RiggedGrunt09BaseColor = "Models/RiggedGrunts/Textures/Grunt09_BaseColor";
    private const string RiggedGrunt10Model = "Models/RiggedGrunts/HandToHandGrunt10_Animated";
    private const string RiggedGrunt10BaseColor = "Models/RiggedGrunts/Textures/Grunt10_BaseColor";
    private const string SourceGrunt01Model = "Models/HandToHandGrunts/HandToHandGrunt_01";
    private const string PlayerAgentModel = "Models/Player/SecretAgent_Animated";
    private const string PlayerFirstPersonArmsModel = "Models/Player/SecretAgent_FirstPersonArms";
    private const float LairOperationsY = 9.5f;
    private const float LairCommandY = 19f;
    private const float LairRoofY = 25.5f;
    private const float LairRoomHeight = 5.2f;
    // Half-width of the central atrium building. Department rooms are built as
    // full-size annexes beyond these side walls instead of boxes inside the wings.
    private const float LairHalfWidth = 22f;
    private const float LairDepthMinZ = -25f;
    private const float LairDepthMaxZ = 35f;
    private const float LairWallHeight = 26f;
    private const float HangarMechScale = 1.8f;

    private struct LairRoomSpec
    {
        public RoomType roomType;
        public bool isRecruitQuarters;
        public float side;          // -1 = west annex, +1 = east annex
        public float floorY;        // walking surface height of the room floor
        public float centerZ;
        public float width;         // extent along X, measured outward from the atrium wall
        public float depth;         // extent along Z
        public float height;        // floor to ceiling
        public float doorWidth;
        public float doorHeight;
        public Vector2 legacySize;  // original room footprint, used to spread props proportionally
        public bool exteriorDoor;   // entered from outside through the south facade instead of the atrium
        public bool isMissionRoom;  // mission operations: always open, holds the mission briefing table

        // Exterior doors sit in the south wall, this far out from the atrium wall.
        public float ExteriorDoorX => side * (LairHalfWidth + 14f);
        public float SouthWallZ => centerZ - depth * 0.5f;

        public Vector3 Position => new Vector3(side * (LairHalfWidth + width * 0.5f), floorY, centerZ);
        public float EntranceWallX => side * LairHalfWidth;
        public float OuterWallX => side * (LairHalfWidth + width);
    }

    // Edit these numbers to resize or move a lair department.
    // Rooms on the same side and level must not overlap along Z, and must stay
    // between LairDepthMinZ and LairDepthMaxZ.
    private static readonly LairRoomSpec[] LairRoomSpecs =
    {
        Room(RoomType.Garage,                 -1f, 0f,                     2.15f, 34f, 53.7f, 9f, 12f,  7f,   15f, 18f, false, true),
        Room(RoomType.MedicalBay,              1f, 0f,                     22f, 24f, 24f, 8.5f, 4.4f, 4f,   10f, 11f),
        Room(RoomType.IntelCenter,            -1f, LairCommandY + 0.2f,    19f, 26f, 28f, 7.5f, 5f,   4.2f, 10f, 11f, false, false, true),
        Room(RoomType.Armory,                 -1f, LairOperationsY + 0.2f, -12f, 22f, 23f, 8.5f, 4.4f, 4f,   10f, 11f),
        Room(RoomType.TrainingRoom,           -1f, LairOperationsY + 0.2f, 17f,  30f, 32f, 8.5f, 6f,   4.5f, 14f, 17f),
        Room(RoomType.ResearchLab,             1f, LairOperationsY + 0.2f, -6f,  26f, 30f, 8.5f, 4.4f, 4f,   10f, 11f),
        Room(RoomType.OperativeCustomization, -1f, LairCommandY + 0.2f,    -8f,  22f, 24f, 7.5f, 4.4f, 4f,   11f, 11f),
        Room(RoomType.IntelCenter,             1f, LairCommandY + 0.2f,    -11f, 26f, 25f, 7.5f, 4.4f, 4f,   10f, 11f),
        Room(RoomType.IntelCenter,             1f, LairCommandY + 0.2f,    17f,  24f, 28f, 7.5f, 4.4f, 4f,   11f, 13f, true),
    };

    private static LairRoomSpec Room(
        RoomType roomType,
        float side,
        float floorY,
        float centerZ,
        float width,
        float depth,
        float height,
        float doorWidth,
        float doorHeight,
        float legacyWidth,
        float legacyDepth,
        bool isRecruitQuarters = false,
        bool exteriorDoor = false,
        bool isMissionRoom = false)
    {
        return new LairRoomSpec
        {
            roomType = roomType,
            isRecruitQuarters = isRecruitQuarters,
            exteriorDoor = exteriorDoor,
            isMissionRoom = isMissionRoom,
            side = side,
            floorY = floorY,
            centerZ = centerZ,
            width = width,
            depth = depth,
            height = height,
            doorWidth = doorWidth,
            doorHeight = doorHeight,
            legacySize = new Vector2(legacyWidth, legacyDepth)
        };
    }

    // Scales hard-coded prop offsets from the original small rooms up to the
    // current room's size. Reset to one outside room construction.
    private Vector2 hubRoomSpread = Vector2.one;
    private float hubRoomHeight = LairRoomHeight;
    private bool hubRoomHasAtriumDoor = true;
    private float hubRoomExteriorDoorX;
    private float hubRoomSouthWallZ;

    private Vector3 Spread(Vector3 offset)
    {
        return new Vector3(offset.x * hubRoomSpread.x, offset.y, offset.z * hubRoomSpread.y);
    }

    // Visual roster for the future Hand-to-Hand Grunt melee class. The formal
    // class and its behavior remain intentionally unimplemented.
    private static readonly string[] HandToHandGruntModels =
    {
        RiggedGrunt01Model,
        RiggedGrunt02Model,
        RiggedGrunt03Model,
        RiggedGrunt04Model,
        RiggedGrunt05Model,
        RiggedGrunt06Model,
        RiggedGrunt07Model,
        RiggedGrunt08Model,
        RiggedGrunt09Model,
        RiggedGrunt10Model
    };
    private static readonly int[] AnimatedGruntVariantIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    public bool buildOnStart = true;
    public bool startInSecretLair = true;

    [Header("Character Models")]
    [Tooltip("The Artcraft models are extremely detailed, so the prototype loads one grunt model at a time.")]
    public bool useHandToHandGruntModels = true;
    [Tooltip("Use -1 for animated variety, or 0-9 to force a specific animated variant.")]
    [Range(-1, 9)] public int handToHandGruntVariant = 0;
    public float handToHandGruntScale = 1.55f;
    public bool enableRiggedGruntRagdoll = true;
    [SerializeField, HideInInspector] private int settingsVersion;
    private Transform runtimeRoot;
    private float missionEntryZ = -11.5f;
    private float missionHalfWidth = 14f;
    private float missionSouthEdge = -14f;
    private float missionNorthEdge = 30f;

    private void Awake()
    {
        UpgradeSerializedSettings();
    }

    private void OnValidate()
    {
        UpgradeSerializedSettings();
    }

    private void Start()
    {
        if (!buildOnStart) return;

        EnsureManagers();
        GameFlowManager flow = FindAnyObjectByType<GameFlowManager>();
        flow.Configure(this);

        if (startInSecretLair)
        {
            flow.EnterHubImmediate();
        }
        else
        {
            flow.EnterMissionImmediate(flow.GetMission(0));
        }

    }

    private void UpgradeSerializedSettings()
    {
        if (settingsVersion >= CurrentSettingsVersion) return;

        handToHandGruntVariant = 0;
        enableRiggedGruntRagdoll = true;
        settingsVersion = CurrentSettingsVersion;
    }

    [ContextMenu("Build Prototype Mission")]
    public void Build()
    {
        BuildMission(GameFlowManager.Instance != null ? GameFlowManager.Instance.GetMission(0) : null);
    }

    public void BuildMission(MissionDefinition definition)
    {
        PrepareRuntimeWorld("Mission Runtime World");
        DisableExistingSceneCameras();
        EnsureManagers();
        MissionEnvironmentType environment = definition != null
            ? definition.environmentType
            : MissionEnvironmentType.CorporateAnnex;
        PrototypeEnvironmentArt.BeginMission(environment, definition != null ? definition.contractNumber : 1);
        CreateMissionEnvironment(definition);
        CreatePlayer(new Vector3(0f, 1f, missionEntryZ));
        CreateMissionSecurity(definition);
        CreateMissionObjectives(definition);
        CreateMissionAlarmLighting(definition);
        CreateMissionCheckpoints(definition);
        CreateMissionObjectiveItems(definition);
        CreateEnemies(definition);
        CreateMissionReinforcements(definition);
        CreateExtractionZone();
        CreateMissionReturnTerminal();
        CreateMissionSupplies(definition);
        PrototypeEnvironmentArt.FinalizeMission(runtimeRoot, environment);

        Debug.Log($"Mission environment built: {(definition != null ? definition.displayName : "Prototype Mission")}.");
    }

    [ContextMenu("Build Secret Lair")]
    public void BuildSecretLair()
    {
        PrepareRuntimeWorld("Secret Lair Runtime World");
        DisableExistingSceneCameras();
        EnsureManagers();
        PrototypeEnvironmentArt.BeginHub();
        CreateSecretLairEnvironment();
        CreateLairExteriorGrounds();
        CreatePlayer(new Vector3(0f, 1f, -62f));
        CreateMissionOperationsRoom();
        CreateHubRoomBays();
        CreateRecruitQuarters();
        CreateHubRecruitResidents();
        PrototypeEnvironmentArt.FinalizeHub(runtimeRoot);

        Debug.Log("Secret lair hub built. Use the central mission terminal to deploy.");
    }

    private void PrepareRuntimeWorld(string rootName)
    {
        if (runtimeRoot != null)
        {
            runtimeRoot.gameObject.SetActive(false);
            Destroy(runtimeRoot.gameObject);
        }

        runtimeRoot = new GameObject(rootName).transform;
    }

    private void DisableExistingSceneCameras()
    {
        Camera[] sceneCameras = FindObjectsByType<Camera>();
        foreach (Camera sceneCamera in sceneCameras)
        {
            if (sceneCamera != null)
            {
                sceneCamera.enabled = false;
            }
        }
    }

    private void EnsureManagers()
    {
        if (FindAnyObjectByType<SecretLairManager>() == null)
        {
            new GameObject("SecretLairManager").AddComponent<SecretLairManager>();
        }

        if (FindAnyObjectByType<RecruitRoster>() == null)
        {
            new GameObject("RecruitRoster").AddComponent<RecruitRoster>();
        }

        if (FindAnyObjectByType<MissionManager>() == null)
        {
            new GameObject("MissionManager").AddComponent<MissionManager>();
        }

        if (FindAnyObjectByType<GameFlowManager>() == null)
        {
            new GameObject("GameFlowManager").AddComponent<GameFlowManager>();
        }
    }

    private void CreateMissionObjectives(MissionDefinition definition)
    {
        MissionManager mission = FindAnyObjectByType<MissionManager>();
        if (mission == null) return;
        mission.Configure(definition ?? MissionDefinition.CreatePrototypeMission());
    }

    private void CreateMissionSecurity(MissionDefinition definition)
    {
        int contractNumber = definition != null ? definition.contractNumber : 1;
        if (contractNumber <= 1) return;

        int cameraCount = Mathf.Clamp(2 + (contractNumber - 2) / 5, 2, 5);
        float routeStart = missionEntryZ + 12f;
        float routeEnd = missionNorthEdge - 10f;
        Color cameraColor = new Color(0.11f, 0.15f, 0.17f);
        Color securityAccent = new Color(0.16f, 0.92f, 0.74f);

        for (int index = 0; index < cameraCount; index++)
        {
            float progress = (index + 1f) / (cameraCount + 1f);
            float z = Mathf.Lerp(routeStart, routeEnd, progress);
            float side = index % 2 == 0 ? -1f : 1f;
            float cameraLane = Mathf.Min(missionHalfWidth - 2.2f, 12f + (index % 3) * 2f);
            float x = side * cameraLane;
            Vector3 cameraPosition = new Vector3(x, 3.35f, z);
            Vector3 watchPoint = new Vector3(0f, 1.1f, z + (index % 2 == 0 ? 5f : -5f));

            CreateBlock(
                $"Security Camera {index + 1} Mast",
                new Vector3(x, 1.55f, z),
                new Vector3(0.28f, 3.1f, 0.28f),
                Color.Lerp(cameraColor, Color.black, 0.12f));

            MissionSecurityCamera securityCamera = CreateSecurityCamera(
                $"Security Camera {index + 1}",
                cameraPosition,
                watchPoint,
                cameraColor,
                securityAccent);

            float panelX = x - side * 1.2f;
            Vector3 panelPosition = new Vector3(panelX, 0f, z + (index % 2 == 0 ? 1.7f : -1.7f));
            CreateSecurityCameraControlPanel(
                $"Camera Network Panel {index + 1}",
                panelPosition,
                Quaternion.LookRotation(new Vector3(-side, 0f, 0f), Vector3.up),
                securityCamera,
                securityAccent);
        }

        CreateOptionalSecurityVault(definition, contractNumber);
    }

    private void CreateMissionAlarmLighting(MissionDefinition definition)
    {
        GameObject networkObject = new GameObject("Facility Alarm Lighting Network");
        networkObject.transform.SetParent(runtimeRoot, false);

        float middleZ = (missionSouthEdge + missionNorthEdge) * 0.5f;
        float sideOffset = Mathf.Clamp(missionHalfWidth * 0.58f, 5f, 15f);
        Vector3[] beaconPositions =
        {
            new Vector3(0f, 2.7f, missionEntryZ + 10f),
            new Vector3(-sideOffset, 2.7f, middleZ),
            new Vector3(sideOffset, 2.7f, middleZ),
            new Vector3(0f, 2.7f, missionNorthEdge - 8f),
            new Vector3(-sideOffset * 0.55f, 6.1f, middleZ + 11f),
            new Vector3(sideOffset * 0.55f, 6.1f, middleZ + 11f)
        };

        List<Light> alarmLights = new List<Light>();
        List<Renderer> beaconRenderers = new List<Renderer>();
        for (int index = 0; index < beaconPositions.Length; index++)
        {
            Vector3 position = beaconPositions[index];
            Quaternion rotation = Quaternion.Euler(
                0f,
                index % 2 == 0 ? 0f : 180f,
                0f);
            GameObject beacon = CreateEnvironmentModel(
                "AlarmBeacon_A",
                $"Facility Alarm Beacon {index + 1}",
                position,
                rotation,
                Vector3.one * 0.82f,
                new Color(1f, 0.035f, 0.018f),
                networkObject.transform);
            if (beacon != null)
            {
                beaconRenderers.AddRange(beacon.GetComponentsInChildren<Renderer>(true));
            }

            GameObject lightObject = new GameObject($"Alarm Beacon Light {index + 1}");
            lightObject.transform.SetParent(networkObject.transform, false);
            lightObject.transform.position = position + Vector3.down * 0.15f;
            Light alarmLight = lightObject.AddComponent<Light>();
            alarmLight.type = LightType.Point;
            alarmLight.color = new Color(1f, 0.035f, 0.018f);
            alarmLight.range = 11f;
            alarmLight.intensity = 0f;
            alarmLight.shadows = LightShadows.None;
            alarmLight.enabled = false;
            alarmLights.Add(alarmLight);
        }

        MissionAlarmLightingController controller =
            networkObject.AddComponent<MissionAlarmLightingController>();
        controller.Configure(runtimeRoot, alarmLights, beaconRenderers);
    }

    private void CreateOptionalSecurityVault(MissionDefinition definition, int contractNumber)
    {
        if (contractNumber < 3) return;

        int clearanceLevel = Mathf.Clamp(1 + (contractNumber - 3) / 7, 1, 3);
        float side = contractNumber % 2 == 0 ? -1f : 1f;
        const float vaultWidth = 8f;
        const float vaultDepth = 9f;
        const float vaultHeight = 3.1f;
        float vaultZ = Mathf.Lerp(missionEntryZ + 16f, missionNorthEdge - 12f, 0.62f);
        float vaultX = side * (missionHalfWidth - vaultWidth * 0.5f - 0.9f);
        Vector3 vaultCenter = new Vector3(vaultX, 0f, vaultZ);
        float innerWallX = vaultX - side * vaultWidth * 0.5f;
        float outerWallX = vaultX + side * vaultWidth * 0.5f;
        Color wallColor = new Color(0.16f, 0.2f, 0.22f);
        Color accentColor = clearanceLevel == 1
            ? new Color(0.16f, 0.88f, 0.72f)
            : clearanceLevel == 2
                ? new Color(0.95f, 0.65f, 0.12f)
                : new Color(0.9f, 0.18f, 0.22f);

        CreateBlock(
            $"Clearance {clearanceLevel} Vault Floor",
            vaultCenter + Vector3.up * 0.03f,
            new Vector3(vaultWidth, 0.12f, vaultDepth),
            Color.Lerp(wallColor, Color.black, 0.12f));
        CreateBlock(
            $"Clearance {clearanceLevel} Vault Outer Wall",
            new Vector3(outerWallX, vaultHeight * 0.5f, vaultZ),
            new Vector3(0.32f, vaultHeight, vaultDepth),
            wallColor);
        CreateBlock(
            $"Clearance {clearanceLevel} Vault South Wall",
            new Vector3(vaultX, vaultHeight * 0.5f, vaultZ - vaultDepth * 0.5f),
            new Vector3(vaultWidth, vaultHeight, 0.32f),
            wallColor);
        CreateBlock(
            $"Clearance {clearanceLevel} Vault North Wall",
            new Vector3(vaultX, vaultHeight * 0.5f, vaultZ + vaultDepth * 0.5f),
            new Vector3(vaultWidth, vaultHeight, 0.32f),
            wallColor);
        CreateVerticalDoorWall(
            $"Clearance {clearanceLevel} Vault Entrance Wall",
            innerWallX,
            vaultZ,
            vaultDepth,
            vaultHeight * 0.5f,
            vaultHeight,
            vaultZ,
            2.8f,
            wallColor);
        CreateBlock(
            $"Clearance {clearanceLevel} Vault Roof",
            new Vector3(vaultX, vaultHeight, vaultZ),
            new Vector3(vaultWidth + 0.3f, 0.26f, vaultDepth + 0.3f),
            Color.Lerp(wallColor, Color.black, 0.18f));

        Quaternion entranceRotation = Quaternion.Euler(0f, 90f, 0f);
        CreateEnvironmentModel(
            "DoorFrame_A",
            $"Clearance {clearanceLevel} Vault Modeled Door Frame",
            new Vector3(innerWallX, 0f, vaultZ),
            entranceRotation,
            new Vector3(0.78f, 0.92f, 1f),
            accentColor);
        SlidingSecurityDoor vaultDoor = CreateSlidingSecurityDoor(
            $"Clearance {clearanceLevel} Vault Door",
            new Vector3(innerWallX, 1.4f, vaultZ),
            entranceRotation,
            2.8f,
            2.75f,
            Color.Lerp(wallColor, Color.black, 0.18f),
            accentColor);
        vaultDoor?.ConfigureLock(clearanceLevel);

        CreateEnvironmentModel(
            "WallPanel_A",
            $"Clearance {clearanceLevel} Vault Interior Panel",
            new Vector3(outerWallX - side * 0.18f, 0f, vaultZ),
            Quaternion.Euler(0f, side > 0f ? -90f : 90f, 0f),
            new Vector3(1.45f, 1f, 1f),
            accentColor);
        CreateWorldLabel(
            $"SECURITY VAULT C{clearanceLevel}",
            new Vector3(innerWallX - side * 0.55f, 2.65f, vaultZ),
            accentColor,
            0.08f);

        Vector3 keycardPosition = new Vector3(
            -side * Mathf.Min(10f, missionHalfWidth * 0.34f),
            0.9f,
            Mathf.Lerp(missionEntryZ + 10f, vaultZ, 0.36f));
        CreateMissionKeycard(
            $"Clearance {clearanceLevel} Keycard",
            keycardPosition,
            clearanceLevel,
            accentColor);

        CreateSupplyCache(
            $"Clearance {clearanceLevel} Vault Supply Cache",
            vaultCenter + new Vector3(0f, 0f, -1.5f),
            28 + clearanceLevel * 12,
            20 + clearanceLevel * 10);

        string rewardWeaponId = clearanceLevel == 1
            ? "shock-pistol"
            : clearanceLevel == 2
                ? "dart-smg"
                : "tranq-rifle";
        CreateFloatingWeapon(
            $"Clearance {clearanceLevel} Vault Weapon",
            GameFlowManager.Instance?.GetWeapon(rewardWeaponId),
            vaultCenter + new Vector3(0f, 1.2f, 2.1f));
    }

    private void CreateMissionKeycard(
        string keycardName,
        Vector3 position,
        int clearanceLevel,
        Color accentColor)
    {
        GameObject keycard = CreateEnvironmentModel(
            "Keycard_A",
            keycardName,
            position,
            Quaternion.Euler(0f, 20f, 0f),
            new Vector3(0.72f, 0.72f, 0.72f),
            accentColor);
        if (keycard == null)
        {
            keycard = CreateBlock(
                keycardName,
                position,
                new Vector3(0.5f, 0.08f, 0.75f),
                accentColor);
        }

        EnsureBoxCollider(
            keycard,
            new Vector3(0.82f, 1.1f, 0.28f),
            new Vector3(0f, 0.5f, 0f));
        MissionAccessCredential credential = keycard.AddComponent<MissionAccessCredential>();
        credential.Configure(clearanceLevel);
        CreateWorldLabel(
            $"CLEARANCE {clearanceLevel} KEYCARD",
            position + Vector3.up * 1.55f,
            accentColor,
            0.065f);
    }

    private MissionSecurityCamera CreateSecurityCamera(
        string cameraName,
        Vector3 position,
        Vector3 watchPoint,
        Color bodyColor,
        Color accentColor)
    {
        Vector3 forward = watchPoint - position;
        forward.y = 0f;
        Quaternion cameraRotation = forward.sqrMagnitude > 0.01f
            ? Quaternion.LookRotation(forward.normalized, Vector3.up)
            : Quaternion.identity;
        GameObject modeledCamera = CreateEnvironmentModel(
            "SecurityCamera_A",
            cameraName,
            position,
            cameraRotation,
            Vector3.one,
            accentColor);

        if (modeledCamera != null)
        {
            Transform modeledPivot = FindDescendant(modeledCamera, "Camera_Yaw_Pivot");
            Transform modeledLens = FindDescendant(modeledCamera, "Camera_Lens");
            if (modeledPivot != null && modeledLens != null)
            {
                EnsureBoxCollider(
                    modeledCamera,
                    new Vector3(0.95f, 0.65f, 1.2f),
                    new Vector3(0f, -0.05f, 0.35f));
                GameObject sight = new GameObject("Camera Sight Origin");
                sight.transform.position = modeledLens.position + modeledCamera.transform.forward * 0.2f;
                sight.transform.rotation = modeledCamera.transform.rotation;
                sight.transform.SetParent(modeledPivot, true);

                MissionSecurityCamera modeledController = modeledCamera.AddComponent<MissionSecurityCamera>();
                modeledController.Configure(
                    modeledPivot,
                    sight.transform,
                    modeledLens.GetComponent<Renderer>());
                return modeledController;
            }

            Destroy(modeledCamera);
        }

        GameObject cameraRoot = new GameObject(cameraName);
        cameraRoot.transform.position = position;
        cameraRoot.transform.rotation = cameraRotation;
        ParentToRuntime(cameraRoot);

        CreateSecurityCameraPart(
            "Wall Bracket",
            PrimitiveType.Cube,
            cameraRoot.transform,
            new Vector3(0f, 0f, -0.34f),
            new Vector3(0.32f, 0.24f, 0.55f),
            bodyColor);

        GameObject pivotObject = new GameObject("Scan Pivot");
        pivotObject.transform.SetParent(cameraRoot.transform, false);
        pivotObject.transform.localPosition = Vector3.zero;

        GameObject housing = CreateSecurityCameraPart(
            "Camera Housing",
            PrimitiveType.Cube,
            pivotObject.transform,
            new Vector3(0f, -0.05f, 0.22f),
            new Vector3(0.72f, 0.38f, 0.82f),
            bodyColor);
        housing.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

        GameObject lens = CreateSecurityCameraPart(
            "Camera Lens",
            PrimitiveType.Sphere,
            pivotObject.transform,
            new Vector3(0f, -0.1f, 0.68f),
            new Vector3(0.24f, 0.24f, 0.16f),
            accentColor);

        MissionSecurityCamera camera = cameraRoot.AddComponent<MissionSecurityCamera>();
        camera.Configure(pivotObject.transform, lens.transform, lens.GetComponent<Renderer>());
        return camera;
    }

    private GameObject CreateSecurityCameraPart(
        string partName,
        PrimitiveType primitiveType,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        SetColor(part, color);
        return part;
    }

    private void CreateSecurityCameraControlPanel(
        string panelName,
        Vector3 position,
        Quaternion rotation,
        MissionSecurityCamera linkedCamera,
        Color accentColor)
    {
        GameObject panel = CreateEnvironmentModel(
            "AccessPanel_A",
            panelName,
            position,
            rotation,
            Vector3.one,
            accentColor);

        if (panel == null)
        {
            panel = CreateBlock(
                panelName,
                position + Vector3.up * 0.72f,
                new Vector3(1.1f, 1.45f, 0.5f),
                new Color(0.1f, 0.2f, 0.22f));
            panel.transform.rotation = rotation;
        }

        EnsureBoxCollider(panel, new Vector3(0.85f, 1.5f, 0.42f), new Vector3(0f, 0.75f, 0f));
        Transform statusTransform = FindDescendant(panel, "Access Panel Status Screen");
        Renderer statusRenderer = statusTransform != null
            ? statusTransform.GetComponent<Renderer>()
            : panel.GetComponentInChildren<Renderer>();
        SecurityCameraControlPanel controller = panel.AddComponent<SecurityCameraControlPanel>();
        controller.Configure(linkedCamera, statusRenderer);
    }

    private void CreateMissionCheckpoints(MissionDefinition definition)
    {
        int contractNumber = definition != null ? definition.contractNumber : 1;
        int checkpointCount = contractNumber <= 1 ? 1 : 2;
        float routeStart = missionEntryZ + 8f;
        float routeEnd = missionNorthEdge - 10f;

        for (int index = 0; index < checkpointCount; index++)
        {
            float progress = checkpointCount == 1 ? 0.56f : (index == 0 ? 0.36f : 0.7f);
            float z = Mathf.Lerp(routeStart, routeEnd, progress);

            GameObject trigger = new GameObject($"Field Checkpoint {index + 1}");
            trigger.transform.position = new Vector3(0f, 1.4f, z);
            ParentToRuntime(trigger);
            BoxCollider volume = trigger.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = new Vector3(Mathf.Max(4f, missionHalfWidth * 2f - 4f), 3f, 2.4f);

            MissionCheckpoint checkpoint = trigger.AddComponent<MissionCheckpoint>();
            checkpoint.checkpointId = $"contract-{contractNumber:00}-checkpoint-{index + 1}";
            checkpoint.respawnPosition = new Vector3(0f, 1f, z - 3f);
            checkpoint.respawnEuler = Vector3.zero;

            float terminalX = index % 2 == 0
                ? -missionHalfWidth + 2.5f
                : missionHalfWidth - 2.5f;
            CreateEnvironmentModel(
                "LabConsole_A",
                $"Checkpoint Relay {index + 1}",
                new Vector3(terminalX, 0f, z),
                Quaternion.Euler(0f, index % 2 == 0 ? 90f : -90f, 0f),
                new Vector3(0.62f, 0.62f, 0.62f),
                new Color(0.12f, 0.9f, 0.78f));
            CreateWorldLabel(
                $"FIELD CHECKPOINT {index + 1}",
                new Vector3(terminalX, 2.4f, z),
                new Color(0.35f, 1f, 0.82f),
                0.07f);
        }
    }

    private void CreateMissionObjectiveItems(MissionDefinition definition)
    {
        if (definition == null) return;

        foreach (MissionItemDefinition item in definition.missionItems)
        {
            if (item == null) continue;

            bool prototypeWeapon = item.pickupType == PickupType.PrototypeWeapon;
            GameObject objective = new GameObject(item.displayName);
            objective.name = item.displayName;
            objective.transform.position = prototypeWeapon
                ? item.position + Vector3.down * 1.05f
                : item.position;
            ParentToRuntime(objective);

            BoxCollider interactionCollider = objective.AddComponent<BoxCollider>();
            interactionCollider.isTrigger = true;
            interactionCollider.size = prototypeWeapon
                ? new Vector3(1.75f, 1.55f, 1.35f)
                : new Vector3(1.4f, 1.25f, 0.75f);
            interactionCollider.center = prototypeWeapon
                ? new Vector3(0f, 0.72f, 0f)
                : new Vector3(0f, 0.48f, 0f);

            string displayAsset = prototypeWeapon
                ? "PrototypePedestal_A"
                : "IntelDatapad_A";
            GameObject display = CreateEnvironmentModel(
                displayAsset,
                $"{item.displayName} Display",
                objective.transform.position,
                Quaternion.Euler(0f, 18f, 0f),
                Vector3.one,
                item.accentColor,
                objective.transform);
            DisableChildColliders(display);

            if (prototypeWeapon)
            {
                GameObject weaponPrefab = Resources.Load<GameObject>(
                    "Models/Weapons/ShockPistol");
                if (weaponPrefab != null)
                {
                    GameObject weaponDisplay = Instantiate(weaponPrefab, objective.transform);
                    weaponDisplay.name = $"{item.displayName} Weapon";
                    weaponDisplay.transform.localPosition = new Vector3(0f, 1.28f, 0f);
                    weaponDisplay.transform.localRotation = Quaternion.Euler(0f, 90f, -8f);
                    weaponDisplay.transform.localScale = Vector3.one * 0.42f;
                    DisableChildColliders(weaponDisplay);
                }
            }

            MissionObjectiveInteractable interactable = objective.AddComponent<MissionObjectiveInteractable>();
            interactable.displayName = item.displayName;
            interactable.objectiveType = item.pickupType;
            CreateWorldLabel(
                item.displayName.ToUpperInvariant(),
                item.position + Vector3.up * 1.55f,
                Color.white,
                0.09f,
                objective.transform);
        }
    }

    private static void DisableChildColliders(GameObject target)
    {
        if (target == null) return;
        foreach (Collider childCollider in target.GetComponentsInChildren<Collider>(true))
        {
            childCollider.enabled = false;
        }
    }

    private void CreateMissionEnvironment(MissionDefinition definition)
    {
        MissionEnvironmentType environment = definition != null
            ? definition.environmentType
            : MissionEnvironmentType.CorporateAnnex;
        bool laboratoryLayout = IsLaboratoryLayout(environment);
        bool strongholdLayout = IsStrongholdLayout(environment);
        bool woodlandLayout = IsWoodlandLayout(environment);
        bool undergroundLayout = IsUndergroundLayout(environment);
        Color floorColor = laboratoryLayout
            ? new Color(0.11f, 0.16f, 0.17f)
            : strongholdLayout
                ? new Color(0.19f, 0.16f, 0.17f)
                : woodlandLayout
                    ? new Color(0.2f, 0.24f, 0.17f)
                    : new Color(0.3f, 0.32f, 0.34f);
        Color wallColor = undergroundLayout
            ? new Color(0.66f, 0.69f, 0.72f)
            : Color.Lerp(floorColor, Color.white, 0.16f);
        Color accentColor = environment == MissionEnvironmentType.Dockyard
            ? new Color(0.92f, 0.54f, 0.12f)
            : environment == MissionEnvironmentType.DataCenter
                ? new Color(0.12f, 0.82f, 0.92f)
                : environment == MissionEnvironmentType.ArcticRelay
                    ? new Color(0.42f, 0.78f, 0.95f)
                    : laboratoryLayout
            ? new Color(0.15f, 0.72f, 0.68f)
            : strongholdLayout
                ? new Color(0.72f, 0.16f, 0.2f)
                : woodlandLayout
                    ? new Color(0.24f, 0.5f, 0.62f)
                    : new Color(0.16f, 0.42f, 0.62f);

        float missionCenterZ;
        float missionLength;
        if (laboratoryLayout)
        {
            missionHalfWidth = 44f;
            missionSouthEdge = -112f;
            missionNorthEdge = 95f;
            missionEntryZ = -104f;
        }
        else if (strongholdLayout)
        {
            missionHalfWidth = 22f;
            missionSouthEdge = -24f;
            missionNorthEdge = 48f;
            missionEntryZ = -20f;
        }
        else if (woodlandLayout)
        {
            missionHalfWidth = 34f;
            missionSouthEdge = -82f;
            missionNorthEdge = 54f;
            missionEntryZ = -74f;
        }
        else
        {
            missionHalfWidth = 30f;
            missionSouthEdge = -82f;
            missionNorthEdge = 54f;
            missionEntryZ = -74f;
        }

        missionCenterZ = (missionSouthEdge + missionNorthEdge) * 0.5f;
        missionLength = missionNorthEdge - missionSouthEdge;
        CreateBlock("Mission Floor", new Vector3(0f, -0.55f, missionCenterZ), new Vector3(missionHalfWidth * 2f, 1f, missionLength), floorColor);
        CreateBlock("West Perimeter", new Vector3(-missionHalfWidth, 1.7f, missionCenterZ), new Vector3(0.5f, 4.5f, missionLength), wallColor);
        CreateBlock("East Perimeter", new Vector3(missionHalfWidth, 1.7f, missionCenterZ), new Vector3(0.5f, 4.5f, missionLength), wallColor);
        CreateBlock("South Perimeter", new Vector3(0f, 1.7f, missionSouthEdge), new Vector3(missionHalfWidth * 2f, 4.5f, 0.5f), wallColor);
        CreateBlock("North Perimeter", new Vector3(0f, 1.7f, missionNorthEdge), new Vector3(missionHalfWidth * 2f, 4.5f, 0.5f), wallColor);

        if (environment == MissionEnvironmentType.DataCenter)
        {
            CreateDataCenterInfiltration(wallColor, accentColor);
        }
        else if (environment == MissionEnvironmentType.Dockyard)
        {
            CreateDockyardInfiltration(wallColor, accentColor);
        }
        else if (environment == MissionEnvironmentType.Embassy)
        {
            CreateEmbassyInfiltration(wallColor, accentColor);
        }
        else if (environment == MissionEnvironmentType.ArcticRelay)
        {
            CreateArcticRelayInfiltration(wallColor, accentColor);
        }
        else if (undergroundLayout)
        {
            CreateUndergroundRecordsStation(wallColor, accentColor);
        }
        else if (laboratoryLayout)
        {
            CreateWeaponsLaboratoryInfiltration(wallColor, accentColor);
        }
        else if (woodlandLayout)
        {
            CreateWoodlandAnnexInfiltration(wallColor, accentColor);
        }
        else
        {
            CreateBlock("Service Entry West", new Vector3(-4f, 1.5f, -5f), new Vector3(0.3f, 3f, 17f), wallColor);
            CreateBlock("Service Entry East", new Vector3(4f, 1.5f, -5f), new Vector3(0.3f, 3f, 17f), wallColor);
            CreateDoorwayWall("Outer Security Door", 2f, 5f, 3.5f, wallColor);
            CreateBlock("Stronghold Gate West", new Vector3(-8f, 1.7f, 6f), new Vector3(7.5f, 3.4f, 0.5f), wallColor);
            CreateBlock("Stronghold Gate East", new Vector3(8f, 1.7f, 6f), new Vector3(7.5f, 3.4f, 0.5f), wallColor);
            CreateDoorwayWall("Inner Security Door", 11f, -5f, 3.5f, wallColor);
            CreateDoorwayWall("Executive Door", 18f, 0f, 4f, wallColor);
            CreateBlock("Security Post West", new Vector3(-8.5f, 1f, 13f), new Vector3(3f, 2f, 3f), accentColor);
            CreateBlock("Security Post East", new Vector3(8.5f, 1f, 13f), new Vector3(3f, 2f, 3f), accentColor);
            CreateBlock("Executive Dais", new Vector3(0f, 0.3f, 21f), new Vector3(8f, 0.6f, 6f), new Color(0.28f, 0.24f, 0.25f));
            CreateBlock("Central Cover", new Vector3(0f, 0.8f, 11f), new Vector3(4f, 1.6f, 1.2f), wallColor);
            CreateBlock("West Gallery Cover", new Vector3(-10.5f, 0.8f, 20f), new Vector3(3f, 1.6f, 1.2f), accentColor);
            CreateBlock("East Gallery Cover", new Vector3(10.5f, 0.8f, 20f), new Vector3(3f, 1.6f, 1.2f), accentColor);
        }

        CreateExtendedEnvironmentDressing(definition, environment, wallColor, accentColor);
        CreateContractRouteVariation(definition, environment, accentColor);
        CreateTacticalRouteDressing(definition, environment, accentColor);
        CreateMissionNavigationCues(definition, accentColor);
        string missionTitle = definition != null ? definition.displayName.ToUpperInvariant() : "FIELD OPERATION";
        CreateWorldLabel(missionTitle, new Vector3(0f, 3.2f, missionNorthEdge - 0.35f), Color.white, 0.22f);
    }

    private void CreateContractRouteVariation(
        MissionDefinition definition,
        MissionEnvironmentType environment,
        Color accentColor)
    {
        if (definition == null || definition.contractNumber <= 4) return;

        int contractNumber = definition.contractNumber;
        int visitIndex = Mathf.Max(0, (contractNumber - 5) / 4);
        float direction = contractNumber % 2 == 0 ? -1f : 1f;
        Color secondaryAccent = Color.Lerp(accentColor, Color.white, 0.22f);

        if (environment == MissionEnvironmentType.Dockyard)
        {
            CreateContractRouteProp(
                "CargoContainer_A",
                $"{definition.displayName} Insertion Container",
                new Vector3(direction * 13f, 0f, -63f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1f, 1f, 1.55f),
                new Vector3(6.1f, 2.55f, 2.45f),
                new Vector3(0f, 1.275f, 0f),
                accentColor);
            CreateContractRouteProp(
                "SupplyCrate_A",
                $"{definition.displayName} Customs Cover",
                new Vector3(-direction * 8f, 0f, -55f),
                Quaternion.Euler(0f, direction * 18f, 0f),
                Vector3.one * 1.45f,
                new Vector3(2.45f, 1.65f, 2.25f),
                new Vector3(0f, 0.82f, 0f),
                secondaryAccent);

            if (visitIndex >= 1)
            {
                CreateContractRouteProp(
                    visitIndex >= 3 ? "PipeCluster_A" : "CargoContainer_A",
                    $"{definition.displayName} Reversed Freight Gate",
                    new Vector3(-direction * 14f, 0f, -34f),
                    Quaternion.Euler(0f, 90f, 0f),
                    visitIndex >= 3 ? Vector3.one * 1.75f : new Vector3(1f, 1f, 1.45f),
                    visitIndex >= 3
                        ? new Vector3(3.2f, 2.35f, 1.65f)
                        : new Vector3(6.1f, 2.55f, 2.45f),
                    visitIndex >= 3
                        ? new Vector3(0f, 1.12f, 0f)
                        : new Vector3(0f, 1.275f, 0f),
                    secondaryAccent);
            }
        }
        else if (environment == MissionEnvironmentType.Embassy)
        {
            CreateContractRouteProp(
                visitIndex >= 2 ? "EmbassyFountain_A" : "FacilityBench_A",
                $"{definition.displayName} Processional Cover",
                new Vector3(direction * 8.5f, 0f, -8f),
                Quaternion.Euler(0f, direction > 0f ? 25f : -25f, 0f),
                visitIndex >= 2 ? Vector3.one * 0.72f : Vector3.one,
                visitIndex >= 2
                    ? new Vector3(3.7f, 2.4f, 3.7f)
                    : new Vector3(3.25f, 1.85f, 0.9f),
                visitIndex >= 2
                    ? new Vector3(0f, 1.2f, 0f)
                    : new Vector3(0f, 0.9f, 0f),
                accentColor);
            CreateContractRouteProp(
                "OfficeDesk_A",
                $"{definition.displayName} Reception Checkpoint",
                new Vector3(-direction * 7f, 0f, -2f),
                Quaternion.Euler(0f, direction * 90f, 0f),
                Vector3.one,
                new Vector3(3.25f, 2.2f, 1.35f),
                new Vector3(0f, 1.05f, 0f),
                secondaryAccent);

            if (visitIndex >= 2)
            {
                CreateContractRouteProp(
                    "GuardBooth_A",
                    $"{definition.displayName} Inner Security Gate",
                    new Vector3(direction * 11f, 0f, 20f),
                    Quaternion.Euler(0f, -direction * 90f, 0f),
                    Vector3.one * 0.72f,
                    new Vector3(4.2f, 3.1f, 3.2f),
                    new Vector3(0f, 1.55f, 0f),
                    accentColor);
            }
        }
        else if (environment == MissionEnvironmentType.DataCenter)
        {
            CreateContractRouteProp(
                "ServerRack_A",
                $"{definition.displayName} Service-Trench Rack",
                new Vector3(direction * 5.2f, 0f, -61f),
                Quaternion.Euler(0f, direction > 0f ? 90f : -90f, 0f),
                new Vector3(1.35f, 1.35f, 1.75f),
                new Vector3(1.55f, 2.75f, 1.25f),
                new Vector3(0f, 1.38f, 0f),
                accentColor);
            CreateContractRouteProp(
                visitIndex >= 2 ? "CryoCore_A" : "LockerBank_A",
                $"{definition.displayName} First Hall Island",
                new Vector3(-direction * 5.6f, 0f, -39f),
                Quaternion.Euler(0f, -direction * 90f, 0f),
                visitIndex >= 2 ? Vector3.one * 0.82f : Vector3.one,
                visitIndex >= 2
                    ? new Vector3(3f, 3.9f, 3f)
                    : new Vector3(3.85f, 2.65f, 0.82f),
                visitIndex >= 2
                    ? new Vector3(0f, 1.95f, 0f)
                    : new Vector3(0f, 1.32f, 0f),
                secondaryAccent);

            if (visitIndex >= 1)
            {
                CreateContractRouteProp(
                    "ServerRack_A",
                    $"{definition.displayName} Reversed Data Gate",
                    new Vector3(-direction * 5.2f, 0f, -8f),
                    Quaternion.Euler(0f, direction > 0f ? -90f : 90f, 0f),
                    new Vector3(1.35f, 1.35f, 1.75f),
                    new Vector3(1.55f, 2.75f, 1.25f),
                    new Vector3(0f, 1.38f, 0f),
                    accentColor);
            }
        }
        else if (environment == MissionEnvironmentType.ArcticRelay)
        {
            CreateContractRouteProp(
                "RockFormation_A",
                $"{definition.displayName} South Ice Outcrop",
                new Vector3(direction * 10f, 0.2f, -47f),
                Quaternion.Euler(0f, direction * 24f, 0f),
                new Vector3(2f, 1.45f, 1.7f),
                new Vector3(4f, 2.3f, 3.4f),
                new Vector3(0f, 1.05f, 0f),
                accentColor);
            CreateContractRouteProp(
                visitIndex >= 2 ? "ArcticBunker_A" : "SupplyCrate_A",
                $"{definition.displayName} Field Shelter",
                new Vector3(-direction * 12f, 0f, -28f),
                Quaternion.Euler(0f, -direction * 18f, 0f),
                visitIndex >= 2 ? Vector3.one * 0.72f : Vector3.one * 1.35f,
                visitIndex >= 2
                    ? new Vector3(7f, 3.35f, 5f)
                    : new Vector3(2.45f, 1.65f, 2.25f),
                visitIndex >= 2
                    ? new Vector3(0f, 1.68f, 0f)
                    : new Vector3(0f, 0.82f, 0f),
                secondaryAccent);

            if (visitIndex >= 1)
            {
                CreateContractRouteProp(
                    "RockFormation_A",
                    $"{definition.displayName} North Ice Outcrop",
                    new Vector3(-direction * 9f, 0.2f, -13f),
                    Quaternion.Euler(0f, -direction * 32f, 0f),
                    new Vector3(2.2f, 1.55f, 1.8f),
                    new Vector3(4.3f, 2.45f, 3.7f),
                    new Vector3(0f, 1.1f, 0f),
                    accentColor);
            }
        }
        else if (environment == MissionEnvironmentType.ExecutiveStronghold &&
                 contractNumber >= 20)
        {
            CreateContractRouteProp(
                "GuardBooth_A",
                "Last Light Outer Command Post",
                new Vector3(direction * 8.5f, 0f, -7f),
                Quaternion.Euler(0f, -direction * 90f, 0f),
                Vector3.one * 0.72f,
                new Vector3(4.2f, 3.1f, 3.2f),
                new Vector3(0f, 1.55f, 0f),
                accentColor);
            CreateContractRouteProp(
                "WeaponRack_A",
                "Last Light Inner Armament Gate",
                new Vector3(-direction * 8f, 0f, 11f),
                Quaternion.Euler(0f, direction * 90f, 0f),
                Vector3.one,
                new Vector3(3.4f, 2.7f, 0.8f),
                new Vector3(0f, 1.35f, 0f),
                secondaryAccent);
        }

        CreateWorldLabel(
            $"CONTRACT {contractNumber:00} ROUTE",
            new Vector3(direction * Mathf.Min(10f, missionHalfWidth * 0.42f), 3f, missionEntryZ + 18f),
            Color.white,
            0.06f);
    }

    private void CreateContractRouteProp(
        string assetName,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Vector3 colliderSize,
        Vector3 colliderCenter,
        Color accentColor)
    {
        GameObject prop = CreateEnvironmentModel(
            assetName,
            objectName,
            position,
            rotation,
            scale,
            accentColor);
        if (prop != null)
        {
            EnsureBoxCollider(prop, colliderSize, colliderCenter);
        }
    }

    private void CreateTacticalRouteDressing(
        MissionDefinition definition,
        MissionEnvironmentType environment,
        Color accentColor)
    {
        float routeLength = missionNorthEdge - missionEntryZ;
        float stagingZ = missionEntryZ + routeLength * 0.22f;
        float midpointZ = missionEntryZ + routeLength * 0.5f;
        float approachZ = missionEntryZ + routeLength * 0.76f;
        float lateral = Mathf.Clamp(missionHalfWidth * 0.47f, 8.5f, 20f);
        int contractNumber = definition != null ? definition.contractNumber : 1;

        if (environment == MissionEnvironmentType.Dockyard)
        {
            CreateTacticalRouteProp(
                "CargoContainer_A",
                "Insertion Freight Cover",
                new Vector3(-lateral, 0f, stagingZ),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1f, 1f, 1.35f),
                new Vector3(6.1f, 2.55f, 2.45f),
                new Vector3(0f, 1.275f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "SupplyCrate_A",
                "Dockside Resupply Cover",
                new Vector3(lateral, 0f, midpointZ),
                Quaternion.Euler(0f, -18f, 0f),
                Vector3.one * 1.25f,
                new Vector3(2.4f, 1.55f, 2.1f),
                new Vector3(0f, 0.78f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "PipeCluster_A",
                "Objective Fuel Junction",
                new Vector3(-lateral * 0.82f, 0.15f, approachZ),
                Quaternion.Euler(0f, 90f, 0f),
                Vector3.one * 1.45f,
                new Vector3(2.9f, 2.2f, 1.45f),
                new Vector3(0f, 1.1f, 0f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.DataCenter)
        {
            CreateTacticalRouteProp(
                "ServerRack_A",
                "Ingress Server Island",
                new Vector3(-lateral, 0f, stagingZ),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1.35f, 1.35f, 1.8f),
                new Vector3(1.55f, 2.75f, 1.25f),
                new Vector3(0f, 1.38f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "LockerBank_A",
                "Network Security Locker Cover",
                new Vector3(lateral, 0f, midpointZ),
                Quaternion.Euler(0f, -90f, 0f),
                Vector3.one,
                new Vector3(3.85f, 2.65f, 0.82f),
                new Vector3(0f, 1.32f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "SupplyCrate_A",
                "Core Maintenance Supplies",
                new Vector3(-lateral * 0.82f, 0f, approachZ),
                Quaternion.Euler(0f, 14f, 0f),
                Vector3.one * 1.15f,
                new Vector3(2.3f, 1.45f, 2f),
                new Vector3(0f, 0.72f, 0f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.Embassy ||
                 environment == MissionEnvironmentType.ExecutiveStronghold)
        {
            CreateTacticalRouteProp(
                "FacilityBench_A",
                "Reception Waiting Cover",
                new Vector3(-lateral, 0f, stagingZ),
                Quaternion.Euler(0f, 18f, 0f),
                Vector3.one,
                new Vector3(3.25f, 1.85f, 0.9f),
                new Vector3(0f, 0.9f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "OfficeDesk_A",
                "Diplomatic Security Desk",
                new Vector3(lateral, 0f, midpointZ),
                Quaternion.Euler(0f, -90f, 0f),
                Vector3.one,
                new Vector3(3.25f, 2.2f, 1.35f),
                new Vector3(0f, 1.05f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "BriefingTable_A",
                contractNumber >= 18 ? "Command Strategy Table" : "Operations Briefing Table",
                new Vector3(-lateral * 0.76f, 0f, approachZ),
                Quaternion.Euler(0f, 12f, 0f),
                Vector3.one,
                new Vector3(3.7f, 1.5f, 2.25f),
                new Vector3(0f, 0.75f, 0f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.WoodlandAnnex ||
                 environment == MissionEnvironmentType.ArcticRelay)
        {
            CreateTacticalRouteProp(
                "RockFormation_A",
                "Natural Insertion Cover",
                new Vector3(-lateral, 0.2f, stagingZ),
                Quaternion.Euler(0f, 24f, 0f),
                new Vector3(1.8f, 1.35f, 1.55f),
                new Vector3(3.7f, 2.15f, 3.2f),
                new Vector3(0f, 0.95f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "SupplyCrate_A",
                "Field Logistics Cache",
                new Vector3(lateral, 0f, midpointZ),
                Quaternion.Euler(0f, -16f, 0f),
                Vector3.one * 1.2f,
                new Vector3(2.4f, 1.5f, 2.1f),
                new Vector3(0f, 0.75f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "RockFormation_A",
                "Objective Overwatch Cover",
                new Vector3(-lateral * 0.82f, 0.2f, approachZ),
                Quaternion.Euler(0f, -32f, 0f),
                new Vector3(2.1f, 1.5f, 1.7f),
                new Vector3(4.1f, 2.35f, 3.5f),
                new Vector3(0f, 1.05f, 0f),
                accentColor);
        }
        else
        {
            CreateTacticalRouteProp(
                "SupplyCrate_A",
                "Insertion Equipment Stack",
                new Vector3(-lateral, 0f, stagingZ),
                Quaternion.Euler(0f, 18f, 0f),
                Vector3.one * 1.2f,
                new Vector3(2.4f, 1.5f, 2.1f),
                new Vector3(0f, 0.75f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "LockerBank_A",
                "Security Equipment Cover",
                new Vector3(lateral, 0f, midpointZ),
                Quaternion.Euler(0f, -90f, 0f),
                Vector3.one,
                new Vector3(3.85f, 2.65f, 0.82f),
                new Vector3(0f, 1.32f, 0f),
                accentColor);
            CreateTacticalRouteProp(
                "OfficeDesk_A",
                "Forward Operations Desk",
                new Vector3(-lateral * 0.82f, 0f, approachZ),
                Quaternion.Euler(0f, 16f, 0f),
                Vector3.one,
                new Vector3(3.25f, 2.2f, 1.35f),
                new Vector3(0f, 1.05f, 0f),
                accentColor);
        }

        CreateWorldLabel(
            "TACTICAL STAGING",
            new Vector3(-lateral, 2.8f, stagingZ),
            Color.white,
            0.065f);
    }

    private void CreateTacticalRouteProp(
        string assetName,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Vector3 colliderSize,
        Vector3 colliderCenter,
        Color accentColor)
    {
        GameObject prop = CreateEnvironmentModel(
            assetName,
            objectName,
            position,
            rotation,
            scale,
            accentColor);
        if (prop != null)
        {
            EnsureBoxCollider(prop, colliderSize, colliderCenter);
        }
    }

    private void CreateMissionNavigationCues(MissionDefinition definition, Color accentColor)
    {
        float midRouteZ = Mathf.Lerp(missionEntryZ, missionNorthEdge, 0.5f);
        string missionCode = definition != null
            ? $"CONTRACT {definition.contractNumber:00}"
            : "FIELD CONTRACT";

        CreateMissionDirectory(
            "Insertion Route Directory",
            new Vector3(-8f, 0f, missionEntryZ + 10f),
            $"{missionCode}\nOBJECTIVE  NORTH\nEXTRACTION  SOUTH",
            18f,
            accentColor);
        CreateMissionDirectory(
            "Mid Route Directory",
            new Vector3(8f, 0f, midRouteZ),
            "SECURE ZONE  NORTH\nFIELD EXIT  SOUTH",
            -18f,
            accentColor);
        CreateMissionDirectory(
            "Objective Approach Directory",
            new Vector3(-8f, 0f, missionNorthEdge - 10f),
            "PRIMARY AREA\nEXTRACTION  SOUTH",
            18f,
            accentColor);
    }

    private void CreateMissionDirectory(
        string objectName,
        Vector3 position,
        string label,
        float yaw,
        Color accentColor)
    {
        GameObject directory = CreateEnvironmentModel(
            "AccessPanel_A",
            objectName,
            position,
            Quaternion.Euler(0f, yaw, 0f),
            Vector3.one * 1.25f,
            accentColor);
        if (directory != null)
        {
            EnsureBoxCollider(
                directory,
                new Vector3(1.05f, 2.55f, 0.35f),
                new Vector3(0f, 1.25f, 0f));
        }
        CreateWorldLabel(
            label,
            position + Vector3.up * 2.65f,
            Color.white,
            0.065f,
            directory != null ? directory.transform : null);
    }

    private void CreateExtendedEnvironmentDressing(
        MissionDefinition definition,
        MissionEnvironmentType environment,
        Color wallColor,
        Color accentColor)
    {
        int contractNumber = definition != null ? definition.contractNumber : 1;
        if (environment == MissionEnvironmentType.Dockyard)
        {
            for (int lane = -1; lane <= 1; lane += 2)
            {
                for (int row = 0; row < 4; row++)
                {
                    Vector3 position = new Vector3(lane * 34f, 0f, -48f + row * 29f);
                    GameObject container = CreateEnvironmentModel(
                        "CargoContainer_A",
                        $"Cargo Container {lane} {row}",
                        position,
                        Quaternion.Euler(0f, 90f, 0f),
                        new Vector3(1f, 1f, 1.85f),
                        row % 2 == 0 ? accentColor : Color.Lerp(accentColor, Color.red, 0.36f));
                    if (container != null)
                    {
                        EnsureBoxCollider(container, new Vector3(6.1f, 2.55f, 2.45f), new Vector3(0f, 1.275f, 0f));
                    }
                }
            }
            CreateEnvironmentModel("PipeCluster_A", "Dock Fuel Manifold", new Vector3(-29f, 0f, 55f), Quaternion.identity, new Vector3(1.8f, 1.8f, 1.8f), accentColor);
            CreateEnvironmentModel("PipeCluster_A", "Dock Coolant Manifold", new Vector3(29f, 0f, 55f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.8f, 1.8f, 1.8f), accentColor);
            CreateEnvironmentModel(
                "DockCrane_A",
                contractNumber >= 17 ? "Flagship Cargo Crane" : "Freight Transfer Crane",
                new Vector3(0f, 0f, 78f),
                Quaternion.identity,
                contractNumber >= 17 ? new Vector3(1.35f, 1.35f, 1.35f) : Vector3.one,
                accentColor);
            GameObject dockBooth = CreateEnvironmentModel(
                "GuardBooth_A",
                "Dock Customs Booth",
                new Vector3(-29f, 0f, -17f),
                Quaternion.Euler(0f, 90f, 0f),
                Vector3.one,
                accentColor);
            if (dockBooth != null)
            {
                EnsureBoxCollider(dockBooth, new Vector3(4.2f, 3.1f, 3.2f), new Vector3(0f, 1.55f, 0f));
            }
        }
        else if (environment == MissionEnvironmentType.Embassy)
        {
            GameObject portal = CreateEnvironmentModel(
                "EmbassyPortal_A",
                "Embassy Grand Portal",
                new Vector3(0f, 0f, 18f),
                Quaternion.identity,
                Vector3.one,
                accentColor);
            if (portal != null)
            {
                EnsureBoxCollider(portal, new Vector3(7.65f, 0.8f, 1.05f), new Vector3(0f, 3.82f, 0f));
            }
            for (int side = -1; side <= 1; side += 2)
            {
                CreateEnvironmentModel("SupportColumn_A", $"Embassy Column {side} A", new Vector3(side * 8f, 0f, 8f), Quaternion.identity, new Vector3(1.35f, 1.45f, 1.35f), accentColor);
                CreateEnvironmentModel("SupportColumn_A", $"Embassy Column {side} B", new Vector3(side * 8f, 0f, 18f), Quaternion.identity, new Vector3(1.35f, 1.45f, 1.35f), accentColor);
                CreateEnvironmentModel("CeilingLight_A", $"Embassy Sconce {side}", new Vector3(side * 5f, 3.2f, 13f), Quaternion.identity, Vector3.one, accentColor);
            }
            GameObject fountain = CreateEnvironmentModel(
                "EmbassyFountain_A",
                contractNumber >= 14 ? "Crystal Reception Fountain" : "Diplomatic Fountain",
                new Vector3(-12f, 0f, 25f),
                Quaternion.identity,
                new Vector3(0.82f, 0.82f, 0.82f),
                accentColor);
            if (fountain != null)
            {
                EnsureBoxCollider(fountain, new Vector3(4.1f, 2.7f, 4.1f), new Vector3(0f, 1.35f, 0f));
            }
            CreateEnvironmentModel(
                "GuardBooth_A",
                contractNumber >= 18 ? "Fortified Diplomatic Checkpoint" : "Embassy Security Desk",
                new Vector3(11.8f, 0f, 30f),
                Quaternion.Euler(0f, -90f, 0f),
                new Vector3(0.8f, 0.8f, 0.8f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.DataCenter)
        {
            for (int row = 0; row < 4; row++)
            {
                float z = -42f + row * 24f;
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 rackPosition = new Vector3(side * 19f, 0f, z);
                    GameObject rack = CreateEnvironmentModel(
                        "ServerRack_A",
                        $"Server Rack {side} {row}",
                        rackPosition,
                        Quaternion.Euler(0f, side > 0 ? 90f : -90f, 0f),
                        new Vector3(1.25f, 1.25f, 1.7f),
                        accentColor);
                    if (rack != null)
                    {
                        EnsureBoxCollider(rack, new Vector3(1.45f, 2.55f, 1.1f), new Vector3(0f, 1.275f, 0f));
                    }
                }
            }
            CreateEnvironmentModel("PipeCluster_A", "Data Cooling Junction", new Vector3(-24f, 0f, 39f), Quaternion.identity, new Vector3(1.4f, 1.4f, 1.4f), accentColor);
            CreateEnvironmentModel("VentUnit_A", "Data Exhaust Unit", new Vector3(24f, 0f, 39f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.4f, 1.4f, 1.4f), accentColor);
            CreateEnvironmentModel(
                "LabConsole_A",
                contractNumber >= 19 ? "Ghost Network Command Console" : "Data Operations Console",
                new Vector3(-11f, 0f, 44f),
                Quaternion.Euler(0f, 20f, 0f),
                Vector3.one,
                accentColor);
            if (contractNumber >= 15)
            {
                GameObject cryoCore = CreateEnvironmentModel(
                    "CryoCore_A",
                    contractNumber >= 19 ? "Command Network Core" : "Cryogenic Weapon Core",
                    new Vector3(10.5f, 0f, 44f),
                    Quaternion.identity,
                    Vector3.one,
                    accentColor);
                if (cryoCore != null)
                {
                    EnsureBoxCollider(cryoCore, new Vector3(3f, 3.9f, 3f), new Vector3(0f, 1.95f, 0f));
                }
            }
        }
        else if (environment == MissionEnvironmentType.ArcticRelay)
        {
            for (int i = 0; i < 10; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float z = -62f + i * 12f;
                CreateMissionRock($"Arctic Rock {i + 1}", new Vector3(side * (25f + (i % 3) * 2f), 0.25f, z), new Vector3(2.8f, 1.5f, 2.2f), wallColor);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject relay = CreateEnvironmentModel(
                    "RelayAntenna_A",
                    $"Arctic Relay Antenna {side}",
                    new Vector3(side * 19f, 0f, 35f),
                    Quaternion.Euler(0f, side > 0 ? 18f : -18f, 0f),
                    new Vector3(1.2f, 1.2f, 1.2f),
                    accentColor);
                if (relay != null)
                {
                    EnsureBoxCollider(relay, new Vector3(2.4f, 1.55f, 2.4f), new Vector3(0f, 0.78f, 0f));
                }
            }
            CreateEnvironmentModel("VentUnit_A", "Relay Heater West", new Vector3(-20f, 0f, 36f), Quaternion.identity, new Vector3(1.6f, 1.6f, 1.6f), accentColor);
            CreateEnvironmentModel("VentUnit_A", "Relay Heater East", new Vector3(20f, 0f, 36f), Quaternion.Euler(0f, 180f, 0f), new Vector3(1.6f, 1.6f, 1.6f), accentColor);
            GameObject bunker = CreateEnvironmentModel(
                "ArcticBunker_A",
                contractNumber >= 16 ? "Polar Garrison Bunker" : "Relay Field Shelter",
                new Vector3(-24f, 0f, -2f),
                Quaternion.Euler(0f, 18f, 0f),
                new Vector3(0.9f, 0.9f, 0.9f),
                accentColor);
            if (bunker != null)
            {
                EnsureBoxCollider(bunker, new Vector3(7f, 3.35f, 5f), new Vector3(0f, 1.68f, 0f));
            }
            CreateEnvironmentModel(
                "LabConsole_A",
                "Relay Field Control",
                new Vector3(21f, 0f, -3f),
                Quaternion.Euler(0f, -24f, 0f),
                new Vector3(0.85f, 0.85f, 0.85f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.WeaponsLaboratory)
        {
            CreateEnvironmentModel(
                "GuardBooth_A",
                "Black Glass Perimeter Booth",
                new Vector3(-25f, 0f, -12f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(0.9f, 0.9f, 0.9f),
                accentColor);
            CreateEnvironmentModel(
                "LabConsole_A",
                "Arc Prototype Control Console",
                new Vector3(-5.5f, 6.8f, 66f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(0.9f, 0.9f, 0.9f),
                accentColor);
            CreateEnvironmentModel(
                "CryoCore_A",
                "Arc Prototype Containment Core",
                new Vector3(6f, 6.8f, 66f),
                Quaternion.identity,
                new Vector3(0.82f, 0.82f, 0.82f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.CorporateAnnex)
        {
            CreateEnvironmentModel(
                "GuardBooth_A",
                "Arrival Security Booth",
                new Vector3(-10f, 0f, -69f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(0.8f, 0.8f, 0.8f),
                accentColor);
            CreateEnvironmentModel(
                "LabConsole_A",
                "Central Archive Terminal",
                new Vector3(-7.5f, 0f, 46f),
                Quaternion.Euler(0f, 25f, 0f),
                new Vector3(0.9f, 0.9f, 0.9f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.WoodlandAnnex)
        {
            CreateEnvironmentModel(
                "GuardBooth_A",
                "Annex Gatehouse",
                new Vector3(20f, 0f, -8f),
                Quaternion.Euler(0f, -90f, 0f),
                new Vector3(0.85f, 0.85f, 0.85f),
                accentColor);
            CreateEnvironmentModel(
                "LabConsole_A",
                "Command Roster Terminal",
                new Vector3(-4.8f, 3.4f, 31f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(0.82f, 0.82f, 0.82f),
                accentColor);
        }
        else if (environment == MissionEnvironmentType.ExecutiveStronghold)
        {
            CreateEnvironmentModel(
                "EmbassyPortal_A",
                contractNumber >= 20 ? "Last Light Citadel Arch" : "Crownfall Executive Arch",
                new Vector3(0f, 0f, 6f),
                Quaternion.identity,
                new Vector3(1.12f, 1.12f, 1.12f),
                accentColor);
            CreateEnvironmentModel(
                "GuardBooth_A",
                contractNumber >= 20 ? "Citadel Command Post" : "Executive Security Post",
                new Vector3(10.5f, 0f, 13f),
                Quaternion.Euler(0f, -90f, 0f),
                new Vector3(0.75f, 0.75f, 0.75f),
                accentColor);
            CreateEnvironmentModel(
                "LabConsole_A",
                contractNumber >= 20 ? "Syndicate Final Command Console" : "Cipher Command Console",
                new Vector3(-8f, 0f, 21f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(0.88f, 0.88f, 0.88f),
                accentColor);
            if (contractNumber >= 20)
            {
                CreateEnvironmentModel(
                    "CryoCore_A",
                    "Last Light Prototype Vault",
                    new Vector3(8f, 0f, 21f),
                    Quaternion.identity,
                    new Vector3(0.9f, 0.9f, 0.9f),
                    accentColor);
            }
        }
    }

    private static bool IsLaboratoryLayout(MissionEnvironmentType environment)
    {
        return environment == MissionEnvironmentType.WeaponsLaboratory ||
            environment == MissionEnvironmentType.Dockyard;
    }

    private static bool IsStrongholdLayout(MissionEnvironmentType environment)
    {
        return environment == MissionEnvironmentType.ExecutiveStronghold ||
            environment == MissionEnvironmentType.Embassy;
    }

    private static bool IsWoodlandLayout(MissionEnvironmentType environment)
    {
        return environment == MissionEnvironmentType.WoodlandAnnex ||
            environment == MissionEnvironmentType.ArcticRelay;
    }

    private static bool IsUndergroundLayout(MissionEnvironmentType environment)
    {
        return environment == MissionEnvironmentType.CorporateAnnex ||
            environment == MissionEnvironmentType.DataCenter;
    }

    private void CreateUndergroundRecordsStation(Color wallColor, Color accentColor)
    {
        const float wallHeight = 3.95f;
        const float wallY = wallHeight * 0.5f;
        const float centralHalfWidth = 5f;
        const float sideRoomCenterX = 17.5f;
        Color interiorWall = Color.Lerp(wallColor, Color.white, 0.08f);
        Color darkPanel = new Color(0.08f, 0.12f, 0.2f);
        Color floorAccent = new Color(0.14f, 0.18f, 0.23f);

        CreateBlock(
            "Underground Facility Ceiling",
            new Vector3(0f, 4.15f, -14f),
            new Vector3(59.5f, 0.4f, 135.5f),
            new Color(0.19f, 0.21f, 0.23f));

        CreateSegmentedVerticalWall(
            "Transit Corridor West Wall",
            -centralHalfWidth,
            -61f,
            31f,
            new[] { -45f, -15f, 15f },
            4f,
            wallHeight,
            2.85f,
            interiorWall);
        CreateSegmentedVerticalWall(
            "Transit Corridor East Wall",
            centralHalfWidth,
            -61f,
            31f,
            new[] { -45f, -15f, 15f },
            4f,
            wallHeight,
            2.85f,
            interiorWall);

        float[] roomDividers = { -61f, -29f, 1f, 31f };
        foreach (float dividerZ in roomDividers)
        {
            CreateHorizontalDoorWall(
                $"West Wing Partition {dividerZ}",
                -sideRoomCenterX,
                dividerZ,
                25f,
                wallY,
                wallHeight,
                4f,
                interiorWall);
            CreateHorizontalDoorWall(
                $"East Wing Partition {dividerZ}",
                sideRoomCenterX,
                dividerZ,
                25f,
                wallY,
                wallHeight,
                4f,
                interiorWall);
        }

        CreateBlock("Arrival Hall Rear Panel", new Vector3(0f, wallY, -78.5f), new Vector3(14f, wallHeight, 0.35f), darkPanel);
        CreateHorizontalDoorWall("Maintenance Elevator", 0f, -77.9f, 7f, wallY, wallHeight, 3.4f, accentColor);
        CreateWorldLabel("ARRIVAL + EXTRACTION", new Vector3(0f, 2.9f, -64f), Color.white, 0.1f);

        CreateBlock("Archive Security Gate West", new Vector3(-10.25f, wallY, 31f), new Vector3(10.5f, wallHeight, 0.3f), interiorWall);
        CreateBlock("Archive Security Gate East", new Vector3(10.25f, wallY, 31f), new Vector3(10.5f, wallHeight, 0.3f), interiorWall);
        CreateBlock("Archive Security Gate Header", new Vector3(0f, 3.55f, 31f), new Vector3(10f, 0.8f, 0.3f), interiorWall);

        CreateBlock("Archive Objective Dais", new Vector3(0f, 0.35f, 47f), new Vector3(7f, 0.7f, 4f), floorAccent);
        CreateMissionInteriorProp(
            "OfficeDesk_A",
            "Archive Operations Desk West",
            new Vector3(-10f, 0f, 42f),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 1.08f,
            new Vector3(3.5f, 2.25f, 1.45f),
            new Vector3(0f, 1.08f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "OfficeDesk_A",
            "Archive Operations Desk East",
            new Vector3(10f, 0f, 42f),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 1.08f,
            new Vector3(3.5f, 2.25f, 1.45f),
            new Vector3(0f, 1.08f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "LabConsole_A",
            "Security Monitoring Console",
            new Vector3(-17f, 0f, -40f),
            Quaternion.Euler(0f, 90f, 0f),
            Vector3.one,
            new Vector3(1.85f, 1.8f, 1.35f),
            new Vector3(0f, 0.9f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "LockerBank_A",
            "Locker Bay Storage Wall",
            new Vector3(20f, 0f, -40f),
            Quaternion.Euler(0f, -90f, 0f),
            Vector3.one,
            new Vector3(3.85f, 2.65f, 0.82f),
            new Vector3(0f, 1.32f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "FacilityBench_A",
            "Locker Bay Bench",
            new Vector3(15.5f, 0f, -40f),
            Quaternion.Euler(0f, -90f, 0f),
            Vector3.one,
            new Vector3(3.25f, 1.85f, 0.9f),
            new Vector3(0f, 0.9f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "OfficeDesk_A",
            "Records Sorting Desk",
            new Vector3(-17f, 0f, -10f),
            Quaternion.Euler(0f, 90f, 0f),
            Vector3.one,
            new Vector3(3.25f, 2.2f, 1.35f),
            new Vector3(0f, 1.05f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "MedicalStation_A",
            "Medical Examination Station",
            new Vector3(17f, 0f, -10f),
            Quaternion.Euler(0f, -90f, 0f),
            Vector3.one,
            new Vector3(2.85f, 2.55f, 1.5f),
            new Vector3(0f, 1.25f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "BriefingTable_A",
            "Operations Planning Table",
            new Vector3(-17f, 0f, 20f),
            Quaternion.Euler(0f, 90f, 0f),
            Vector3.one * 1.12f,
            new Vector3(4.1f, 1.55f, 2.45f),
            new Vector3(0f, 0.78f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "SupplyCrate_A",
            "Storage Supply Crate A",
            new Vector3(14f, 0f, 20f),
            Quaternion.Euler(0f, 12f, 0f),
            Vector3.one * 1.15f,
            new Vector3(2.3f, 1.45f, 2f),
            new Vector3(0f, 0.72f, 0f),
            accentColor);
        CreateMissionInteriorProp(
            "SupplyCrate_A",
            "Storage Supply Crate B",
            new Vector3(19f, 0f, 23f),
            Quaternion.Euler(0f, -18f, 0f),
            Vector3.one,
            new Vector3(2f, 1.3f, 1.75f),
            new Vector3(0f, 0.65f, 0f),
            accentColor);

        CreateDecorationBlock(
            "Transit Corridor West Accent",
            new Vector3(-centralHalfWidth + 0.17f, 1.55f, -15f),
            new Vector3(0.08f, 0.32f, 91f),
            accentColor);
        CreateDecorationBlock(
            "Transit Corridor East Accent",
            new Vector3(centralHalfWidth - 0.17f, 1.55f, -15f),
            new Vector3(0.08f, 0.32f, 91f),
            accentColor);
        CreateDecorationBlock(
            "West Perimeter Accent",
            new Vector3(-missionHalfWidth + 0.26f, 1.55f, -14f),
            new Vector3(0.08f, 0.32f, 132f),
            accentColor);
        CreateDecorationBlock(
            "East Perimeter Accent",
            new Vector3(missionHalfWidth - 0.26f, 1.55f, -14f),
            new Vector3(0.08f, 0.32f, 132f),
            accentColor);

        CreateFacilityRoomLabel("SECURITY", new Vector3(-5.15f, 2.75f, -45f));
        CreateFacilityRoomLabel("LOCKER BAY", new Vector3(5.15f, 2.75f, -45f));
        CreateFacilityRoomLabel("RECORDS", new Vector3(-5.15f, 2.75f, -15f));
        CreateFacilityRoomLabel("MEDICAL", new Vector3(5.15f, 2.75f, -15f));
        CreateFacilityRoomLabel("OPERATIONS", new Vector3(-5.15f, 2.75f, 15f));
        CreateFacilityRoomLabel("STORAGE", new Vector3(5.15f, 2.75f, 15f));
        CreateFacilityRoomLabel("CENTRAL ARCHIVE", new Vector3(0f, 2.9f, 32f));

        float[] corridorLights = { -70f, -52f, -34f, -16f, 2f, 20f, 38f, 50f };
        foreach (float lightZ in corridorLights)
        {
            CreateFacilityCeilingLight($"Transit Light {lightZ}", new Vector3(0f, 3.8f, lightZ), accentColor, 13f);
        }

        float[] roomLightZ = { -45f, -15f, 15f, 43f };
        foreach (float lightZ in roomLightZ)
        {
            CreateFacilityCeilingLight($"West Room Light {lightZ}", new Vector3(-17f, 3.8f, lightZ), accentColor, 12f);
            CreateFacilityCeilingLight($"East Room Light {lightZ}", new Vector3(17f, 3.8f, lightZ), accentColor, 12f);
        }

        CreateUndergroundArchitectureDressing(accentColor);
    }

    private void CreateUndergroundArchitectureDressing(Color accentColor)
    {
        float[] corridorSections = { -67f, -53f, -39f, -25f, -11f, 3f, 17f, 31f, 45f };
        for (int section = 0; section < corridorSections.Length; section++)
        {
            float z = corridorSections[section];
            CreateEnvironmentModel(
                "FloorPanel_A",
                $"Transit Modeled Floor {section + 1}",
                new Vector3(0f, 0.06f, z),
                Quaternion.identity,
                new Vector3(1.2f, 0.7f, 1.65f),
                accentColor);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"Transit West Modeled Wall {section + 1}",
                new Vector3(-4.78f, 0.08f, z),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1.7f, 1.28f, 1f),
                accentColor);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"Transit East Modeled Wall {section + 1}",
                new Vector3(4.78f, 0.08f, z),
                Quaternion.Euler(0f, -90f, 0f),
                new Vector3(1.7f, 1.28f, 1f),
                accentColor);
        }

        float[] wingDoorZ = { -45f, -15f, 15f };
        foreach (float z in wingDoorZ)
        {
            CreateEnvironmentModel(
                "DoorFrame_A",
                $"West Wing Modeled Door {z}",
                new Vector3(-5.02f, 0.02f, z),
                Quaternion.Euler(0f, 90f, 0f),
                Vector3.one,
                accentColor);
            CreateEnvironmentModel(
                "DoorFrame_A",
                $"East Wing Modeled Door {z}",
                new Vector3(5.02f, 0.02f, z),
                Quaternion.Euler(0f, -90f, 0f),
                Vector3.one,
                accentColor);
        }

        CreateEnvironmentModel(
            "DoorFrame_A",
            "Central Archive Modeled Security Door",
            new Vector3(0f, 0.02f, 31.18f),
            Quaternion.identity,
            new Vector3(1.25f, 1.08f, 1f),
            accentColor);
        CreateEnvironmentModel(
            "PipeCluster_A",
            "Records Wing Utility Pipes",
            new Vector3(-29.72f, 0.2f, -9f),
            Quaternion.Euler(0f, 90f, 0f),
            Vector3.one,
            accentColor);
        CreateEnvironmentModel(
            "VentUnit_A",
            "Medical Wing Ventilation",
            new Vector3(17f, 2.4f, -28.82f),
            Quaternion.identity,
            Vector3.one,
            accentColor);
    }

    private void CreateWoodlandAnnexInfiltration(Color wallColor, Color accentColor)
    {
        CreateApproachRoute("Woodland Service Trail", -10f, true, new Color(0.2f, 0.24f, 0.17f), wallColor);
        CreateCompoundBoundary("Annex", -10f, 50f, 30f, wallColor, accentColor);

        CreateMissionBuilding(
            "Central Archive",
            new Vector3(0f, 0f, 31f),
            new Vector2(18f, 18f),
            2,
            wallColor,
            accentColor,
            true);
        CreateMissionBuilding(
            "West Barracks",
            new Vector3(-19f, 0f, 15f),
            new Vector2(14f, 16f),
            2,
            wallColor,
            new Color(0.34f, 0.48f, 0.58f),
            false);
        CreateMissionBuilding(
            "East Security",
            new Vector3(19f, 0f, 15f),
            new Vector2(14f, 16f),
            2,
            wallColor,
            new Color(0.55f, 0.24f, 0.24f),
            true);
        CreateMissionBuilding(
            "West Utility",
            new Vector3(-21f, 0f, 39f),
            new Vector2(11f, 10f),
            1,
            wallColor,
            accentColor,
            false);
        CreateMissionBuilding(
            "East Logistics",
            new Vector3(21f, 0f, 39f),
            new Vector2(11f, 10f),
            1,
            wallColor,
            accentColor,
            false);

        CreateBlock("Annex Courtyard Cover West", new Vector3(-8f, 0.65f, 13f), new Vector3(4f, 1.3f, 1.2f), accentColor);
        CreateBlock("Annex Courtyard Cover East", new Vector3(8f, 0.65f, 20f), new Vector3(4f, 1.3f, 1.2f), accentColor);
        CreateWorldLabel("ANNEX COMPOUND", new Vector3(0f, 3f, -9.5f), Color.white, 0.12f);
    }

    private void CreateDockyardInfiltration(Color wallColor, Color accentColor)
    {
        CreateApproachRoute(
            "Harbor Customs Access Road",
            -22f,
            false,
            new Color(0.12f, 0.16f, 0.18f),
            wallColor);
        CreateCompoundBoundary("Harbor Customs", -22f, 90f, 40f, wallColor, accentColor);

        CreateMissionBuilding(
            "West Customs Warehouse",
            new Vector3(-22f, 0f, 17f),
            new Vector2(15f, 22f),
            2,
            wallColor,
            accentColor,
            false);
        CreateMissionBuilding(
            "East Freight Warehouse",
            new Vector3(22f, 0f, 17f),
            new Vector2(15f, 22f),
            2,
            wallColor,
            new Color(0.72f, 0.35f, 0.12f),
            false);
        CreateMissionBuilding(
            "West Harbor Security",
            new Vector3(-23f, 0f, 51f),
            new Vector2(14f, 16f),
            2,
            wallColor,
            new Color(0.52f, 0.18f, 0.2f),
            true);
        CreateMissionBuilding(
            "East Manifest Office",
            new Vector3(23f, 0f, 51f),
            new Vector2(14f, 16f),
            2,
            wallColor,
            accentColor,
            false);
        CreateMissionBuilding(
            "Harbor Control Tower",
            new Vector3(0f, 0f, 66f),
            new Vector2(22f, 22f),
            3,
            Color.Lerp(wallColor, Color.white, 0.07f),
            accentColor,
            true);

        CreateBlock(
            "Container Inspection Lane West",
            new Vector3(-8f, 0.5f, 32f),
            new Vector3(4.8f, 1f, 15f),
            Color.Lerp(wallColor, Color.black, 0.12f));
        CreateBlock(
            "Container Inspection Lane East",
            new Vector3(8f, 0.5f, 43f),
            new Vector3(4.8f, 1f, 15f),
            Color.Lerp(wallColor, Color.black, 0.12f));
        CreateBlock(
            "Harbor Guidance Strip",
            new Vector3(0f, 0.025f, 32f),
            new Vector3(0.8f, 0.05f, 104f),
            accentColor);
        CreateWorldLabel("HARBOR CUSTOMS COMPLEX", new Vector3(0f, 3f, -21.5f), Color.white, 0.12f);
    }

    private void CreateEmbassyInfiltration(Color wallColor, Color accentColor)
    {
        CreateApproachRoute(
            "Embassy Processional Approach",
            2f,
            false,
            new Color(0.16f, 0.15f, 0.2f),
            wallColor);
        CreateCompoundBoundary("Embassy", 2f, 44f, 20f, wallColor, accentColor);

        CreateMissionBuilding(
            "Embassy Chancery",
            new Vector3(0f, 0f, 30f),
            new Vector2(30f, 20f),
            2,
            Color.Lerp(wallColor, Color.white, 0.06f),
            accentColor,
            true);
        CreateMissionBuilding(
            "West Consular Pavilion",
            new Vector3(-13f, 0f, 10f),
            new Vector2(10f, 10f),
            1,
            wallColor,
            new Color(0.44f, 0.18f, 0.28f),
            false);
        CreateMissionBuilding(
            "East Security Pavilion",
            new Vector3(13f, 0f, 10f),
            new Vector2(10f, 10f),
            1,
            wallColor,
            new Color(0.2f, 0.34f, 0.52f),
            false);

        CreateBlock(
            "Embassy Courtyard Walkway",
            new Vector3(0f, 0.025f, 14f),
            new Vector3(12f, 0.05f, 12f),
            Color.Lerp(wallColor, Color.white, 0.14f));
        CreateBlock(
            "Embassy Courtyard Cover West",
            new Vector3(-7f, 0.65f, 17f),
            new Vector3(3.6f, 1.3f, 1.2f),
            accentColor);
        CreateBlock(
            "Embassy Courtyard Cover East",
            new Vector3(7f, 0.65f, 12f),
            new Vector3(3.6f, 1.3f, 1.2f),
            accentColor);
        CreateWorldLabel("DIPLOMATIC SECURITY ZONE", new Vector3(0f, 3f, 2.4f), Color.white, 0.11f);
    }

    private void CreateDataCenterInfiltration(Color wallColor, Color accentColor)
    {
        const float wallHeight = 4f;
        const float wallY = 2f;
        Color darkPanel = new Color(0.055f, 0.095f, 0.12f);

        CreateBlock(
            "Data Center Ceiling",
            new Vector3(0f, 4.25f, -14f),
            new Vector3(59.5f, 0.42f, 135.5f),
            new Color(0.12f, 0.17f, 0.19f));
        CreateApproachRoute(
            "Data Center Service Trench",
            -54f,
            false,
            darkPanel,
            wallColor);

        float[] hallCenters = { -39f, -8f, 23f };
        foreach (float hallZ in hallCenters)
        {
            CreateHorizontalDoorWall(
                $"Data Hall {hallZ} South Partition",
                0f,
                hallZ - 12f,
                56f,
                wallY,
                wallHeight,
                5f,
                wallColor);
            CreateHorizontalDoorWall(
                $"Data Hall {hallZ} North Partition",
                0f,
                hallZ + 12f,
                56f,
                wallY,
                wallHeight,
                5f,
                wallColor);

            CreateBlock(
                $"Data Hall {hallZ} West Core",
                new Vector3(-10f, 1.45f, hallZ),
                new Vector3(8f, 2.9f, 10f),
                darkPanel);
            CreateBlock(
                $"Data Hall {hallZ} East Core",
                new Vector3(10f, 1.45f, hallZ),
                new Vector3(8f, 2.9f, 10f),
                darkPanel);
            CreateFacilityCeilingLight(
                $"Data Hall Light {hallZ}",
                new Vector3(0f, 3.85f, hallZ),
                accentColor,
                14f);
        }

        CreateSegmentedVerticalWall(
            "Data Center West Routing Wall",
            -18f,
            -53f,
            38f,
            hallCenters,
            5f,
            wallHeight,
            2.9f,
            wallColor);
        CreateSegmentedVerticalWall(
            "Data Center East Routing Wall",
            18f,
            -53f,
            38f,
            hallCenters,
            5f,
            wallHeight,
            2.9f,
            wallColor);

        CreateHorizontalDoorWall(
            "Ghost Network Vault",
            0f,
            40f,
            30f,
            wallY,
            wallHeight,
            5f,
            accentColor);
        CreateBlock(
            "Ghost Network Objective Platform",
            new Vector3(0f, 0.35f, 47f),
            new Vector3(9f, 0.7f, 6f),
            darkPanel);
        CreateWorldLabel("GHOST NETWORK VAULT", new Vector3(0f, 3f, 40.2f), Color.white, 0.11f);
    }

    private void CreateArcticRelayInfiltration(Color wallColor, Color accentColor)
    {
        CreateApproachRoute(
            "Arctic Relay Supply Trail",
            -12f,
            true,
            new Color(0.43f, 0.52f, 0.56f),
            wallColor);
        CreateCompoundBoundary("Polar Relay", -12f, 50f, 30f, wallColor, accentColor);

        CreateMissionBuilding(
            "Polar Relay Control",
            new Vector3(0f, 0f, 31f),
            new Vector2(18f, 18f),
            2,
            Color.Lerp(wallColor, Color.white, 0.1f),
            accentColor,
            true);
        CreateMissionBuilding(
            "West Garrison Bunker",
            new Vector3(-20f, 0f, 15f),
            new Vector2(14f, 15f),
            1,
            wallColor,
            new Color(0.3f, 0.52f, 0.62f),
            false);
        CreateMissionBuilding(
            "East Power Bunker",
            new Vector3(20f, 0f, 15f),
            new Vector2(14f, 15f),
            1,
            wallColor,
            new Color(0.32f, 0.4f, 0.46f),
            false);
        CreateMissionBuilding(
            "North Weather Station",
            new Vector3(-20f, 0f, 40f),
            new Vector2(12f, 10f),
            1,
            wallColor,
            accentColor,
            false);
        CreateMissionBuilding(
            "North Generator Station",
            new Vector3(20f, 0f, 40f),
            new Vector2(12f, 10f),
            1,
            wallColor,
            accentColor,
            false);

        CreateBlock(
            "Relay Ice Trench West",
            new Vector3(-8f, 0.5f, 17f),
            new Vector3(4f, 1f, 13f),
            Color.Lerp(wallColor, Color.black, 0.08f));
        CreateBlock(
            "Relay Ice Trench East",
            new Vector3(8f, 0.5f, 23f),
            new Vector3(4f, 1f, 13f),
            Color.Lerp(wallColor, Color.black, 0.08f));
        CreateWorldLabel("POLAR RELAY GARRISON", new Vector3(0f, 3f, -11.5f), Color.white, 0.11f);
    }

    private void CreateSegmentedVerticalWall(
        string wallName,
        float x,
        float minZ,
        float maxZ,
        float[] doorCenters,
        float doorWidth,
        float wallHeight,
        float doorHeight,
        Color color)
    {
        float cursor = minZ;
        float wallY = wallHeight * 0.5f;
        float headerHeight = Mathf.Max(0f, wallHeight - doorHeight);

        for (int index = 0; index < doorCenters.Length; index++)
        {
            float doorMin = Mathf.Clamp(doorCenters[index] - doorWidth * 0.5f, minZ, maxZ);
            float doorMax = Mathf.Clamp(doorCenters[index] + doorWidth * 0.5f, minZ, maxZ);
            float segmentDepth = doorMin - cursor;
            if (segmentDepth > 0.05f)
            {
                CreateBlock(
                    $"{wallName} Segment {index + 1}",
                    new Vector3(x, wallY, cursor + segmentDepth * 0.5f),
                    new Vector3(0.32f, wallHeight, segmentDepth),
                    color);
            }

            if (headerHeight > 0.05f)
            {
                CreateBlock(
                    $"{wallName} Door {index + 1} Header",
                    new Vector3(x, doorHeight + headerHeight * 0.5f, doorCenters[index]),
                    new Vector3(0.32f, headerHeight, doorWidth),
                    color);
            }

            cursor = Mathf.Max(cursor, doorMax);
        }

        float finalDepth = maxZ - cursor;
        if (finalDepth > 0.05f)
        {
            CreateBlock(
                $"{wallName} Final Segment",
                new Vector3(x, wallY, cursor + finalDepth * 0.5f),
                new Vector3(0.32f, wallHeight, finalDepth),
                color);
        }
    }

    private void CreateFacilityRoomLabel(string text, Vector3 position)
    {
        CreateWorldLabel(text, position, Color.white, 0.075f);
    }

    private void CreateFacilityCeilingLight(
        string lightName,
        Vector3 position,
        Color accentColor,
        float range,
        Transform parent = null,
        float intensity = 1.5f)
    {
        GameObject fixture = CreateEnvironmentModel(
            "CeilingLight_A",
            $"{lightName} Fixture",
            position + Vector3.up * 0.16f,
            Quaternion.identity,
            Vector3.one,
            accentColor,
            parent);
        if (fixture == null)
        {
            CreateDecorationBlock(
                $"{lightName} Fixture",
                position + Vector3.up * 0.12f,
                new Vector3(2.4f, 0.14f, 0.42f),
                Color.Lerp(accentColor, Color.white, 0.6f),
                parent);
        }

        GameObject lightObject = new GameObject(lightName);
        lightObject.transform.position = position;
        lightObject.transform.SetParent(parent != null ? parent : runtimeRoot, true);
        Light facilityLight = lightObject.AddComponent<Light>();
        facilityLight.type = LightType.Point;
        facilityLight.color = Color.Lerp(accentColor, Color.white, 0.72f);
        facilityLight.range = range;
        facilityLight.intensity = intensity;
        facilityLight.shadows = LightShadows.None;
    }

    private void CreateWeaponsLaboratoryInfiltration(Color wallColor, Color accentColor)
    {
        CreateApproachRoute("Black Glass Maintenance Route", -18f, false, new Color(0.13f, 0.19f, 0.19f), wallColor);
        CreateCompoundBoundary("Black Glass", -18f, 90f, 38f, wallColor, accentColor);

        CreateMissionBuilding(
            "West Communications",
            new Vector3(-25f, 0f, 20f),
            new Vector2(16f, 18f),
            2,
            wallColor,
            new Color(0.18f, 0.58f, 0.7f),
            true);
        CreateMissionBuilding(
            "East Fabrication",
            new Vector3(25f, 0f, 20f),
            new Vector2(16f, 18f),
            2,
            wallColor,
            new Color(0.72f, 0.42f, 0.14f),
            false);
        CreateMissionBuilding(
            "West Security",
            new Vector3(-25f, 0f, 52f),
            new Vector2(16f, 16f),
            2,
            wallColor,
            new Color(0.62f, 0.16f, 0.2f),
            true);
        CreateMissionBuilding(
            "East Storage",
            new Vector3(25f, 0f, 52f),
            new Vector2(16f, 16f),
            2,
            wallColor,
            accentColor,
            false);
        CreateMissionBuilding(
            "Central Arsenal",
            new Vector3(0f, 0f, 65f),
            new Vector2(24f, 24f),
            3,
            Color.Lerp(wallColor, Color.white, 0.08f),
            accentColor,
            true);

        CreateBlock("Laboratory Courtyard Cover A", new Vector3(-10f, 0.8f, 24f), new Vector3(5f, 1.6f, 1.3f), wallColor);
        CreateBlock("Laboratory Courtyard Cover B", new Vector3(10f, 0.8f, 35f), new Vector3(5f, 1.6f, 1.3f), wallColor);
        CreateBlock("Laboratory Courtyard Cover C", new Vector3(0f, 0.8f, 48f), new Vector3(5f, 1.6f, 1.3f), accentColor);
        CreateWeaponsLaboratoryDressing();
        CreateBlock(
            "Decontamination Exit Route",
            new Vector3(0f, 0.025f, -58f),
            new Vector3(1.2f, 0.05f, 82f),
            new Color(0.12f, 0.72f, 0.68f));
        CreateWorldLabel(
            "EXIT + CAPTIVE DROP-OFF",
            new Vector3(0f, 1.15f, missionEntryZ + 14f),
            new Color(0.35f, 1f, 0.82f),
            0.09f);
        CreateWorldLabel("BLACK GLASS COMPLEX", new Vector3(0f, 3f, -17.5f), Color.white, 0.12f);
    }

    private void CreateWeaponsLaboratoryDressing()
    {
        Vector3 standardScale = Vector3.one;

        CreateFacilityProp(
            "Models/Facility/SecurityCheckpoint",
            "Black Glass Gate Checkpoint West",
            new Vector3(-10f, 0f, -12f),
            new Vector3(0f, 18f, 0f),
            standardScale,
            new Vector3(2.8f, 2.3f, 1.4f),
            new Vector3(0f, 1.15f, 0f));
        CreateFacilityProp(
            "Models/Facility/SecurityCheckpoint",
            "Black Glass Gate Checkpoint East",
            new Vector3(10f, 0f, -12f),
            new Vector3(0f, -18f, 0f),
            standardScale,
            new Vector3(2.8f, 2.3f, 1.4f),
            new Vector3(0f, 1.15f, 0f));

        for (int rack = 0; rack < 3; rack++)
        {
            CreateFacilityProp(
                "Models/Facility/ServerRack",
                $"Communications Server Rack {rack + 1}",
                new Vector3(-28f + rack * 3f, 0f, 26f),
                new Vector3(0f, 180f, 0f),
                standardScale,
                new Vector3(1f, 2.3f, 0.85f),
                new Vector3(0f, 1.15f, 0f));
        }

        CreateFacilityProp(
            "Models/Facility/SecurityCheckpoint",
            "Communications Operations Console",
            new Vector3(-25f, 0f, 15f),
            Vector3.zero,
            standardScale,
            new Vector3(2.8f, 2.3f, 1.4f),
            new Vector3(0f, 1.15f, 0f));
        CreateFacilityProp(
            "Models/Facility/ServerRack",
            "Upper Communications Relay",
            new Vector3(-25f, 3.4f, 25f),
            new Vector3(0f, 180f, 0f),
            standardScale,
            new Vector3(1f, 2.3f, 0.85f),
            new Vector3(0f, 1.15f, 0f));

        CreateFacilityProp(
            "Models/Facility/FabricationBench",
            "Fabrication Bench West",
            new Vector3(22f, 0f, 16f),
            Vector3.zero,
            standardScale,
            new Vector3(2.6f, 2f, 1.2f),
            new Vector3(0f, 1f, 0f));
        CreateFacilityProp(
            "Models/Facility/FabricationBench",
            "Fabrication Bench East",
            new Vector3(28f, 0f, 16f),
            Vector3.zero,
            standardScale,
            new Vector3(2.6f, 2f, 1.2f),
            new Vector3(0f, 1f, 0f));
        CreateFacilityProp(
            "Models/Facility/Generator",
            "Fabrication Generator",
            new Vector3(25f, 0f, 25f),
            new Vector3(0f, 180f, 0f),
            standardScale,
            new Vector3(2.7f, 1.9f, 1.5f),
            new Vector3(0f, 0.95f, 0f));
        CreateFacilityProp(
            "Models/Facility/LaboratoryTank",
            "Fabrication Compound Tank",
            new Vector3(28.5f, 0f, 24f),
            Vector3.zero,
            standardScale,
            new Vector3(1.5f, 3f, 1.5f),
            new Vector3(0f, 1.5f, 0f));
        CreateFacilityProp(
            "Models/Facility/FabricationBench",
            "Upper Fabrication Bench",
            new Vector3(25f, 3.4f, 16f),
            Vector3.zero,
            standardScale,
            new Vector3(2.6f, 2f, 1.2f),
            new Vector3(0f, 1f, 0f));

        CreateFacilityProp(
            "Models/Facility/SecurityCheckpoint",
            "Security Command Desk",
            new Vector3(-25f, 0f, 48f),
            Vector3.zero,
            standardScale,
            new Vector3(2.8f, 2.3f, 1.4f),
            new Vector3(0f, 1.15f, 0f));
        CreateFacilityProp(
            "Models/Facility/ServerRack",
            "Security Evidence Server",
            new Vector3(-28f, 3.4f, 56f),
            new Vector3(0f, 180f, 0f),
            standardScale,
            new Vector3(1f, 2.3f, 0.85f),
            new Vector3(0f, 1.15f, 0f));

        Vector3[] storagePositions =
        {
            new Vector3(22f, 0f, 48f),
            new Vector3(25f, 0f, 48f),
            new Vector3(28f, 0f, 48f),
            new Vector3(22f, 0f, 55f),
            new Vector3(28f, 0f, 55f),
            new Vector3(24f, 3.4f, 55f),
            new Vector3(27f, 3.4f, 55f)
        };
        for (int crate = 0; crate < storagePositions.Length; crate++)
        {
            CreateFacilityProp(
                "Models/Facility/CargoCrate",
                $"Black Glass Cargo Crate {crate + 1}",
                storagePositions[crate],
                new Vector3(0f, crate % 2 == 0 ? 0f : 90f, 0f),
                standardScale,
                new Vector3(1.35f, 1.3f, 1.2f),
                new Vector3(0f, 0.65f, 0f));
        }

        CreateFacilityProp(
            "Models/Facility/ArsenalDisplay",
            "Central Arsenal Ground Display",
            new Vector3(0f, 0f, 59f),
            Vector3.zero,
            standardScale,
            new Vector3(2.9f, 2.8f, 1f),
            new Vector3(0f, 1.4f, 0f));
        CreateFacilityProp(
            "Models/Facility/LaboratoryTank",
            "Central Arsenal Sedative Tank",
            new Vector3(-7f, 0f, 71f),
            Vector3.zero,
            standardScale,
            new Vector3(1.5f, 3f, 1.5f),
            new Vector3(0f, 1.5f, 0f));
        CreateFacilityProp(
            "Models/Facility/ArsenalDisplay",
            "Central Arsenal Upper Display",
            new Vector3(0f, 3.4f, 59f),
            Vector3.zero,
            standardScale,
            new Vector3(2.9f, 2.8f, 1f),
            new Vector3(0f, 1.4f, 0f));
        CreateFacilityProp(
            "Models/Facility/Generator",
            "Central Arsenal Power Unit",
            new Vector3(7f, 3.4f, 71f),
            new Vector3(0f, 180f, 0f),
            standardScale,
            new Vector3(2.7f, 1.9f, 1.5f),
            new Vector3(0f, 0.95f, 0f));
        CreateFacilityProp(
            "Models/Facility/ArsenalDisplay",
            "Central Arsenal Prototype Display",
            new Vector3(0f, 6.8f, 61f),
            Vector3.zero,
            standardScale,
            new Vector3(2.9f, 2.8f, 1f),
            new Vector3(0f, 1.4f, 0f));

        CreateFacilityProp(
            "Models/Facility/Generator",
            "Courtyard Auxiliary Generator",
            new Vector3(-17f, 0f, 36f),
            new Vector3(0f, 90f, 0f),
            standardScale,
            new Vector3(2.7f, 1.9f, 1.5f),
            new Vector3(0f, 0.95f, 0f));
        CreateFacilityProp(
            "Models/Facility/CargoCrate",
            "Courtyard Cargo Crate West",
            new Vector3(-11.5f, 0f, 25.5f),
            new Vector3(0f, 20f, 0f),
            standardScale,
            new Vector3(1.35f, 1.3f, 1.2f),
            new Vector3(0f, 0.65f, 0f));
        CreateFacilityProp(
            "Models/Facility/CargoCrate",
            "Courtyard Cargo Crate East",
            new Vector3(11.5f, 0f, 36.5f),
            new Vector3(0f, -25f, 0f),
            standardScale,
            new Vector3(1.35f, 1.3f, 1.2f),
            new Vector3(0f, 0.65f, 0f));

        Vector3[] lampPositions =
        {
            new Vector3(7f, 0f, -70f),
            new Vector3(-32f, 0f, 2f),
            new Vector3(32f, 0f, 2f),
            new Vector3(-14f, 0f, 36f),
            new Vector3(14f, 0f, 42f),
            new Vector3(-17f, 0f, 80f),
            new Vector3(17f, 0f, 80f)
        };
        for (int lamp = 0; lamp < lampPositions.Length; lamp++)
        {
            CreateFacilityLamp($"Black Glass Lamp {lamp + 1}", lampPositions[lamp]);
        }
    }

    private void CreateApproachRoute(string routeName, float compoundFrontZ, bool wooded, Color routeColor, Color coverColor)
    {
        float routeStart = missionEntryZ + 4f;
        float routeEnd = compoundFrontZ - 2f;
        float routeLength = routeEnd - routeStart;
        CreateBlock(
            routeName,
            new Vector3(0f, 0.015f, routeStart + routeLength * 0.5f),
            new Vector3(6f, 0.06f, routeLength),
            routeColor);

        for (int index = 0; index < 12; index++)
        {
            float t = (index + 0.5f) / 12f;
            float z = Mathf.Lerp(routeStart + 4f, routeEnd - 3f, t);
            float leftX = -10f - (index % 3) * 3.6f;
            float rightX = 10f + ((index + 1) % 3) * 3.8f;

            if (wooded || index % 2 == 0)
            {
                CreateMissionTree($"{routeName} Tree L{index + 1}", new Vector3(leftX, 0f, z), coverColor);
                CreateMissionTree($"{routeName} Tree R{index + 1}", new Vector3(rightX, 0f, z + 1.8f), coverColor);
            }
            else
            {
                CreateMissionRock($"{routeName} Rock L{index + 1}", new Vector3(leftX, 0.35f, z), new Vector3(3.2f, 1.6f, 2.4f), coverColor);
                CreateMissionRock($"{routeName} Rock R{index + 1}", new Vector3(rightX, 0.45f, z + 1.8f), new Vector3(2.8f, 1.9f, 2.8f), coverColor);
            }
        }

        CreateWorldLabel("INSERTION ROUTE", new Vector3(0f, 1.4f, missionEntryZ + 10f), Color.white, 0.09f);
    }

    private void CreateCompoundBoundary(
        string compoundName,
        float frontZ,
        float backZ,
        float halfWidth,
        Color wallColor,
        Color accentColor)
    {
        float centerZ = (frontZ + backZ) * 0.5f;
        float depth = backZ - frontZ;
        CreateDoorwayWall($"{compoundName} Main Gate", frontZ, 0f, 9f, wallColor, halfWidth);
        CreateBlock($"{compoundName} West Fence", new Vector3(-halfWidth, 1.35f, centerZ), new Vector3(0.4f, 2.7f, depth), wallColor);
        CreateBlock($"{compoundName} East Fence", new Vector3(halfWidth, 1.35f, centerZ), new Vector3(0.4f, 2.7f, depth), wallColor);
        CreateBlock($"{compoundName} North Fence", new Vector3(0f, 1.35f, backZ), new Vector3(halfWidth * 2f, 2.7f, 0.4f), wallColor);
        CreateBlock($"{compoundName} Gatehouse West", new Vector3(-7f, 1.4f, frontZ + 2.5f), new Vector3(4f, 2.8f, 5f), wallColor);
        CreateBlock($"{compoundName} Gatehouse East", new Vector3(7f, 1.4f, frontZ + 2.5f), new Vector3(4f, 2.8f, 5f), wallColor);
        CreateDecorationBlock(
            $"{compoundName} Gate Light",
            new Vector3(0f, 3.25f, frontZ),
            new Vector3(8.5f, 0.18f, 0.25f),
            accentColor);
    }

    private void CreateMissionBuilding(
        string buildingName,
        Vector3 center,
        Vector2 size,
        int floorCount,
        Color wallColor,
        Color accentColor,
        bool addAntenna)
    {
        const float floorHeight = 3.4f;
        const float wallHeight = 3.1f;
        const float wallThickness = 0.32f;
        float width = size.x;
        float depth = size.y;

        CreateDecorationBlock(
            $"{buildingName} Foundation",
            center - Vector3.up * 0.01f,
            new Vector3(width + 1f, 0.08f, depth + 1f),
            Color.Lerp(wallColor, Color.black, 0.18f));

        for (int floor = 0; floor < floorCount; floor++)
        {
            float baseY = floor * floorHeight;
            float wallY = baseY + wallHeight * 0.5f;
            if (floor > 0)
            {
                CreateBlock(
                    $"{buildingName} Floor {floor + 1}",
                    center + Vector3.up * (baseY - 0.15f),
                    new Vector3(width, 0.3f, depth),
                    Color.Lerp(wallColor, Color.black, 0.08f));
            }

            if (floor == 0)
            {
                const float entranceWidth = 3.2f;
                CreateHorizontalDoorWall(
                    $"{buildingName} Floor 1 Entrance",
                    center.x,
                    center.z - depth * 0.5f,
                    width,
                    wallY,
                    wallHeight,
                    entranceWidth,
                    wallColor);
                CreateSlidingSecurityDoor(
                    $"{buildingName} Sliding Entrance",
                    new Vector3(center.x, 1.4f, center.z - depth * 0.5f),
                    Quaternion.identity,
                    entranceWidth,
                    2.75f,
                    Color.Lerp(wallColor, Color.black, 0.2f),
                    accentColor);
            }
            else
            {
                CreateBlock(
                    $"{buildingName} Floor {floor + 1} Front Wall",
                    new Vector3(center.x, wallY, center.z - depth * 0.5f),
                    new Vector3(width, wallHeight, wallThickness),
                    wallColor);
            }

            CreateBlock(
                $"{buildingName} Floor {floor + 1} Rear Wall",
                new Vector3(center.x, wallY, center.z + depth * 0.5f),
                new Vector3(width, wallHeight, wallThickness),
                wallColor);

            bool incomingEastStair = floor > 0 && (floor - 1) % 2 == 0;
            bool incomingWestStair = floor > 0 && !incomingEastStair;
            bool outgoingEastStair = floor < floorCount - 1 && floor % 2 == 0;
            bool outgoingWestStair = floor < floorCount - 1 && !outgoingEastStair;
            float incomingLandingZ = incomingEastStair
                ? center.z + depth * 0.5f - 2f
                : center.z - depth * 0.5f + 2f;
            float outgoingLandingZ = outgoingEastStair
                ? center.z - depth * 0.5f + 2f
                : center.z + depth * 0.5f - 2f;

            if (incomingWestStair || outgoingWestStair)
            {
                CreateVerticalDoorWall(
                    $"{buildingName} Floor {floor + 1} West Stair Door",
                    center.x - width * 0.5f,
                    center.z,
                    depth,
                    wallY,
                    wallHeight,
                    incomingWestStair ? incomingLandingZ : outgoingLandingZ,
                    2.8f,
                    wallColor);
            }
            else
            {
                CreateBlock(
                    $"{buildingName} Floor {floor + 1} West Wall",
                    new Vector3(center.x - width * 0.5f, wallY, center.z),
                    new Vector3(wallThickness, wallHeight, depth),
                    wallColor);
            }

            if (incomingEastStair || outgoingEastStair)
            {
                CreateVerticalDoorWall(
                    $"{buildingName} Floor {floor + 1} East Stair Door",
                    center.x + width * 0.5f,
                    center.z,
                    depth,
                    wallY,
                    wallHeight,
                    incomingEastStair ? incomingLandingZ : outgoingLandingZ,
                    2.8f,
                    wallColor);
            }
            else
            {
                CreateBlock(
                    $"{buildingName} Floor {floor + 1} East Wall",
                    new Vector3(center.x + width * 0.5f, wallY, center.z),
                    new Vector3(wallThickness, wallHeight, depth),
                    wallColor);
            }

            CreateBlock(
                $"{buildingName} Floor {floor + 1} Interior West",
                new Vector3(center.x - width * 0.29f, baseY + 1.15f, center.z + 1f),
                new Vector3(width * 0.32f, 2.3f, 0.22f),
                Color.Lerp(wallColor, Color.black, 0.08f));
            CreateBlock(
                $"{buildingName} Floor {floor + 1} Interior East",
                new Vector3(center.x + width * 0.29f, baseY + 1.15f, center.z + 1f),
                new Vector3(width * 0.32f, 2.3f, 0.22f),
                Color.Lerp(wallColor, Color.black, 0.08f));

            CreateDecorationBlock(
                $"{buildingName} Floor {floor + 1} Front Window",
                new Vector3(center.x, baseY + 1.85f, center.z - depth * 0.5f - 0.18f),
                new Vector3(Mathf.Max(2f, width * 0.42f), 0.65f, 0.08f),
                accentColor);
            CreateDecorationBlock(
                $"{buildingName} Floor {floor + 1} Rear Window",
                new Vector3(center.x, baseY + 1.85f, center.z + depth * 0.5f + 0.18f),
                new Vector3(Mathf.Max(2f, width * 0.42f), 0.65f, 0.08f),
                accentColor);
        }

        float roofY = floorCount * floorHeight - 0.15f;
        CreateBlock(
            $"{buildingName} Roof",
            new Vector3(center.x, roofY, center.z),
            new Vector3(width + 0.6f, 0.3f, depth + 0.6f),
            Color.Lerp(wallColor, Color.black, 0.12f));

        for (int transition = 0; transition < floorCount - 1; transition++)
        {
            bool eastSide = transition % 2 == 0;
            float startZ = eastSide
                ? center.z - depth * 0.5f + 2f
                : center.z + depth * 0.5f - 2f;
            float run = Mathf.Max(6f, depth - 4f);
            float direction = eastSide ? 1f : -1f;
            float stairX = center.x + (eastSide ? 1f : -1f) * (width * 0.5f + 1.25f);
            CreateBlock(
                $"{buildingName} Floor {transition + 1} Stair Departure Landing",
                new Vector3(stairX, transition * floorHeight - 0.12f, startZ),
                new Vector3(2.4f, 0.24f, 2.4f),
                accentColor);
            CreateBuildingStairFlight(
                $"{buildingName} Stair {transition + 1}",
                new Vector3(stairX, transition * floorHeight, startZ),
                direction,
                run,
                floorHeight,
                accentColor);

            float landingZ = startZ + direction * run;
            CreateBlock(
                $"{buildingName} Floor {transition + 2} Stair Landing",
                new Vector3(
                    center.x + (eastSide ? 1f : -1f) * (width * 0.5f + 1.1f),
                    (transition + 1) * floorHeight - 0.12f,
                    landingZ),
                new Vector3(2.4f, 0.24f, 2.4f),
                accentColor);
        }

        if (addAntenna)
        {
            CreateBuildingAntenna(
                $"{buildingName} Rooftop Antenna",
                new Vector3(center.x, roofY + 0.2f, center.z),
                accentColor);
        }

        CreateMissionBuildingArchitecture(
            buildingName,
            center,
            size,
            floorCount,
            accentColor);
        CreateMissionBuildingDressing(buildingName, center, size, floorCount, accentColor);
        CreateWorldLabel(
            buildingName.ToUpperInvariant(),
            new Vector3(center.x, 2.6f, center.z - depth * 0.5f - 0.45f),
            Color.white,
            0.08f);
    }

    private void CreateMissionBuildingArchitecture(
        string buildingName,
        Vector3 center,
        Vector2 size,
        int floorCount,
        Color accentColor)
    {
        const float floorHeight = 3.4f;
        float frontZ = center.z - size.y * 0.5f;
        float rearZ = center.z + size.y * 0.5f;
        float facadeScaleX = Mathf.Max(0.8f, size.x / 8f);
        float facadeScaleY = 3.35f / 4.2f;

        GameObject entrance = CreateEnvironmentModel(
            "RoomBulkhead_A",
            $"{buildingName} Architectural Entrance",
            new Vector3(center.x, 0f, frontZ - 0.22f),
            Quaternion.identity,
            new Vector3(0.9f, facadeScaleY, 1f),
            accentColor);
        DisableChildColliders(entrance);

        for (int floor = 0; floor < floorCount; floor++)
        {
            float baseY = floor * floorHeight;
            GameObject rearFacade = CreateEnvironmentModel(
                "WindowFacade_A",
                $"{buildingName} Floor {floor + 1} Rear Facade",
                new Vector3(center.x, baseY, rearZ + 0.22f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(facadeScaleX, facadeScaleY, 1f),
                accentColor);
            DisableChildColliders(rearFacade);

            if (floor > 0)
            {
                GameObject frontFacade = CreateEnvironmentModel(
                    "WindowFacade_A",
                    $"{buildingName} Floor {floor + 1} Front Facade",
                    new Vector3(center.x, baseY, frontZ - 0.22f),
                    Quaternion.identity,
                    new Vector3(facadeScaleX, facadeScaleY, 1f),
                    accentColor);
                DisableChildColliders(frontFacade);
            }
        }

        float roofY = floorCount * floorHeight + 0.02f;
        GameObject roofServices = CreateEnvironmentModel(
            "RoofServiceCluster_A",
            $"{buildingName} Rooftop Service Cluster",
            new Vector3(center.x, roofY, center.z),
            Quaternion.Euler(0f, 90f, 0f),
            new Vector3(
                Mathf.Clamp(size.y / 8f, 0.7f, 1.25f),
                0.78f,
                Mathf.Clamp(size.x / 12f, 0.7f, 1.2f)),
            accentColor);
        DisableChildColliders(roofServices);

        for (int transition = 0; transition < floorCount - 1; transition++)
        {
            bool eastSide = transition % 2 == 0;
            float startZ = eastSide
                ? center.z - size.y * 0.5f + 2f
                : center.z + size.y * 0.5f - 2f;
            float direction = eastSide ? 1f : -1f;
            float run = Mathf.Max(6f, size.y - 4f);
            float stairX = center.x + (eastSide ? 1f : -1f) * (size.x * 0.5f + 1.25f);
            GameObject stairSkin = CreateEnvironmentModel(
                "StairFlight_A",
                $"{buildingName} Stair {transition + 1} Architectural Skin",
                new Vector3(
                    stairX,
                    transition * floorHeight,
                    startZ + direction * run * 0.5f),
                Quaternion.Euler(0f, direction > 0f ? 0f : 180f, 0f),
                new Vector3(2.4f / 2.8f, 1f, run / 5.6f),
                accentColor);
            DisableChildColliders(stairSkin);
        }
    }

    private void CreateMissionBuildingDressing(
        string buildingName,
        Vector3 center,
        Vector2 size,
        int floorCount,
        Color accentColor)
    {
        const float floorHeight = 3.4f;
        float frontZ = center.z - size.y * 0.5f;
        float rearZ = center.z + size.y * 0.5f;

        CreateEnvironmentModel(
            "DoorFrame_A",
            $"{buildingName} Modeled Entrance",
            new Vector3(center.x, 0.02f, frontZ - 0.18f),
            Quaternion.identity,
            new Vector3(0.82f, 0.98f, 1f),
            accentColor);

        for (int floor = 0; floor < floorCount; floor++)
        {
            float baseY = floor * floorHeight;
            float facadeOffset = Mathf.Min(4f, size.x * 0.27f);
            float panelScale = Mathf.Clamp(size.x * 0.16f, 0.72f, 1.2f);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"{buildingName} Floor {floor + 1} West Facade Panel",
                new Vector3(center.x - facadeOffset, baseY + 0.08f, frontZ - 0.18f),
                Quaternion.identity,
                new Vector3(panelScale, 1.05f, 1f),
                accentColor);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"{buildingName} Floor {floor + 1} East Facade Panel",
                new Vector3(center.x + facadeOffset, baseY + 0.08f, frontZ - 0.18f),
                Quaternion.identity,
                new Vector3(panelScale, 1.05f, 1f),
                accentColor);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"{buildingName} Floor {floor + 1} Rear Service Panel",
                new Vector3(center.x, baseY + 0.08f, rearZ + 0.18f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(panelScale, 1.05f, 1f),
                accentColor);
            CreateEnvironmentModel(
                "VentUnit_A",
                $"{buildingName} Floor {floor + 1} Vent",
                new Vector3(
                    center.x + size.x * 0.25f,
                    baseY + 1.55f,
                    rearZ + 0.22f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(0.78f, 0.78f, 0.78f),
                accentColor);
            CreateFacilityCeilingLight(
                $"{buildingName} Floor {floor + 1} Interior Light",
                new Vector3(center.x, baseY + 2.92f, center.z),
                accentColor,
                Mathf.Clamp(size.x * 0.65f, 8f, 16f));

            float sideOffset = Mathf.Min(3.2f, size.x * 0.24f);
            float depthOffset = Mathf.Min(2.8f, size.y * 0.2f);
            switch (floor % 3)
            {
                case 0:
                    CreateMissionInteriorProp(
                        "OfficeDesk_A",
                        $"{buildingName} Floor {floor + 1} Operations Desk",
                        new Vector3(center.x - sideOffset, baseY, center.z + depthOffset),
                        Quaternion.Euler(0f, 180f, 0f),
                        Vector3.one,
                        new Vector3(3.25f, 2.2f, 1.35f),
                        new Vector3(0f, 1.05f, 0f),
                        accentColor);
                    break;
                case 1:
                    CreateMissionInteriorProp(
                        "LockerBank_A",
                        $"{buildingName} Floor {floor + 1} Security Lockers",
                        new Vector3(center.x + sideOffset, baseY, rearZ - 0.55f),
                        Quaternion.Euler(0f, 180f, 0f),
                        Vector3.one,
                        new Vector3(3.85f, 2.65f, 0.82f),
                        new Vector3(0f, 1.32f, 0f),
                        accentColor);
                    break;
                default:
                    CreateMissionInteriorProp(
                        "FacilityBench_A",
                        $"{buildingName} Floor {floor + 1} Waiting Bench",
                        new Vector3(center.x - sideOffset, baseY, center.z + depthOffset),
                        Quaternion.identity,
                        Vector3.one,
                        new Vector3(3.25f, 1.85f, 0.9f),
                        new Vector3(0f, 0.9f, 0f),
                        accentColor);
                    break;
            }
        }

        CreateMissionBuildingIdentityProp(buildingName, center, size, accentColor);

        CreateEnvironmentModel(
            "PipeCluster_A",
            $"{buildingName} Exterior Utility Pipes",
            new Vector3(
                center.x + size.x * 0.5f + 0.2f,
                0.2f,
                center.z + size.y * 0.2f),
            Quaternion.Euler(0f, -90f, 0f),
            Vector3.one,
            accentColor);
    }

    private void CreateMissionInteriorProp(
        string assetName,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Vector3 colliderSize,
        Vector3 colliderCenter,
        Color accentColor)
    {
        GameObject prop = CreateEnvironmentModel(
            assetName,
            objectName,
            position,
            rotation,
            scale,
            accentColor);
        if (prop != null)
        {
            EnsureBoxCollider(prop, colliderSize, colliderCenter);
        }
    }

    private void CreateMissionBuildingIdentityProp(
        string buildingName,
        Vector3 center,
        Vector2 size,
        Color accentColor)
    {
        string normalizedName = buildingName.ToUpperInvariant();
        float frontZ = center.z - size.y * 0.5f;
        float rearZ = center.z + size.y * 0.5f;
        float sideOffset = Mathf.Min(3.1f, size.x * 0.22f);

        if (normalizedName.Contains("LAB")
            || normalizedName.Contains("MEDICAL")
            || normalizedName.Contains("RESEARCH")
            || normalizedName.Contains("CRYO"))
        {
            CreateMissionInteriorProp(
                "MedicalStation_A",
                $"{buildingName} Medical Workstation",
                new Vector3(center.x + sideOffset, 0f, rearZ - 1.2f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one,
                new Vector3(2.85f, 2.55f, 1.5f),
                new Vector3(0f, 1.25f, 0f),
                accentColor);
            return;
        }

        if (normalizedName.Contains("ARSENAL")
            || normalizedName.Contains("SECURITY")
            || normalizedName.Contains("BARRACK")
            || normalizedName.Contains("COMMAND")
            || normalizedName.Contains("ARMORY"))
        {
            CreateMissionInteriorProp(
                "WeaponRack_A",
                $"{buildingName} Secured Weapon Rack",
                new Vector3(center.x + sideOffset, 0f, rearZ - 0.46f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one,
                new Vector3(3.55f, 2.75f, 0.58f),
                new Vector3(0f, 1.35f, 0f),
                accentColor);
            return;
        }

        CreateMissionInteriorProp(
            "BriefingTable_A",
            $"{buildingName} Briefing Table",
            new Vector3(center.x + sideOffset, 0f, Mathf.Lerp(frontZ, rearZ, 0.66f)),
            Quaternion.identity,
            Vector3.one * 0.88f,
            new Vector3(3.85f, 1.35f, 2.15f),
            new Vector3(0f, 0.65f, 0f),
            accentColor);
    }

    private void CreateHorizontalDoorWall(
        string wallName,
        float centerX,
        float z,
        float width,
        float y,
        float height,
        float doorWidth,
        Color color)
    {
        float segmentWidth = Mathf.Max(0.1f, (width - doorWidth) * 0.5f);
        float segmentOffset = doorWidth * 0.5f + segmentWidth * 0.5f;
        CreateBlock($"{wallName} West", new Vector3(centerX - segmentOffset, y, z), new Vector3(segmentWidth, height, 0.32f), color);
        CreateBlock($"{wallName} East", new Vector3(centerX + segmentOffset, y, z), new Vector3(segmentWidth, height, 0.32f), color);
        CreateBlock($"{wallName} Header", new Vector3(centerX, y + height * 0.38f, z), new Vector3(doorWidth, height * 0.24f, 0.32f), color);
    }

    private SlidingSecurityDoor CreateSlidingSecurityDoor(
        string doorName,
        Vector3 position,
        Quaternion rotation,
        float doorWidth,
        float doorHeight,
        Color panelColor,
        Color accentColor)
    {
        GameObject root = new GameObject(doorName);
        root.transform.position = position;
        root.transform.rotation = rotation;
        ParentToRuntime(root);

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = Vector3.zero;
        trigger.size = new Vector3(doorWidth + 2.4f, doorHeight, 3.2f);

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        float panelWidth = doorWidth * 0.5f;
        GameObject leftPanel = CreateBlock(
            $"{doorName} Left Panel",
            position,
            new Vector3(panelWidth, doorHeight, 0.16f),
            panelColor,
            root.transform);
        leftPanel.transform.localPosition = new Vector3(-panelWidth * 0.5f, 0f, 0f);
        leftPanel.transform.localRotation = Quaternion.identity;

        GameObject rightPanel = CreateBlock(
            $"{doorName} Right Panel",
            position,
            new Vector3(panelWidth, doorHeight, 0.16f),
            panelColor,
            root.transform);
        rightPanel.transform.localPosition = new Vector3(panelWidth * 0.5f, 0f, 0f);
        rightPanel.transform.localRotation = Quaternion.identity;

        CreateDecorationBlock(
            $"{doorName} Left Status Strip",
            position,
            new Vector3(0.08f, doorHeight * 0.8f, 0.04f),
            accentColor,
            leftPanel.transform).transform.localPosition = new Vector3(panelWidth * 0.32f, 0f, -0.11f);
        CreateDecorationBlock(
            $"{doorName} Right Status Strip",
            position,
            new Vector3(0.08f, doorHeight * 0.8f, 0.04f),
            accentColor,
            rightPanel.transform).transform.localPosition = new Vector3(-panelWidth * 0.32f, 0f, -0.11f);

        SlidingSecurityDoor door = root.AddComponent<SlidingSecurityDoor>();
        door.Initialize(leftPanel.transform, rightPanel.transform, panelWidth);
        return door;
    }

    private void CreateVerticalDoorWall(
        string wallName,
        float x,
        float centerZ,
        float depth,
        float y,
        float height,
        float doorCenterZ,
        float doorWidth,
        Color color,
        Transform parent = null,
        float doorHeight = -1f)
    {
        float minZ = centerZ - depth * 0.5f;
        float maxZ = centerZ + depth * 0.5f;
        float doorMin = Mathf.Clamp(doorCenterZ - doorWidth * 0.5f, minZ, maxZ);
        float doorMax = Mathf.Clamp(doorCenterZ + doorWidth * 0.5f, minZ, maxZ);
        float southDepth = doorMin - minZ;
        float northDepth = maxZ - doorMax;

        if (southDepth > 0.1f)
        {
            CreateBlock(
                $"{wallName} South",
                new Vector3(x, y, minZ + southDepth * 0.5f),
                new Vector3(0.32f, height, southDepth),
                color,
                parent);
        }

        if (northDepth > 0.1f)
        {
            CreateBlock(
                $"{wallName} North",
                new Vector3(x, y, doorMax + northDepth * 0.5f),
                new Vector3(0.32f, height, northDepth),
                color,
                parent);
        }

        if (doorHeight > 0f)
        {
            // Explicit door height: the header fills everything above the opening.
            float bottom = y - height * 0.5f;
            float headerHeight = height - doorHeight;
            if (headerHeight > 0.05f)
            {
                CreateBlock(
                    $"{wallName} Header",
                    new Vector3(x, bottom + doorHeight + headerHeight * 0.5f, doorCenterZ),
                    new Vector3(0.32f, headerHeight, doorWidth),
                    color,
                    parent);
            }

            return;
        }

        CreateBlock(
            $"{wallName} Header",
            new Vector3(x, y + height * 0.38f, doorCenterZ),
            new Vector3(0.32f, height * 0.24f, doorWidth),
            color,
            parent);
    }

    private void CreateBuildingStairFlight(
        string stairName,
        Vector3 start,
        float zDirection,
        float run,
        float rise,
        Color color)
    {
        const int stepCount = 14;
        float stepDepth = run / stepCount;
        float stepRise = rise / stepCount;
        for (int index = 0; index < stepCount; index++)
        {
            float stepHeight = stepRise * (index + 1);
            CreateBlock(
                $"{stairName} Step {index + 1}",
                new Vector3(
                    start.x,
                    start.y + stepHeight * 0.5f,
                    start.z + zDirection * stepDepth * (index + 0.5f)),
                new Vector3(2.4f, stepHeight, stepDepth + 0.04f),
                color);
        }
    }

    private void CreateBuildingAntenna(string antennaName, Vector3 basePosition, Color color)
    {
        GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mast.name = antennaName;
        mast.transform.position = basePosition + Vector3.up * 1.8f;
        mast.transform.localScale = new Vector3(0.22f, 1.8f, 0.22f);
        ParentToRuntime(mast);
        SetColor(mast, Color.Lerp(color, Color.white, 0.35f));

        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beacon.name = $"{antennaName} Beacon";
        beacon.transform.position = basePosition + Vector3.up * 3.65f;
        beacon.transform.localScale = Vector3.one * 0.38f;
        ParentToRuntime(beacon);
        SetColor(beacon, color);
        Collider beaconCollider = beacon.GetComponent<Collider>();
        if (beaconCollider != null) beaconCollider.enabled = false;
    }

    private void CreateMissionTree(string treeName, Vector3 position, Color color)
    {
        float scaleVariation = 0.9f + Mathf.Repeat(Mathf.Abs(position.x * 0.071f + position.z * 0.037f), 0.28f);
        float yaw = Mathf.Repeat(Mathf.Abs(position.x * 19f + position.z * 11f), 360f);
        GameObject modeledTree = CreateEnvironmentModel(
            "PineTree_A",
            treeName,
            position,
            Quaternion.Euler(0f, yaw, 0f),
            Vector3.one * scaleVariation,
            color);
        if (modeledTree != null)
        {
            CapsuleCollider treeCollider = modeledTree.AddComponent<CapsuleCollider>();
            treeCollider.center = new Vector3(0f, 2.75f, 0f);
            treeCollider.height = 5.5f;
            treeCollider.radius = 0.32f;
            return;
        }

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = treeName;
        trunk.transform.position = position + Vector3.up * 1.35f;
        trunk.transform.localScale = new Vector3(0.32f, 1.35f, 0.32f);
        ParentToRuntime(trunk);
        SetColor(trunk, new Color(0.23f, 0.16f, 0.1f));

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = $"{treeName} Crown";
        crown.transform.position = position + Vector3.up * 3.25f;
        crown.transform.localScale = new Vector3(2.5f, 1.8f, 2.5f);
        ParentToRuntime(crown);
        SetColor(crown, Color.Lerp(color, new Color(0.08f, 0.3f, 0.12f), 0.65f));
        Collider crownCollider = crown.GetComponent<Collider>();
        if (crownCollider != null) crownCollider.enabled = false;
    }

    private void CreateMissionRock(string rockName, Vector3 position, Vector3 scale, Color color)
    {
        Vector3 modeledScale = new Vector3(
            scale.x / 3.6f,
            scale.y / 1.45f,
            scale.z / 2.5f);
        GameObject modeledRock = CreateEnvironmentModel(
            "RockFormation_A",
            rockName,
            position,
            Quaternion.Euler(0f, Mathf.Abs(position.z * 13f) % 180f, 0f),
            modeledScale,
            color);
        if (modeledRock != null)
        {
            BoxCollider rockCollider = modeledRock.AddComponent<BoxCollider>();
            rockCollider.center = new Vector3(0f, 0.55f, 0f);
            rockCollider.size = new Vector3(3.6f, 1.45f, 2.5f);
            return;
        }

        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = rockName;
        rock.transform.position = position;
        rock.transform.localScale = scale;
        rock.transform.rotation = Quaternion.Euler(0f, Mathf.Abs(position.z * 13f) % 180f, 8f);
        ParentToRuntime(rock);
        SetColor(rock, Color.Lerp(color, Color.black, 0.28f));
    }

    private GameObject CreateDecorationBlock(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Color color,
        Transform parent = null)
    {
        GameObject decoration = CreateBlock(objectName, position, scale, color, parent);
        Collider collider = decoration.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        return decoration;
    }

    private SpyPlayerController CreatePlayer(Vector3 spawnPosition)
    {
        GameObject playerObject = new GameObject("Player Secret Agent");
        playerObject.name = "Player Secret Agent";
        playerObject.transform.position = spawnPosition;
        ParentToRuntime(playerObject);
        playerObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.45f;

        SpyWeaponController weapon = playerObject.AddComponent<SpyWeaponController>();
        SpyPlayerController player = playerObject.AddComponent<SpyPlayerController>();
        CharacterAnimationDriver playerAnimation = playerObject.AddComponent<CharacterAnimationDriver>();
        GameObject playerVisual = CreatePlayerAgentVisual(playerObject.transform);
        Animator playerAnimator = playerVisual != null ? playerVisual.GetComponentInChildren<Animator>() : null;
        if (playerVisual != null)
        {
            if (playerAnimator == null) playerAnimator = playerVisual.AddComponent<Animator>();
            playerAnimation.walkSpeedReference = player.walkSpeed;
            playerAnimation.runSpeedReference = player.sprintSpeed;
            playerAnimation.Initialize(playerAnimator, PlayerAgentModel, playerObject.transform);
        }
        CompanionSummoner companionSummoner = playerObject.AddComponent<CompanionSummoner>();
        if (GameFlowManager.Instance != null)
        {
            weapon.currentWeapon = GameFlowManager.Instance.GetEquippedWeapon();
        }

        Transform fpPitch = new GameObject("FirstPersonPitchRoot").transform;
        fpPitch.SetParent(playerObject.transform);
        fpPitch.localPosition = new Vector3(0f, 0.72f, 0f);

        Camera fpCamera = new GameObject("FirstPersonCamera").AddComponent<Camera>();
        fpCamera.transform.SetParent(fpPitch);
        fpCamera.transform.localPosition = new Vector3(0f, 0f, 0.25f);
        fpCamera.transform.localRotation = Quaternion.identity;
        fpCamera.nearClipPlane = 0.05f;

        Transform tpPitch = new GameObject("ThirdPersonPitchRoot").transform;
        tpPitch.SetParent(playerObject.transform);
        tpPitch.localPosition = new Vector3(0f, 0.75f, 0f);

        Camera tpCamera = new GameObject("ThirdPersonCamera").AddComponent<Camera>();
        tpCamera.transform.SetParent(tpPitch);
        tpCamera.transform.localPosition = new Vector3(1.2f, 1.3f, -4.5f);
        tpCamera.transform.localRotation = Quaternion.Euler(12f, -15f, 0f);

        Transform weaponMount;
        FirstPersonViewModel firstPersonViewModel = CreateFirstPersonViewModel(
            fpCamera.transform,
            player,
            weapon,
            playerObject.layer,
            out weaponMount);

        Transform muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(weaponMount);
        muzzle.localPosition = new Vector3(0f, 0.08f, 0.52f);
        muzzle.localRotation = Quaternion.identity;
        muzzle.gameObject.layer = playerObject.layer;

        Transform carrySocket = new GameObject("CarrySocket").transform;
        carrySocket.SetParent(playerObject.transform);
        carrySocket.localPosition = new Vector3(0f, 0.65f, 0f);

        Transform leftHandTarget = new GameObject("LeftHandCarryTarget").transform;
        leftHandTarget.SetParent(carrySocket, false);
        leftHandTarget.localPosition = new Vector3(-0.45f, 0.05f, 0.65f);
        leftHandTarget.localRotation = Quaternion.Euler(0f, 90f, 0f);

        Transform rightHandTarget = new GameObject("RightHandCarryTarget").transform;
        rightHandTarget.SetParent(carrySocket, false);
        rightHandTarget.localPosition = new Vector3(0.45f, 0.05f, 0.65f);
        rightHandTarget.localRotation = Quaternion.Euler(0f, -90f, 0f);

        weapon.muzzle = muzzle;
        weapon.weaponVisualRoot = weaponMount;
        weapon.aimCamera = fpCamera;
        Transform rightHand = FindDescendant(playerVisual, "RightHand");
        Transform leftHand = FindDescendant(playerVisual, "LeftHand");
        weapon.thirdPersonWeaponRoot = CreateCharacterWeaponMount(
            rightHand,
            "Right Hand Weapon Mount",
            new Vector3(0f, 0.035f, 0.02f),
            new Vector3(0f, 90f, -90f));
        weapon.thirdPersonShoulderWeaponRoot = CreateCharacterWeaponMount(
            leftHand,
            "Left Hand Weapon Mount",
            new Vector3(0f, 0.035f, 0.02f),
            new Vector3(0f, -90f, 90f));

        player.firstPersonCamera = fpCamera;
        player.thirdPersonCamera = tpCamera;
        player.firstPersonPitchRoot = fpPitch;
        player.thirdPersonPitchRoot = tpPitch;
        player.playerBodyRenderer = playerVisual != null ? playerVisual.GetComponentInChildren<Renderer>() : null;
        player.playerBodyVisual = playerVisual;
        player.firstPersonViewModel = firstPersonViewModel;
        player.characterAnimator = playerAnimator;
        player.carrySocket = carrySocket;
        player.leftHandCarryTarget = leftHandTarget;
        player.rightHandCarryTarget = rightHandTarget;
        player.weaponController = weapon;
        player.animationDriver = playerAnimation;
        player.companionSummoner = companionSummoner;
        companionSummoner.player = playerObject.transform;
        companionSummoner.roster = RecruitRoster.Instance;
        int medicalLevel = SecretLairManager.Instance != null ? SecretLairManager.Instance.GetRoomLevel(RoomType.MedicalBay) : 0;
        int startingArmor = SecretLairManager.Instance != null ? SecretLairManager.Instance.GetStartingArmor() : 0;
        player.SetStartingVitals(100f + medicalLevel * 10f, startingArmor);
        player.SetCameraMode(true);

        PrototypeHUD hud = playerObject.AddComponent<PrototypeHUD>();
        hud.player = player;
        hud.weapon = weapon;

        return player;
    }

    private FirstPersonViewModel CreateFirstPersonViewModel(
        Transform cameraTransform,
        SpyPlayerController player,
        SpyWeaponController weapon,
        int layer,
        out Transform weaponMount)
    {
        GameObject rootObject = new GameObject("First Person View Model");
        rootObject.transform.SetParent(cameraTransform, false);
        rootObject.layer = layer;

        FirstPersonViewModel viewModel = rootObject.AddComponent<FirstPersonViewModel>();
        viewModel.player = player;
        viewModel.weapon = weapon;
        viewModel.restLocalPosition = new Vector3(0f, -0.02f, 0.04f);
        viewModel.restLocalEuler = Vector3.zero;

        GameObject armsPrefab = Resources.Load<GameObject>(PlayerFirstPersonArmsModel);
        if (armsPrefab != null)
        {
            GameObject arms = Instantiate(armsPrefab, rootObject.transform);
            arms.name = "Secret Agent First Person Arms";
            arms.transform.localPosition = new Vector3(0f, -0.22f, 0.02f);
            arms.transform.localRotation = Quaternion.identity;
            arms.transform.localScale = Vector3.one * 0.82f;
            SetLayerRecursively(arms, layer);
            ApplyPlayerAgentMaterials(arms);
            viewModel.armsVisual = arms.transform;
            viewModel.leftArm = FindDescendant(arms, "LeftArmAssembly");
            viewModel.rightArm = FindDescendant(arms, "RightArmAssembly");
        }
        else
        {
            Debug.LogWarning($"Could not load first-person arms at Resources/{PlayerFirstPersonArmsModel}.");
        }

        weaponMount = new GameObject("WeaponPresentationRoot").transform;
        weaponMount.SetParent(rootObject.transform, false);
        weaponMount.localPosition = new Vector3(0.28f, -0.18f, 0.48f);
        weaponMount.localRotation = Quaternion.identity;
        weaponMount.gameObject.layer = layer;
        return viewModel;
    }

    private static Transform CreateCharacterWeaponMount(
        Transform hand,
        string mountName,
        Vector3 localPosition,
        Vector3 localEuler)
    {
        if (hand == null) return null;

        Transform mount = new GameObject(mountName).transform;
        mount.SetParent(hand, false);
        mount.localPosition = localPosition;
        mount.localRotation = Quaternion.Euler(localEuler);
        mount.gameObject.layer = hand.gameObject.layer;
        return mount;
    }

    private static Transform FindDescendant(GameObject root, string objectName)
    {
        if (root == null) return null;
        foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
        {
            if (candidate.name == objectName) return candidate;
        }
        return null;
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null) return;
        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private GameObject CreatePlayerAgentVisual(Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(PlayerAgentModel);
        if (prefab == null)
        {
            Debug.LogWarning($"Could not load player model at Resources/{PlayerAgentModel}. Third-person mode will use the controller-only fallback.");
            return null;
        }

        GameObject visual = Instantiate(prefab, parent);
        visual.name = "Secret Agent Character Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * handToHandGruntScale;
        OrientAndGroundCharacterVisual(visual, parent);
        visual.transform.localPosition += Vector3.down;
        ApplyPlayerAgentMaterials(visual);

        foreach (Collider modelCollider in visual.GetComponentsInChildren<Collider>())
        {
            modelCollider.enabled = false;
            Destroy(modelCollider);
        }
        return visual;
    }

    private void ApplyPlayerAgentMaterials(GameObject visual)
    {
        if (visual == null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) return;

        foreach (Renderer modelRenderer in visual.GetComponentsInChildren<Renderer>())
        {
            Material[] sourceMaterials = modelRenderer.sharedMaterials;
            Material[] runtimeMaterials = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                string materialName = sourceMaterials[i] != null ? sourceMaterials[i].name : "Agent Suit";
                Color tint = GetPlayerAgentMaterialColor(materialName);
                Material runtimeMaterial = new Material(shader) { name = $"{materialName} Runtime" };
                if (runtimeMaterial.HasProperty("_BaseColor")) runtimeMaterial.SetColor("_BaseColor", tint);
                if (runtimeMaterial.HasProperty("_Color")) runtimeMaterial.SetColor("_Color", tint);
                if (runtimeMaterial.HasProperty("_Metallic"))
                {
                    runtimeMaterial.SetFloat("_Metallic", materialName.Contains("Leather") || materialName.Contains("Belt") ? 0.42f : 0.08f);
                }
                if (runtimeMaterial.HasProperty("_Smoothness"))
                {
                    runtimeMaterial.SetFloat("_Smoothness", materialName.Contains("Skin") ? 0.34f : 0.46f);
                }
                if (materialName.Contains("Cyan"))
                {
                    runtimeMaterial.EnableKeyword("_EMISSION");
                    if (runtimeMaterial.HasProperty("_EmissionColor")) runtimeMaterial.SetColor("_EmissionColor", tint * 0.65f);
                }
                runtimeMaterials[i] = runtimeMaterial;
            }
            modelRenderer.sharedMaterials = runtimeMaterials;
        }
    }

    private static Color GetPlayerAgentMaterialColor(string materialName)
    {
        string lowerName = string.IsNullOrEmpty(materialName) ? string.Empty : materialName.ToLowerInvariant();
        if (lowerName.Contains("skin")) return new Color(0.58f, 0.35f, 0.24f);
        if (lowerName.Contains("shirt")) return new Color(0.76f, 0.82f, 0.84f);
        if (lowerName.Contains("cyan")) return new Color(0.05f, 0.72f, 0.82f);
        if (lowerName.Contains("hair")) return new Color(0.018f, 0.014f, 0.012f);
        if (lowerName.Contains("eye")) return new Color(0.02f, 0.055f, 0.065f);
        if (lowerName.Contains("leather") || lowerName.Contains("belt")) return new Color(0.012f, 0.018f, 0.024f);
        if (lowerName.Contains("navy")) return new Color(0.035f, 0.095f, 0.15f);
        return new Color(0.025f, 0.05f, 0.075f);
    }

    private void CreateEnemies(MissionDefinition definition)
    {
        if (definition != null && definition.enemySpawns.Count > 0)
        {
            for (int spawnIndex = 0; spawnIndex < definition.enemySpawns.Count; spawnIndex++)
            {
                EnemySpawnDefinition spawn = definition.enemySpawns[spawnIndex];
                if (spawn == null) continue;
                string modelPath = spawn.usePrimaryGruntModel && useHandToHandGruntModels
                    ? GetHandToHandGruntModel()
                    : spawn.modelResourcePath;
                EnemyOperative enemy = CreateEnemy(
                    spawn.displayName,
                    spawn.position,
                    spawn.rank,
                    spawn.specialization,
                    spawn.fallbackColor,
                    modelPath);
                ConfigureEnemyPatrol(
                    enemy,
                    spawn.position,
                    spawnIndex,
                    definition.environmentType);
            }

            return;
        }

        string gruntModel = useHandToHandGruntModels ? GetHandToHandGruntModel() : null;
        CreateEnemy("Velvet Grunt", new Vector3(-4f, 0f, 5f), EnemyRank.Grunt, OperativeSpecialization.HandToHand, Color.red, gruntModel);
        CreateEnemy("Ghost Lieutenant", new Vector3(1.5f, 0f, 8f), EnemyRank.Lieutenant, OperativeSpecialization.Stealth, new Color(0.55f, 0.1f, 0.9f));
        CreateEnemy("Lady Cipher Commander", new Vector3(5f, 0f, 10f), EnemyRank.Commander, OperativeSpecialization.Hacker, new Color(0.95f, 0.72f, 0.14f));
    }

    private static void ConfigureEnemyPatrol(
        EnemyOperative enemy,
        Vector3 origin,
        int spawnIndex,
        MissionEnvironmentType environment)
    {
        if (enemy == null) return;
        EnemyCombatController controller = enemy.GetComponent<EnemyCombatController>();
        if (controller == null) return;

        bool underground = environment == MissionEnvironmentType.CorporateAnnex ||
            environment == MissionEnvironmentType.DataCenter;
        bool upperFloor = origin.y > 0.5f;
        float direction = spawnIndex % 2 == 0 ? 1f : -1f;
        float lateral;
        float longitudinal;

        if (underground)
        {
            bool centralCorridor = Mathf.Abs(origin.x) < 6f;
            lateral = centralCorridor ? 0.8f : 1.8f;
            longitudinal = upperFloor ? 2.2f : centralCorridor ? 4.5f : 3.2f;
        }
        else
        {
            lateral = upperFloor ? 1.6f : 2.8f;
            longitudinal = upperFloor ? 2.3f : 4f;
        }

        List<Vector3> route = new List<Vector3>
        {
            origin,
            origin + new Vector3(direction * lateral, 0f, longitudinal),
            origin + new Vector3(-direction * lateral, 0f, -longitudinal * 0.72f)
        };
        controller.ConfigurePatrolRoute(route, 0.95f + (spawnIndex % 3) * 0.22f);
    }

    private void CreateMissionReinforcements(MissionDefinition definition)
    {
        int contractNumber = definition != null ? definition.contractNumber : 1;
        if (contractNumber < 4) return;

        int reserveCount = Mathf.Clamp(1 + (contractNumber - 4) / 6, 1, 4);
        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        if (difficulty.difficulty == MissionDifficulty.BlackOps)
        {
            reserveCount = Mathf.Min(5, reserveCount + 1);
        }

        OperativeSpecialization[] responseRoles =
        {
            OperativeSpecialization.Tank,
            OperativeSpecialization.Marksman,
            OperativeSpecialization.Medic,
            OperativeSpecialization.Hacker,
            OperativeSpecialization.Stealth,
            OperativeSpecialization.HandToHand
        };

        List<GameObject> reserves = new List<GameObject>();
        string modelPath = useHandToHandGruntModels ? GetHandToHandGruntModel() : null;
        for (int index = 0; index < reserveCount; index++)
        {
            int roleOffset = contractNumber >= 13
                ? 0
                : contractNumber >= 9
                    ? 1
                    : 4;
            OperativeSpecialization role = responseRoles[
                (index + roleOffset) % responseRoles.Length];
            EnemyRank rank = contractNumber >= 17
                ? EnemyRank.SpecialOperative
                : contractNumber >= 10
                    ? EnemyRank.Sergeant
                    : EnemyRank.Grunt;
            float x = (index - (reserveCount - 1) * 0.5f) * 2.5f;
            float z = missionNorthEdge - 5f - (index % 2) * 2.2f;
            EnemyOperative reserve = CreateEnemy(
                $"Response {index + 1} - {role}",
                new Vector3(x, 0f, z),
                rank,
                role,
                GetSpecializationColor(role),
                modelPath);
            if (reserve == null) continue;
            reserve.gameObject.SetActive(false);
            reserves.Add(reserve.gameObject);
        }

        if (reserves.Count == 0) return;

        GameObject controllerObject = new GameObject("Mission Reinforcement Response");
        ParentToRuntime(controllerObject);
        MissionReinforcementController response = controllerObject.AddComponent<MissionReinforcementController>();
        response.Configure(reserves, 3.5f, 1.2f);
    }

    private string GetHandToHandGruntModel()
    {
        int variant = handToHandGruntVariant;
        if (variant < 0 || variant >= HandToHandGruntModels.Length)
        {
            variant = enableRiggedGruntRagdoll
                ? AnimatedGruntVariantIndices[Random.Range(0, AnimatedGruntVariantIndices.Length)]
                : Random.Range(0, HandToHandGruntModels.Length);
        }
        else if (enableRiggedGruntRagdoll &&
            System.Array.IndexOf(AnimatedGruntVariantIndices, variant) < 0)
        {
            variant = 0;
        }

        return HandToHandGruntModels[variant];
    }

    private EnemyOperative CreateEnemy(
        string enemyName,
        Vector3 position,
        EnemyRank rank,
        OperativeSpecialization specialization,
        Color fallbackColor,
        string modelResourcePath = null)
    {
        GameObject enemyObject = new GameObject(enemyName);
        enemyObject.name = enemyName;
        enemyObject.transform.position = position;
        ParentToRuntime(enemyObject);

        GameObject characterVisual = TryCreateCharacterVisual(enemyObject.transform, modelResourcePath);
        if (characterVisual == null)
        {
            characterVisual = CreateFallbackEnemyVisual(enemyObject.transform, fallbackColor);
        }
        AttachEnemyRoleGear(characterVisual, specialization, fallbackColor);

        CapsuleCollider hitbox = enemyObject.AddComponent<CapsuleCollider>();
        if (characterVisual != null)
        {
            FitCapsuleToVisual(hitbox, characterVisual);
        }
        else
        {
            hitbox.center = new Vector3(0f, 1f, 0f);
            hitbox.height = 2f;
            hitbox.radius = 0.5f;
        }

        EnemyOperative enemy = enemyObject.AddComponent<EnemyOperative>();
        enemy.fallbackRank = rank;
        enemy.fallbackSpecialization = specialization;
        enemy.visualResourcePath = modelResourcePath;
        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        enemy.ApplyDifficulty(difficulty);
        enemy.currentHealth = enemy.MaxHealth;

        CharacterAnimationDriver animationDriver = null;
        if (characterVisual != null)
        {
            Animator animator = characterVisual.GetComponentInChildren<Animator>();
            if (animator == null) animator = characterVisual.AddComponent<Animator>();
            animationDriver = enemyObject.AddComponent<CharacterAnimationDriver>();
            animationDriver.walkSpeedReference = 2.6f;
            animationDriver.runSpeedReference = 5.2f;
            animationDriver.Initialize(animator, modelResourcePath, enemyObject.transform);
        }

        EnemyCombatController combat = enemyObject.AddComponent<EnemyCombatController>();
        combat.animationDriver = animationDriver;
        combat.Configure(enemy);
        enemy.aiBehavioursToDisable = new MonoBehaviour[] { combat };
        ConfigureEnemyLoot(enemyObject, enemy, specialization, rank);

        if (enableRiggedGruntRagdoll && characterVisual != null &&
            PrototypeRagdollBuilder.TryBuild(characterVisual, out Rigidbody[] ragdollBodies, out Collider[] ragdollColliders))
        {
            enemy.ConfigureRagdoll(ragdollBodies, ragdollColliders);
        }

        if (rank == EnemyRank.Commander)
        {
            enemy.currentHealth = enemy.MaxHealth;
        }

        CreateWorldLabel($"{rank.ToString().ToUpperInvariant()}  {specialization.ToString().ToUpperInvariant()}", enemyObject.transform.position + Vector3.up * 2.2f, Color.white, 0.09f, enemyObject.transform, true);
        return enemy;
    }

    private void AttachEnemyRoleGear(
        GameObject characterVisual,
        OperativeSpecialization specialization,
        Color accentColor)
    {
        if (characterVisual == null) return;

        string resourcePath;
        string boneName;
        Vector3 bonePosition;
        Vector3 boneEuler;
        Vector3 fallbackPosition;
        float scale;
        switch (specialization)
        {
            case OperativeSpecialization.HandToHand:
                resourcePath = "Models/EnemyGear/HandToHandBracer";
                boneName = "RightLowerArm";
                bonePosition = new Vector3(0f, 0.05f, 0f);
                boneEuler = new Vector3(0f, 0f, 90f);
                fallbackPosition = new Vector3(0.42f, 0.12f, 0.32f);
                scale = 0.95f;
                break;
            case OperativeSpecialization.Stealth:
                resourcePath = "Models/EnemyGear/StealthBlade";
                boneName = "RightHand";
                bonePosition = new Vector3(0f, 0.08f, 0.02f);
                boneEuler = new Vector3(90f, 0f, 0f);
                fallbackPosition = new Vector3(0.4f, 0.08f, 0.35f);
                scale = 1f;
                break;
            case OperativeSpecialization.Tank:
                resourcePath = "Models/EnemyGear/TankArmorPack";
                boneName = "Chest";
                bonePosition = new Vector3(0f, -0.1f, -0.13f);
                boneEuler = Vector3.zero;
                fallbackPosition = new Vector3(0f, 0.08f, -0.52f);
                scale = 0.92f;
                break;
            case OperativeSpecialization.Marksman:
                resourcePath = "Models/EnemyGear/MarksmanRifle";
                boneName = "RightHand";
                bonePosition = new Vector3(0f, 0.11f, 0.02f);
                boneEuler = new Vector3(90f, 0f, 0f);
                fallbackPosition = new Vector3(0.36f, 0.12f, 0.35f);
                scale = 0.86f;
                break;
            case OperativeSpecialization.Hacker:
                resourcePath = "Models/EnemyGear/HackerSignalPack";
                boneName = "Chest";
                bonePosition = new Vector3(0f, -0.09f, -0.13f);
                boneEuler = Vector3.zero;
                fallbackPosition = new Vector3(0f, 0.08f, -0.52f);
                scale = 0.9f;
                break;
            default:
                resourcePath = "Models/EnemyGear/MedicTraumaPack";
                boneName = "Chest";
                bonePosition = new Vector3(0f, -0.09f, -0.13f);
                boneEuler = Vector3.zero;
                fallbackPosition = new Vector3(0f, 0.08f, -0.52f);
                scale = 0.9f;
                break;
        }

        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null) return;

        Transform anchor = FindDescendant(characterVisual, boneName);
        bool attachedToBone = anchor != null;
        if (!attachedToBone) anchor = characterVisual.transform;

        GameObject gear = Instantiate(prefab, anchor);
        gear.name = $"{specialization} Role Gear";
        gear.transform.localPosition = attachedToBone ? bonePosition : fallbackPosition;
        gear.transform.localRotation = Quaternion.Euler(boneEuler);
        gear.transform.localScale = Vector3.one * scale;
        SetLayerRecursively(gear, characterVisual.layer);
        PrototypeEnvironmentArt.StyleKitInstance(gear, accentColor);
    }

    private GameObject TryCreateCharacterVisual(Transform parent, string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath)) return null;

        bool primaryRiggedGrunt = resourcePath == RiggedGrunt01Model ||
            resourcePath == LegacyRiggedGrunt01Model;
        bool secondaryRiggedGrunt = resourcePath == RiggedGrunt02Model;
        bool thirdRiggedGrunt = resourcePath == RiggedGrunt03Model;
        bool fourthRiggedGrunt = resourcePath == RiggedGrunt04Model;
        bool fifthRiggedGrunt = resourcePath == RiggedGrunt05Model;
        bool sixthRiggedGrunt = resourcePath == RiggedGrunt06Model;
        bool seventhRiggedGrunt = resourcePath == RiggedGrunt07Model;
        bool eighthRiggedGrunt = resourcePath == RiggedGrunt08Model;
        bool ninthRiggedGrunt = resourcePath == RiggedGrunt09Model;
        bool tenthRiggedGrunt = resourcePath == RiggedGrunt10Model;
        GameObject modelPrefab = Resources.Load<GameObject>(resourcePath);
        if (modelPrefab == null && resourcePath == RiggedGrunt01Model)
        {
            resourcePath = LegacyRiggedGrunt01Model;
            modelPrefab = Resources.Load<GameObject>(resourcePath);
        }
        if (modelPrefab == null && resourcePath == LegacyRiggedGrunt01Model)
        {
            resourcePath = SourceGrunt01Model;
            modelPrefab = Resources.Load<GameObject>(resourcePath);
        }
        if (modelPrefab == null)
        {
            Debug.LogWarning($"Could not load character model at Resources/{resourcePath}. Using the capsule fallback.");
            return null;
        }

        GameObject visual = Instantiate(modelPrefab, parent);
        visual.name = "Character Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * handToHandGruntScale;
        if (primaryRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt01BaseColor, "Grunt 01");
        }
        else if (secondaryRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt02BaseColor, "Grunt 02");
        }
        else if (thirdRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt03BaseColor, "Grunt 03");
        }
        else if (fourthRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt04BaseColor, "Grunt 04");
        }
        else if (fifthRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt05BaseColor, "Grunt 05");
        }
        else if (sixthRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt06BaseColor, "Grunt 06");
        }
        else if (seventhRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt07BaseColor, "Grunt 07");
        }
        else if (eighthRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt08BaseColor, "Grunt 08");
        }
        else if (ninthRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt09BaseColor, "Grunt 09");
        }
        else if (tenthRiggedGrunt)
        {
            ApplyRiggedGruntMaterial(visual, RiggedGrunt10BaseColor, "Grunt 10");
        }
        OrientAndGroundCharacterVisual(visual, parent);

        foreach (Collider modelCollider in visual.GetComponentsInChildren<Collider>())
        {
            modelCollider.enabled = false;
            Destroy(modelCollider);
        }

        Debug.Log($"Loaded hand-to-hand grunt model: {resourcePath}");
        return visual;
    }

    private void ApplyRiggedGruntMaterial(
        GameObject visual,
        string baseColorResourcePath,
        string materialLabel)
    {
        Texture2D baseColor = Resources.Load<Texture2D>(baseColorResourcePath);
        if (baseColor == null)
        {
            Debug.LogWarning($"Could not load the rigged grunt texture at Resources/{baseColorResourcePath}.");
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) return;

        Material material = new Material(shader) { name = $"{materialLabel} Runtime Material" };
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", baseColor);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", baseColor);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.05f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.4f);

        foreach (Renderer modelRenderer in visual.GetComponentsInChildren<Renderer>())
        {
            if (modelRenderer.name == "Grunt01_Body_LOD0")
            {
                modelRenderer.sharedMaterial = material;
            }
        }
    }

    private void OrientAndGroundCharacterVisual(GameObject visual, Transform parent)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (!TryGetRendererBounds(parent, renderers, out Bounds bounds)) return;

        Vector3 size = bounds.size;
        int longestAxis = size.x > size.y && size.x > size.z ? 0 : size.z > size.y ? 2 : 1;
        if (longestAxis != 1)
        {
            Quaternion startingRotation = visual.transform.localRotation;
            Quaternion positiveTurn = longestAxis == 0
                ? Quaternion.Euler(0f, 0f, 90f)
                : Quaternion.Euler(90f, 0f, 0f);
            Quaternion negativeTurn = Quaternion.Inverse(positiveTurn);

            visual.transform.localRotation = positiveTurn * startingRotation;
            TryGetRendererBounds(parent, renderers, out Bounds positiveBounds);

            visual.transform.localRotation = negativeTurn * startingRotation;
            TryGetRendererBounds(parent, renderers, out Bounds negativeBounds);

            visual.transform.localRotation = UprightScore(positiveBounds) <= UprightScore(negativeBounds)
                ? positiveTurn * startingRotation
                : negativeTurn * startingRotation;
            TryGetRendererBounds(parent, renderers, out bounds);
        }

        visual.transform.localPosition += Vector3.up * -bounds.min.y;
    }

    private float UprightScore(Bounds bounds)
    {
        float belowGroundPenalty = bounds.center.y < 0f ? 100f : 0f;
        return Mathf.Abs(bounds.min.y) + belowGroundPenalty;
    }

    private void FitCapsuleToVisual(CapsuleCollider hitbox, GameObject visual)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (!TryGetRendererBounds(hitbox.transform, renderers, out Bounds bounds))
        {
            hitbox.center = new Vector3(0f, 1f, 0f);
            hitbox.height = 2f;
            hitbox.radius = 0.5f;
            return;
        }

        Vector3 size = bounds.size;
        int direction = size.x > size.y && size.x > size.z ? 0 : size.z > size.y ? 2 : 1;
        float length = size[direction];
        float firstWidth = size[(direction + 1) % 3];
        float secondWidth = size[(direction + 2) % 3];

        hitbox.direction = direction;
        hitbox.center = bounds.center;
        hitbox.radius = Mathf.Max(0.22f, Mathf.Max(firstWidth, secondWidth) * 0.42f);
        hitbox.height = Mathf.Max(length, hitbox.radius * 2f);
    }

    private bool TryGetRendererBounds(Transform root, Renderer[] renderers, out Bounds bounds)
    {
        bounds = new Bounds();
        bool hasBounds = false;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null) continue;

            Bounds worldBounds = targetRenderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        Vector3 worldCorner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 localCorner = root.InverseTransformPoint(worldCorner);

                        if (!hasBounds)
                        {
                            bounds = new Bounds(localCorner, Vector3.zero);
                            hasBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(localCorner);
                        }
                    }
                }
            }
        }

        return hasBounds;
    }

    private GameObject CreateFallbackEnemyVisual(Transform parent, Color color)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Capsule Visual";
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = new Vector3(0f, 1f, 0f);
        SetColor(visual, color);

        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
        {
            visualCollider.enabled = false;
            Destroy(visualCollider);
        }
        return visual;
    }

    private void CreateExtractionZone()
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = "Extraction Zone";
        zone.transform.position = new Vector3(-2.5f, 0.1f, missionEntryZ + 3f);
        zone.transform.localScale = new Vector3(3.6f, 0.2f, 3.6f);
        ParentToRuntime(zone);
        SetColor(zone, new Color(0.1f, 0.7f, 0.35f));

        Collider collider = zone.GetComponent<Collider>();
        collider.isTrigger = true;
        zone.AddComponent<ExtractionZone>();

        GameObject beacon = CreateBlock(
            "Captive Extraction Beacon",
            zone.transform.position + new Vector3(-1.65f, 1.35f, 0f),
            new Vector3(0.12f, 2.7f, 0.12f),
            new Color(0.2f, 1f, 0.5f));
        Collider beaconCollider = beacon.GetComponent<Collider>();
        if (beaconCollider != null) beaconCollider.enabled = false;

        CreateWorldLabel(
            "CAPTIVE DROP-OFF\nCARRY TARGET HERE + PRESS E",
            zone.transform.position + Vector3.up * 1.65f,
            Color.white,
            0.085f);
    }

    private void CreateMissionReturnTerminal()
    {
        Vector3 position = new Vector3(2.5f, 0f, missionEntryZ + 3f);
        Color accentColor = new Color(0.12f, 0.75f, 0.72f);
        GameObject terminal = CreateEnvironmentModel(
            "LabConsole_A",
            "Mission Return Terminal",
            position,
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 0.92f,
            accentColor);
        if (terminal == null)
        {
            terminal = CreateBlock(
                "Mission Return Terminal Fallback",
                position + Vector3.up * 0.7f,
                new Vector3(1.4f, 1.4f, 0.5f),
                accentColor);
        }

        EnsureBoxCollider(
            terminal,
            new Vector3(1.75f, 1.6f, 1.3f),
            new Vector3(0f, 0.8f, 0f));
        terminal.AddComponent<MissionReturnTerminal>();
        CreateWorldLabel(
            "MISSION EXIT",
            position + new Vector3(0f, 2.05f, 0f),
            Color.white,
            0.1f,
            terminal.transform);
    }

    private void CreateSecretLairEnvironment()
    {
        Color floor = new Color(0.075f, 0.095f, 0.11f);
        Color platform = new Color(0.17f, 0.19f, 0.21f);
        Color wall = new Color(0.2f, 0.22f, 0.24f);
        Color rail = new Color(0.12f, 0.34f, 0.38f);
        Color shaft = new Color(0.08f, 0.11f, 0.13f);
        const float lairWidth = 44f;
        const float lairDepth = 60f;
        const float lairCenterZ = 5f;
        const float wallHeight = 26f;

        CreateBlock("Ground Floor", new Vector3(0f, -0.55f, lairCenterZ), new Vector3(lairWidth, 1f, lairDepth), floor);
        CreateLairSideWall(-1f, wall);
        CreateLairSideWall(1f, wall);
        CreateLairAnnexShell(-1f, wall);
        CreateLairAnnexShell(1f, wall);
        CreateLairFrontFacade(wall, rail);
        CreateBlock("North Wall", new Vector3(0f, 12.5f, 35f), new Vector3(lairWidth, wallHeight, 0.6f), wall);
        CreateBlock("Lair Roof West", new Vector3(-13f, LairRoofY, lairCenterZ), new Vector3(17f, 0.6f, lairDepth), shaft);
        CreateBlock("Lair Roof East", new Vector3(13f, LairRoofY, lairCenterZ), new Vector3(17f, 0.6f, lairDepth), shaft);
        CreateBlock("Lair Roof Center", new Vector3(0f, LairRoofY, lairCenterZ), new Vector3(9f, 0.35f, lairDepth), new Color(0.12f, 0.2f, 0.22f));

        CreateLairUpperFloor("Level 2", LairOperationsY, platform, rail);
        CreateLairUpperFloor("Level 3", LairCommandY, platform, rail);
        CreateAtriumSupportPillars(platform, rail);


        CreateDecorationBlock("West Operations Light Band", new Vector3(-21.66f, LairOperationsY, lairCenterZ), new Vector3(0.08f, 0.28f, lairDepth - 1f), rail);
        CreateDecorationBlock("East Operations Light Band", new Vector3(21.66f, LairOperationsY, lairCenterZ), new Vector3(0.08f, 0.28f, lairDepth - 1f), rail);
        CreateDecorationBlock("West Command Light Band", new Vector3(-21.66f, LairCommandY, lairCenterZ), new Vector3(0.08f, 0.28f, lairDepth - 1f), new Color(0.32f, 0.58f, 0.66f));
        CreateDecorationBlock("East Command Light Band", new Vector3(21.66f, LairCommandY, lairCenterZ), new Vector3(0.08f, 0.28f, lairDepth - 1f), new Color(0.32f, 0.58f, 0.66f));

        float[] atriumLightZ = { -15f, -2f, 11f, 24f };
        for (int lightIndex = 0; lightIndex < atriumLightZ.Length; lightIndex++)
        {
            float z = atriumLightZ[lightIndex];
            CreateFacilityCeilingLight(
                $"Ground Atrium Light {lightIndex + 1}",
                new Vector3(0f, LairOperationsY - 1.15f, z),
                new Color(0.18f, 0.66f, 0.7f),
                24f,
                null,
                2.6f);
            CreateFacilityCeilingLight(
                $"Operations Atrium Light {lightIndex + 1}",
                new Vector3(0f, LairCommandY - 1.15f, z),
                new Color(0.2f, 0.62f, 0.74f),
                24f,
                null,
                2.6f);
            CreateFacilityCeilingLight(
                $"Command Atrium Light {lightIndex + 1}",
                new Vector3(0f, LairRoofY - 0.85f, z),
                new Color(0.34f, 0.58f, 0.68f),
                24f,
                null,
                2.6f);
        }

        CreateLairElevator(shaft);
        CreateLairWayfindingDirectories();

        CreateLairArrivalCourtyard(wall, platform, rail);
        CreateLairReceptionHub(wall, platform, rail);
        CreateWorldLabel("THE AERIE", new Vector3(0f, 23.8f, 34.6f), new Color(0.85f, 0.94f, 0.96f), 0.32f);
        CreateWorldLabel("AERIE SECURE ACCESS", new Vector3(0f, 7.9f, -25.4f), Color.white, 0.18f);
        CreateWorldLabel("LEVEL 2  OPERATIONS", new Vector3(0f, LairOperationsY + 4.3f, -24.6f), Color.white, 0.16f);
        CreateWorldLabel("LEVEL 3  COMMAND", new Vector3(0f, LairCommandY + 4.3f, -24.6f), Color.white, 0.16f);
    }

    // Builds one atrium side wall as solid blocks with an opening for every
    // department doorway on that side.
    private void CreateLairSideWall(float side, Color wallColor)
    {
        const float wallBottom = -0.5f;
        float wallTop = wallBottom + LairWallHeight;
        float x = side * LairHalfWidth;
        string sideName = side < 0f ? "West" : "East";

        List<Rect> openings = new List<Rect>();
        foreach (LairRoomSpec spec in LairRoomSpecs)
        {
            if (!Mathf.Approximately(spec.side, side) || spec.exteriorDoor) continue;
            float openingWidth = spec.doorWidth + 0.2f;
            openings.Add(new Rect(
                spec.centerZ - openingWidth * 0.5f,
                spec.floorY - 0.05f,
                openingWidth,
                spec.doorHeight + 0.1f));
        }

        List<float> zCuts = new List<float> { LairDepthMinZ, LairDepthMaxZ };
        List<float> yCuts = new List<float> { wallBottom, wallTop };
        foreach (Rect opening in openings)
        {
            zCuts.Add(Mathf.Clamp(opening.xMin, LairDepthMinZ, LairDepthMaxZ));
            zCuts.Add(Mathf.Clamp(opening.xMax, LairDepthMinZ, LairDepthMaxZ));
            yCuts.Add(Mathf.Clamp(opening.yMin, wallBottom, wallTop));
            yCuts.Add(Mathf.Clamp(opening.yMax, wallBottom, wallTop));
        }

        zCuts.Sort();
        yCuts.Sort();
        int blockIndex = 0;
        for (int zi = 0; zi < zCuts.Count - 1; zi++)
        {
            float z0 = zCuts[zi];
            float z1 = zCuts[zi + 1];
            if (z1 - z0 < 0.01f) continue;

            float runStart = float.NaN;
            for (int yi = 0; yi < yCuts.Count - 1; yi++)
            {
                float y0 = yCuts[yi];
                float y1 = yCuts[yi + 1];
                if (y1 - y0 < 0.01f) continue;

                Vector2 cellCenter = new Vector2((z0 + z1) * 0.5f, (y0 + y1) * 0.5f);
                bool open = false;
                foreach (Rect opening in openings)
                {
                    if (opening.Contains(cellCenter))
                    {
                        open = true;
                        break;
                    }
                }

                if (!open && float.IsNaN(runStart))
                {
                    runStart = y0;
                }
                else if (open && !float.IsNaN(runStart))
                {
                    CreateLairWallPiece(sideName, blockIndex++, x, z0, z1, runStart, y0, wallColor);
                    runStart = float.NaN;
                }
            }

            if (!float.IsNaN(runStart))
            {
                CreateLairWallPiece(sideName, blockIndex++, x, z0, z1, runStart, wallTop, wallColor);
            }
        }
    }

    private void CreateLairWallPiece(string sideName, int index, float x, float z0, float z1, float y0, float y1, Color color)
    {
        CreateBlock(
            $"{sideName} Wall Section {index + 1}",
            new Vector3(x, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f),
            new Vector3(0.6f, y1 - y0, z1 - z0),
            color);
    }

    // Encloses the department annexes on one side so they read as part of the
    // building from the arrival courtyard instead of floating boxes.
    private void CreateLairAnnexShell(float side, Color wallColor)
    {
        float outerExtent = LairHalfWidth;
        float shellTop = LairWallHeight;
        foreach (LairRoomSpec spec in LairRoomSpecs)
        {
            if (!Mathf.Approximately(spec.side, side)) continue;
            outerExtent = Mathf.Max(outerExtent, LairHalfWidth + spec.width + 1f);
            shellTop = Mathf.Max(shellTop, spec.floorY + spec.height + 1f);
        }

        if (outerExtent <= LairHalfWidth) return;

        string sideName = side < 0f ? "West" : "East";
        Color shellColor = Color.Lerp(wallColor, Color.black, 0.18f);
        float innerX = side * (LairHalfWidth + 0.3f);
        float outerX = side * outerExtent;
        float spanX = Mathf.Abs(outerX - innerX);
        float centerX = (innerX + outerX) * 0.5f;
        float depth = LairDepthMaxZ - LairDepthMinZ;
        float centerZ = (LairDepthMinZ + LairDepthMaxZ) * 0.5f;
        float height = shellTop + 0.5f;
        float centerY = -0.5f + height * 0.5f;

        // South facade, with an opening for any room entered from outside.
        float facadeMinX = Mathf.Min(innerX, outerX);
        float facadeMaxX = Mathf.Max(innerX, outerX);
        float facadeBottom = -0.5f;
        float facadeTop = facadeBottom + height;
        bool hasOpening = false;
        foreach (LairRoomSpec spec in LairRoomSpecs)
        {
            if (!Mathf.Approximately(spec.side, side) || !spec.exteriorDoor) continue;
            hasOpening = true;
            float doorMinX = spec.ExteriorDoorX - spec.doorWidth * 0.5f;
            float doorMaxX = spec.ExteriorDoorX + spec.doorWidth * 0.5f;
            float doorTop = spec.floorY + spec.doorHeight;
            CreateFacadeSpan($"{sideName} Annex South Facade A", facadeMinX, doorMinX, facadeBottom, facadeTop, wallColor);
            CreateFacadeSpan($"{sideName} Annex South Facade B", doorMaxX, facadeMaxX, facadeBottom, facadeTop, wallColor);
            CreateFacadeSpan($"{sideName} Annex South Facade Over Door", doorMinX, doorMaxX, doorTop, facadeTop, wallColor);
            break;
        }

        if (!hasOpening)
        {
            CreateBlock($"{sideName} Annex South Facade", new Vector3(centerX, centerY, LairDepthMinZ), new Vector3(spanX, height, 0.6f), wallColor);
        }
        CreateBlock($"{sideName} Annex North Wall", new Vector3(centerX, centerY, LairDepthMaxZ), new Vector3(spanX, height, 0.6f), shellColor);
        CreateBlock($"{sideName} Annex Outer Wall", new Vector3(outerX, centerY, centerZ), new Vector3(0.6f, height, depth), shellColor);
        CreateBlock($"{sideName} Annex Roof", new Vector3(centerX, shellTop, centerZ), new Vector3(spanX, 0.6f, depth), shellColor);
        CreateBlock($"{sideName} Annex Foundation", new Vector3(centerX, -0.55f, centerZ), new Vector3(spanX, 1f, depth), new Color(0.075f, 0.095f, 0.11f));
    }

    // Atrium layout shared by the upper floors, the elevator, and the railings.
    private const float AtriumHalfWidth = 4.5f;
    private const float AtriumOpeningMinZ = -13f;
    private const float AtriumOpeningMaxZ = 24f;
    private const float ElevatorShaftHalfWidth = 2.6f;
    private const float ElevatorShaftFrontZ = 29f;

    // One upper level of the atrium building: wings that run wall to wall, a
    // front bridge that meets the facade, an elevator landing, and floor beside
    // the shaft, so the only opening left is the atrium itself.
    private void CreateLairUpperFloor(string levelName, float levelY, Color platformColor, Color railColor)
    {
        float innerWall = LairHalfWidth - 0.3f;
        float wingWidth = innerWall - AtriumHalfWidth;
        float wingCenterX = AtriumHalfWidth + wingWidth * 0.5f;
        float depth = LairDepthMaxZ - LairDepthMinZ;
        float centerZ = (LairDepthMinZ + LairDepthMaxZ) * 0.5f;

        CreateBlock($"{levelName} West Wing Floor", new Vector3(-wingCenterX, levelY, centerZ), new Vector3(wingWidth, 0.4f, depth), platformColor);
        CreateBlock($"{levelName} East Wing Floor", new Vector3(wingCenterX, levelY, centerZ), new Vector3(wingWidth, 0.4f, depth), platformColor);

        float frontDepth = AtriumOpeningMinZ - LairDepthMinZ;
        CreateBlock($"{levelName} Front Bridge Floor", new Vector3(0f, levelY, LairDepthMinZ + frontDepth * 0.5f), new Vector3(AtriumHalfWidth * 2f, 0.4f, frontDepth), platformColor);

        float landingDepth = ElevatorShaftFrontZ - AtriumOpeningMaxZ;
        CreateBlock($"{levelName} Elevator Landing Floor", new Vector3(0f, levelY, AtriumOpeningMaxZ + landingDepth * 0.5f), new Vector3(AtriumHalfWidth * 2f, 0.4f, landingDepth), platformColor);

        float besideShaftWidth = AtriumHalfWidth - ElevatorShaftHalfWidth;
        float besideShaftDepth = LairDepthMaxZ - ElevatorShaftFrontZ;
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBlock(
                $"{levelName} {(side < 0 ? "West" : "East")} Shaft-Side Floor",
                new Vector3(side * (ElevatorShaftHalfWidth + besideShaftWidth * 0.5f), levelY, ElevatorShaftFrontZ + besideShaftDepth * 0.5f),
                new Vector3(besideShaftWidth, 0.4f, besideShaftDepth),
                platformColor);
        }

        // Railings around the whole atrium opening, plus a lit fascia on its edge.
        float railY = levelY + 0.2f + 0.55f;
        float openingDepth = AtriumOpeningMaxZ - AtriumOpeningMinZ;
        float openingCenterZ = (AtriumOpeningMinZ + AtriumOpeningMaxZ) * 0.5f;
        for (int side = -1; side <= 1; side += 2)
        {
            string sideName = side < 0 ? "West" : "East";
            CreateBlock($"{levelName} {sideName} Atrium Rail", new Vector3(side * (AtriumHalfWidth + 0.1f), railY, openingCenterZ), new Vector3(0.18f, 1.1f, openingDepth), railColor);
            CreateDecorationBlock($"{levelName} {sideName} Atrium Edge Light", new Vector3(side * (AtriumHalfWidth + 0.02f), levelY - 0.05f, openingCenterZ), new Vector3(0.06f, 0.18f, openingDepth), railColor);
        }

        CreateBlock($"{levelName} South Atrium Rail", new Vector3(0f, railY, AtriumOpeningMinZ - 0.1f), new Vector3(AtriumHalfWidth * 2f + 0.2f, 1.1f, 0.18f), railColor);
        CreateBlock($"{levelName} North Atrium Rail", new Vector3(0f, railY, AtriumOpeningMaxZ + 0.1f), new Vector3(AtriumHalfWidth * 2f + 0.2f, 1.1f, 0.18f), railColor);
        CreateDecorationBlock($"{levelName} South Atrium Edge Light", new Vector3(0f, levelY - 0.05f, AtriumOpeningMinZ - 0.02f), new Vector3(AtriumHalfWidth * 2f, 0.18f, 0.06f), railColor);
        CreateDecorationBlock($"{levelName} North Atrium Edge Light", new Vector3(0f, levelY - 0.05f, AtriumOpeningMaxZ + 0.02f), new Vector3(AtriumHalfWidth * 2f, 0.18f, 0.06f), railColor);

        // Lights on the underside of this floor so the level below isn't left in the dark.
        float[] lightZ = { -18f, -4f, 10f, 24f };
        foreach (float z in lightZ)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                CreateFacilityCeilingLight(
                    $"{levelName} {(side < 0 ? "West" : "East")} Underside Light {z:0}",
                    new Vector3(side * wingCenterX, levelY - 0.55f, z),
                    new Color(0.2f, 0.62f, 0.68f),
                    16f,
                    null,
                    2.2f);
            }
        }
    }

    // Structural pillars carrying the upper floors along the atrium edges, from
    // the ground floor up to the roof.
    private void CreateAtriumSupportPillars(Color pillarColor, Color accentColor)
    {
        float[] pillarZ = { AtriumOpeningMinZ, -3f, 7f, 17f, AtriumOpeningMaxZ };
        float height = LairRoofY + 0.05f;
        foreach (float z in pillarZ)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * (AtriumHalfWidth + 0.65f);
                CreateBlock(
                    $"Atrium Support Pillar {(side < 0 ? "W" : "E")}{z:0}",
                    new Vector3(x, height * 0.5f - 0.05f, z),
                    new Vector3(0.8f, height, 0.8f),
                    Color.Lerp(pillarColor, Color.black, 0.15f));
                CreateDecorationBlock(
                    $"Atrium Pillar Light Strip {(side < 0 ? "W" : "E")}{z:0}",
                    new Vector3(x - side * 0.41f, height * 0.5f - 0.05f, z),
                    new Vector3(0.04f, height - 0.6f, 0.12f),
                    accentColor);
            }
        }
    }

    private void CreateFacadeSpan(string spanName, float minX, float maxX, float bottom, float top, Color color)
    {
        if (maxX - minX < 0.05f || top - bottom < 0.05f) return;
        CreateBlock(
            spanName,
            new Vector3((minX + maxX) * 0.5f, (bottom + top) * 0.5f, LairDepthMinZ),
            new Vector3(maxX - minX, top - bottom, 0.6f),
            color);
    }

    // Front facade of the main building, with glazed openings: glass entrance
    // doors, lounge windows either side of the entrance, and a band of
    // windows on Levels 2 and 3 overlooking the courtyard approach. The upper
    // bands double as defensive firing positions: the glass is bulletproof
    // one-way glass that stops incoming fire but lets the player shoot out.
    private void CreateLairFrontFacade(Color wallColor, Color accentColor)
    {
        const float facadeZ = -25f;
        const float bottom = -0.5f;
        float top = LairRoofY;
        float minX = -(LairHalfWidth + 0.3f);
        float maxX = LairHalfWidth + 0.3f;

        Rect entrance = Rect.MinMaxRect(-5f, bottom, 5f, 7f);
        List<Rect> windows = new List<Rect>
        {
            // Ground floor: reception lounge windows.
            Rect.MinMaxRect(-19f, 1f, -7f, 4.2f),
            Rect.MinMaxRect(7f, 1f, 19f, 4.2f)
        };

        float[] upperFloors = { LairOperationsY + 0.2f, LairCommandY + 0.2f };
        foreach (float floorY in upperFloors)
        {
            float sill = floorY + 1f;
            float head = floorY + 3.2f;
            windows.Add(Rect.MinMaxRect(-20.5f, sill, -6f, head));
            windows.Add(Rect.MinMaxRect(-4f, sill, 4f, head));
            windows.Add(Rect.MinMaxRect(6f, sill, 20.5f, head));
        }

        List<Rect> openings = new List<Rect>(windows) { entrance };
        CreateWallWithOpeningsX("South Facade", facadeZ, minX, maxX, bottom, top, 0.6f, openings, wallColor);

        foreach (Rect window in windows)
        {
            CreateGlazedOpening("Facade", window, facadeZ, accentColor);
        }

        CreateEntranceStorefront(entrance, facadeZ, accentColor);
    }

    // Automatic glass entrance in the style of a commercial sliding door
    // system: an aluminium frame filling the opening, a door-operator header,
    // glass transom panes above it, fixed glass side panels, and a pair of
    // framed glass doors in the middle that slide open behind the side panels.
    private void CreateEntranceStorefront(Rect opening, float wallZ, Color accentColor)
    {
        Color aluminium = new Color(0.72f, 0.75f, 0.78f);
        Color dark = new Color(0.08f, 0.09f, 0.1f);
        GameObject root = new GameObject("Main Entrance Storefront");
        root.transform.SetParent(runtimeRoot, false);
        Transform parent = root.transform;

        float floorY = 0f;
        float headerBottom = 3f;
        float headerTop = 3.45f;
        float halfWidth = opening.width * 0.5f;
        float centerX = (opening.xMin + opening.xMax) * 0.5f;
        const float leafWidth = 2f;
        const float frameDepth = 0.14f;

        // Perimeter frame and door-operator header.
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBlock($"Entrance Frame Jamb Post {side}", new Vector3(centerX + side * (halfWidth - 0.06f), (floorY + opening.yMax) * 0.5f, wallZ), new Vector3(0.12f, opening.yMax - floorY, frameDepth), aluminium, parent);
        }

        CreateBlock("Entrance Door Operator Header Rail", new Vector3(centerX, (headerBottom + headerTop) * 0.5f, wallZ + 0.05f), new Vector3(opening.width, headerTop - headerBottom, 0.36f), aluminium, parent);
        CreateDecorationBlock("Entrance Frame Top Rail", new Vector3(centerX, opening.yMax - 0.06f, wallZ), new Vector3(opening.width, 0.12f, frameDepth), aluminium, parent);
        CreateDecorationBlock("Entrance Floor Track Rail", new Vector3(centerX, floorY + 0.01f, wallZ + 0.12f), new Vector3(opening.width, 0.03f, 0.3f), dark, parent);
        CreateDecorationBlock("Entrance Motion Sensor Post", new Vector3(centerX, headerBottom - 0.08f, wallZ + 0.24f), new Vector3(0.45f, 0.12f, 0.12f), dark, parent);
        CreateDecorationBlock("Entrance Motion Sensor Light", new Vector3(centerX, headerBottom - 0.14f, wallZ + 0.24f), new Vector3(0.12f, 0.02f, 0.06f), accentColor, parent);

        // Transom glazing above the header, divided by mullions.
        float transomHeight = opening.yMax - 0.12f - headerTop;
        GameObject transom = CreateBlock("Entrance Transom Defense Glass", new Vector3(centerX, headerTop + transomHeight * 0.5f, wallZ), new Vector3(opening.width - 0.24f, transomHeight, 0.04f), aluminium, parent);
        ApplyGlassMaterial(transom);
        for (int i = 1; i < 4; i++)
        {
            float x = opening.xMin + opening.width * i / 4f;
            CreateDecorationBlock($"Entrance Transom Mullion Post {i}", new Vector3(x, headerTop + transomHeight * 0.5f, wallZ), new Vector3(0.08f, transomHeight, frameDepth), aluminium, parent);
        }

        // Fixed glass side panels either side of the sliding doors.
        float sideWidth = halfWidth - 0.12f - leafWidth;
        for (int side = -1; side <= 1; side += 2)
        {
            float x = centerX + side * (leafWidth + sideWidth * 0.5f);
            GameObject sideLite = CreateBlock($"Entrance Side Defense Glass {side}", new Vector3(x, (floorY + headerBottom) * 0.5f, wallZ), new Vector3(sideWidth, headerBottom - floorY, 0.04f), aluminium, parent);
            ApplyGlassMaterial(sideLite);
            CreateDecorationBlock($"Entrance Side Mid Rail {side}", new Vector3(x, floorY + 1f, wallZ), new Vector3(sideWidth, 0.08f, frameDepth), aluminium, parent);
            CreateDecorationBlock($"Entrance Side Bottom Rail {side}", new Vector3(x, floorY + 0.06f, wallZ), new Vector3(sideWidth, 0.12f, frameDepth), aluminium, parent);
            CreateDecorationBlock($"Entrance Meeting Post {side}", new Vector3(centerX + side * leafWidth, (floorY + headerBottom) * 0.5f, wallZ), new Vector3(0.1f, headerBottom - floorY, frameDepth), aluminium, parent);
        }

        // The sliding doors: run on the inside face and slide behind the side panels.
        float leafZ = wallZ + 0.16f;
        GameObject doorRoot = new GameObject("Main Entrance Sliding Doors");
        doorRoot.transform.SetParent(parent, false);
        doorRoot.transform.position = new Vector3(centerX, floorY, leafZ);
        BoxCollider trigger = doorRoot.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.5f, 0f);
        trigger.size = new Vector3(leafWidth * 2f + 2f, 3f, 7f);
        Rigidbody body = doorRoot.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        Transform leftLeaf = CreateSlidingGlassLeaf("Left", doorRoot.transform, -leafWidth * 0.5f, leafWidth, headerBottom - floorY - 0.02f, aluminium, 1f);
        Transform rightLeaf = CreateSlidingGlassLeaf("Right", doorRoot.transform, leafWidth * 0.5f, leafWidth, headerBottom - floorY - 0.02f, aluminium, -1f);

        SlidingSecurityDoor door = doorRoot.AddComponent<SlidingSecurityDoor>();
        door.slideSpeed = 2.6f;
        door.closeDelay = 1.5f;
        door.Initialize(leftLeaf, rightLeaf, leafWidth - 0.05f);

        // Entrance mats on both sides.
        CreateDecorationBlock("Entrance Floor Mat Inside", new Vector3(centerX, 0.0f, wallZ + 1.6f), new Vector3(leafWidth * 2f, 0.02f, 2f), new Color(0.06f, 0.07f, 0.08f), parent);
        CreateDecorationBlock("Entrance Floor Mat Outside", new Vector3(centerX, 0.0f, wallZ - 1.4f), new Vector3(leafWidth * 2f, 0.02f, 1.6f), new Color(0.06f, 0.07f, 0.08f), parent);
    }

    // One framed glass door leaf: glass, stiles, top/bottom/mid rails, and a
    // pull handle on the edge where the two doors meet.
    private Transform CreateSlidingGlassLeaf(string side, Transform doorRoot, float localX, float width, float height, Color frameColor, float handleSide)
    {
        GameObject leaf = new GameObject($"Entrance Door Leaf {side}");
        leaf.transform.SetParent(doorRoot, false);
        leaf.transform.localPosition = new Vector3(localX, 0f, 0f);
        Transform t = leaf.transform;
        Vector3 origin = t.position;
        const float frame = 0.08f;
        const float depth = 0.07f;

        GameObject glass = CreateBlock($"Entrance Door {side} Defense Glass", origin + new Vector3(0f, height * 0.5f, 0f), new Vector3(width - frame, height - frame, 0.03f), frameColor, t);
        ApplyGlassMaterial(glass);
        CreateDecorationBlock($"Entrance Door {side} Top Rail", origin + new Vector3(0f, height - frame * 0.5f, 0f), new Vector3(width, frame, depth), frameColor, t);
        CreateDecorationBlock($"Entrance Door {side} Bottom Rail", origin + new Vector3(0f, 0.08f, 0f), new Vector3(width, 0.16f, depth), frameColor, t);
        CreateDecorationBlock($"Entrance Door {side} Mid Rail", origin + new Vector3(0f, 1f, 0f), new Vector3(width, 0.07f, depth), frameColor, t);
        for (int edge = -1; edge <= 1; edge += 2)
        {
            CreateDecorationBlock($"Entrance Door {side} Stile Post {edge}", origin + new Vector3(edge * (width * 0.5f - frame * 0.5f), height * 0.5f, 0f), new Vector3(frame, height, depth), frameColor, t);
        }

        // Pull handles on both faces near the meeting edge.
        float handleX = handleSide * (width * 0.5f - 0.18f);
        for (int face = -1; face <= 1; face += 2)
        {
            CreateDecorationBlock($"Entrance Door {side} Handle Post {face}", origin + new Vector3(handleX, 1.1f, face * 0.07f), new Vector3(0.035f, 0.7f, 0.035f), new Color(0.85f, 0.87f, 0.9f), t);
        }

        return t;
    }

    // A glass pane in an opening, with a frame, mullions roughly every three
    // metres, and a sill on the inside that works as waist-high cover.
    private void CreateGlazedOpening(string prefix, Rect opening, float wallZ, Color accentColor)
    {
        Color frameColor = new Color(0.1f, 0.12f, 0.13f);
        float centerX = (opening.xMin + opening.xMax) * 0.5f;
        float centerY = (opening.yMin + opening.yMax) * 0.5f;

        GameObject glass = CreateBlock(
            $"{prefix} Defense Glass {centerX:0}_{centerY:0}",
            new Vector3(centerX, centerY, wallZ),
            new Vector3(opening.width, opening.height, 0.06f),
            new Color(0.6f, 0.75f, 0.8f));
        ApplyGlassMaterial(glass);

        CreateDecorationBlock($"{prefix} Glazing Frame Top {centerX:0}_{centerY:0}", new Vector3(centerX, opening.yMax - 0.05f, wallZ), new Vector3(opening.width, 0.1f, 0.16f), frameColor);
        CreateDecorationBlock($"{prefix} Glazing Frame Bottom {centerX:0}_{centerY:0}", new Vector3(centerX, opening.yMin + 0.05f, wallZ), new Vector3(opening.width, 0.1f, 0.16f), frameColor);
        int mullions = Mathf.Max(0, Mathf.RoundToInt(opening.width / 3f) - 1);
        for (int i = 1; i <= mullions; i++)
        {
            float x = opening.xMin + opening.width * i / (mullions + 1);
            CreateDecorationBlock($"{prefix} Glazing Mullion {centerX:0}_{centerY:0}_{i}", new Vector3(x, centerY, wallZ), new Vector3(0.08f, opening.height, 0.14f), frameColor);
        }

        // Interior sill shelf, slightly lit along its edge.
        CreateBlock($"{prefix} Glazing Sill Bench {centerX:0}_{centerY:0}", new Vector3(centerX, opening.yMin - 0.05f, wallZ + 0.45f), new Vector3(opening.width, 0.1f, 0.6f), frameColor);
        CreateDecorationBlock($"{prefix} Glazing Sill Strip {centerX:0}_{centerY:0}", new Vector3(centerX, opening.yMin - 0.1f, wallZ + 0.76f), new Vector3(opening.width, 0.03f, 0.03f), accentColor);
    }

    // Solid wall in the X-Y plane with rectangular openings cut out of it.
    private void CreateWallWithOpeningsX(string wallName, float z, float minX, float maxX, float bottom, float top, float thickness, List<Rect> openings, Color color)
    {
        List<float> xCuts = new List<float> { minX, maxX };
        List<float> yCuts = new List<float> { bottom, top };
        foreach (Rect opening in openings)
        {
            xCuts.Add(Mathf.Clamp(opening.xMin, minX, maxX));
            xCuts.Add(Mathf.Clamp(opening.xMax, minX, maxX));
            yCuts.Add(Mathf.Clamp(opening.yMin, bottom, top));
            yCuts.Add(Mathf.Clamp(opening.yMax, bottom, top));
        }

        xCuts.Sort();
        yCuts.Sort();
        int index = 0;
        for (int xi = 0; xi < xCuts.Count - 1; xi++)
        {
            float x0 = xCuts[xi];
            float x1 = xCuts[xi + 1];
            if (x1 - x0 < 0.01f) continue;

            float runStart = float.NaN;
            for (int yi = 0; yi < yCuts.Count - 1; yi++)
            {
                float y0 = yCuts[yi];
                float y1 = yCuts[yi + 1];
                if (y1 - y0 < 0.01f) continue;

                Vector2 cell = new Vector2((x0 + x1) * 0.5f, (y0 + y1) * 0.5f);
                bool open = false;
                foreach (Rect opening in openings)
                {
                    if (opening.Contains(cell))
                    {
                        open = true;
                        break;
                    }
                }

                if (!open && float.IsNaN(runStart))
                {
                    runStart = y0;
                }
                else if (open && !float.IsNaN(runStart))
                {
                    CreateBlock($"{wallName} Section {++index}", new Vector3((x0 + x1) * 0.5f, (runStart + y0) * 0.5f, z), new Vector3(x1 - x0, y0 - runStart, thickness), color);
                    runStart = float.NaN;
                }
            }

            if (!float.IsNaN(runStart))
            {
                CreateBlock($"{wallName} Section {++index}", new Vector3((x0 + x1) * 0.5f, (runStart + top) * 0.5f, z), new Vector3(x1 - x0, top - runStart, thickness), color);
            }
        }
    }

    private static Material glassMaterial;

    // Transparent URP glass. Replaces the block's textured material.
    private static void ApplyGlassMaterial(GameObject target)
    {
        if (target == null) return;
        if (glassMaterial == null)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;
            glassMaterial = new Material(lit) { name = "Lair Defense Glass" };
            glassMaterial.SetFloat("_Surface", 1f);
            glassMaterial.SetFloat("_Blend", 0f);
            glassMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glassMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            glassMaterial.SetFloat("_ZWrite", 0f);
            glassMaterial.SetFloat("_Smoothness", 0.95f);
            glassMaterial.SetFloat("_Metallic", 0.1f);
            glassMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            glassMaterial.SetOverrideTag("RenderType", "Transparent");
            glassMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            glassMaterial.SetColor("_BaseColor", new Color(0.55f, 0.78f, 0.85f, 0.22f));
        }

        Renderer glassRenderer = target.GetComponent<Renderer>();
        if (glassRenderer == null) return;
        glassRenderer.SetPropertyBlock(null);
        glassRenderer.sharedMaterial = glassMaterial;
        glassRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // Grounds around the lair: ground everywhere (no more falling into the
    // void), an access road from the courtyard gate to a perimeter checkpoint,
    // forest and rocks, and a perimeter wall that bounds the playable area.
    // The existing distant ridges and skyline sit just beyond it.
    private const float ExteriorBoundaryX = 118f;
    private const float ExteriorBoundarySouthZ = -128f;
    private const float ExteriorBoundaryNorthZ = 118f;

    private void CreateLairExteriorGrounds()
    {
        GameObject root = new GameObject("Lair Exterior Grounds");
        root.transform.SetParent(runtimeRoot, false);
        Transform parent = root.transform;
        Color grass = new Color(0.12f, 0.2f, 0.11f);
        Color wallColor = new Color(0.22f, 0.23f, 0.22f);

        // Ground: extends past the perimeter wall out to the ridges.
        CreateBlock("Exterior Grass Ground", new Vector3(0f, -0.6f, -5f), new Vector3(320f, 1f, 320f), grass, parent);

        // Access road from the courtyard gate south to the perimeter checkpoint.
        float roadStart = -70.5f;
        float roadEnd = ExteriorBoundarySouthZ + 1f;
        float roadLength = roadStart - roadEnd;
        float roadCenter = (roadStart + roadEnd) * 0.5f;
        CreateDecorationBlock("Exterior Access Road", new Vector3(0f, -0.095f, roadCenter), new Vector3(10f, 0.03f, roadLength), new Color(0.1f, 0.1f, 0.11f), parent);
        for (int edge = -1; edge <= 1; edge += 2)
        {
            CreateDecorationBlock($"Exterior Road Edge Line {edge}", new Vector3(edge * 4.6f, -0.075f, roadCenter), new Vector3(0.15f, 0.01f, roadLength), new Color(0.75f, 0.72f, 0.6f), parent);
        }

        for (float z = roadStart - 6f; z > roadEnd + 4f; z -= 9f)
        {
            CreateDecorationBlock($"Exterior Road Center Dash {z:0}", new Vector3(0f, -0.075f, z), new Vector3(0.18f, 0.01f, 4f), new Color(0.8f, 0.7f, 0.3f), parent);
        }

        float[] lampZ = { -82f, -102f, -122f };
        foreach (float z in lampZ)
        {
            CreateFacilityLamp($"Exterior Road Lamp W {z:0}", new Vector3(-6.5f, 0f, z));
            CreateFacilityLamp($"Exterior Road Lamp E {z:0}", new Vector3(6.5f, 0f, z - 10f));
        }

        // Perimeter wall around the grounds, with a guarded checkpoint where
        // the road meets it.
        float spanX = ExteriorBoundaryX * 2f;
        float spanZ = ExteriorBoundaryNorthZ - ExteriorBoundarySouthZ;
        float centerZ = (ExteriorBoundaryNorthZ + ExteriorBoundarySouthZ) * 0.5f;
        const float wallHeight = 4f;
        CreateBlock("Exterior Perimeter Wall North", new Vector3(0f, wallHeight * 0.5f - 0.1f, ExteriorBoundaryNorthZ), new Vector3(spanX, wallHeight, 0.6f), wallColor, parent);
        CreateBlock("Exterior Perimeter Wall West", new Vector3(-ExteriorBoundaryX, wallHeight * 0.5f - 0.1f, centerZ), new Vector3(0.6f, wallHeight, spanZ), wallColor, parent);
        CreateBlock("Exterior Perimeter Wall East", new Vector3(ExteriorBoundaryX, wallHeight * 0.5f - 0.1f, centerZ), new Vector3(0.6f, wallHeight, spanZ), wallColor, parent);
        float gateHalf = 7f;
        float southSpan = ExteriorBoundaryX - gateHalf;
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBlock($"Exterior Perimeter Wall South {(side < 0 ? "W" : "E")}", new Vector3(side * (gateHalf + southSpan * 0.5f), wallHeight * 0.5f - 0.1f, ExteriorBoundarySouthZ), new Vector3(southSpan, wallHeight, 0.6f), wallColor, parent);
            CreateBlock($"Exterior Checkpoint Gate Pillar {(side < 0 ? "W" : "E")}", new Vector3(side * gateHalf, 2.6f, ExteriorBoundarySouthZ), new Vector3(1.2f, 5.4f, 1.2f), new Color(0.17f, 0.19f, 0.21f), parent);
        }

        // The checkpoint barrier closes the road (the grounds end here).
        CreateBlock("Exterior Checkpoint Barrier Gate", new Vector3(0f, 1.6f, ExteriorBoundarySouthZ), new Vector3(gateHalf * 2f - 1.2f, 3.4f, 0.4f), new Color(0.3f, 0.3f, 0.28f), parent);
        CreateDecorationBlock("Exterior Checkpoint Barrier Hazard Stripe", new Vector3(0f, 2.2f, ExteriorBoundarySouthZ + 0.22f), new Vector3(gateHalf * 2f - 1.4f, 0.3f, 0.04f), new Color(0.92f, 0.68f, 0.12f), parent);
        GameObject booth = CreateEnvironmentModel("GuardBooth_A", "Exterior Checkpoint Guard Booth", new Vector3(10.5f, 0f, ExteriorBoundarySouthZ + 4f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, new Color(0.16f, 0.76f, 0.72f), parent);
        DisableChildColliders(booth);
        if (booth != null)
        {
            BoxCollider boothCollider = booth.AddComponent<BoxCollider>();
            boothCollider.center = new Vector3(0f, 1.5f, 0f);
            boothCollider.size = new Vector3(3f, 3f, 3f);
        }

        // Taller invisible barrier on the perimeter so the wall can't be jumped.
        CreateInvisibleBarrier("Exterior Boundary North", new Vector3(0f, 10f, ExteriorBoundaryNorthZ), new Vector3(spanX, 20f, 1f), parent);
        CreateInvisibleBarrier("Exterior Boundary South", new Vector3(0f, 10f, ExteriorBoundarySouthZ), new Vector3(spanX, 20f, 1f), parent);
        CreateInvisibleBarrier("Exterior Boundary West", new Vector3(-ExteriorBoundaryX, 10f, centerZ), new Vector3(1f, 20f, spanZ), parent);
        CreateInvisibleBarrier("Exterior Boundary East", new Vector3(ExteriorBoundaryX, 10f, centerZ), new Vector3(1f, 20f, spanZ), parent);

        // Forest and rocks, scattered deterministically and kept clear of the
        // building, courtyard, driveway, and road.
        System.Random random = new System.Random(9173);
        int treesPlaced = 0;
        for (int attempt = 0; attempt < 900 && treesPlaced < 140; attempt++)
        {
            float x = Mathf.Lerp(-ExteriorBoundaryX + 4f, ExteriorBoundaryX - 4f, (float)random.NextDouble());
            float z = Mathf.Lerp(ExteriorBoundarySouthZ + 4f, ExteriorBoundaryNorthZ - 4f, (float)random.NextDouble());
            if (IsExteriorClearZone(x, z, 7f)) continue;

            float scale = 0.7f + (float)random.NextDouble() * 0.6f;
            GameObject tree = CreateEnvironmentModel(
                "PineTree_A",
                $"Exterior Pine {treesPlaced + 1}",
                new Vector3(x, -0.1f, z),
                Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f),
                Vector3.one * scale,
                new Color(0.16f, 0.3f, 0.15f),
                parent);
            if (tree == null) break;
            DisableChildColliders(tree);
            CapsuleCollider trunk = tree.AddComponent<CapsuleCollider>();
            trunk.center = new Vector3(0f, 2f, 0f);
            trunk.radius = 0.4f;
            trunk.height = 4f;
            treesPlaced++;
        }

        int rocksPlaced = 0;
        for (int attempt = 0; attempt < 300 && rocksPlaced < 34; attempt++)
        {
            float x = Mathf.Lerp(-ExteriorBoundaryX + 3f, ExteriorBoundaryX - 3f, (float)random.NextDouble());
            float z = Mathf.Lerp(ExteriorBoundarySouthZ + 3f, ExteriorBoundaryNorthZ - 3f, (float)random.NextDouble());
            if (IsExteriorClearZone(x, z, 5f)) continue;

            float scale = 0.8f + (float)random.NextDouble() * 1.4f;
            CreateEnvironmentModel(
                "RockFormation_A",
                $"Exterior Rock {rocksPlaced + 1}",
                new Vector3(x, -0.25f, z),
                Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f),
                Vector3.one * scale,
                new Color(0.3f, 0.3f, 0.29f),
                parent);
            rocksPlaced++;
        }
    }

    private static bool IsExteriorClearZone(float x, float z, float margin)
    {
        bool InRect(float minX, float maxX, float minZ, float maxZ) =>
            x > minX - margin && x < maxX + margin && z > minZ - margin && z < maxZ + margin;

        if (InRect(-58f, 50f, -26f, 36f)) return true;     // lair and annexes
        if (InRect(-49f, 28f, -71f, -24f)) return true;    // courtyard and hangar driveway
        if (InRect(-7f, 7f, -130f, -70f)) return true;     // access road
        if (InRect(-14f, 14f, -130f, -118f)) return true;  // perimeter checkpoint
        return false;
    }

    private void CreateInvisibleBarrier(string barrierName, Vector3 position, Vector3 size, Transform parent)
    {
        GameObject barrier = new GameObject(barrierName);
        barrier.transform.SetParent(parent, false);
        barrier.transform.position = position;
        BoxCollider collider = barrier.AddComponent<BoxCollider>();
        collider.size = size;
    }

    private void CreateLairArchitecturalOverlays(Color accentColor)
    {
        float[] floors = { LairOperationsY, LairCommandY };
        for (int floorIndex = 0; floorIndex < floors.Length; floorIndex++)
        {
            float floorY = floors[floorIndex];
            string floorName = floorIndex == 0 ? "Operations" : "Command";
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject railing = CreateEnvironmentModel(
                    "SafetyRailing_A",
                    $"{floorName} {(side < 0 ? "West" : "East")} Architectural Railing",
                    new Vector3(side * 4.62f, floorY + 0.22f, 5.5f),
                    Quaternion.Euler(0f, 90f, 0f),
                    new Vector3(9.25f, 1f, 1f),
                    accentColor);
                DisableChildColliders(railing);
            }
        }
    }

    // Floor directories are wall plaques on the elevator shaft beside the
    // doors. The elevator header already names the floor, so the plaque just
    // lists where to go.
    private void CreateLairWayfindingDirectories()
    {
        float[] floorY = { 0f, LairOperationsY + 0.2f, LairCommandY + 0.2f };
        string[] directoryText =
        {
            "MEDICAL BAY  >  EAST\nMECH HANGAR  VIA WEST COURTYARD\nMISSION OPERATIONS  LEVEL 3",
            "ARMORY  <  WEST  >  RESEARCH LAB\nTRAINING ROOM  <  WEST",
            "CUSTOMIZATION  <  WEST  >  INTEL CENTER\nMISSION OPERATIONS  <  WEST\nRECRUIT QUARTERS  >  EAST"
        };

        for (int level = 0; level < floorY.Length; level++)
        {
            CreateWorldLabel(
                directoryText[level],
                new Vector3(-3.45f, floorY[level] + 2.1f, ElevatorShaftFrontZ - 0.1f),
                new Color(0.86f, 0.94f, 0.96f),
                0.07f);
        }
    }

    private void CreateLairDirectoryPylon(
        string objectName,
        Vector3 position,
        string floorLabel,
        string directoryText,
        Color accentColor)
    {
        GameObject column = CreateEnvironmentModel(
            "SupportColumn_A",
            objectName,
            position,
            Quaternion.identity,
            new Vector3(1.25f, 1.38f, 1.25f),
            accentColor);
        DisableChildColliders(column);

        GameObject directoryPanel = CreateEnvironmentModel(
            "WallPanel_A",
            $"{objectName} Display",
            position + new Vector3(0f, 0.65f, -0.42f),
            Quaternion.identity,
            new Vector3(0.95f, 0.82f, 1f),
            accentColor);
        DisableChildColliders(directoryPanel);

        CreateWorldLabel(
            floorLabel,
            position + new Vector3(0f, 3.62f, -0.72f),
            Color.white,
            0.12f);
        CreateWorldLabel(
            directoryText,
            position + new Vector3(0f, 2.15f, -0.72f),
            new Color(0.86f, 0.94f, 0.96f),
            0.07f);
    }

    private void CreateLairArrivalCourtyard(Color wallColor, Color platformColor, Color accentColor)
    {
        Color courtyardFloor = new Color(0.09f, 0.115f, 0.125f);
        Color pathColor = new Color(0.16f, 0.2f, 0.22f);
        const float courtyardCenterZ = -47.5f;
        const float courtyardDepth = 45f;

        CreateBlock(
            "Aerie Courtyard Floor",
            new Vector3(0f, -0.46f, courtyardCenterZ),
            new Vector3(54f, 0.8f, courtyardDepth),
            courtyardFloor);
        // The north part of the west retaining wall is left open onto the
        // mech hangar's driveway apron.
        CreateBlock(
            "Courtyard West Retaining Wall",
            new Vector3(-27f, 1.45f, -57.5f),
            new Vector3(0.55f, 3.2f, 25f),
            wallColor);
        CreateBlock(
            "Courtyard East Retaining Wall",
            new Vector3(27f, 1.45f, courtyardCenterZ),
            new Vector3(0.55f, 3.2f, courtyardDepth),
            wallColor);

        CreateBlock("Courtyard Gate West", new Vector3(-16.5f, 1.45f, -70f), new Vector3(21f, 3.2f, 0.55f), wallColor);
        CreateBlock("Courtyard Gate East", new Vector3(16.5f, 1.45f, -70f), new Vector3(21f, 3.2f, 0.55f), wallColor);
        CreateBlock("Courtyard Gate Pillar West", new Vector3(-6f, 2.6f, -70f), new Vector3(1.1f, 5.4f, 1.1f), platformColor);
        CreateBlock("Courtyard Gate Pillar East", new Vector3(6f, 2.6f, -70f), new Vector3(1.1f, 5.4f, 1.1f), platformColor);
        CreateBlock("Courtyard Gate Header", new Vector3(0f, 5.05f, -70f), new Vector3(13f, 0.8f, 1.1f), accentColor);

        CreateDecorationBlock(
            "Courtyard Main Approach",
            new Vector3(0f, -0.015f, courtyardCenterZ),
            new Vector3(8f, 0.08f, courtyardDepth - 2f),
            pathColor);
        CreateDecorationBlock(
            "Courtyard West Landing Pad",
            new Vector3(-17f, -0.005f, -45f),
            new Vector3(13f, 0.09f, 14f),
            new Color(0.12f, 0.16f, 0.18f));
        CreateDecorationBlock(
            "Courtyard East Landing Pad",
            new Vector3(17f, -0.005f, -45f),
            new Vector3(13f, 0.09f, 14f),
            new Color(0.12f, 0.16f, 0.18f));
        CreateDecorationBlock("West Landing Pad Stripe", new Vector3(-17f, 0.05f, -45f), new Vector3(0.3f, 0.06f, 11f), accentColor);
        CreateDecorationBlock("East Landing Pad Stripe", new Vector3(17f, 0.05f, -45f), new Vector3(0.3f, 0.06f, 11f), accentColor);

        CreateBlock("Aerie Entrance Canopy", new Vector3(0f, 6.2f, -28.5f), new Vector3(34f, 0.75f, 7f), platformColor);
        CreateBlock("Entrance Canopy West Support", new Vector3(-15f, 2.9f, -28.5f), new Vector3(1.2f, 6.2f, 5.5f), wallColor);
        CreateBlock("Entrance Canopy East Support", new Vector3(15f, 2.9f, -28.5f), new Vector3(1.2f, 6.2f, 5.5f), wallColor);
        CreateDecorationBlock("Entrance Canopy Light", new Vector3(0f, 5.78f, -30f), new Vector3(18f, 0.16f, 0.5f), accentColor);
        CreateDecorationBlock(
            "Facade Operations Divider",
            new Vector3(0f, LairOperationsY, -25.36f),
            new Vector3(43f, 0.18f, 0.08f),
            accentColor);
        CreateDecorationBlock(
            "Facade Command Divider",
            new Vector3(0f, LairCommandY, -25.36f),
            new Vector3(43f, 0.18f, 0.08f),
            new Color(0.32f, 0.58f, 0.66f));

        CreateFacilityProp(
            "Models/Facility/SecurityCheckpoint",
            "Courtyard Checkpoint West",
            new Vector3(-7.5f, 0f, -31.5f),
            new Vector3(0f, 16f, 0f),
            Vector3.one,
            new Vector3(2.8f, 2.3f, 1.4f),
            new Vector3(0f, 1.15f, 0f));
        CreateFacilityProp(
            "Models/Facility/SecurityCheckpoint",
            "Courtyard Checkpoint East",
            new Vector3(7.5f, 0f, -31.5f),
            new Vector3(0f, -16f, 0f),
            Vector3.one,
            new Vector3(2.8f, 2.3f, 1.4f),
            new Vector3(0f, 1.15f, 0f));
        CreateFacilityProp(
            "Models/Facility/CargoCrate",
            "Courtyard Supply Crate West",
            new Vector3(-21f, 0f, -34f),
            Vector3.zero,
            Vector3.one,
            new Vector3(1.35f, 1.3f, 1.2f),
            new Vector3(0f, 0.65f, 0f));
        CreateFacilityProp(
            "Models/Facility/CargoCrate",
            "Courtyard Supply Crate East",
            new Vector3(21f, 0f, -56f),
            new Vector3(0f, 90f, 0f),
            Vector3.one,
            new Vector3(1.35f, 1.3f, 1.2f),
            new Vector3(0f, 0.65f, 0f));

        Vector3[] courtyardLights =
        {
            new Vector3(-23f, 0f, -62f),
            new Vector3(23f, 0f, -62f),
            new Vector3(-23f, 0f, -45f),
            new Vector3(23f, 0f, -45f),
            new Vector3(-23f, 0f, -29f),
            new Vector3(23f, 0f, -29f)
        };
        for (int light = 0; light < courtyardLights.Length; light++)
        {
            CreateFacilityLamp($"Aerie Courtyard Lamp {light + 1}", courtyardLights[light]);
        }

        CreateWorldLabel("THE AERIE", new Vector3(0f, 4.35f, -69.55f), Color.white, 0.22f);
    }

    private void CreateLairReceptionHub(Color wallColor, Color platformColor, Color accentColor)
    {
        Color receptionWall = Color.Lerp(wallColor, Color.white, 0.06f);
        Color receptionCeiling = new Color(0.11f, 0.14f, 0.16f);
        const float receptionCenterZ = -14.5f;
        const float receptionDepth = 20.5f;
        const float receptionHeight = 6.5f;

        // The reception hall spans the full width of the building, wall to wall,
        // so there are no dead pockets beside it. Its sides are the lair walls.
        float hallHalfWidth = LairHalfWidth - 0.3f;
        CreateBlock(
            "Reception Ceiling",
            new Vector3(0f, receptionHeight, receptionCenterZ),
            new Vector3(hallHalfWidth * 2f, 0.4f, receptionDepth),
            receptionCeiling);

        float atriumWallWidth = hallHalfWidth - 3.5f;
        CreateBlock("Reception Atrium Wall West", new Vector3(-(3.5f + atriumWallWidth * 0.5f), 3.25f, -4f), new Vector3(atriumWallWidth, 6.5f, 0.4f), receptionWall);
        CreateBlock("Reception Atrium Wall East", new Vector3(3.5f + atriumWallWidth * 0.5f, 3.25f, -4f), new Vector3(atriumWallWidth, 6.5f, 0.4f), receptionWall);
        CreateBlock("Reception Atrium Door Header", new Vector3(0f, 5.6f, -4f), new Vector3(7f, 1.8f, 0.4f), receptionWall);
        CreateBlock("Atrium Scanner West", new Vector3(-3.85f, 2.35f, -4.2f), new Vector3(0.35f, 4.7f, 0.65f), accentColor);
        CreateBlock("Atrium Scanner East", new Vector3(3.85f, 2.35f, -4.2f), new Vector3(0.35f, 4.7f, 0.65f), accentColor);
        CreateBlock("Atrium Scanner Header", new Vector3(0f, 4.55f, -4.2f), new Vector3(8f, 0.3f, 0.65f), accentColor);

        CreateDecorationBlock(
            "Reception Floor Guide",
            new Vector3(0f, 0.045f, receptionCenterZ),
            new Vector3(1.2f, 0.09f, receptionDepth - 1f),
            accentColor);
        CreateReceptionFurnishings(platformColor, accentColor);

        CreateFacilityProp(
            "Models/Facility/ServerRack",
            "Reception Security Server West",
            new Vector3(-10.4f, 0f, -7f),
            new Vector3(0f, 90f, 0f),
            Vector3.one,
            new Vector3(1.1f, 2.8f, 1.1f),
            new Vector3(0f, 1.4f, 0f));
        CreateFacilityProp(
            "Models/Facility/ServerRack",
            "Reception Security Server East",
            new Vector3(10.4f, 0f, -7f),
            new Vector3(0f, -90f, 0f),
            Vector3.one,
            new Vector3(1.1f, 2.8f, 1.1f),
            new Vector3(0f, 1.4f, 0f));

        float[] receptionLights = { -21f, -13f, -5.5f };
        float[] receptionLightX = { -14f, 0f, 14f };
        for (int light = 0; light < receptionLights.Length; light++)
        {
            foreach (float x in receptionLightX)
            {
                CreateFacilityCeilingLight(
                    $"Reception Ceiling Light {light + 1} {x:0}",
                    new Vector3(x, 6.12f, receptionLights[light]),
                    accentColor,
                    15f,
                    null,
                    2.2f);
            }
        }

        CreateWorldLabel("RECEPTION", new Vector3(0f, 5.15f, -23.9f), Color.white, 0.16f);
        CreateWorldLabel("ATRIUM ACCESS", new Vector3(0f, 5.05f, -4.25f), Color.white, 0.13f);
    }

    // Reception hall furnishings: the front desk on its dais, a security scanner
    // line inside the entrance, waiting lounges on both sides, planters, wall
    // screens, and weapon display cases by the atrium doors.
    private void CreateReceptionFurnishings(Color platformColor, Color accentColor)
    {
        GameObject root = new GameObject("Reception Furnishings");
        root.transform.SetParent(runtimeRoot, false);
        Transform parent = root.transform;
        Color holo = new Color(0.25f, 0.9f, 0.95f);
        const float deskZ = -12.5f;

        // Front desk on a raised dais, with a lit border and a holographic emblem.
        CreateBlock("Reception Desk Dais", new Vector3(0f, 0.15f, deskZ), new Vector3(8f, 0.3f, 6f), new Color(0.23f, 0.27f, 0.29f), parent);
        for (int edge = -1; edge <= 1; edge += 2)
        {
            CreateDecorationBlock($"Reception Dais Edge Light Long {edge}", new Vector3(0f, 0.31f, deskZ + edge * 2.98f), new Vector3(8f, 0.04f, 0.06f), accentColor, parent);
            CreateDecorationBlock($"Reception Dais Edge Light Short {edge}", new Vector3(edge * 3.98f, 0.31f, deskZ), new Vector3(0.06f, 0.04f, 6f), accentColor, parent);
        }

        GameObject desk = CreateEnvironmentModel(
            "BriefingTable_A",
            "Reception Front Desk",
            new Vector3(0f, 0.3f, deskZ),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 1.28f,
            accentColor,
            parent);
        if (desk == null)
        {
            desk = CreateBlock("Reception Front Desk Fallback", new Vector3(0f, 1.2f, deskZ), new Vector3(4.8f, 1.8f, 1f), new Color(0.06f, 0.22f, 0.28f), parent);
        }

        EnsureBoxCollider(desk, new Vector3(4.7f, 1.55f, 2.5f), new Vector3(0f, 0.78f, 0f));
        CreateDecorationBlock("Reception Emblem Holo Ring", new Vector3(0f, 2.6f, deskZ), new Vector3(1.8f, 0.03f, 1.8f), holo, parent);
        CreateDecorationBlock("Reception Emblem Holo Core", new Vector3(0f, 2.3f, deskZ), new Vector3(0.05f, 0.6f, 0.05f), holo, parent);
        CreateDecorationBlock("Reception Emblem Holo Screen", new Vector3(0f, 2.75f, deskZ), new Vector3(0.9f, 0.3f, 0.04f), Color.Lerp(holo, Color.black, 0.3f), parent);
        CreateWorldLabel("RECEPTION", new Vector3(0f, 4.4f, deskZ), Color.white, 0.2f, parent);

        // Security scanner line just inside the entrance, leaving the centre walkway clear.
        for (int side = -1; side <= 1; side += 2)
        {
            CreateFacilityProp(
                "Models/Facility/SecurityCheckpoint",
                $"Reception Security Scanner {(side < 0 ? "West" : "East")}",
                new Vector3(side * 3.3f, 0f, -22.4f),
                Vector3.zero,
                Vector3.one,
                new Vector3(2.8f, 2.3f, 1.4f),
                new Vector3(0f, 1.15f, 0f),
                parent);
        }

        CreateDecorationBlock("Reception Scanner Arch Light", new Vector3(0f, 3.4f, -22.4f), new Vector3(3.6f, 0.12f, 0.12f), accentColor, parent);

        // Waiting lounges on both sides of the hall.
        for (int side = -1; side <= 1; side += 2)
        {
            string sideName = side < 0 ? "West" : "East";
            float loungeX = side * 14f;
            const float loungeZ = -16f;
            for (int row = -1; row <= 1; row += 2)
            {
                GameObject bench = CreateEnvironmentModel(
                    "FacilityBench_A",
                    $"Reception {sideName} Lounge Bench {(row < 0 ? "S" : "N")}",
                    new Vector3(loungeX, 0f, loungeZ + row * 1.9f),
                    Quaternion.Euler(0f, row < 0 ? 0f : 180f, 0f),
                    Vector3.one,
                    accentColor,
                    parent);
                EnsureBoxCollider(bench, new Vector3(3.2f, 0.9f, 0.9f), new Vector3(0f, 0.45f, 0f));
            }

            CreateBlock($"Reception {sideName} Lounge Low Table Bench", new Vector3(loungeX, 0.22f, loungeZ), new Vector3(1.8f, 0.44f, 1.1f), platformColor, parent);
            CreateDecorationBlock($"Reception {sideName} Lounge Table Screen", new Vector3(loungeX, 0.45f, loungeZ), new Vector3(1.4f, 0.02f, 0.7f), new Color(0.06f, 0.26f, 0.3f), parent);
            CreateDecorationBlock($"Reception {sideName} Lounge Floor Inset", new Vector3(loungeX, 0.02f, loungeZ), new Vector3(6f, 0.02f, 6f), new Color(0.07f, 0.09f, 0.1f), parent);

            // Planters in the corners of the hall.
            float[] planterZ = { -22.5f, -7.5f };
            foreach (float z in planterZ)
            {
                Vector3 planterPosition = new Vector3(side * 19.8f, 0f, z);
                CreateBlock($"Reception {sideName} Planter Box {z:0}", planterPosition + new Vector3(0f, 0.35f, 0f), new Vector3(1.6f, 0.7f, 1.6f), new Color(0.14f, 0.16f, 0.17f), parent);
                CreateDecorationBlock($"Reception {sideName} Planter Grass {z:0}", planterPosition + new Vector3(0f, 0.71f, 0f), new Vector3(1.45f, 0.02f, 1.45f), new Color(0.2f, 0.36f, 0.18f), parent);
                GameObject plant = CreateEnvironmentModel(
                    "PineTree_A",
                    $"Reception {sideName} Planter Plant {z:0}",
                    planterPosition + new Vector3(0f, 0.7f, 0f),
                    Quaternion.Euler(0f, z * 13f, 0f),
                    Vector3.one * 0.3f,
                    new Color(0.2f, 0.36f, 0.18f),
                    parent);
                DisableChildColliders(plant);
            }

            // Large wall screen on each side wall.
            float wallX = side * (LairHalfWidth - 0.34f);
            CreateDecorationBlock($"Reception {sideName} Wall Screen Frame", new Vector3(wallX + side * 0.02f, 3.3f, -14.5f), new Vector3(0.05f, 2.7f, 7.3f), new Color(0.1f, 0.12f, 0.13f), parent);
            CreateDecorationBlock($"Reception {sideName} Wall Screen", new Vector3(wallX - side * 0.01f, 3.3f, -14.5f), new Vector3(0.04f, 2.4f, 7f), new Color(0.06f, 0.3f, 0.36f), parent);
            CreateWorldLabel(
                side < 0 ? "THE AERIE\nSECURE OPERATIONS" : "WELCOME, AGENT\nALL CONTRACTS: LEVEL 3",
                new Vector3(wallX - side * 0.12f, 3.3f, -14.5f),
                Color.white,
                0.16f,
                parent);

            // Weapon display cases either side of the atrium doors.
            CreateFacilityProp(
                "Models/Facility/ArsenalDisplay",
                $"Reception {sideName} Display Case",
                new Vector3(side * 6.9f, 0f, -4.75f),
                new Vector3(0f, 180f, 0f),
                Vector3.one,
                new Vector3(2.7f, 2.6f, 0.8f),
                new Vector3(0f, 1.3f, 0f),
                parent);
        }
    }

    // Mission Operations: the room where the player picks a contract. Always
    // open (it isn't a buildable department), with a briefing table in the
    // middle that opens the mission directory.
    private void CreateMissionOperationsRoom()
    {
        GameFlowManager flow = GameFlowManager.Instance;
        LairRoomSpec spec = default;
        bool found = false;
        foreach (LairRoomSpec candidate in LairRoomSpecs)
        {
            if (!candidate.isMissionRoom) continue;
            spec = candidate;
            found = true;
            break;
        }

        if (!found) return;

        Vector3 position = spec.Position;
        float side = spec.side;
        float roomWidth = spec.width;
        float roomDepth = spec.depth;
        float roomHeight = spec.height;
        float doorWidth = spec.doorWidth;
        float doorHeight = spec.doorHeight;
        float entranceWallX = spec.EntranceWallX;
        float outerWallX = spec.OuterWallX;
        Color accent = new Color(0.18f, 0.82f, 0.8f);
        Color wallColor = new Color(0.2f, 0.23f, 0.25f);

        GameObject room = new GameObject("Mission Operations Room");
        room.transform.SetParent(runtimeRoot, false);
        room.transform.position = position;
        Transform parent = room.transform;

        CreateBlock("Mission Operations Finished Floor", position + new Vector3(0f, 0.02f, 0f), new Vector3(roomWidth, 0.12f, roomDepth), new Color(0.12f, 0.14f, 0.15f), parent);
        CreateBlock("Mission Operations Outer Wall", new Vector3(outerWallX, position.y + roomHeight * 0.5f, position.z), new Vector3(0.32f, roomHeight, roomDepth), wallColor, parent);
        CreateBlock("Mission Operations North Wall", position + new Vector3(0f, roomHeight * 0.5f, roomDepth * 0.5f), new Vector3(roomWidth, roomHeight, 0.32f), wallColor, parent);
        CreateBlock("Mission Operations South Wall", position + new Vector3(0f, roomHeight * 0.5f, -roomDepth * 0.5f), new Vector3(roomWidth, roomHeight, 0.32f), wallColor, parent);
        CreateVerticalDoorWall("Mission Operations Atrium Entrance", entranceWallX, position.z, roomDepth, position.y + roomHeight * 0.5f, roomHeight, position.z, doorWidth, wallColor, parent, doorHeight);
        CreateBlock("Mission Operations Finished Ceiling", position + new Vector3(0f, roomHeight - 0.05f, 0f), new Vector3(roomWidth, 0.18f, roomDepth), new Color(0.1f, 0.12f, 0.135f), parent);

        float frameX = entranceWallX - side * 0.18f;
        CreateDecorationBlock("Mission Operations Door Frame South", new Vector3(frameX, position.y + doorHeight * 0.5f, position.z - doorWidth * 0.5f), new Vector3(0.18f, doorHeight, 0.24f), accent, parent);
        CreateDecorationBlock("Mission Operations Door Frame North", new Vector3(frameX, position.y + doorHeight * 0.5f, position.z + doorWidth * 0.5f), new Vector3(0.18f, doorHeight, 0.24f), accent, parent);
        CreateDecorationBlock("Mission Operations Door Frame Header", new Vector3(frameX, position.y + doorHeight, position.z), new Vector3(0.18f, 0.24f, doorWidth), accent, parent);
        CreateDecorationBlock("Mission Operations Threshold", new Vector3(entranceWallX, position.y + 0.08f, position.z), new Vector3(1.1f, 0.06f, doorWidth - 0.35f), accent, parent);

        CreateLargeRoomStructure(RoomType.IntelCenter, position, roomWidth, roomDepth, roomHeight, entranceWallX, outerWallX, side, doorWidth, accent, parent);
        CreateRoomLightingGrid("Mission Operations", position, roomWidth, roomDepth, roomHeight, accent, parent);

        // Raised operations floor around the table.
        CreateBlock("Mission Operations Dais Floor", position + new Vector3(0f, 0.15f, 0f), new Vector3(12f, 0.3f, 9f), new Color(0.18f, 0.21f, 0.23f), parent);
        CreateDecorationBlock("Mission Operations Dais Edge Light", position + new Vector3(0f, 0.31f, -4.45f), new Vector3(12f, 0.04f, 0.08f), accent, parent);
        CreateDecorationBlock("Mission Operations Dais Edge Light North", position + new Vector3(0f, 0.31f, 4.45f), new Vector3(12f, 0.04f, 0.08f), accent, parent);

        GameObject table = CreateBriefingTable(position + new Vector3(0f, 0.3f, 0f), accent, parent);
        if (flow != null && flow.missions.Count > 0)
        {
            table.AddComponent<MissionTerminal>();
        }

        // Wall displays: a large screen on the outer wall facing the door, and
        // smaller status panels along the north and south walls.
        float screenX = outerWallX - side * 0.22f;
        CreateDecorationBlock("Mission Operations Main Screen", new Vector3(screenX, position.y + roomHeight * 0.5f, position.z), new Vector3(0.08f, roomHeight * 0.45f, roomDepth * 0.45f), new Color(0.08f, 0.32f, 0.38f), parent);
        CreateDecorationBlock("Mission Operations Main Screen Frame", new Vector3(screenX + side * 0.05f, position.y + roomHeight * 0.5f, position.z), new Vector3(0.06f, roomHeight * 0.45f + 0.3f, roomDepth * 0.45f + 0.3f), new Color(0.1f, 0.12f, 0.13f), parent);
        for (int panel = -1; panel <= 1; panel += 2)
        {
            for (int slot = 0; slot < 3; slot++)
            {
                float x = entranceWallX + side * (roomWidth * (0.3f + slot * 0.22f));
                CreateDecorationBlock(
                    $"Mission Operations Status Screen {(panel < 0 ? "S" : "N")}{slot + 1}",
                    new Vector3(x, position.y + 2.4f, position.z + panel * (roomDepth * 0.5f - 0.22f)),
                    new Vector3(3f, 1.6f, 0.06f),
                    new Color(0.06f, 0.26f, 0.3f),
                    parent);
                CreateFacilityProp(
                    "Models/Facility/ServerRack",
                    $"Mission Operations Console Rack {(panel < 0 ? "S" : "N")}{slot + 1}",
                    new Vector3(x, position.y, position.z + panel * (roomDepth * 0.5f - 1.1f)),
                    new Vector3(0f, panel < 0 ? 0f : 180f, 0f),
                    Vector3.one,
                    new Vector3(1.1f, 2.8f, 1.1f),
                    new Vector3(0f, 1.4f, 0f),
                    parent);
            }
        }

        CreateWorldLabel("MISSION OPERATIONS", new Vector3(frameX, position.y + doorHeight + 0.5f, position.z), Color.white, 0.15f, parent);
        CreateWorldLabel("MISSION OPERATIONS", new Vector3(screenX - side * 0.1f, position.y + roomHeight * 0.5f + roomHeight * 0.26f, position.z), Color.white, 0.24f, parent);
    }

    // A proper briefing table built from parts: pedestal base, thick tabletop
    // with lit edges, a glowing map surface, and a holographic projection above
    // it. The root carries the interaction collider.
    private GameObject CreateBriefingTable(Vector3 floorPosition, Color accent, Transform parent)
    {
        const float topWidth = 6.4f;
        const float topDepth = 3.6f;
        const float tableHeight = 1.05f;
        Color frameColor = new Color(0.12f, 0.14f, 0.155f);
        Color baseColor = new Color(0.09f, 0.1f, 0.11f);

        GameObject table = new GameObject("Mission Briefing Table");
        table.transform.SetParent(parent, false);
        table.transform.position = floorPosition;
        Transform t = table.transform;

        // Pedestal base: a wide plinth and two support columns.
        CreateBlock("Briefing Table Plinth", floorPosition + new Vector3(0f, 0.08f, 0f), new Vector3(topWidth - 1.4f, 0.16f, topDepth - 1f), baseColor, t);
        for (int leg = -1; leg <= 1; leg += 2)
        {
            CreateBlock($"Briefing Table Support Post {(leg < 0 ? "A" : "B")}", floorPosition + new Vector3(leg * (topWidth * 0.3f), tableHeight * 0.5f, 0f), new Vector3(0.7f, tableHeight - 0.2f, topDepth - 1.4f), frameColor, t);
        }

        // Tabletop with a lit edge and an inset map surface.
        float topY = floorPosition.y + tableHeight;
        CreateBlock("Briefing Table Top Bench", new Vector3(floorPosition.x, topY - 0.09f, floorPosition.z), new Vector3(topWidth, 0.18f, topDepth), frameColor, t);
        CreateDecorationBlock("Briefing Table Map Screen", new Vector3(floorPosition.x, topY + 0.005f, floorPosition.z), new Vector3(topWidth - 0.5f, 0.02f, topDepth - 0.5f), new Color(0.05f, 0.28f, 0.32f), t);
        for (int edge = -1; edge <= 1; edge += 2)
        {
            CreateDecorationBlock($"Briefing Table Edge Light Long {edge}", new Vector3(floorPosition.x, topY - 0.09f, floorPosition.z + edge * topDepth * 0.5f), new Vector3(topWidth + 0.02f, 0.05f, 0.04f), accent, t);
            CreateDecorationBlock($"Briefing Table Edge Light Short {edge}", new Vector3(floorPosition.x + edge * topWidth * 0.5f, topY - 0.09f, floorPosition.z), new Vector3(0.04f, 0.05f, topDepth + 0.02f), accent, t);
        }

        // Holographic projection floating over the table.
        Color holo = new Color(0.25f, 0.9f, 0.95f);
        CreateDecorationBlock("Briefing Holo Screen Ring", new Vector3(floorPosition.x, topY + 0.9f, floorPosition.z), new Vector3(2.6f, 0.03f, 1.6f), holo, t);
        CreateDecorationBlock("Briefing Holo Screen Core", new Vector3(floorPosition.x, topY + 0.55f, floorPosition.z), new Vector3(0.05f, 0.7f, 0.05f), holo, t);
        CreateDecorationBlock("Briefing Holo Screen Tower A", new Vector3(floorPosition.x - 0.7f, topY + 0.3f, floorPosition.z + 0.3f), new Vector3(0.35f, 0.6f, 0.35f), Color.Lerp(holo, Color.black, 0.35f), t);
        CreateDecorationBlock("Briefing Holo Screen Tower B", new Vector3(floorPosition.x + 0.6f, topY + 0.2f, floorPosition.z - 0.4f), new Vector3(0.5f, 0.4f, 0.4f), Color.Lerp(holo, Color.black, 0.35f), t);

        GameObject holoLightObject = new GameObject("Briefing Holo Light");
        holoLightObject.transform.SetParent(t, false);
        holoLightObject.transform.position = new Vector3(floorPosition.x, topY + 0.8f, floorPosition.z);
        Light holoLight = holoLightObject.AddComponent<Light>();
        holoLight.type = LightType.Point;
        holoLight.color = holo;
        holoLight.range = 6f;
        holoLight.intensity = 1.6f;
        holoLight.shadows = LightShadows.None;

        BoxCollider interaction = table.AddComponent<BoxCollider>();
        interaction.center = new Vector3(0f, tableHeight * 0.5f, 0f);
        interaction.size = new Vector3(topWidth, tableHeight, topDepth);

        CreateWorldLabel("SELECT CONTRACT\nE  ACCESS", new Vector3(floorPosition.x, topY + 1.45f, floorPosition.z), Color.white, 0.12f, t);
        return table;
    }

    private void CreateHubRoomBays()
    {
        foreach (LairRoomSpec spec in LairRoomSpecs)
        {
            if (spec.isRecruitQuarters || spec.isMissionRoom) continue;
            CreateHubRoomBay(spec, GetHubRoomAccentColor(spec.roomType));
        }
    }

    private static Color GetHubRoomAccentColor(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Garage: return new Color(0.48f, 0.52f, 0.56f);
            case RoomType.MedicalBay: return new Color(0.78f, 0.84f, 0.86f);
            case RoomType.Armory: return new Color(0.72f, 0.22f, 0.18f);
            case RoomType.ResearchLab: return new Color(0.16f, 0.62f, 0.78f);
            case RoomType.TrainingRoom: return new Color(0.82f, 0.58f, 0.16f);
            case RoomType.OperativeCustomization: return new Color(0.68f, 0.3f, 0.62f);
            default: return new Color(0.28f, 0.64f, 0.38f);
        }
    }

    private void CreateHubRoomBay(LairRoomSpec spec, Color accentColor)
    {
        RoomType roomType = spec.roomType;
        Vector3 position = spec.Position;
        GameObject shell = new GameObject(roomType == RoomType.Garage ? "Mech Garage Hangar" : $"{roomType} Room");
        shell.transform.SetParent(runtimeRoot, false);
        shell.transform.position = position;

        float roomWidth = spec.width;
        float roomDepth = spec.depth;
        float roomHeight = spec.height;
        float sideDirection = spec.side;
        float outerWallX = spec.OuterWallX;
        float entranceWallX = spec.EntranceWallX;
        float doorWidth = spec.doorWidth;
        float doorHeight = spec.doorHeight;
        hubRoomSpread = new Vector2(roomWidth / spec.legacySize.x, roomDepth / spec.legacySize.y);
        hubRoomHeight = roomHeight;
        hubRoomHasAtriumDoor = !spec.exteriorDoor;
        hubRoomExteriorDoorX = spec.ExteriorDoorX;
        hubRoomSouthWallZ = spec.SouthWallZ;
        Color floorColor = new Color(0.14f, 0.16f, 0.17f);
        Color wallColor = Color.Lerp(new Color(0.22f, 0.24f, 0.26f), accentColor, 0.08f);
        Color ceilingColor = new Color(0.105f, 0.125f, 0.14f);
        float wallY = position.y + roomHeight * 0.5f;

        CreateBlock(
            $"{roomType} Finished Floor",
            position + new Vector3(0f, 0.02f, 0f),
            new Vector3(roomWidth, 0.12f, roomDepth),
            floorColor,
            shell.transform);
        CreateBlock(
            $"{roomType} Outer Wall",
            new Vector3(outerWallX, wallY, position.z),
            new Vector3(0.32f, roomHeight, roomDepth),
            wallColor,
            shell.transform);
        CreateBlock(
            $"{roomType} North Wall",
            position + new Vector3(0f, roomHeight * 0.5f, roomDepth * 0.5f),
            new Vector3(roomWidth, roomHeight, 0.32f),
            wallColor,
            shell.transform);
        if (spec.exteriorDoor)
        {
            // Entered from outside: the south wall gets the big door and the
            // atrium side stays solid.
            float southZ = spec.SouthWallZ;
            float roomMinX = Mathf.Min(entranceWallX, outerWallX);
            float roomMaxX = Mathf.Max(entranceWallX, outerWallX);
            float doorMinX = spec.ExteriorDoorX - doorWidth * 0.5f;
            float doorMaxX = spec.ExteriorDoorX + doorWidth * 0.5f;
            CreateBlock($"{roomType} South Wall A", new Vector3((roomMinX + doorMinX) * 0.5f, wallY, southZ), new Vector3(doorMinX - roomMinX, roomHeight, 0.32f), wallColor, shell.transform);
            CreateBlock($"{roomType} South Wall B", new Vector3((doorMaxX + roomMaxX) * 0.5f, wallY, southZ), new Vector3(roomMaxX - doorMaxX, roomHeight, 0.32f), wallColor, shell.transform);
            CreateBlock($"{roomType} South Wall Over Door", new Vector3(spec.ExteriorDoorX, position.y + (doorHeight + roomHeight) * 0.5f, southZ), new Vector3(doorWidth, roomHeight - doorHeight, 0.32f), wallColor, shell.transform);
            CreateBlock($"{roomType} Atrium Side Wall", new Vector3(entranceWallX, wallY, position.z), new Vector3(0.32f, roomHeight, roomDepth), wallColor, shell.transform);
            CreateHangarExteriorDoor(spec, accentColor);
        }
        else
        {
            CreateBlock(
                $"{roomType} South Wall",
                position + new Vector3(0f, roomHeight * 0.5f, -roomDepth * 0.5f),
                new Vector3(roomWidth, roomHeight, 0.32f),
                wallColor,
                shell.transform);
            CreateVerticalDoorWall(
                $"{roomType} Atrium Entrance",
                entranceWallX,
                position.z,
                roomDepth,
                wallY,
                roomHeight,
                position.z,
                doorWidth,
                wallColor,
                shell.transform,
                doorHeight);
        }
        CreateBlock(
            $"{roomType} Finished Ceiling",
            position + new Vector3(0f, roomHeight - 0.05f, 0f),
            new Vector3(roomWidth, 0.18f, roomDepth),
            ceilingColor,
            shell.transform);

        // Everything that makes the room look furnished lives under the
        // constructed-contents root, which the lair manager only shows once the
        // room has been built. Unbuilt rooms are bare walls, floor, and lights.
        GameObject contentRoot = new GameObject($"{roomType} Constructed Contents");
        contentRoot.transform.SetParent(shell.transform, false);
        GameObject fitOut = new GameObject($"{roomType} Base Fit-Out");
        fitOut.transform.SetParent(contentRoot.transform, false);

        CreateHubRoomArchitecture(
            roomType,
            position,
            roomWidth,
            roomDepth,
            roomHeight,
            entranceWallX,
            outerWallX,
            sideDirection,
            doorWidth,
            doorHeight,
            accentColor,
            fitOut.transform);

        float frameX = entranceWallX - sideDirection * 0.18f;
        if (hubRoomHasAtriumDoor)
        {
            CreateDecorationBlock(
                $"{roomType} Door Frame South",
                new Vector3(frameX, position.y + doorHeight * 0.5f, position.z - doorWidth * 0.5f),
                new Vector3(0.18f, doorHeight, 0.24f),
                accentColor,
                shell.transform);
            CreateDecorationBlock(
                $"{roomType} Door Frame North",
                new Vector3(frameX, position.y + doorHeight * 0.5f, position.z + doorWidth * 0.5f),
                new Vector3(0.18f, doorHeight, 0.24f),
                accentColor,
                shell.transform);
            CreateDecorationBlock(
                $"{roomType} Door Frame Header",
                new Vector3(frameX, position.y + doorHeight, position.z),
                new Vector3(0.18f, 0.24f, doorWidth),
                accentColor,
                shell.transform);
            CreateDecorationBlock(
                $"{roomType} Door Threshold",
                new Vector3(entranceWallX, position.y + 0.08f, position.z),
                new Vector3(1.1f, 0.06f, doorWidth - 0.35f),
                accentColor,
                shell.transform);
        }

        CreateRoomWallDetails(
            roomType,
            position,
            roomWidth,
            roomDepth,
            outerWallX,
            sideDirection,
            accentColor,
            fitOut.transform);
        CreateModularRoomDressing(
            roomType,
            position,
            roomWidth,
            roomDepth,
            roomHeight,
            entranceWallX,
            outerWallX,
            sideDirection,
            doorWidth,
            accentColor,
            fitOut.transform);
        CreateLargeRoomStructure(
            roomType,
            position,
            roomWidth,
            roomDepth,
            roomHeight,
            entranceWallX,
            outerWallX,
            sideDirection,
            doorWidth,
            accentColor,
            fitOut.transform);
        CreateRoomLightingGrid(
            $"{roomType}",
            position,
            roomWidth,
            roomDepth,
            roomHeight,
            accentColor,
            shell.transform);

        GameObject[] levelVisuals = new GameObject[5];
        for (int i = 0; i < levelVisuals.Length; i++)
        {
            levelVisuals[i] = new GameObject($"{roomType} Upgrade Stage {i + 1}");
            levelVisuals[i].transform.SetParent(contentRoot.transform, false);
        }

        if (roomType == RoomType.Armory)
        {
            GameObject loadoutTerminal = CreateLairModel(
                "Models/Lair/ArmoryRack",
                "Armory Loadout Rack",
                position + Spread(new Vector3(0f, 0f, 1.15f)),
                Quaternion.identity,
                Vector3.one,
                levelVisuals[0].transform);
            AddInteractionCollider(loadoutTerminal, new Vector3(3f, 2.8f, 1f), new Vector3(0f, 1.4f, 0f));
            loadoutTerminal.AddComponent<ArmoryLoadoutTerminal>();
            CreateArmoryDisplays(position, levelVisuals);
        }
        else if (roomType == RoomType.ResearchLab)
        {
            CreateResearchStation("Tranquilizer Research", position + Spread(new Vector3(-1.8f, 0.55f, -1.1f)), ResearchTrack.Tranquilizers, accentColor, levelVisuals[0].transform);
            CreateResearchStation("Armor Research", position + Spread(new Vector3(0f, 0.55f, -1.1f)), ResearchTrack.Armor, new Color(0.78f, 0.8f, 0.84f), levelVisuals[0].transform);
            CreateResearchStation("Weapons Research", position + Spread(new Vector3(1.8f, 0.55f, -1.1f)), ResearchTrack.Weapons, new Color(0.9f, 0.42f, 0.18f), levelVisuals[0].transform);
        }
        else if (roomType == RoomType.OperativeCustomization)
        {
            GameObject customization = CreateLairModel(
                "Models/Lair/CustomizationStation",
                "Operative Customization Station",
                position + Spread(new Vector3(0f, 0f, 0.5f)),
                Quaternion.identity,
                Vector3.one,
                levelVisuals[0].transform);
            AddInteractionCollider(customization, new Vector3(3f, 3.1f, 2.2f), new Vector3(0f, 1.5f, 0f));
            customization.AddComponent<OperativeCustomizationTerminal>();
            CreateLairModel("Models/Lair/CustomizationStation", "Secondary Wardrobe", position + Spread(new Vector3(-2.15f, 0f, 1.15f)), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.8f, levelVisuals[2].transform);
        }
        else if (roomType == RoomType.Garage)
        {
            CreateGarageInterior(position, sideDirection, levelVisuals);
        }
        else if (roomType == RoomType.MedicalBay)
        {
            CreateLairModel("Models/Lair/MedicalPod", "Medical Recovery Pod", position + Spread(new Vector3(0f, 0f, 0.8f)), Quaternion.Euler(0f, 180f, 0f), Vector3.one, levelVisuals[0].transform);
            CreateLairModel("Models/Lair/MedicalPod", "Advanced Medical Pod", position + Spread(new Vector3(-1.7f, 0f, 0.5f)), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 0.85f, levelVisuals[2].transform);
        }
        else if (roomType == RoomType.IntelCenter)
        {
            CreateLairModel("Models/Lair/IntelConsole", "Intelligence Analysis Console", position + Spread(new Vector3(0f, 0f, 0.8f)), Quaternion.identity, Vector3.one, levelVisuals[0].transform);
            CreateLairModel("Models/Lair/IntelConsole", "Strategic Analysis Console", position + Spread(new Vector3(-1.6f, 0f, -0.8f)), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 0.8f, levelVisuals[2].transform);
        }
        else if (roomType == RoomType.TrainingRoom)
        {
            CreateTrainingGround(position, levelVisuals, accentColor);
        }

        CreateRoomEquipmentDressing(
            roomType,
            position,
            sideDirection,
            roomWidth,
            roomDepth,
            accentColor,
            levelVisuals);

        // The build/upgrade console stands outside the room, in the atrium
        // beside the doorway, so the room can be built before anyone goes in.
        float terminalX = entranceWallX - sideDirection * 1.2f;
        float terminalZ = position.z - doorWidth * 0.5f - 1.6f;
        float terminalYaw = sideDirection < 0f ? -90f : 90f;
        if (spec.exteriorDoor)
        {
            // Outside on the driveway, beside the door, so the room can be
            // built or upgraded before going in.
            terminalX = spec.ExteriorDoorX - sideDirection * (doorWidth * 0.5f + 2.2f);
            terminalZ = LairDepthMinZ - 1.6f;
            terminalYaw = 0f;
        }
        GameObject terminal = CreateEnvironmentModel(
            "LabConsole_A",
            $"{roomType} Upgrade Terminal",
            new Vector3(terminalX, position.y, terminalZ),
            Quaternion.Euler(0f, terminalYaw, 0f),
            Vector3.one * 0.62f,
            accentColor,
            shell.transform);
        if (terminal == null)
        {
            terminal = CreateBlock(
                $"{roomType} Upgrade Terminal Fallback",
                new Vector3(terminalX, position.y + 0.55f, terminalZ),
                new Vector3(0.55f, 1.1f, 0.7f),
                accentColor,
                shell.transform);
        }

        EnsureBoxCollider(
            terminal,
            new Vector3(1.25f, 1.5f, 1.15f),
            new Vector3(0f, 0.75f, 0f));
        BaseUpgradeTerminal upgrade = terminal.AddComponent<BaseUpgradeTerminal>();
        upgrade.roomToUpgrade = roomType;
        upgrade.lairManager = SecretLairManager.Instance;

        if (hubRoomHasAtriumDoor)
        {
            CreateWorldLabel(
                FormatRoomName(roomType),
                new Vector3(frameX, position.y + doorHeight + 0.48f, position.z),
                Color.white,
                roomType == RoomType.OperativeCustomization ? 0.12f : 0.15f,
                shell.transform);
        }
        else
        {
            CreateWorldLabel(
                FormatRoomName(roomType),
                new Vector3(spec.ExteriorDoorX, position.y + doorHeight + 0.9f, LairDepthMinZ - 0.4f),
                Color.white,
                0.3f,
                shell.transform);
        }

        SecretLairManager.Instance?.RegisterRoomVisual(roomType, contentRoot, levelVisuals);
        hubRoomSpread = Vector2.one;
        hubRoomHeight = LairRoomHeight;
        hubRoomHasAtriumDoor = true;
    }

    private void CreateHubRoomArchitecture(
        RoomType roomType,
        Vector3 position,
        float roomWidth,
        float roomDepth,
        float roomHeight,
        float entranceWallX,
        float outerWallX,
        float sideDirection,
        float doorWidth,
        float doorHeight,
        Color accentColor,
        Transform parent)
    {
        float roomYaw = sideDirection < 0f ? -90f : 90f;
        string shellAsset = roomType == RoomType.Garage
            ? "MechServiceBay_A"
            : "FacilityRoomShell_A";
        Vector3 shellScale = roomType == RoomType.Garage
            ? new Vector3(
                roomDepth / 13f,
                roomHeight / 6.2f,
                roomWidth / 10f)
            : new Vector3(
                roomDepth / 10f,
                roomHeight / 4.2f,
                roomWidth / 8f);

        GameObject modeledShell = CreateEnvironmentModel(
            shellAsset,
            $"{roomType} Authored Architectural Shell",
            position,
            Quaternion.Euler(0f, roomYaw, 0f),
            shellScale,
            accentColor,
            parent);
        DisableChildColliders(modeledShell);
        if (!hubRoomHasAtriumDoor)
        {
            // The modeled bay's end walls land on the hangar's north and south
            // walls; the south one would cover the exterior doorway. The room's
            // own walls already enclose the space, so hide both.
            HideModelParts(modeledShell, "Left Wall", "Right Wall");
        }

        if (!hubRoomHasAtriumDoor)
        {
            CreateGarageServiceShutter(roomType, position, roomDepth, roomHeight, outerWallX, sideDirection, roomYaw, accentColor, parent);
            return;
        }

        GameObject bulkhead = CreateEnvironmentModel(
            "RoomBulkhead_A",
            $"{roomType} Authored Entrance Bulkhead",
            new Vector3(
                entranceWallX - sideDirection * 0.22f,
                position.y,
                position.z),
            Quaternion.Euler(0f, roomYaw, 0f),
            new Vector3(
                Mathf.Min(roomDepth - 0.8f, doorWidth * 2.12f) / 8f,
                Mathf.Min(roomHeight, doorHeight + 1.2f) / 4.2f,
                1f),
            accentColor,
            parent);
        DisableChildColliders(bulkhead);

        float plaqueZ = position.z
            + Mathf.Min(roomDepth * 0.34f, doorWidth * 0.5f + 1.15f);
        GameObject identityPlaque = CreateEnvironmentModel(
            "WallPanel_A",
            $"{roomType} Department Identity Plaque",
            new Vector3(
                entranceWallX - sideDirection * 0.26f,
                position.y + 0.72f,
                plaqueZ),
            Quaternion.Euler(0f, roomYaw, 0f),
            new Vector3(0.48f, 0.7f, 1f),
            accentColor,
            parent);
        DisableChildColliders(identityPlaque);
        CreateWorldLabel(
            FormatRoomName(roomType),
            new Vector3(
                entranceWallX - sideDirection * 0.48f,
                position.y + 2.65f,
                plaqueZ),
            Color.white,
            roomType == RoomType.OperativeCustomization ? 0.07f : 0.09f,
            parent);

        if (roomType == RoomType.Garage)
        {
            GameObject serviceDoor = CreateEnvironmentModel(
                "GarageDoor_A",
                "Mech Hangar Rear Service Shutter",
                new Vector3(
                    outerWallX - sideDirection * 0.25f,
                    position.y,
                    position.z),
                Quaternion.Euler(0f, roomYaw, 0f),
                new Vector3(
                    Mathf.Min(roomDepth - 1.5f, 16f) / 10f,
                    Mathf.Min(roomHeight - 1f, 7f) / 5.23f,
                    1f),
                accentColor,
                parent);
            DisableChildColliders(serviceDoor);
        }
    }

    private void CreateRoomWallDetails(
        RoomType roomType,
        Vector3 position,
        float roomWidth,
        float roomDepth,
        float outerWallX,
        float sideDirection,
        Color accentColor,
        Transform parent)
    {
        float insideOuterWallX = outerWallX - sideDirection * 0.18f;
        CreateDecorationBlock(
            $"{roomType} North Wall Trim",
            position + new Vector3(0f, 1.05f, roomDepth * 0.5f - 0.18f),
            new Vector3(roomWidth - 0.5f, 0.14f, 0.08f),
            accentColor,
            parent);
        CreateDecorationBlock(
            $"{roomType} South Wall Trim",
            position + new Vector3(0f, 1.05f, -roomDepth * 0.5f + 0.18f),
            new Vector3(roomWidth - 0.5f, 0.14f, 0.08f),
            accentColor,
            parent);
        CreateDecorationBlock(
            $"{roomType} Outer Wall Trim",
            new Vector3(insideOuterWallX, position.y + 1.05f, position.z),
            new Vector3(0.08f, 0.14f, roomDepth - 0.5f),
            accentColor,
            parent);

        float panelSpacing = roomDepth * 0.27f;
        for (int panel = -1; panel <= 1; panel++)
        {
            CreateDecorationBlock(
                $"{roomType} Outer Wall Panel {panel + 2}",
                new Vector3(insideOuterWallX, position.y + 2.65f, position.z + panel * panelSpacing),
                new Vector3(0.08f, 2.65f, 0.5f),
                Color.Lerp(accentColor, Color.white, 0.22f),
                parent);
        }

        CreateDecorationBlock(
            $"{roomType} Floor Guide",
            position + new Vector3(-sideDirection * 0.25f, 0.095f, 0f),
            new Vector3(roomWidth - 1.2f, 0.04f, 0.18f),
            accentColor,
            parent);

        if (roomType != RoomType.Garage) return;

        for (int bay = -1; bay <= 1; bay++)
        {
            float bayZ = position.z + bay * 5.1f * hubRoomSpread.y;
            CreateDecorationBlock(
                $"Mech Service Berth {bay + 2} Centerline",
                new Vector3(position.x, position.y + 0.1f, bayZ),
                new Vector3(roomWidth - 1.4f, 0.045f, 0.16f),
                accentColor,
                parent);
            CreateDecorationBlock(
                $"Mech Service Berth {bay + 2} North Rail",
                new Vector3(position.x, position.y + 0.105f, bayZ + 1.45f),
                new Vector3(roomWidth - 2.1f, 0.04f, 0.1f),
                Color.Lerp(accentColor, Color.white, 0.35f),
                parent);
            CreateDecorationBlock(
                $"Mech Service Berth {bay + 2} South Rail",
                new Vector3(position.x, position.y + 0.105f, bayZ - 1.45f),
                new Vector3(roomWidth - 2.1f, 0.04f, 0.1f),
                Color.Lerp(accentColor, Color.white, 0.35f),
                parent);
        }
    }

    private void CreateModularRoomDressing(
        RoomType roomType,
        Vector3 position,
        float roomWidth,
        float roomDepth,
        float roomHeight,
        float entranceWallX,
        float outerWallX,
        float sideDirection,
        float doorWidth,
        Color accentColor,
        Transform parent)
    {
        float doorwayYaw = sideDirection < 0f ? 90f : -90f;
        if (hubRoomHasAtriumDoor) CreateEnvironmentModel(
            "DoorFrame_A",
            $"{roomType} Modeled Door Frame",
            new Vector3(entranceWallX - sideDirection * 0.2f, position.y + 0.02f, position.z),
            Quaternion.Euler(0f, doorwayYaw, 0f),
            new Vector3(doorWidth / 4f, 1.08f, 1f),
            accentColor,
            parent);

        int floorTilesX = Mathf.Clamp(Mathf.CeilToInt(roomWidth / 5f), 2, 8);
        int floorTilesZ = Mathf.Clamp(Mathf.CeilToInt(roomDepth / 5f), 2, 10);
        float tileWidth = (roomWidth - 0.65f) / floorTilesX;
        float tileDepth = (roomDepth - 0.65f) / floorTilesZ;
        for (int x = 0; x < floorTilesX; x++)
        {
            for (int z = 0; z < floorTilesZ; z++)
            {
                Vector3 tilePosition = new Vector3(
                    position.x - (roomWidth - 0.65f) * 0.5f + tileWidth * (x + 0.5f),
                    position.y + 0.08f,
                    position.z - (roomDepth - 0.65f) * 0.5f + tileDepth * (z + 0.5f));
                CreateEnvironmentModel(
                    "FloorPanel_A",
                    $"{roomType} Floor Plate {x + 1}-{z + 1}",
                    tilePosition,
                    Quaternion.identity,
                    new Vector3(tileWidth / 4f, 0.72f, tileDepth / 4f),
                    accentColor,
                    parent);
            }
        }

        int outerPanelCount = Mathf.Clamp(Mathf.RoundToInt(roomDepth / 4f), 2, 12);
        float outerPanelSpacing = (roomDepth - 0.8f) / outerPanelCount;
        for (int panel = 0; panel < outerPanelCount; panel++)
        {
            float z = position.z - (roomDepth - 0.8f) * 0.5f + outerPanelSpacing * (panel + 0.5f);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"{roomType} Outer Modeled Panel {panel + 1}",
                new Vector3(outerWallX - sideDirection * 0.2f, position.y + 0.12f, z),
                Quaternion.Euler(0f, doorwayYaw, 0f),
                new Vector3(outerPanelSpacing / 4f, 1.32f, 1f),
                accentColor,
                parent);
        }

        int endPanelCount = Mathf.Clamp(Mathf.RoundToInt(roomWidth / 4f), 2, 10);
        float endPanelSpacing = (roomWidth - 0.8f) / endPanelCount;
        for (int panel = 0; panel < endPanelCount; panel++)
        {
            float x = position.x - (roomWidth - 0.8f) * 0.5f + endPanelSpacing * (panel + 0.5f);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"{roomType} North Modeled Panel {panel + 1}",
                new Vector3(x, position.y + 0.12f, position.z + roomDepth * 0.5f - 0.2f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(endPanelSpacing / 4f, 1.32f, 1f),
                accentColor,
                parent);
            CreateEnvironmentModel(
                "WallPanel_A",
                $"{roomType} South Modeled Panel {panel + 1}",
                new Vector3(x, position.y + 0.12f, position.z - roomDepth * 0.5f + 0.2f),
                Quaternion.identity,
                new Vector3(endPanelSpacing / 4f, 1.32f, 1f),
                accentColor,
                parent);
        }

        CreateEnvironmentModel(
            "VentUnit_A",
            $"{roomType} Ventilation Unit",
            new Vector3(
                position.x + sideDirection * roomWidth * 0.22f,
                position.y + Mathf.Max(2.8f, roomHeight * 0.62f),
                position.z + roomDepth * 0.5f - 0.24f),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one,
            accentColor,
            parent);
        CreateEnvironmentModel(
            "PipeCluster_A",
            $"{roomType} Utility Pipe Cluster",
            new Vector3(
                outerWallX - sideDirection * 0.26f,
                position.y + 0.25f,
                position.z - roomDepth * 0.3f),
            Quaternion.Euler(0f, doorwayYaw, 0f),
            Vector3.one,
            accentColor,
            parent);

        if (roomType != RoomType.Garage && roomType != RoomType.TrainingRoom) return;
        if (!hubRoomHasAtriumDoor) return;

        float entrancePostZ = doorWidth * 0.5f + 0.55f;
        CreateEnvironmentModel(
            "SupportColumn_A",
            $"{roomType} Entrance Support North",
            new Vector3(
                entranceWallX - sideDirection * 0.48f,
                position.y,
                position.z + entrancePostZ),
            Quaternion.identity,
            new Vector3(1f, 1.3f, 1f),
            accentColor,
            parent);
        CreateEnvironmentModel(
            "SupportColumn_A",
            $"{roomType} Entrance Support South",
            new Vector3(
                entranceWallX - sideDirection * 0.48f,
                position.y,
                position.z - entrancePostZ),
            Quaternion.identity,
            new Vector3(1f, 1.3f, 1f),
            accentColor,
            parent);
    }

    // Evenly spaced ceiling lights sized to the room, replacing the fixed pair
    // of lights the small rooms used.
    private void CreateRoomLightingGrid(
        string roomName,
        Vector3 position,
        float roomWidth,
        float roomDepth,
        float roomHeight,
        Color accentColor,
        Transform parent)
    {
        const float targetSpacing = 11f;
        int countX = Mathf.Max(1, Mathf.RoundToInt(roomWidth / targetSpacing));
        int countZ = Mathf.Max(1, Mathf.RoundToInt(roomDepth / targetSpacing));
        float cellWidth = roomWidth / countX;
        float cellDepth = roomDepth / countZ;
        float range = Mathf.Max(10f, Mathf.Max(cellWidth, cellDepth) * 1.1f + roomHeight * 0.6f);
        int index = 0;
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                Vector3 lightPosition = new Vector3(
                    position.x - roomWidth * 0.5f + cellWidth * (x + 0.5f),
                    position.y + roomHeight - 0.38f,
                    position.z - roomDepth * 0.5f + cellDepth * (z + 0.5f));
                CreateFacilityCeilingLight(
                    $"{roomName} Interior Light {++index}",
                    lightPosition,
                    accentColor,
                    range,
                    parent);
            }
        }
    }

    // Structural dressing that keeps the larger rooms from reading as empty
    // boxes: ceiling beams, wall pilasters, and (for the hangar) an overhead
    // gantry, a mezzanine catwalk, and service equipment along the walls.
    private void CreateLargeRoomStructure(
        RoomType roomType,
        Vector3 position,
        float roomWidth,
        float roomDepth,
        float roomHeight,
        float entranceWallX,
        float outerWallX,
        float sideDirection,
        float doorWidth,
        Color accentColor,
        Transform parent)
    {
        Color beamColor = new Color(0.13f, 0.15f, 0.165f);
        Color hazardColor = new Color(0.92f, 0.68f, 0.12f);

        // Ceiling beams spanning the room width.
        int beamCount = Mathf.Max(1, Mathf.FloorToInt(roomDepth / 6f));
        for (int beam = 1; beam < beamCount; beam++)
        {
            float z = position.z - roomDepth * 0.5f + roomDepth * beam / beamCount;
            CreateDecorationBlock(
                $"{roomType} Ceiling Beam {beam}",
                new Vector3(position.x, position.y + roomHeight - 0.42f, z),
                new Vector3(roomWidth - 0.4f, 0.5f, 0.35f),
                beamColor,
                parent);
        }

        // Pilasters along the north, south, and outer walls.
        float columnScaleY = roomHeight / 3.34f;
        int endColumns = Mathf.Max(1, Mathf.FloorToInt(roomWidth / 6f));
        for (int column = 1; column < endColumns; column++)
        {
            float x = entranceWallX + sideDirection * roomWidth * column / endColumns;
            for (int end = -1; end <= 1; end += 2)
            {
                GameObject pilaster = CreateEnvironmentModel(
                    "SupportColumn_A",
                    $"{roomType} {(end < 0 ? "South" : "North")} Pilaster {column}",
                    new Vector3(x, position.y, position.z + end * (roomDepth * 0.5f - 0.55f)),
                    Quaternion.identity,
                    new Vector3(0.9f, columnScaleY, 0.9f),
                    accentColor,
                    parent);
                DisableChildColliders(pilaster);
            }
        }

        int sideColumns = Mathf.Max(1, Mathf.FloorToInt(roomDepth / 6f));
        for (int column = 1; column < sideColumns; column++)
        {
            float z = position.z - roomDepth * 0.5f + roomDepth * column / sideColumns;
            GameObject pilaster = CreateEnvironmentModel(
                "SupportColumn_A",
                $"{roomType} Outer Pilaster {column}",
                new Vector3(outerWallX - sideDirection * 0.55f, position.y, z),
                Quaternion.identity,
                new Vector3(0.9f, columnScaleY, 0.9f),
                accentColor,
                parent);
            DisableChildColliders(pilaster);
        }

        if (roomType != RoomType.Garage) return;

        float doorwayYaw = sideDirection < 0f ? 90f : -90f;

        // Overhead gantry crane: two rails running the length of the hangar and
        // a bridge beam with a hoist block.
        float railY = position.y + roomHeight - 1.3f;
        for (int rail = -1; rail <= 1; rail += 2)
        {
            CreateDecorationBlock(
                $"Hangar Gantry Rail {(rail < 0 ? "A" : "B")}",
                new Vector3(position.x + rail * roomWidth * 0.3f, railY, position.z),
                new Vector3(0.45f, 0.6f, roomDepth - 1.2f),
                beamColor,
                parent);
        }

        float bridgeZ = position.z - roomDepth * 0.12f;
        CreateDecorationBlock(
            "Hangar Gantry Bridge",
            new Vector3(position.x, railY - 0.55f, bridgeZ),
            new Vector3(roomWidth * 0.6f + 0.8f, 0.7f, 0.9f),
            hazardColor,
            parent);
        CreateDecorationBlock(
            "Hangar Gantry Hoist",
            new Vector3(position.x + sideDirection * roomWidth * 0.08f, railY - 1.3f, bridgeZ),
            new Vector3(1.1f, 0.9f, 1.1f),
            beamColor,
            parent);
        CreateDecorationBlock(
            "Hangar Gantry Cable",
            new Vector3(position.x + sideDirection * roomWidth * 0.08f, railY - 2.6f, bridgeZ),
            new Vector3(0.06f, 1.7f, 0.06f),
            new Color(0.08f, 0.08f, 0.09f),
            parent);

        // Mezzanine catwalk along the outer wall.
        float catwalkY = position.y + Mathf.Min(4.4f, roomHeight * 0.5f);
        float catwalkX = outerWallX - sideDirection * 1.35f;
        int catwalkSections = Mathf.Max(1, Mathf.FloorToInt((roomDepth - 2f) / 6f));
        float catwalkStart = position.z - catwalkSections * 3f;
        for (int section = 0; section < catwalkSections; section++)
        {
            float z = catwalkStart + section * 6f + 3f;
            // Leave the rear service shutter clear.
            if (Mathf.Abs(z - position.z) < Mathf.Min(roomDepth - 1.5f, 16f) * 0.5f + 1.5f) continue;
            GameObject catwalk = CreateEnvironmentModel(
                "CatwalkSection_A",
                $"Hangar Mezzanine Catwalk {section + 1}",
                new Vector3(catwalkX, catwalkY, z),
                Quaternion.Euler(0f, 90f, 0f),
                Vector3.one,
                accentColor,
                parent);
            DisableChildColliders(catwalk);
            GameObject railing = CreateEnvironmentModel(
                "SafetyRailing_A",
                $"Hangar Mezzanine Railing {section + 1}",
                new Vector3(catwalkX - sideDirection * 1.15f, catwalkY + 0.1f, z),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1.5f, 1f, 1f),
                accentColor,
                parent);
            DisableChildColliders(railing);
        }

        // Hazard striping across the entrance and around the service berths.
        if (hubRoomHasAtriumDoor)
        {
            CreateDecorationBlock(
                "Hangar Entrance Hazard Stripe",
                new Vector3(entranceWallX + sideDirection * 1.4f, position.y + 0.1f, position.z),
                new Vector3(0.5f, 0.04f, doorWidth),
                hazardColor,
                parent);
        }
        else
        {
            CreateDecorationBlock(
                "Hangar Exterior Door Hazard Stripe",
                new Vector3(hubRoomExteriorDoorX, position.y + 0.1f, hubRoomSouthWallZ + 1.4f),
                new Vector3(doorWidth, 0.04f, 0.5f),
                hazardColor,
                parent);
        }
        CreateDecorationBlock(
            "Hangar Berth Boundary North",
            new Vector3(position.x, position.y + 0.1f, position.z + roomDepth * 0.38f),
            new Vector3(roomWidth * 0.62f, 0.04f, 0.35f),
            hazardColor,
            parent);
        CreateDecorationBlock(
            "Hangar Berth Boundary South",
            new Vector3(position.x, position.y + 0.1f, position.z - roomDepth * 0.38f),
            new Vector3(roomWidth * 0.62f, 0.04f, 0.35f),
            hazardColor,
            parent);

        // Service equipment lining the north and south walls.
        float wallInsetZ = roomDepth * 0.5f - 1.6f;
        for (int slot = 0; slot < 4; slot++)
        {
            float x = entranceWallX + sideDirection * (4f + slot * (roomWidth - 8f) / 3f);
            CreateFacilityProp(
                slot % 2 == 0 ? "Models/Facility/CargoCrate" : "Models/Facility/Generator",
                $"Hangar North Service Equipment {slot + 1}",
                new Vector3(x, position.y, position.z + wallInsetZ),
                new Vector3(0f, 180f, 0f),
                Vector3.one,
                slot % 2 == 0 ? new Vector3(1.35f, 1.3f, 1.2f) : new Vector3(2.2f, 2.1f, 1.4f),
                slot % 2 == 0 ? new Vector3(0f, 0.65f, 0f) : new Vector3(0f, 1.05f, 0f),
                parent);
            if (!hubRoomHasAtriumDoor && Mathf.Abs(x - hubRoomExteriorDoorX) < doorWidth * 0.5f + 2.5f) continue;
            CreateFacilityProp(
                slot % 2 == 0 ? "Models/Facility/FabricationBench" : "Models/Facility/CargoCrate",
                $"Hangar South Service Equipment {slot + 1}",
                new Vector3(x, position.y, position.z - wallInsetZ),
                Vector3.zero,
                Vector3.one,
                slot % 2 == 0 ? new Vector3(2.6f, 1.5f, 1.25f) : new Vector3(1.35f, 1.3f, 1.2f),
                slot % 2 == 0 ? new Vector3(0f, 0.75f, 0f) : new Vector3(0f, 0.65f, 0f),
                parent);
        }

        GameObject toolWall = CreateEnvironmentModel(
            "WeaponRack_A",
            "Hangar Tool Wall",
            new Vector3(outerWallX - sideDirection * 0.45f, position.y, position.z + roomDepth * 0.3f),
            Quaternion.Euler(0f, doorwayYaw, 0f),
            Vector3.one * 1.2f,
            accentColor,
            parent);
        DisableChildColliders(toolWall);
    }

    private void CreateGarageServiceShutter(
        RoomType roomType,
        Vector3 position,
        float roomDepth,
        float roomHeight,
        float outerWallX,
        float sideDirection,
        float roomYaw,
        Color accentColor,
        Transform parent)
    {
        if (roomType != RoomType.Garage) return;
        GameObject serviceDoor = CreateEnvironmentModel(
            "GarageDoor_A",
            "Mech Hangar Rear Service Shutter",
            new Vector3(outerWallX - sideDirection * 0.25f, position.y, position.z),
            Quaternion.Euler(0f, roomYaw, 0f),
            new Vector3(
                Mathf.Min(roomDepth - 1.5f, 16f) / 10f,
                Mathf.Min(roomHeight - 1f, 7f) / 5.23f,
                1f),
            accentColor,
            parent);
        DisableChildColliders(serviceDoor);
    }

    // The hangar's main entrance: a large sliding door in the south facade that
    // opens onto a driveway apron off the arrival courtyard.
    private void CreateHangarExteriorDoor(LairRoomSpec spec, Color accentColor)
    {
        float doorWidth = spec.doorWidth;
        float doorHeight = spec.doorHeight;
        float doorX = spec.ExteriorDoorX;
        Color panelColor = new Color(0.2f, 0.22f, 0.24f);
        Color hazardColor = new Color(0.92f, 0.68f, 0.12f);

        SlidingSecurityDoor door = CreateSlidingSecurityDoor(
            "Mech Hangar Exterior Door",
            new Vector3(doorX, spec.floorY + doorHeight * 0.5f, LairDepthMinZ),
            Quaternion.identity,
            doorWidth,
            doorHeight,
            panelColor,
            accentColor);
        door.slideSpeed = 3.2f;
        door.closeDelay = 2.5f;
        BoxCollider trigger = door.GetComponent<BoxCollider>();
        if (trigger != null) trigger.size = new Vector3(doorWidth + 4f, doorHeight, 12f);

        // Door surround.
        for (int side = -1; side <= 1; side += 2)
        {
            CreateDecorationBlock(
                $"Hangar Door Hazard Jamb {(side < 0 ? "A" : "B")}",
                new Vector3(doorX + side * (doorWidth * 0.5f + 0.25f), spec.floorY + doorHeight * 0.5f, LairDepthMinZ - 0.34f),
                new Vector3(0.5f, doorHeight, 0.08f),
                hazardColor);
        }

        CreateDecorationBlock(
            "Hangar Door Header Light",
            new Vector3(doorX, spec.floorY + doorHeight + 0.25f, LairDepthMinZ - 0.34f),
            new Vector3(doorWidth + 1f, 0.3f, 0.08f),
            accentColor);

        CreateHangarApron(spec);
    }

    // Paved driveway from the arrival courtyard to the hangar door.
    private void CreateHangarApron(LairRoomSpec spec)
    {
        Color apronFloor = new Color(0.1f, 0.12f, 0.13f);
        Color wallColor = new Color(0.2f, 0.22f, 0.24f);
        Color hazardColor = new Color(0.92f, 0.68f, 0.12f);
        float side = spec.side;
        float courtyardEdgeX = side * 27f;
        float apronOuterX = spec.ExteriorDoorX + side * (spec.doorWidth * 0.5f + 6f);
        float minX = Mathf.Min(courtyardEdgeX, apronOuterX);
        float maxX = Mathf.Max(courtyardEdgeX, apronOuterX);
        const float apronSouthZ = -45f;
        float apronNorthZ = LairDepthMinZ - 0.3f;
        float depth = apronNorthZ - apronSouthZ;
        float centerZ = (apronNorthZ + apronSouthZ) * 0.5f;

        CreateBlock("Hangar Apron Courtyard Floor", new Vector3((minX + maxX) * 0.5f, -0.46f, centerZ), new Vector3(maxX - minX, 0.8f, depth), apronFloor);
        CreateBlock("Hangar Apron Outer Retaining Wall", new Vector3(apronOuterX, 1.45f, centerZ), new Vector3(0.55f, 3.2f, depth), wallColor);
        CreateBlock("Hangar Apron South Retaining Wall", new Vector3((minX + maxX) * 0.5f, 1.45f, apronSouthZ), new Vector3(maxX - minX, 3.2f, 0.55f), wallColor);

        CreateDecorationBlock("Hangar Apron Approach", new Vector3(spec.ExteriorDoorX, -0.015f, centerZ), new Vector3(spec.doorWidth, 0.08f, depth - 1f), new Color(0.15f, 0.17f, 0.19f));
        for (int edge = -1; edge <= 1; edge += 2)
        {
            CreateDecorationBlock(
                $"Hangar Apron Hazard Strip {(edge < 0 ? "A" : "B")}",
                new Vector3(spec.ExteriorDoorX + edge * spec.doorWidth * 0.5f, 0.03f, centerZ),
                new Vector3(0.3f, 0.06f, depth - 1f),
                hazardColor);
        }

        CreateFacilityLamp("Hangar Apron Lamp A", new Vector3(apronOuterX - side * 1.2f, 0f, apronSouthZ + 4f));
        CreateFacilityLamp("Hangar Apron Lamp B", new Vector3(apronOuterX - side * 1.2f, 0f, apronNorthZ - 3f));
    }

    private void CreateGarageInterior(Vector3 position, float sideDirection, GameObject[] levelVisuals)
    {
        CreateLairModel(
            "Models/Lair/HangarMech",
            "Scout Mech",
            position + Spread(new Vector3(sideDirection * 0.8f, 0f, 0f)),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 0.78f * HangarMechScale,
            levelVisuals[0].transform);
        CreateLairModel(
            "Models/Lair/HangarMech",
            "Assault Mech",
            position + Spread(new Vector3(sideDirection * 0.8f, 0f, 5.1f)),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 0.86f * HangarMechScale,
            levelVisuals[2].transform);
        CreateLairModel(
            "Models/Lair/HangarMech",
            "Aegis Mech",
            position + Spread(new Vector3(sideDirection * 0.8f, 0f, -5.1f)),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 0.94f * HangarMechScale,
            levelVisuals[4].transform);

        CreateFacilityProp(
            "Models/Facility/FabricationBench",
            "Garage Fabrication Bench",
            position + Spread(new Vector3(sideDirection * 5.6f, 0f, 6.7f)),
            new Vector3(0f, sideDirection < 0f ? 90f : -90f, 0f),
            Vector3.one,
            new Vector3(2.6f, 1.5f, 1.25f),
            new Vector3(0f, 0.75f, 0f),
            levelVisuals[0].transform);
        CreateFacilityProp(
            "Models/Facility/CargoCrate",
            "Garage Parts Crate",
            position + Spread(new Vector3(sideDirection * 5.7f, 0f, -6.8f)),
            new Vector3(0f, 90f, 0f),
            Vector3.one,
            new Vector3(1.35f, 1.3f, 1.2f),
            new Vector3(0f, 0.65f, 0f),
            levelVisuals[1].transform);
        CreateFacilityProp(
            "Models/Facility/Generator",
            "Garage Heavy Power Unit",
            position + Spread(new Vector3(sideDirection * 5.3f, 0f, -4.9f)),
            new Vector3(0f, sideDirection < 0f ? 90f : -90f, 0f),
            Vector3.one,
            new Vector3(2.2f, 2.1f, 1.4f),
            new Vector3(0f, 1.05f, 0f),
            levelVisuals[2].transform);
    }

    private void CreateRoomEquipmentDressing(
        RoomType roomType,
        Vector3 position,
        float sideDirection,
        float roomWidth,
        float roomDepth,
        Color accentColor,
        GameObject[] levelVisuals)
    {
        float wallFacingRotation = sideDirection < 0f ? 90f : -90f;
        Vector3 outerEquipmentPosition = position + Spread(new Vector3(sideDirection * 3.7f, 0f, 0f));

        switch (roomType)
        {
            case RoomType.Armory:
                CreateFacilityProp(
                    "Models/Facility/ArsenalDisplay",
                    "Armory Secured Weapon Wall",
                    outerEquipmentPosition,
                    new Vector3(0f, wallFacingRotation, 0f),
                    Vector3.one,
                    new Vector3(2.8f, 2.5f, 0.8f),
                    new Vector3(0f, 1.25f, 0f),
                    levelVisuals[0].transform);
                CreateFacilityProp(
                    "Models/Facility/CargoCrate",
                    "Armory Ammunition Case",
                    position + Spread(new Vector3(sideDirection * 3.6f, 0f, -3.8f)),
                    Vector3.zero,
                    Vector3.one,
                    new Vector3(1.35f, 1.3f, 1.2f),
                    new Vector3(0f, 0.65f, 0f),
                    levelVisuals[1].transform);
                break;
            case RoomType.ResearchLab:
                CreateFacilityProp(
                    "Models/Facility/ServerRack",
                    "Research Data Server",
                    position + Spread(new Vector3(sideDirection * 3.7f, 0f, -3.6f)),
                    new Vector3(0f, wallFacingRotation, 0f),
                    Vector3.one,
                    new Vector3(1.1f, 2.8f, 1.1f),
                    new Vector3(0f, 1.4f, 0f),
                    levelVisuals[0].transform);
                CreateFacilityProp(
                    "Models/Facility/LaboratoryTank",
                    "Research Specimen Tank",
                    position + Spread(new Vector3(sideDirection * 3.6f, 0f, 3.5f)),
                    Vector3.zero,
                    Vector3.one,
                    new Vector3(1.6f, 3f, 1.6f),
                    new Vector3(0f, 1.5f, 0f),
                    levelVisuals[1].transform);
                break;
            case RoomType.OperativeCustomization:
                CreateFacilityProp(
                    "Models/Facility/FabricationBench",
                    "Operative Equipment Tailoring Bench",
                    outerEquipmentPosition,
                    new Vector3(0f, wallFacingRotation, 0f),
                    Vector3.one,
                    new Vector3(2.6f, 1.5f, 1.25f),
                    new Vector3(0f, 0.75f, 0f),
                    levelVisuals[1].transform);
                break;
            case RoomType.MedicalBay:
                CreateFacilityProp(
                    "Models/Facility/ServerRack",
                    "Medical Diagnostic Server",
                    position + Spread(new Vector3(sideDirection * 3.7f, 0f, -3.4f)),
                    new Vector3(0f, wallFacingRotation, 0f),
                    Vector3.one,
                    new Vector3(1.1f, 2.8f, 1.1f),
                    new Vector3(0f, 1.4f, 0f),
                    levelVisuals[0].transform);
                CreateFacilityProp(
                    "Models/Facility/LaboratoryTank",
                    "Medical Regeneration Chamber",
                    position + Spread(new Vector3(sideDirection * 3.6f, 0f, 3.3f)),
                    Vector3.zero,
                    Vector3.one,
                    new Vector3(1.6f, 3f, 1.6f),
                    new Vector3(0f, 1.5f, 0f),
                    levelVisuals[2].transform);
                break;
            case RoomType.IntelCenter:
                CreateFacilityProp(
                    "Models/Facility/ServerRack",
                    "Intel Server Array A",
                    position + Spread(new Vector3(sideDirection * 3.7f, 0f, -2.8f)),
                    new Vector3(0f, wallFacingRotation, 0f),
                    Vector3.one,
                    new Vector3(1.1f, 2.8f, 1.1f),
                    new Vector3(0f, 1.4f, 0f),
                    levelVisuals[0].transform);
                CreateFacilityProp(
                    "Models/Facility/ServerRack",
                    "Intel Server Array B",
                    position + Spread(new Vector3(sideDirection * 3.7f, 0f, 2.8f)),
                    new Vector3(0f, wallFacingRotation, 0f),
                    Vector3.one,
                    new Vector3(1.1f, 2.8f, 1.1f),
                    new Vector3(0f, 1.4f, 0f),
                    levelVisuals[2].transform);
                break;
            case RoomType.TrainingRoom:
                CreateFacilityProp(
                    "Models/Facility/CargoCrate",
                    "Training Weighted Supply Case",
                    position + Spread(new Vector3(sideDirection * 5f, 0f, -6.1f)),
                    Vector3.zero,
                    Vector3.one,
                    new Vector3(1.35f, 1.3f, 1.2f),
                    new Vector3(0f, 0.65f, 0f),
                    levelVisuals[0].transform);
                break;
        }

        CreateRoomUpgradeInfrastructure(
            roomType,
            position,
            sideDirection,
            roomWidth,
            roomDepth,
            wallFacingRotation,
            accentColor,
            levelVisuals);
    }

    private void CreateRoomUpgradeInfrastructure(
        RoomType roomType,
        Vector3 position,
        float sideDirection,
        float roomWidth,
        float roomDepth,
        float wallFacingRotation,
        Color accentColor,
        GameObject[] levelVisuals)
    {
        float sideInset = Mathf.Min(roomWidth * 0.32f, (roomType == RoomType.Garage ? 5.2f : 3.8f) * hubRoomSpread.x);
        float depthInset = Mathf.Min(roomDepth * 0.3f, (roomType == RoomType.Garage ? 5.8f : 3.6f) * hubRoomSpread.y);
        float equipmentX = position.x + sideDirection * sideInset;

        CreateRoomUpgradeProp(
            "SupplyCrate_A",
            $"{roomType} Stage 2 Secured Supplies",
            new Vector3(equipmentX, position.y, position.z - depthInset),
            Quaternion.Euler(0f, wallFacingRotation, 0f),
            roomType == RoomType.Garage ? Vector3.one * 1.15f : Vector3.one * 0.88f,
            new Vector3(1.5f, 1.25f, 1.35f),
            new Vector3(0f, 0.62f, 0f),
            accentColor,
            levelVisuals[1].transform);

        string stageThreeAsset;
        string stageFourAsset;
        switch (roomType)
        {
            case RoomType.Armory:
                stageThreeAsset = "WeaponRack_A";
                stageFourAsset = "LabConsole_A";
                break;
            case RoomType.ResearchLab:
                stageThreeAsset = "ServerRack_A";
                stageFourAsset = "CryoCore_A";
                break;
            case RoomType.OperativeCustomization:
                stageThreeAsset = "LockerBank_A";
                stageFourAsset = "OfficeDesk_A";
                break;
            case RoomType.Garage:
                stageThreeAsset = "PipeCluster_A";
                stageFourAsset = "VentUnit_A";
                break;
            case RoomType.MedicalBay:
                stageThreeAsset = "MedicalStation_A";
                stageFourAsset = "CryoCore_A";
                break;
            case RoomType.IntelCenter:
                stageThreeAsset = "ServerRack_A";
                stageFourAsset = "BriefingTable_A";
                break;
            default:
                stageThreeAsset = "TrainingTarget_A";
                stageFourAsset = "WeaponRack_A";
                break;
        }

        CreateRoomUpgradeProp(
            stageThreeAsset,
            $"{roomType} Stage 3 Specialty Hardware",
            new Vector3(equipmentX, position.y, position.z + depthInset),
            Quaternion.Euler(0f, wallFacingRotation, 0f),
            roomType == RoomType.Garage ? Vector3.one * 1.2f : Vector3.one,
            new Vector3(2.2f, 2.75f, 1.4f),
            new Vector3(0f, 1.35f, 0f),
            accentColor,
            levelVisuals[2].transform);

        CreateRoomUpgradeProp(
            stageFourAsset,
            $"{roomType} Stage 4 Advanced System",
            new Vector3(
                position.x - sideDirection * Mathf.Min(roomWidth * 0.2f, 2.6f * hubRoomSpread.x),
                position.y,
                position.z + depthInset),
            Quaternion.Euler(0f, -wallFacingRotation, 0f),
            roomType == RoomType.Garage ? Vector3.one * 1.25f : Vector3.one,
            new Vector3(2.65f, 2.6f, 1.65f),
            new Vector3(0f, 1.28f, 0f),
            accentColor,
            levelVisuals[3].transform);

        GameObject finalWallSystem = CreateEnvironmentModel(
            "WallPanel_A",
            $"{roomType} Stage 5 Integrated Wall System",
            new Vector3(
                position.x + sideDirection * (roomWidth * 0.5f - 0.28f),
                position.y + 0.38f,
                position.z),
            Quaternion.Euler(0f, wallFacingRotation, 0f),
            new Vector3(
                Mathf.Clamp(roomDepth / 8f, 0.9f, 1.75f),
                1.35f,
                1f),
            accentColor,
            levelVisuals[4].transform);
        DisableChildColliders(finalWallSystem);
        CreateFacilityCeilingLight(
            $"{roomType} Stage 5 High Output Lighting",
            position + new Vector3(0f, hubRoomHeight - 0.34f, 0f),
            Color.Lerp(accentColor, Color.white, 0.18f),
            Mathf.Max(roomType == RoomType.Garage || roomType == RoomType.TrainingRoom ? 15f : 11f, hubRoomHeight * 2.2f),
            levelVisuals[4].transform);
    }

    private void CreateRoomUpgradeProp(
        string assetName,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Vector3 colliderSize,
        Vector3 colliderCenter,
        Color accentColor,
        Transform parent)
    {
        GameObject prop = CreateEnvironmentModel(
            assetName,
            objectName,
            position,
            rotation,
            scale,
            accentColor,
            parent);
        EnsureBoxCollider(prop, colliderSize, colliderCenter);
    }

    private static LairRoomSpec GetRecruitQuartersSpec()
    {
        foreach (LairRoomSpec spec in LairRoomSpecs)
        {
            if (spec.isRecruitQuarters) return spec;
        }

        return Room(RoomType.IntelCenter, 1f, LairCommandY + 0.2f, 17f, 24f, 28f, 7.5f, 4.4f, 4f, 11f, 13f, true);
    }

    private void CreateRecruitQuarters()
    {
        LairRoomSpec spec = GetRecruitQuartersSpec();
        Vector3 position = spec.Position;
        Color accent = new Color(0.22f, 0.55f, 0.62f);
        float roomWidth = spec.width;
        float roomDepth = spec.depth;
        float roomHeight = spec.height;
        float doorWidth = spec.doorWidth;
        float doorHeight = spec.doorHeight;
        float side = spec.side;
        float entranceWallX = spec.EntranceWallX;
        float outerWallX = spec.OuterWallX;
        Color wallColor = new Color(0.22f, 0.25f, 0.27f);
        hubRoomSpread = new Vector2(roomWidth / spec.legacySize.x, roomDepth / spec.legacySize.y);

        GameObject quarters = new GameObject("Recruit Quarters Room");
        quarters.transform.SetParent(runtimeRoot, false);
        quarters.transform.position = position;

        CreateBlock("Recruit Quarters Finished Floor", position + new Vector3(0f, 0.02f, 0f), new Vector3(roomWidth, 0.12f, roomDepth), new Color(0.15f, 0.17f, 0.18f), quarters.transform);
        CreateBlock("Recruit Quarters Outer Wall", new Vector3(outerWallX, position.y + roomHeight * 0.5f, position.z), new Vector3(0.32f, roomHeight, roomDepth), wallColor, quarters.transform);
        CreateBlock("Recruit Quarters North Wall", position + new Vector3(0f, roomHeight * 0.5f, roomDepth * 0.5f), new Vector3(roomWidth, roomHeight, 0.32f), wallColor, quarters.transform);
        CreateBlock("Recruit Quarters South Wall", position + new Vector3(0f, roomHeight * 0.5f, -roomDepth * 0.5f), new Vector3(roomWidth, roomHeight, 0.32f), wallColor, quarters.transform);
        CreateVerticalDoorWall(
            "Recruit Quarters Atrium Entrance",
            entranceWallX,
            position.z,
            roomDepth,
            position.y + roomHeight * 0.5f,
            roomHeight,
            position.z,
            doorWidth,
            wallColor,
            quarters.transform,
            doorHeight);
        CreateBlock("Recruit Quarters Finished Ceiling", position + new Vector3(0f, roomHeight - 0.05f, 0f), new Vector3(roomWidth, 0.18f, roomDepth), new Color(0.105f, 0.125f, 0.14f), quarters.transform);

        float frameX = entranceWallX - side * 0.18f;
        CreateDecorationBlock("Recruit Quarters Door Frame South", new Vector3(frameX, position.y + doorHeight * 0.5f, position.z - doorWidth * 0.5f), new Vector3(0.18f, doorHeight, 0.24f), accent, quarters.transform);
        CreateDecorationBlock("Recruit Quarters Door Frame North", new Vector3(frameX, position.y + doorHeight * 0.5f, position.z + doorWidth * 0.5f), new Vector3(0.18f, doorHeight, 0.24f), accent, quarters.transform);
        CreateDecorationBlock("Recruit Quarters Door Frame Header", new Vector3(frameX, position.y + doorHeight, position.z), new Vector3(0.18f, 0.24f, doorWidth), accent, quarters.transform);
        CreateDecorationBlock("Recruit Quarters Threshold", new Vector3(entranceWallX, position.y + 0.08f, position.z), new Vector3(1.1f, 0.06f, doorWidth - 0.35f), accent, quarters.transform);
        CreateDecorationBlock("Recruit Quarters North Wall Trim", position + new Vector3(0f, 1.05f, roomDepth * 0.5f - 0.18f), new Vector3(roomWidth - 0.5f, 0.14f, 0.08f), accent, quarters.transform);
        CreateDecorationBlock("Recruit Quarters South Wall Trim", position + new Vector3(0f, 1.05f, -roomDepth * 0.5f + 0.18f), new Vector3(roomWidth - 0.5f, 0.14f, 0.08f), accent, quarters.transform);

        // Two rows of bunks along the north and south walls, with a lounge in the middle.
        int bunksPerRow = Mathf.Clamp(Mathf.FloorToInt((roomWidth - 4f) / 3f), 3, 7);
        float bunkSpacing = (roomWidth - 4f) / bunksPerRow;
        for (int row = -1; row <= 1; row += 2)
        {
            for (int bunk = 0; bunk < bunksPerRow; bunk++)
            {
                float x = entranceWallX + side * (3f + bunkSpacing * (bunk + 0.5f));
                CreateLairModel(
                    "Models/Lair/RecruitBunk",
                    $"Recruit Bunk {(row < 0 ? "S" : "N")}{bunk + 1}",
                    new Vector3(x, position.y, position.z + row * (roomDepth * 0.5f - 1.6f)),
                    Quaternion.Euler(0f, row > 0 ? 180f : 0f, 0f),
                    Vector3.one,
                    quarters.transform);
            }
        }

        CreateBlock("Recruit Quarters Lounge Bench A", position + new Vector3(side * 2f, 0.45f, -2.2f), new Vector3(3.4f, 0.9f, 1.1f), new Color(0.16f, 0.2f, 0.22f), quarters.transform);
        CreateBlock("Recruit Quarters Lounge Bench B", position + new Vector3(side * 2f, 0.45f, 2.2f), new Vector3(3.4f, 0.9f, 1.1f), new Color(0.16f, 0.2f, 0.22f), quarters.transform);
        CreateEnvironmentModel("BriefingTable_A", "Recruit Quarters Lounge Table", position + new Vector3(side * 2f, 0f, 0f), Quaternion.identity, Vector3.one * 0.8f, accent, quarters.transform);
        CreateEnvironmentModel("LockerBank_A", "Recruit Quarters Lockers", new Vector3(outerWallX - side * 0.6f, position.y, position.z), Quaternion.Euler(0f, side < 0f ? 90f : -90f, 0f), Vector3.one, accent, quarters.transform);

        CreateLargeRoomStructure(RoomType.IntelCenter, position, roomWidth, roomDepth, roomHeight, entranceWallX, outerWallX, side, doorWidth, accent, quarters.transform);
        CreateRoomLightingGrid("Recruit Quarters", position, roomWidth, roomDepth, roomHeight, accent, quarters.transform);
        CreateWorldLabel("RECRUIT QUARTERS", new Vector3(frameX, position.y + doorHeight + 0.48f, position.z), Color.white, 0.14f, quarters.transform);
        hubRoomSpread = Vector2.one;
    }

    private void CreateHubRecruitResidents()
    {
        RecruitRoster roster = RecruitRoster.Instance;
        if (roster == null || roster.recruits.Count == 0) return;

        LairRoomSpec quarters = GetRecruitQuartersSpec();
        Vector3 lounge = quarters.Position + new Vector3(quarters.side * 2f, 0f, 0f);

        for (int i = 0; i < roster.recruits.Count; i++)
        {
            RecruitedOperativeData recruit = roster.recruits[i];
            GameObject resident = new GameObject($"Hub Recruit - {recruit.displayName}");
            resident.transform.SetParent(runtimeRoot, false);
            resident.transform.position = new Vector3(
                lounge.x - 3.4f + i % 5 * 1.7f,
                quarters.floorY + 0.05f,
                lounge.z - 4.5f + i / 5 * 1.8f);

            GameObject visual = TryCreateCharacterVisual(resident.transform, recruit.visualResourcePath);
            if (visual == null)
            {
                CreateFallbackEnemyVisual(resident.transform, GetSpecializationColor(recruit.specialization));
            }

            HubRecruitAgent agent = resident.AddComponent<HubRecruitAgent>();
            agent.wanderRadius = 4f;
            CreateWorldLabel(recruit.displayName.ToUpperInvariant(), resident.transform.position + Vector3.up * 2.35f, Color.white, 0.11f, resident.transform, true);
        }
    }

    private static Color GetSpecializationColor(OperativeSpecialization specialization)
    {
        switch (specialization)
        {
            case OperativeSpecialization.Stealth: return new Color(0.45f, 0.2f, 0.72f);
            case OperativeSpecialization.Tank: return new Color(0.72f, 0.28f, 0.18f);
            case OperativeSpecialization.Marksman: return new Color(0.18f, 0.5f, 0.68f);
            case OperativeSpecialization.Hacker: return new Color(0.85f, 0.68f, 0.18f);
            case OperativeSpecialization.Medic: return new Color(0.75f, 0.8f, 0.82f);
            default: return new Color(0.75f, 0.15f, 0.2f);
        }
    }

    private GameObject CreateBlock(string blockName, Vector3 position, Vector3 scale, Color color, Transform parent = null)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = blockName;
        block.transform.position = position;
        block.transform.localScale = scale;
        block.transform.SetParent(parent != null ? parent : runtimeRoot, true);
        PrototypeEnvironmentArt.StyleBlock(block, blockName, scale, color);
        return block;
    }

    private void CreateWorldLabel(string text, Vector3 position, Color color, float characterSize = 0.18f, Transform parent = null, bool floatingTag = false)
    {
        GameObject labelObject = new GameObject($"{text} Label");
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.identity;
        labelObject.transform.SetParent(parent != null ? parent : runtimeRoot, true);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        ConfigureWorldText(label, text, color, characterSize);

        if (floatingTag)
        {
            // Name tags over characters keep floating and turning toward the camera.
            labelObject.AddComponent<WorldSpaceLabelBillboard>();
            return;
        }

        MountWorldLabel(labelObject, label, characterSize);
    }

    private static Font worldTextFont;
    private static Material worldTextMaterial;

    // TextMesh with a depth-tested material so sign text sits on its plate
    // instead of drawing through walls and through the back of the sign.
    // A larger font size with a smaller character size keeps it sharp.
    private static void ConfigureWorldText(TextMesh textMesh, string text, Color color, float characterSize)
    {
        if (worldTextFont == null)
        {
            worldTextFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font.textureRebuilt += OnWorldTextFontRebuilt;
        }

        if (worldTextMaterial == null && worldTextFont != null)
        {
            Shader shader = Resources.Load<Shader>("Shaders/WorldText");
            if (shader != null)
            {
                worldTextMaterial = new Material(shader) { name = "World Text Material" };
                worldTextMaterial.mainTexture = worldTextFont.material.mainTexture;
            }
        }

        textMesh.text = text;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 96;
        textMesh.characterSize = characterSize * 0.1f;
        if (worldTextFont != null) textMesh.font = worldTextFont;

        MeshRenderer textRenderer = textMesh.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sharedMaterial = worldTextMaterial != null
                ? worldTextMaterial
                : worldTextFont != null ? worldTextFont.material : textRenderer.sharedMaterial;
        }
    }

    private static void OnWorldTextFontRebuilt(Font font)
    {
        if (font == worldTextFont && worldTextMaterial != null)
        {
            worldTextMaterial.mainTexture = font.material.mainTexture;
        }
    }

    // Turns a label into a physical sign: mounted flat on a nearby wall with a
    // backing plate, or, when nothing suitable is close, a two-sided display
    // panel on a post down to whatever is beneath it.
    private void MountWorldLabel(GameObject labelObject, TextMesh label, float characterSize)
    {
        Vector3 position = labelObject.transform.position;
        Vector2 textSize = MeasureLabel(labelObject, label, characterSize);
        float plateWidth = textSize.x + 0.3f;
        float plateHeight = textSize.y + 0.2f;
        Color plateColor = new Color(0.045f, 0.06f, 0.07f);
        Color stripColor = new Color(0.16f, 0.76f, 0.78f);

        Physics.SyncTransforms();

        RaycastHit wallHit = default;
        bool foundWall = false;
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (Vector3 direction in directions)
        {
            if (!Physics.Raycast(position, direction, out RaycastHit hit, 1.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (Mathf.Abs(hit.normal.y) > 0.3f) continue;

            Vector3 surfaceSize = hit.collider.bounds.size;
            float surfaceSpan = Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.z) ? surfaceSize.z : surfaceSize.x;
            if (surfaceSpan < plateWidth * 0.8f || surfaceSize.y < plateHeight * 0.8f) continue;

            if (!foundWall || hit.distance < wallHit.distance)
            {
                wallHit = hit;
                foundWall = true;
            }
        }

        if (foundWall)
        {
            Vector3 normal = new Vector3(wallHit.normal.x, 0f, wallHit.normal.z).normalized;
            labelObject.transform.position = new Vector3(wallHit.point.x, position.y, wallHit.point.z) + normal * 0.07f;
            labelObject.transform.rotation = Quaternion.LookRotation(-normal, Vector3.up);
            CreateSignPart(labelObject, "Signage Post Plate", new Vector3(0f, 0f, 0.035f), new Vector3(plateWidth, plateHeight, 0.04f), plateColor);
            CreateSignPart(labelObject, "Signage Accent Strip", new Vector3(0f, -plateHeight * 0.5f + 0.02f, 0.01f), new Vector3(plateWidth, 0.03f, 0.03f), stripColor);
            return;
        }

        // Free-standing: readable from both the south and north sides.
        CreateSignPart(labelObject, "Signage Post Plate", new Vector3(0f, 0f, 0.035f), new Vector3(plateWidth, plateHeight, 0.04f), plateColor);
        CreateSignPart(labelObject, "Signage Accent Strip", new Vector3(0f, -plateHeight * 0.5f + 0.02f, 0.035f), new Vector3(plateWidth + 0.02f, 0.03f, 0.06f), stripColor);

        GameObject backText = new GameObject("Back Text");
        backText.transform.SetParent(labelObject.transform, false);
        backText.transform.localPosition = new Vector3(0f, 0f, 0.07f);
        backText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMesh backLabel = backText.AddComponent<TextMesh>();
        ConfigureWorldText(backLabel, label.text, label.color, characterSize);

        Vector3 plateBottom = position + Vector3.down * (plateHeight * 0.5f);
        Vector3 plateTop = position + Vector3.up * (plateHeight * 0.5f);
        bool hasFloor = Physics.Raycast(plateBottom, Vector3.down, out RaycastHit floorHit, 6f, ~0, QueryTriggerInteraction.Ignore);
        bool hasCeiling = Physics.Raycast(plateTop, Vector3.up, out RaycastHit ceilingHit, 3f, ~0, QueryTriggerInteraction.Ignore);

        // Signs high off the floor under a ceiling hang from it on two rods.
        if (hasCeiling && (!hasFloor || floorHit.distance > 2f))
        {
            float rodLength = ceilingHit.distance;
            for (int rod = -1; rod <= 1; rod += 2)
            {
                CreateSignPart(
                    labelObject,
                    "Signage Hanger Post",
                    new Vector3(rod * Mathf.Max(0.1f, plateWidth * 0.5f - 0.2f), plateHeight * 0.5f + rodLength * 0.5f, 0.035f),
                    new Vector3(0.04f, rodLength, 0.04f),
                    new Color(0.12f, 0.14f, 0.15f));
            }

            return;
        }

        if (hasFloor && floorHit.distance > 0.25f)
        {
            float postLength = floorHit.distance;
            CreateSignPart(
                labelObject,
                "Signage Post",
                new Vector3(0f, -plateHeight * 0.5f - postLength * 0.5f, 0.035f),
                new Vector3(0.09f, postLength, 0.09f),
                new Color(0.12f, 0.14f, 0.15f));
        }
    }

    private void CreateSignPart(GameObject labelObject, string partName, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        Collider partCollider = part.GetComponent<Collider>();
        if (partCollider != null)
        {
            // Disable first so later signs built this frame don't raycast against it.
            partCollider.enabled = false;
            Destroy(partCollider);
        }
        part.transform.SetParent(labelObject.transform, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = localScale;
        PrototypeEnvironmentArt.StyleBlock(part, partName, localScale, color);
    }

    private static Vector2 MeasureLabel(GameObject labelObject, TextMesh label, float characterSize)
    {
        MeshRenderer textRenderer = labelObject.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            Vector3 size = textRenderer.bounds.size;
            if (size.x > 0.01f && size.y > 0.01f) return new Vector2(size.x, size.y);
        }

        // Fallback estimate if the text mesh has not been generated yet.
        string[] lines = label.text.Split('\n');
        int longest = 1;
        foreach (string line in lines) longest = Mathf.Max(longest, line.Length);
        float lineHeight = characterSize * 0.96f;
        return new Vector2(longest * lineHeight * 0.55f, lines.Length * lineHeight);
    }

    private static string FormatRoomName(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.ResearchLab: return "RESEARCH LAB";
            case RoomType.OperativeCustomization: return "OPERATIVE CUSTOMIZATION";
            case RoomType.TrainingRoom: return "TRAINING ROOM";
            case RoomType.IntelCenter: return "INTEL CENTER";
            case RoomType.MedicalBay: return "MEDICAL BAY";
            case RoomType.Garage: return "MECH HANGAR";
            default: return roomType.ToString().ToUpperInvariant();
        }
    }

    private const float ElevatorDoorHalfWidth = 2f;
    private const float ElevatorDoorHeight = 4.1f;
    private const float ElevatorCarCenterZ = 32f;

    // Hollow shaft, a car that travels between floors, sliding landing doors,
    // call buttons on every landing, and floor buttons inside the car.
    private void CreateLairElevator(Color shaftColor)
    {
        float[] stopY = { 0f, LairOperationsY + 0.2f, LairCommandY + 0.2f };
        string[] stopNames = { "Ground Level", "Operations Level", "Command Level" };
        string[] stationLabels = { "GROUND // LOGISTICS", "LEVEL 2 // OPERATIONS", "LEVEL 3 // COMMAND" };
        string[] buttonLabels = { "G  LOGISTICS", "2  OPERATIONS", "3  COMMAND" };
        Color elevatorAccent = new Color(0.14f, 0.76f, 0.78f);
        Color doorColor = new Color(0.11f, 0.15f, 0.17f);
        const float wallBottom = -0.5f;
        float wallTop = LairRoofY;
        float wallHeight = wallTop - wallBottom;
        float wallCenterY = (wallBottom + wallTop) * 0.5f;
        float shaftBackZ = LairDepthMaxZ - 0.3f;
        float shaftDepth = shaftBackZ - ElevatorShaftFrontZ;
        float frontWallZ = ElevatorShaftFrontZ + 0.15f;

        // Shaft side walls (the lair's north wall closes the back).
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBlock(
                $"Elevator Shaft {(side < 0 ? "West" : "East")} Wall",
                new Vector3(side * (ElevatorShaftHalfWidth + 0.15f), wallCenterY, ElevatorShaftFrontZ + shaftDepth * 0.5f),
                new Vector3(0.3f, wallHeight, shaftDepth),
                shaftColor);
        }

        // Front wall: full-height jambs either side of the doorways (the landing
        // doors slide into them), with a lintel between each pair of doorways.
        float jambWidth = AtriumHalfWidth - ElevatorDoorHalfWidth;
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBlock(
                $"Elevator Shaft Front {(side < 0 ? "West" : "East")} Jamb Wall",
                new Vector3(side * (ElevatorDoorHalfWidth + jambWidth * 0.5f), wallCenterY, frontWallZ),
                new Vector3(jambWidth, wallHeight, 0.3f),
                shaftColor);
        }

        float lintelBottom = wallBottom;
        for (int stop = 0; stop <= stopY.Length; stop++)
        {
            float lintelTop = stop < stopY.Length ? stopY[stop] - 0.05f : wallTop;
            if (lintelTop - lintelBottom > 0.05f)
            {
                CreateBlock(
                    $"Elevator Shaft Front Lintel Wall {stop + 1}",
                    new Vector3(0f, (lintelBottom + lintelTop) * 0.5f, frontWallZ),
                    new Vector3(ElevatorDoorHalfWidth * 2f, lintelTop - lintelBottom, 0.3f),
                    shaftColor);
            }

            if (stop < stopY.Length) lintelBottom = stopY[stop] + ElevatorDoorHeight;
        }

        // The car.
        GameObject car = new GameObject("Elevator Car");
        car.transform.SetParent(runtimeRoot, false);
        car.transform.position = new Vector3(0f, stopY[0], ElevatorCarCenterZ);
        CreateElevatorCarShell(car.transform, elevatorAccent);

        LairElevator elevator = car.AddComponent<LairElevator>();

        // Floor buttons on a panel on the car's east wall: a dark plate with a
        // small lit button per floor and the floor name beside it.
        Vector3 panelCenter = car.transform.position + new Vector3(2.27f, 1.55f, -1.45f);
        CreateBlock("Elevator Car Button Panel Plate", panelCenter, new Vector3(0.04f, 1.3f, 1.2f), new Color(0.07f, 0.08f, 0.09f), car.transform);
        for (int stop = 0; stop < stopY.Length; stop++)
        {
            Vector3 buttonPosition = car.transform.position + new Vector3(2.22f, 1.15f + stop * 0.4f, -1.85f);
            GameObject button = CreateBlock(
                $"Elevator Car Floor Button {stop + 1}",
                buttonPosition,
                new Vector3(0.06f, 0.16f, 0.16f),
                new Color(0.12f, 0.55f, 0.58f),
                car.transform);
            LairElevatorButton carButton = button.AddComponent<LairElevatorButton>();
            carButton.elevator = elevator;
            carButton.stopIndex = stop;
            CreateWorldLabel(buttonLabels[stop], buttonPosition + new Vector3(0f, 0f, 0.55f), Color.white, 0.055f, car.transform);
        }

        // Landings.
        for (int stop = 0; stop < stopY.Length; stop++)
        {
            float floorY = stopY[stop];
            CreateBlock(
                $"Elevator Header - {stationLabels[stop]}",
                new Vector3(0f, floorY + ElevatorDoorHeight + 0.35f, ElevatorShaftFrontZ - 0.12f),
                new Vector3(5.8f, 0.26f, 0.32f),
                elevatorAccent);
            CreateBlock(
                $"Elevator Threshold - {stationLabels[stop]}",
                new Vector3(0f, floorY - 0.04f, ElevatorShaftFrontZ - 1.2f),
                new Vector3(5f, 0.16f, 2.3f),
                new Color(0.18f, 0.22f, 0.24f));

            GameObject modeledFront = CreateEnvironmentModel(
                "ElevatorFront_A",
                $"Modeled Elevator Front - {stationLabels[stop]}",
                new Vector3(0f, floorY, ElevatorShaftFrontZ - 0.38f),
                Quaternion.identity,
                new Vector3(1.08f, 1.2f, 1f),
                elevatorAccent);
            DisableChildColliders(modeledFront);
            HideModelParts(modeledFront, "Left Door", "Right Door", "Door Seam");

            // Sliding landing doors set inside the front wall.
            GameObject leftDoor = CreateBlock(
                $"Elevator Landing Door Left - {stationLabels[stop]}",
                new Vector3(-ElevatorDoorHalfWidth * 0.5f, floorY + ElevatorDoorHeight * 0.5f, frontWallZ),
                new Vector3(ElevatorDoorHalfWidth, ElevatorDoorHeight, 0.12f),
                doorColor);
            GameObject rightDoor = CreateBlock(
                $"Elevator Landing Door Right - {stationLabels[stop]}",
                new Vector3(ElevatorDoorHalfWidth * 0.5f, floorY + ElevatorDoorHeight * 0.5f, frontWallZ),
                new Vector3(ElevatorDoorHalfWidth, ElevatorDoorHeight, 0.12f),
                doorColor);
            elevator.AddLanding(stopNames[stop], floorY, leftDoor.transform, rightDoor.transform);

            Vector3 callPosition = new Vector3(2.85f, floorY + 1.45f, ElevatorShaftFrontZ - 0.08f);
            GameObject callButton = CreateBlock(
                $"Elevator Call Button Screen - {stationLabels[stop]}",
                callPosition,
                new Vector3(0.35f, 0.45f, 0.12f),
                elevatorAccent);
            LairElevatorButton call = callButton.AddComponent<LairElevatorButton>();
            call.elevator = elevator;
            call.stopIndex = stop;
            call.isCallButton = true;
            CreateWorldLabel("CALL", callPosition + new Vector3(0f, 0.45f, -0.1f), Color.white, 0.07f);

            CreateWorldLabel(
                stationLabels[stop],
                new Vector3(0f, floorY + ElevatorDoorHeight + 0.85f, ElevatorShaftFrontZ - 0.1f),
                Color.white,
                0.12f);
        }
    }

    private void CreateElevatorCarShell(Transform car, Color accentColor)
    {
        Vector3 origin = car.position;
        Color carWall = new Color(0.16f, 0.19f, 0.21f);
        const float halfWidth = 2.45f;
        const float halfDepth = 2.55f;
        const float height = 4.35f;
        const float wall = 0.15f;

        CreateBlock("Elevator Car Floor", origin + new Vector3(0f, -0.125f, 0f), new Vector3(halfWidth * 2f, 0.25f, halfDepth * 2f), new Color(0.2f, 0.23f, 0.25f), car);
        CreateBlock("Elevator Car Roof", origin + new Vector3(0f, height + wall * 0.5f, 0f), new Vector3(halfWidth * 2f, wall, halfDepth * 2f), carWall, car);
        CreateBlock("Elevator Car Back Wall", origin + new Vector3(0f, height * 0.5f, halfDepth - wall * 0.5f), new Vector3(halfWidth * 2f, height, wall), carWall, car);
        for (int side = -1; side <= 1; side += 2)
        {
            CreateBlock($"Elevator Car {(side < 0 ? "West" : "East")} Wall", origin + new Vector3(side * (halfWidth - wall * 0.5f), height * 0.5f, 0f), new Vector3(wall, height, halfDepth * 2f), carWall, car);
            float postWidth = halfWidth - ElevatorDoorHalfWidth;
            CreateBlock($"Elevator Car Front {(side < 0 ? "West" : "East")} Post", origin + new Vector3(side * (ElevatorDoorHalfWidth + postWidth * 0.5f), height * 0.5f, -halfDepth + wall * 0.5f), new Vector3(postWidth, height, wall), carWall, car);
        }

        CreateBlock("Elevator Car Front Header Wall", origin + new Vector3(0f, (ElevatorDoorHeight + height) * 0.5f, -halfDepth + wall * 0.5f), new Vector3(ElevatorDoorHalfWidth * 2f, height - ElevatorDoorHeight, wall), carWall, car);
        for (int strip = -1; strip <= 1; strip += 2)
        {
            CreateDecorationBlock($"Elevator Car Ceiling Light Strip {strip}", origin + new Vector3(strip * 1.1f, height - 0.04f, 0f), new Vector3(0.12f, 0.04f, 3.4f), Color.Lerp(accentColor, Color.black, 0.45f), car);
        }

        CreateDecorationBlock("Elevator Car Handrail Post", origin + new Vector3(0f, 1f, halfDepth - wall - 0.06f), new Vector3(halfWidth * 2f - 0.5f, 0.05f, 0.05f), new Color(0.45f, 0.48f, 0.5f), car);

        GameObject lightObject = new GameObject("Elevator Car Light");
        lightObject.transform.SetParent(car, false);
        lightObject.transform.localPosition = new Vector3(0f, height - 0.4f, 0f);
        Light carLight = lightObject.AddComponent<Light>();
        carLight.type = LightType.Point;
        carLight.color = Color.Lerp(accentColor, Color.white, 0.75f);
        carLight.range = 7f;
        carLight.intensity = 2.2f;
        carLight.shadows = LightShadows.None;
    }

    private static void HideModelParts(GameObject model, params string[] partNames)
    {
        if (model == null) return;
        foreach (Transform child in model.GetComponentsInChildren<Transform>(true))
        {
            foreach (string partName in partNames)
            {
                if (child.name.Contains(partName))
                {
                    child.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private void CreateRamp(string rampName, Vector3 start, Vector3 end, float width, Color color)
    {
        Vector3 delta = end - start;
        Vector3 center = (start + end) * 0.5f;
        Quaternion rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        GameObject ramp = CreateBlock(rampName, center, new Vector3(width, 0.28f, delta.magnitude), color);
        ramp.transform.rotation = rotation;

        Vector3 side = rotation * Vector3.right * (width * 0.5f - 0.12f);
        GameObject leftRail = CreateBlock($"{rampName} Left Rail", center - side + Vector3.up * 0.35f, new Vector3(0.18f, 0.7f, delta.magnitude), Color.Lerp(color, Color.white, 0.12f));
        GameObject rightRail = CreateBlock($"{rampName} Right Rail", center + side + Vector3.up * 0.35f, new Vector3(0.18f, 0.7f, delta.magnitude), Color.Lerp(color, Color.white, 0.12f));
        leftRail.transform.rotation = rotation;
        rightRail.transform.rotation = rotation;
    }

    private void CreateDoorwayWall(
        string wallName,
        float z,
        float gapCenter,
        float gapWidth,
        Color color,
        float halfWidth = 14f)
    {
        float leftEdge = -halfWidth + 0.25f;
        float rightEdge = halfWidth - 0.25f;
        float gapLeft = Mathf.Clamp(gapCenter - gapWidth * 0.5f, leftEdge, rightEdge);
        float gapRight = Mathf.Clamp(gapCenter + gapWidth * 0.5f, leftEdge, rightEdge);
        float leftWidth = Mathf.Max(0f, gapLeft - leftEdge);
        float rightWidth = Mathf.Max(0f, rightEdge - gapRight);

        if (leftWidth > 0.1f)
        {
            CreateBlock(
                $"{wallName} West",
                new Vector3(leftEdge + leftWidth * 0.5f, 1.7f, z),
                new Vector3(leftWidth, 3.4f, 0.35f),
                color);
        }

        if (rightWidth > 0.1f)
        {
            CreateBlock(
                $"{wallName} East",
                new Vector3(gapRight + rightWidth * 0.5f, 1.7f, z),
                new Vector3(rightWidth, 3.4f, 0.35f),
                color);
        }

        CreateBlock(
            $"{wallName} Header",
            new Vector3(gapCenter, 3.1f, z),
            new Vector3(gapWidth, 0.6f, 0.35f),
            color);
    }

    private void CreateResearchStation(string stationName, Vector3 position, ResearchTrack track, Color color, Transform parent)
    {
        GameObject station = CreateLairModel(
            "Models/Lair/ResearchConsole",
            stationName,
            position - Vector3.up * 0.55f,
            Quaternion.identity,
            Vector3.one * 0.72f,
            parent);
        AddInteractionCollider(station, new Vector3(1.8f, 1.8f, 1.2f), new Vector3(0f, 0.9f, 0f));
        ResearchProjectTerminal terminal = station.AddComponent<ResearchProjectTerminal>();
        terminal.track = track;
        CreateWorldLabel(stationName.ToUpperInvariant(), position + Vector3.up * 1f, Color.white, 0.075f, parent);
    }

    private GameObject CreateLairModel(
        string resourcePath,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        GameObject model;
        if (prefab != null)
        {
            model = Instantiate(prefab);
        }
        else
        {
            model = new GameObject(objectName);
            CreateBlock(
                $"{objectName} Missing Model",
                position + Vector3.up * 0.5f,
                new Vector3(1f, 1f, 1f),
                new Color(0.8f, 0.15f, 0.15f),
                model.transform);
            Debug.LogWarning($"Could not load lair model at Resources/{resourcePath}.");
        }

        model.name = objectName;
        model.transform.SetParent(parent != null ? parent : runtimeRoot, true);
        model.transform.position = position;
        model.transform.rotation = rotation;
        model.transform.localScale = scale;
        return model;
    }

    private GameObject CreateEnvironmentModel(
        string assetName,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Color accentColor,
        Transform parent = null)
    {
        GameObject prefab = Resources.Load<GameObject>($"Models/Environment/{assetName}");
        if (prefab == null)
        {
            Debug.LogWarning($"Could not load environment model at Resources/Models/Environment/{assetName}.");
            return null;
        }

        GameObject model = Instantiate(prefab);
        model.name = objectName;
        model.transform.SetParent(parent != null ? parent : runtimeRoot, true);
        model.transform.position = position;
        model.transform.rotation = rotation;
        model.transform.localScale = scale;
        PrototypeEnvironmentArt.StyleKitInstance(model, accentColor);
        return model;
    }

    private static void EnsureBoxCollider(GameObject target, Vector3 size, Vector3 center)
    {
        if (target == null) return;
        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider == null) collider = target.AddComponent<BoxCollider>();
        collider.size = size;
        collider.center = center;
    }

    private GameObject CreateFacilityProp(
        string resourcePath,
        string objectName,
        Vector3 position,
        Vector3 eulerAngles,
        Vector3 scale,
        Vector3 colliderSize,
        Vector3 colliderCenter,
        Transform parent = null)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"Could not load facility model at Resources/{resourcePath}.");
            return CreateBlock(
                $"{objectName} Missing Model",
                position + Vector3.up * colliderCenter.y,
                colliderSize,
                new Color(0.75f, 0.12f, 0.12f),
                parent);
        }

        GameObject prop = Instantiate(prefab);
        prop.name = objectName;
        prop.transform.SetParent(parent != null ? parent : runtimeRoot, true);
        prop.transform.position = position;
        prop.transform.rotation = Quaternion.Euler(eulerAngles);
        prop.transform.localScale = scale;

        BoxCollider collider = prop.AddComponent<BoxCollider>();
        collider.size = colliderSize;
        collider.center = colliderCenter;
        return prop;
    }

    private void CreateFacilityLamp(string lampName, Vector3 position)
    {
        GameObject lamp = CreateFacilityProp(
            "Models/Facility/FacilityLampPost",
            lampName,
            position,
            Vector3.zero,
            Vector3.one,
            new Vector3(0.55f, 4.8f, 0.55f),
            new Vector3(0f, 2.4f, 0f));

        GameObject lightObject = new GameObject($"{lampName} Light");
        lightObject.transform.SetParent(lamp != null ? lamp.transform : runtimeRoot, true);
        lightObject.transform.position = position + new Vector3(0.78f, 4.2f, 0f);
        Light lampLight = lightObject.AddComponent<Light>();
        lampLight.type = LightType.Point;
        lampLight.color = new Color(0.62f, 0.95f, 0.9f);
        lampLight.range = 17f;
        lampLight.intensity = 1.35f;
        lampLight.shadows = LightShadows.None;
    }

    private static void AddInteractionCollider(GameObject target, Vector3 size, Vector3 center)
    {
        if (target == null) return;
        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider == null) collider = target.AddComponent<BoxCollider>();
        collider.size = size;
        collider.center = center;
    }

    private void CreateTrainingGround(Vector3 position, GameObject[] levelVisuals, Color accentColor)
    {
        for (int lane = 0; lane < 3; lane++)
        {
            float x = position.x + (-3.5f + lane * 3.5f) * hubRoomSpread.x;
            CreateBlock(
                $"Training Lane {lane + 1}",
                new Vector3(x, position.y + 0.08f, position.z),
                new Vector3(0.12f, 0.04f, 11f * hubRoomSpread.y),
                accentColor,
                levelVisuals[0].transform);
            GameObject target = CreateEnvironmentModel(
                "TrainingTarget_A",
                $"Training Target {lane + 1}",
                new Vector3(x, position.y, position.z + 4.7f * hubRoomSpread.y),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 0.8f,
                accentColor,
                lane == 0 ? levelVisuals[0].transform : levelVisuals[1].transform);
            if (target != null)
            {
                EnsureBoxCollider(target, new Vector3(1.65f, 2.65f, 1.35f), new Vector3(0f, 1.32f, 0f));
            }
        }

        CreateBlock("Training Cover Low", position + Spread(new Vector3(-2.5f, 0.55f, -1.4f)), new Vector3(2.2f, 1.1f, 0.6f), new Color(0.18f, 0.2f, 0.22f), levelVisuals[1].transform);
        CreateBlock("Training Cover High", position + Spread(new Vector3(2.5f, 1f, 0.2f)), new Vector3(2.2f, 2f, 0.6f), new Color(0.18f, 0.2f, 0.22f), levelVisuals[2].transform);
        CreateBlock("Training Balance Beam", position + Spread(new Vector3(0f, 0.45f, -3.4f)), new Vector3(5f, 0.3f, 0.45f), accentColor, levelVisuals[3].transform);
        CreateLairModel(
            "Models/Lair/TrainingRig",
            "Advanced Combat Rig",
            position + Spread(new Vector3(0f, 0f, -4.8f)),
            Quaternion.identity,
            Vector3.one,
            levelVisuals[4].transform);
        CreateWorldLabel("FIRING LANES  /  COMBAT SIMULATION", position + Spread(new Vector3(0f, 2.55f, -6.5f)), Color.white, 0.1f);
    }

    private void CreateArmoryDisplays(Vector3 position, GameObject[] levelVisuals)
    {
        GameFlowManager flow = GameFlowManager.Instance;
        SecretLairManager lair = SecretLairManager.Instance;
        if (flow == null) return;

        for (int i = 0; i < flow.weaponCatalog.Count; i++)
        {
            WeaponDefinition weapon = flow.weaponCatalog[i];
            if (weapon == null) continue;
            bool unlocked = lair != null && lair.IsWeaponUnlocked(weapon.weaponId);
            int displayLevel = unlocked ? 0 : Mathf.Clamp(weapon.unlockedAtArmoryLevel - 1, 0, levelVisuals.Length - 1);
            Vector3 displayPosition = position + Spread(new Vector3(-2.25f + i * 1.5f, 1.3f, 2.1f));
            Color color = unlocked ? GetWeaponColor(weapon.weaponClass) : new Color(0.14f, 0.15f, 0.16f);
            GameObject display = CreateBlock(
                $"Armory Display - {weapon.weaponName}",
                displayPosition,
                new Vector3(0.22f, 0.22f, weapon.weaponClass == WeaponClass.Rifle ? 1.25f : 0.8f),
                color,
                levelVisuals[displayLevel].transform);
            display.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            CreateWorldLabel(weapon.weaponName.ToUpperInvariant(), displayPosition + Vector3.up * 0.55f, Color.white, 0.065f, levelVisuals[displayLevel].transform);
        }
    }

    private void CreateMechDisplay(string displayName, Vector3 position, Color color, Transform parent)
    {
        GameObject root = new GameObject(displayName);
        root.transform.SetParent(parent, false);
        CreateBlock($"{displayName} Torso", position + new Vector3(0f, 1.45f, 0f), new Vector3(1.4f, 1.25f, 0.8f), color, root.transform);
        CreateBlock($"{displayName} Cockpit", position + new Vector3(0f, 2.25f, 0.05f), new Vector3(0.85f, 0.55f, 0.65f), Color.Lerp(color, Color.white, 0.25f), root.transform);
        CreateBlock($"{displayName} Left Leg", position + new Vector3(-0.45f, 0.65f, 0f), new Vector3(0.42f, 1.3f, 0.5f), color, root.transform);
        CreateBlock($"{displayName} Right Leg", position + new Vector3(0.45f, 0.65f, 0f), new Vector3(0.42f, 1.3f, 0.5f), color, root.transform);
        CreateBlock($"{displayName} Left Arm", position + new Vector3(-0.95f, 1.5f, 0f), new Vector3(0.4f, 1.15f, 0.45f), color, root.transform);
        CreateBlock($"{displayName} Right Arm", position + new Vector3(0.95f, 1.5f, 0f), new Vector3(0.4f, 1.15f, 0.45f), color, root.transform);
        CreateWorldLabel(displayName.ToUpperInvariant(), position + new Vector3(0f, 3f, 0f), Color.white, 0.085f, root.transform);
    }

    private void ParentToRuntime(GameObject target)
    {
        if (target != null && runtimeRoot != null)
        {
            target.transform.SetParent(runtimeRoot, true);
        }
    }

    private void CreateUpgradeTerminal()
    {
        GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        terminal.name = "Armory Upgrade Terminal";
        terminal.transform.position = new Vector3(7f, 0.6f, -2f);
        terminal.transform.localScale = new Vector3(1.2f, 1.2f, 0.4f);
        ParentToRuntime(terminal);
        SetColor(terminal, new Color(0.1f, 0.45f, 0.9f));

        BaseUpgradeTerminal upgradeTerminal = terminal.AddComponent<BaseUpgradeTerminal>();
        upgradeTerminal.roomToUpgrade = RoomType.Armory;
    }

    private void CreateWeaponPickups()
    {
        if (GameFlowManager.Instance == null) return;

        System.Collections.Generic.List<WeaponDefinition> weapons = GameFlowManager.Instance.GetArmoryAvailableWeapons();
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponDefinition weapon = weapons[i];
            if (weapon == null) continue;

            CreateSupplyPickup(
                $"Field Weapon - {weapon.weaponName}",
                new Vector3(-5.5f + i * 3.5f, 0.9f, 1.5f),
                PickupType.Weapon,
                weapon.ammoPerPickup,
                weapon,
                GetWeaponColor(weapon.weaponClass),
                new Vector3(0.35f, 0.35f, 0.9f));
        }
    }

    private void ConfigureEnemyLoot(
        GameObject enemyObject,
        EnemyOperative enemy,
        OperativeSpecialization specialization,
        EnemyRank rank)
    {
        if (enemyObject == null || enemy == null || GameFlowManager.Instance == null) return;

        WeaponDefinition droppedWeapon = null;
        int ammoAmount = 8 + (int)rank * 3;
        int healthAmount = 0;

        switch (specialization)
        {
            case OperativeSpecialization.Stealth:
                droppedWeapon = GameFlowManager.Instance.GetWeapon("shock-pistol");
                break;
            case OperativeSpecialization.Tank:
                droppedWeapon = GameFlowManager.Instance.GetWeapon("dart-smg");
                break;
            case OperativeSpecialization.Marksman:
                droppedWeapon = GameFlowManager.Instance.GetWeapon("tranq-rifle");
                break;
            case OperativeSpecialization.Hacker:
                droppedWeapon = GameFlowManager.Instance.GetWeapon("shock-pistol");
                break;
            case OperativeSpecialization.Medic:
                droppedWeapon = GameFlowManager.Instance.GetWeapon("tranq-pistol");
                healthAmount = 30;
                break;
        }

        EnemyLootDrop loot = enemyObject.AddComponent<EnemyLootDrop>();
        loot.Configure(enemy, droppedWeapon, ammoAmount, healthAmount);
    }

    private void CreateMissionSupplies(MissionDefinition definition)
    {
        MissionEnvironmentType environment = definition != null
            ? definition.environmentType
            : MissionEnvironmentType.CorporateAnnex;

        if (IsLaboratoryLayout(environment))
        {
            CreateSupplyCache("Insertion Supply Cache", new Vector3(8f, 0f, missionEntryZ + 12f), 24, 25);
            CreateSupplyCache("Maintenance Route Cache", new Vector3(-12f, 0f, -42f), 30, 0);
            CreateSupplyCache("Compound Courtyard Cache", new Vector3(-12f, 0f, 35f), 36, 30);
            CreateSupplyCache("Arsenal Assault Cache", new Vector3(13f, 0f, 52f), 30, 35);
            CreateFloatingWeapon("Sedative SMG Comms Rack", GameFlowManager.Instance?.GetWeapon("dart-smg"), new Vector3(-24f, 4.15f, 22f));
            CreateFloatingWeapon("Precision Rifle Fabrication Rack", GameFlowManager.Instance?.GetWeapon("tranq-rifle"), new Vector3(24f, 4.15f, 22f));
            CreateFloatingWeapon("Shock Pistol Arsenal Rack", GameFlowManager.Instance?.GetWeapon("shock-pistol"), new Vector3(0f, 7.55f, 60f));
        }
        else if (IsStrongholdLayout(environment))
        {
            CreateSupplyCache("Service Entry Cache", new Vector3(10f, 0f, missionEntryZ + 8f), 24, 25);
            CreateSupplyCache("Security Cache", new Vector3(-14f, 0f, 14f), 30, 30);
            CreateFloatingWeapon("Stronghold Rifle", GameFlowManager.Instance?.GetWeapon("tranq-rifle"), new Vector3(14f, 1.1f, 22f));
        }
        else if (IsWoodlandLayout(environment))
        {
            CreateSupplyCache("Woodland Approach Cache", new Vector3(8f, 0f, missionEntryZ + 22f), 20, 25);
            CreateSupplyCache("Annex Courtyard Cache", new Vector3(8f, 0f, 7f), 24, 20);
            CreateFloatingWeapon("Barracks Backup Pistol", GameFlowManager.Instance?.GetWeapon("tranq-pistol"), new Vector3(-19f, 4.15f, 15f));
        }
        else
        {
            CreateSupplyCache("Arrival Hall Cache", new Vector3(9f, 0f, -68f), 20, 25);
            CreateSupplyCache("Operations Supply Cache", new Vector3(-17f, 0f, 8f), 24, 20);
            CreateFloatingWeapon("Locker Bay Backup Pistol", GameFlowManager.Instance?.GetWeapon("tranq-pistol"), new Vector3(17f, 1.1f, -38f));
        }

        CreateSupplyPickup(
            "Field Currency",
            new Vector3(0f, 0.9f, missionEntryZ + 10f),
            PickupType.Currency,
            40,
            null,
            new Color(0.2f, 0.85f, 1f),
            Vector3.one * 0.55f);
    }

    private void CreateSupplyCache(string cacheName, Vector3 position, int ammoAmount, int healthAmount)
    {
        GameObject crate = CreateEnvironmentModel(
            "SupplyCrate_A",
            cacheName,
            position,
            Quaternion.identity,
            Vector3.one,
            new Color(0.15f, 0.72f, 0.68f));
        if (crate != null)
        {
            EnsureBoxCollider(crate, new Vector3(2.25f, 0.95f, 1.35f), new Vector3(0f, 0.475f, 0f));
        }
        else
        {
            CreateBlock(
                cacheName,
                position + new Vector3(0f, 0.45f, 0f),
                new Vector3(2.4f, 0.9f, 1.4f),
                new Color(0.16f, 0.2f, 0.22f));
        }

        if (ammoAmount > 0)
        {
            CreateSupplyPickup(
                $"{cacheName} Ammo",
                position + new Vector3(-0.55f, 1.25f, 0f),
                PickupType.Ammo,
                ammoAmount,
                null,
                new Color(0.18f, 0.75f, 0.9f),
                new Vector3(0.55f, 0.35f, 0.45f));
        }

        if (healthAmount > 0)
        {
            CreateSupplyPickup(
                $"{cacheName} Medical Kit",
                position + new Vector3(0.55f, 1.25f, 0f),
                PickupType.Health,
                healthAmount,
                null,
                new Color(0.2f, 0.85f, 0.35f),
                new Vector3(0.45f, 0.45f, 0.45f));
        }
    }

    private void CreateFloatingWeapon(string pickupName, WeaponDefinition weapon, Vector3 position)
    {
        if (weapon == null) return;
        CreateSupplyPickup(
            pickupName,
            position,
            PickupType.Weapon,
            weapon.ammoPerPickup,
            weapon,
            GetWeaponColor(weapon.weaponClass),
            new Vector3(0.32f, 0.32f, weapon.weaponClass == WeaponClass.Rifle ? 1.25f : 0.9f));
    }

    private void CreateSupplyPickup(
        string pickupName,
        Vector3 position,
        PickupType type,
        int amount,
        WeaponDefinition weapon,
        Color color,
        Vector3 scale)
    {
        GameObject pickupObject = new GameObject(pickupName);
        pickupObject.name = pickupName;
        pickupObject.transform.position = position;
        ParentToRuntime(pickupObject);

        SphereCollider trigger = pickupObject.AddComponent<SphereCollider>();
        trigger.radius = type == PickupType.Weapon ? 0.7f : 0.58f;
        trigger.isTrigger = true;

        string resourcePath = GetPickupModelResourcePath(type, weapon);
        GameObject prefab = string.IsNullOrEmpty(resourcePath)
            ? null
            : Resources.Load<GameObject>(resourcePath);
        if (prefab != null)
        {
            GameObject visual = Instantiate(prefab, pickupObject.transform);
            visual.name = $"{pickupName} Model";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = type == PickupType.Weapon
                ? Quaternion.Euler(0f, 90f, 18f)
                : Quaternion.identity;
            visual.transform.localScale = type == PickupType.Weapon
                ? Vector3.one * 0.42f
                : Vector3.one * 0.9f;
            foreach (Collider visualCollider in visual.GetComponentsInChildren<Collider>())
            {
                visualCollider.enabled = false;
            }
        }
        else
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.name = $"{pickupName} Fallback";
            fallback.transform.SetParent(pickupObject.transform, false);
            fallback.transform.localScale = scale;
            Collider fallbackCollider = fallback.GetComponent<Collider>();
            if (fallbackCollider != null) fallbackCollider.enabled = false;
            SetColor(fallback, color);
        }

        FieldPickup pickup = pickupObject.AddComponent<FieldPickup>();
        pickup.pickupType = type;
        pickup.amount = amount;
        pickup.weaponDefinition = weapon;
    }

    private static string GetPickupModelResourcePath(PickupType type, WeaponDefinition weapon)
    {
        if (type == PickupType.Weapon && weapon != null)
        {
            return GetWeaponModelResourcePath(weapon.weaponId);
        }

        switch (type)
        {
            case PickupType.Ammo:
                return "Models/Environment/AmmoBox_A";
            case PickupType.Health:
                return "Models/Environment/FieldMedkit_A";
            case PickupType.Currency:
                return "Models/Environment/EncryptedCredit_A";
            default:
                return "Models/Environment/SupplyCrate_A";
        }
    }

    private static string GetWeaponModelResourcePath(string weaponId)
    {
        switch (weaponId)
        {
            case "dart-smg":
                return "Models/Weapons/SedativeSMG";
            case "tranq-rifle":
                return "Models/Weapons/PrecisionRifle";
            case "shock-pistol":
                return "Models/Weapons/ShockPistol";
            default:
                return "Models/Weapons/TranquilizerPistol";
        }
    }

    private static Color GetWeaponColor(WeaponClass weaponClass)
    {
        switch (weaponClass)
        {
            case WeaponClass.SubmachineGun: return new Color(0.9f, 0.25f, 0.2f);
            case WeaponClass.Rifle: return new Color(0.25f, 0.7f, 0.32f);
            case WeaponClass.Shotgun: return new Color(0.88f, 0.6f, 0.16f);
            case WeaponClass.Heavy: return new Color(0.45f, 0.28f, 0.22f);
            case WeaponClass.Gadget: return new Color(0.65f, 0.25f, 0.75f);
            default: return new Color(0.15f, 0.75f, 0.9f);
        }
    }

    private void CreatePickup()
    {
        CreateSupplyPickup(
            "Prototype Currency Pickup",
            new Vector3(0f, 1f, 3f),
            PickupType.Currency,
            250,
            null,
            new Color(0.2f, 0.85f, 1f),
            Vector3.one * 0.55f);
    }

    private void SetColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
