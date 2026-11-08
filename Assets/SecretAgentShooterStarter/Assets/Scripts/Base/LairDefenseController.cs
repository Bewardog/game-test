using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base defense: the syndicate sets up a forward camp somewhere out in the
/// expanse and sends attack waves from it toward the lair. The waves keep
/// coming until the player goes out to the camp and captures it by standing
/// at its flag with no conscious enemies nearby.
///
/// Every defense puts the camp in a new spot, in a different direction from
/// the last one. Attackers march to the nearest perimeter gate and on to the
/// lair entrance, and ranged attackers take cover in houses, ruins and behind
/// sandbags when they fight.
///
/// The scene builder sets this up (Configure) and the defense console in the
/// mission room starts it. Campaign code can start it with StartDefense().
/// </summary>
public class LairDefenseController : MonoBehaviour
{
    public enum DefenseState
    {
        Idle,
        Active,
        Captured
    }

    public static LairDefenseController Instance { get; private set; }

    [Header("Waves")]
    public float firstWaveDelay = 8f;
    public float waveInterval = 45f;
    public int maxAttackersAlive = 10;

    [Header("Capture")]
    public float captureRadius = 9f;
    public float captureSeconds = 12f;
    public int captureReward = 300;

    public DefenseState State { get; private set; }
    public int Wave { get; private set; }
    public Vector3 CampPosition { get; private set; }

    private static float lastCampAngle = -1000f;

    private Vector3 groundsCenter;
    private float playAreaRadius;
    private Vector2 perimeterHalfExtents;
    private float perimeterCenterZ;
    private System.Func<float, float, float, bool> isBlocked;
    private System.Func<string, Vector3, EnemyRank, OperativeSpecialization, EnemyOperative> spawnEnemy;
    private Vector3[][] gateRoutes;

    private GameObject campRoot;
    private Transform campFlag;
    private Renderer flagRenderer;
    private Renderer beaconRenderer;
    private Light beaconLight;
    private Vector3 campSpawnPoint;
    private Vector3[] campAssaultRoute;
    private readonly List<EnemyOperative> attackers = new List<EnemyOperative>();
    private readonly List<EnemyOperative> garrison = new List<EnemyOperative>();
    private readonly List<GameObject> clearedScenery = new List<GameObject>();
    private float nextWaveTime;
    private float captureProgress;
    private bool contested;
    private float nextContestCheck;
    private float capturedAt;
    private SpyPlayerController player;

    private static readonly Color SyndicateRed = new Color(0.78f, 0.1f, 0.1f);
    private static readonly Color AgencyTeal = new Color(0.16f, 0.82f, 0.78f);

    /// <param name="center">Middle of the lair grounds.</param>
    /// <param name="radius">Radius of the player's play area.</param>
    /// <param name="perimeterHalfX">Half width of the perimeter wall.</param>
    /// <param name="perimeterSouthZ">Z of the south (main gate) wall.</param>
    /// <param name="perimeterNorthZ">Z of the north wall.</param>
    /// <param name="blockedTest">(x, z, margin) → true where the camp must not go.</param>
    /// <param name="spawner">Creates an enemy: (name, position, rank, specialization).</param>
    /// <param name="routes">Waypoints from outside each gate to the lair entrance:
    /// south, east, west.</param>
    public void Configure(
        Vector3 center,
        float radius,
        float perimeterHalfX,
        float perimeterSouthZ,
        float perimeterNorthZ,
        System.Func<float, float, float, bool> blockedTest,
        System.Func<string, Vector3, EnemyRank, OperativeSpecialization, EnemyOperative> spawner,
        Vector3[][] routes)
    {
        groundsCenter = center;
        playAreaRadius = radius;
        perimeterHalfExtents = new Vector2(perimeterHalfX, (perimeterNorthZ - perimeterSouthZ) * 0.5f);
        perimeterCenterZ = (perimeterNorthZ + perimeterSouthZ) * 0.5f;
        isBlocked = blockedTest;
        spawnEnemy = spawner;
        gateRoutes = routes;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool CanStart => State != DefenseState.Active && spawnEnemy != null;

    public void StartDefense()
    {
        if (!CanStart) return;
        if (player == null) player = FindAnyObjectByType<SpyPlayerController>();

        ClearPreviousDefense();
        if (!ChooseCampLocation(out Vector3 position, out float angle))
        {
            player?.ShowActionMessage("NO ROOM FOR AN ENEMY CAMP", 1.5f);
            return;
        }

        lastCampAngle = angle;
        CampPosition = position;
        ClearSceneryAt(position, 17f);
        BuildCamp(position);
        campAssaultRoute = BuildAssaultRoute(position);
        SpawnGarrison();

        State = DefenseState.Active;
        Wave = 0;
        captureProgress = 0f;
        nextWaveTime = Time.time + firstWaveDelay;
        player?.ShowActionMessage($"ENEMY CAMP SPOTTED TO THE {CompassName(position - groundsCenter)}", 2.5f);
    }

    private void Update()
    {
        if (State == DefenseState.Captured && Time.time - capturedAt > 12f)
        {
            State = DefenseState.Idle;
        }

        if (State != DefenseState.Active) return;
        if (player == null) player = FindAnyObjectByType<SpyPlayerController>();

        if (Time.time >= nextWaveTime)
        {
            if (CountConscious(attackers) < maxAttackersAlive)
            {
                StartCoroutine(SpawnWave(++Wave));
            }

            nextWaveTime = Time.time + waveInterval;
        }

        UpdateCapture();
    }

    // ---------------------------------------------------------------- camp site

    private bool ChooseCampLocation(out Vector3 position, out float angle)
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            angle = Random.Range(0f, 360f);
            if (lastCampAngle > -999f && Mathf.Abs(Mathf.DeltaAngle(angle, lastCampAngle)) < 75f) continue;

            float radius = Random.Range(150f, playAreaRadius - 22f);
            float radians = angle * Mathf.Deg2Rad;
            position = groundsCenter + new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians)) * radius;
            position.y = -0.1f;

            if (!IsOutsidePerimeter(position, 35f)) continue;
            if (isBlocked != null && isBlocked(position.x, position.z, 13f)) continue;
            return true;
        }

        position = Vector3.zero;
        angle = 0f;
        return false;
    }

    private bool IsOutsidePerimeter(Vector3 point, float margin)
    {
        return Mathf.Abs(point.x) > perimeterHalfExtents.x + margin ||
            Mathf.Abs(point.z - perimeterCenterZ) > perimeterHalfExtents.y + margin;
    }

    // Camp → nearest gate → lair entrance, with waypoints every few metres so
    // the walk never stalls on a long leg. Camps behind the lair go round the
    // corner of the wall first.
    private Vector3[] BuildAssaultRoute(Vector3 camp)
    {
        List<Vector3> corners = new List<Vector3>();
        Vector3[] gate;
        float southZ = perimeterCenterZ - perimeterHalfExtents.y;
        float northZ = perimeterCenterZ + perimeterHalfExtents.y;
        float cornerX = perimeterHalfExtents.x + 14f;

        if (camp.z < southZ - 5f && Mathf.Abs(camp.x) < perimeterHalfExtents.x + 32f)
        {
            gate = gateRoutes[0];
        }
        else
        {
            bool east = camp.x >= 0f;
            gate = gateRoutes[east ? 1 : 2];
            float sideX = east ? cornerX : -cornerX;
            if (camp.z > northZ - 5f) corners.Add(new Vector3(sideX, 0f, northZ + 14f));
            else if (camp.z < southZ - 5f) corners.Add(new Vector3(sideX, 0f, southZ - 14f));
        }

        List<Vector3> route = new List<Vector3> { camp };
        route.AddRange(corners);
        route.AddRange(gate);

        List<Vector3> dense = new List<Vector3>();
        for (int i = 1; i < route.Count; i++)
        {
            Vector3 from = route[i - 1];
            Vector3 to = route[i];
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / 9f));
            for (int s = 1; s <= steps; s++)
            {
                Vector3 point = Vector3.Lerp(from, to, s / (float)steps);
                point.y = camp.y;
                dense.Add(point);
            }
        }

        return dense.ToArray();
    }

    // ---------------------------------------------------------------- camp build

    private void BuildCamp(Vector3 position)
    {
        campRoot = new GameObject("Enemy Forward Camp");
        campRoot.transform.SetParent(transform, true);
        campRoot.transform.position = position;
        Transform root = campRoot.transform;

        Color canvas = new Color(0.28f, 0.27f, 0.2f);
        Color darkCanvas = new Color(0.2f, 0.19f, 0.15f);
        Color sandbag = new Color(0.46f, 0.4f, 0.28f);
        Color crate = new Color(0.32f, 0.26f, 0.16f);
        Color steel = new Color(0.16f, 0.17f, 0.18f);
        Color dirt = new Color(0.22f, 0.19f, 0.14f);

        // Local +Z faces the lair; everything is laid out around the flag.
        Block(root, "Camp Dirt Pad Floor", new Vector3(0f, 0.02f, 0f), new Vector3(26f, 0.04f, 24f), dirt, false);

        // Tents at the back of the camp: two sloped canvas panels each.
        float[] tentX = { -7.5f, 0f, 7.5f };
        for (int i = 0; i < tentX.Length; i++)
        {
            Vector3 tent = new Vector3(tentX[i], 0f, -6.5f);
            Block(root, $"Camp Tent {i + 1} Body", tent + new Vector3(0f, 0.9f, 0f), new Vector3(3.6f, 1.8f, 4.6f), darkCanvas, true);
            Block(root, $"Camp Tent {i + 1} Roof Left", tent + new Vector3(-1.05f, 2.15f, 0f), new Vector3(2.5f, 0.08f, 5f), canvas, false, new Vector3(0f, 0f, 32f));
            Block(root, $"Camp Tent {i + 1} Roof Right", tent + new Vector3(1.05f, 2.15f, 0f), new Vector3(2.5f, 0.08f, 5f), canvas, false, new Vector3(0f, 0f, -32f));
            Block(root, $"Camp Tent {i + 1} Door Flap", tent + new Vector3(0f, 0.85f, 2.32f), new Vector3(1.2f, 1.6f, 0.04f), new Color(0.12f, 0.1f, 0.08f), false);
        }

        // Supply truck on the west side.
        Vector3 truck = new Vector3(-9.5f, 0f, 3f);
        Block(root, "Camp Truck Cargo Bed", truck + new Vector3(0f, 1.6f, -1f), new Vector3(2.5f, 2.2f, 4.6f), canvas, true);
        Block(root, "Camp Truck Cab", truck + new Vector3(0f, 1.25f, 2.4f), new Vector3(2.4f, 1.7f, 1.9f), new Color(0.2f, 0.24f, 0.18f), true);
        Block(root, "Camp Truck Chassis", truck + new Vector3(0f, 0.45f, 0f), new Vector3(2.3f, 0.5f, 7f), steel, true);
        for (int axle = 0; axle < 2; axle++)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Block(root, $"Camp Truck Wheel {axle}{side}", truck + new Vector3(side * 1.2f, 0.45f, axle == 0 ? 2.2f : -2f), new Vector3(0.35f, 0.9f, 0.9f), new Color(0.06f, 0.06f, 0.06f), false);
            }
        }

        // Crate stacks.
        Vector3[] crates =
        {
            new Vector3(8.5f, 0.5f, 2.5f), new Vector3(9.6f, 0.5f, 2.5f), new Vector3(9f, 1.5f, 2.5f),
            new Vector3(10f, 0.5f, -1.5f), new Vector3(-4f, 0.5f, -2.5f), new Vector3(4.5f, 0.5f, -2.8f)
        };
        for (int i = 0; i < crates.Length; i++)
        {
            Block(root, $"Camp Supply Crate {i + 1}", crates[i], Vector3.one, crate, true, new Vector3(0f, i * 17f, 0f));
        }

        // Sandbag line across the front, with gaps to walk through.
        float[,] bags =
        {
            { -9f, 9.5f, 5f, 0f }, { -2.5f, 10.5f, 4f, 0f }, { 4.5f, 10f, 5f, 0f }, { 11f, 7f, 4f, 55f }, { -12.2f, 5.5f, 4f, -55f }
        };
        List<Vector3> bagCover = new List<Vector3>();
        for (int i = 0; i < bags.GetLength(0); i++)
        {
            Vector3 center = new Vector3(bags[i, 0], 0.45f, bags[i, 1]);
            float yaw = bags[i, 3];
            Block(root, $"Camp Sandbag Wall {i + 1}", center, new Vector3(bags[i, 2], 0.9f, 0.9f), sandbag, true, new Vector3(0f, yaw, 0f));
            Block(root, $"Camp Sandbag Wall {i + 1} Top Row", center + Vector3.up * 0.5f, new Vector3(bags[i, 2] - 0.4f, 0.12f, 0.75f), sandbag * 0.9f, false, new Vector3(0f, yaw, 0f));
            Vector3 behind = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, -1f);
            bagCover.Add(new Vector3(center.x, 0f, center.z) + behind);
        }

        // Flag pole, capture ring, and a tall signal beacon so the camp can
        // be found from the base.
        GameObject flagRoot = new GameObject("Camp Flag");
        flagRoot.transform.SetParent(root, false);
        flagRoot.transform.localPosition = new Vector3(0f, 0f, 1.5f);
        campFlag = flagRoot.transform;
        Block(flagRoot.transform, "Camp Flag Pole", new Vector3(0f, 4f, 0f), new Vector3(0.14f, 8f, 0.14f), steel, true);
        GameObject flag = Block(flagRoot.transform, "Camp Flag Cloth", new Vector3(0.95f, 7.2f, 0f), new Vector3(1.8f, 1.1f, 0.04f), SyndicateRed, false);
        flagRenderer = flag.GetComponent<Renderer>();

        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI * 2f / 16f;
            Vector3 p = new Vector3(Mathf.Cos(a) * captureRadius, 0.06f, Mathf.Sin(a) * captureRadius);
            Block(flagRoot.transform, $"Camp Capture Ring Marker {i + 1}", p, new Vector3(0.5f, 0.05f, 1.2f), new Color(0.9f, 0.75f, 0.2f), false, new Vector3(0f, -a * Mathf.Rad2Deg, 0f));
        }

        GameObject beacon = Block(flagRoot.transform, "Camp Signal Beacon Beam", new Vector3(0f, 34f, 0f), new Vector3(0.35f, 52f, 0.35f), SyndicateRed, false);
        beaconRenderer = beacon.GetComponent<Renderer>();
        if (beaconRenderer != null) beaconRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        GameObject lightObject = new GameObject("Camp Signal Beacon Light");
        lightObject.transform.SetParent(flagRoot.transform, false);
        lightObject.transform.localPosition = new Vector3(0f, 8.5f, 0f);
        beaconLight = lightObject.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.color = SyndicateRed;
        beaconLight.range = 30f;
        beaconLight.intensity = 3f;
        beaconLight.shadows = LightShadows.None;

        // Turn the camp to face the lair, then set up cover and the spawn spot.
        Vector3 toBase = groundsCenter - position;
        toBase.y = 0f;
        if (toBase.sqrMagnitude > 0.01f) root.rotation = Quaternion.LookRotation(toBase.normalized, Vector3.up);

        for (int i = 0; i < bagCover.Count; i++)
        {
            CreateCover(root, $"Camp Sandbag Cover {i + 1}", root.TransformPoint(bagCover[i]), 1.1f);
        }

        CreateCover(root, "Camp Truck Cover", root.TransformPoint(truck + new Vector3(1.9f, 0f, -1f)), 1.3f);
        CreateCover(root, "Camp Crate Cover", root.TransformPoint(new Vector3(8.4f, 0f, 1.4f)), 1.3f);
        campSpawnPoint = root.TransformPoint(new Vector3(0f, 0f, -10.5f));
        campSpawnPoint.y = position.y;
    }

    private GameObject Block(Transform parent, string blockName, Vector3 localPosition, Vector3 size, Color color, bool solid, Vector3 localEuler = default)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = blockName;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = localPosition;
        block.transform.localRotation = Quaternion.Euler(localEuler);
        block.transform.localScale = size;
        PrototypeEnvironmentArt.StyleBlock(block, blockName, size, color);
        if (!solid)
        {
            Collider blockCollider = block.GetComponent<Collider>();
            if (blockCollider != null) blockCollider.enabled = false;
        }

        return block;
    }

    private static void CreateCover(Transform parent, string coverName, Vector3 worldPosition, float firingHeight)
    {
        GameObject coverObject = new GameObject(coverName);
        coverObject.transform.SetParent(parent, true);
        coverObject.transform.position = worldPosition;
        CoverPoint point = coverObject.AddComponent<CoverPoint>();
        point.firingHeight = firingHeight;
    }

    // ---------------------------------------------------------------- enemies

    private static readonly OperativeSpecialization[] WaveRoles =
    {
        OperativeSpecialization.HandToHand,
        OperativeSpecialization.Marksman,
        OperativeSpecialization.Hacker,
        OperativeSpecialization.HandToHand,
        OperativeSpecialization.Medic,
        OperativeSpecialization.Marksman,
        OperativeSpecialization.Tank,
        OperativeSpecialization.Stealth
    };

    private IEnumerator SpawnWave(int wave)
    {
        int count = Mathf.Min(3 + wave, 8);
        player?.ShowActionMessage($"ATTACK WAVE {wave} INBOUND", 2f);
        for (int i = 0; i < count; i++)
        {
            if (State != DefenseState.Active) yield break;
            OperativeSpecialization role = WaveRoles[(i + wave) % WaveRoles.Length];
            EnemyRank rank = EnemyRank.Grunt;
            if (wave >= 3 && i == 0) rank = EnemyRank.Sergeant;
            if (wave >= 5 && i == 1) rank = EnemyRank.Lieutenant;

            Vector3 jitter = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-2f, 2f));
            EnemyOperative enemy = spawnEnemy($"Syndicate Raider W{wave}-{i + 1}", campSpawnPoint + jitter, rank, role);
            if (enemy != null)
            {
                ConfigureAttacker(enemy, jitter);
                attackers.Add(enemy);
            }

            yield return new WaitForSeconds(0.7f);
        }
    }

    private void ConfigureAttacker(EnemyOperative enemy, Vector3 jitter)
    {
        EnemyCombatController combat = enemy.GetComponent<EnemyCombatController>();
        if (combat == null) return;

        bool ranged = enemy.Specialization == OperativeSpecialization.Marksman ||
            enemy.Specialization == OperativeSpecialization.Hacker ||
            enemy.Specialization == OperativeSpecialization.Medic;
        if (ranged)
        {
            // Out in the open the ranged roles reach further than indoors.
            combat.attackRange = enemy.Specialization == OperativeSpecialization.Marksman
                ? Mathf.Max(combat.attackRange, 32f)
                : Mathf.Max(combat.attackRange, 22f);
        }

        combat.useCover = true;
        combat.navigateAroundObstacles = true;
        combat.stopAtRouteEnd = true;
        combat.routeSpeedMultiplier = 0.9f;
        combat.detectionRange = Mathf.Max(combat.detectionRange, 30f);
        combat.disengageRange = Mathf.Max(combat.disengageRange, combat.attackRange + 12f);

        Vector3 sideways = new Vector3(jitter.x, 0f, jitter.z) * 0.6f;
        List<Vector3> route = new List<Vector3>(campAssaultRoute.Length);
        foreach (Vector3 point in campAssaultRoute) route.Add(point + sideways);
        combat.ConfigurePatrolRoute(route, 0.25f);
    }

    private void SpawnGarrison()
    {
        OperativeSpecialization[] roles =
        {
            OperativeSpecialization.Marksman,
            OperativeSpecialization.HandToHand,
            OperativeSpecialization.Hacker
        };
        Vector3 flag = campFlag.position;
        for (int i = 0; i < roles.Length; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            Vector3 position = flag + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 5f;
            position.y = CampPosition.y;
            EnemyOperative guard = spawnEnemy($"Camp Guard {i + 1}", position, i == 0 ? EnemyRank.Sergeant : EnemyRank.Grunt, roles[i]);
            if (guard == null) continue;

            EnemyCombatController combat = guard.GetComponent<EnemyCombatController>();
            if (combat != null)
            {
                combat.useCover = true;
                combat.navigateAroundObstacles = true;
                if (roles[i] == OperativeSpecialization.Marksman) combat.attackRange = Mathf.Max(combat.attackRange, 28f);
                combat.disengageRange = Mathf.Max(combat.disengageRange, combat.attackRange + 10f);

                // Walk a loop round the flag.
                List<Vector3> loop = new List<Vector3>();
                for (int p = 0; p < 4; p++)
                {
                    float a = angle + p * Mathf.PI * 0.5f;
                    Vector3 point = flag + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 6f;
                    point.y = CampPosition.y;
                    loop.Add(point);
                }

                combat.ConfigurePatrolRoute(loop, 2f);
            }

            garrison.Add(guard);
        }
    }

    private static bool IsConscious(EnemyOperative enemy)
    {
        return enemy != null && enemy.gameObject.activeInHierarchy && !enemy.isDead && !enemy.isKnockedOut && !enemy.isRecruited;
    }

    private static int CountConscious(List<EnemyOperative> list)
    {
        int count = 0;
        foreach (EnemyOperative enemy in list)
        {
            if (IsConscious(enemy)) count++;
        }

        return count;
    }

    // ---------------------------------------------------------------- capture

    private void UpdateCapture()
    {
        if (player == null || campFlag == null) return;

        Vector3 offset = player.transform.position - campFlag.position;
        offset.y = 0f;
        bool inside = offset.magnitude <= captureRadius && !player.IsIncapacitated;

        if (Time.time >= nextContestCheck)
        {
            nextContestCheck = Time.time + 0.25f;
            contested = false;
            foreach (EnemyOperative enemy in FindObjectsByType<EnemyOperative>())
            {
                if (!IsConscious(enemy)) continue;
                Vector3 toFlag = enemy.transform.position - campFlag.position;
                toFlag.y = 0f;
                if (toFlag.magnitude > captureRadius * 1.6f) continue;
                contested = true;
                break;
            }
        }

        if (inside && !contested)
        {
            captureProgress += Time.deltaTime / Mathf.Max(1f, captureSeconds);
        }
        else if (!inside)
        {
            captureProgress = Mathf.Max(0f, captureProgress - Time.deltaTime / (captureSeconds * 3f));
        }

        if (flagRenderer != null)
        {
            flagRenderer.material.color = Color.Lerp(SyndicateRed, AgencyTeal, captureProgress);
        }

        if (captureProgress >= 1f) CompleteCapture();
    }

    private void CompleteCapture()
    {
        State = DefenseState.Captured;
        capturedAt = Time.time;
        StopAllCoroutines();

        if (flagRenderer != null) flagRenderer.material.color = AgencyTeal;
        if (beaconRenderer != null) beaconRenderer.material.color = AgencyTeal;
        if (beaconLight != null) beaconLight.color = AgencyTeal;

        int reward = captureReward + Wave * 50;
        SecretLairManager.Instance?.AddCurrency(reward);
        player?.ShowActionMessage($"CAMP CAPTURED - BASE DEFENDED  +{reward}", 3f);

        // Everyone still standing falls back into the wilderness.
        RetreatAll(attackers);
        RetreatAll(garrison);
    }

    private void RetreatAll(List<EnemyOperative> list)
    {
        foreach (EnemyOperative enemy in list)
        {
            if (!IsConscious(enemy)) continue;
            EnemyCombatController combat = enemy.GetComponent<EnemyCombatController>();
            if (combat != null) combat.enabled = false;
            Vector3 away = enemy.transform.position - groundsCenter;
            away.y = 0f;
            DefenseRetreat retreat = enemy.gameObject.AddComponent<DefenseRetreat>();
            retreat.direction = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.back;
        }

        list.Clear();
    }

    private void ClearPreviousDefense()
    {
        StopAllCoroutines();
        if (campRoot != null) Destroy(campRoot);
        foreach (EnemyOperative enemy in attackers)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }

        foreach (EnemyOperative enemy in garrison)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }

        attackers.Clear();
        garrison.Clear();
        campFlag = null;

        foreach (GameObject scenery in clearedScenery)
        {
            if (scenery != null) scenery.SetActive(true);
        }

        clearedScenery.Clear();
    }

    // Hides the trees and rocks standing where the camp goes; they come back
    // when the camp is removed.
    private void ClearSceneryAt(Vector3 position, float radius)
    {
        foreach (Collider hit in Physics.OverlapSphere(position, radius, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform node = hit.transform;
            while (node != null && !node.name.StartsWith("Exterior Pine") && !node.name.StartsWith("Exterior Rock"))
            {
                node = node.parent;
            }

            if (node == null || !node.gameObject.activeSelf) continue;
            node.gameObject.SetActive(false);
            clearedScenery.Add(node.gameObject);
        }
    }

    // ---------------------------------------------------------------- HUD

    public string StatusText
    {
        get
        {
            if (State == DefenseState.Captured) return "BASE DEFENDED - CAMP CAPTURED";
            if (State != DefenseState.Active) return string.Empty;

            string text = $"BASE UNDER ATTACK\nWAVE {Mathf.Max(1, Wave)}   ATTACKERS {CountConscious(attackers)}";
            if (player != null && campFlag != null)
            {
                Vector3 toCamp = campFlag.position - player.transform.position;
                toCamp.y = 0f;
                text += $"\nENEMY CAMP  {toCamp.magnitude:0} m  {CompassName(toCamp)}";
                if (captureProgress > 0.001f)
                {
                    text += contested
                        ? $"\nCAPTURE {captureProgress * 100f:0}%  - CLEAR THE CAMP"
                        : $"\nCAPTURING CAMP  {captureProgress * 100f:0}%";
                }
            }

            return text;
        }
    }

    private static string CompassName(Vector3 direction)
    {
        float angle = Mathf.Repeat(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 360f);
        string[] names = { "NORTH", "NORTH-EAST", "EAST", "SOUTH-EAST", "SOUTH", "SOUTH-WEST", "WEST", "NORTH-WEST" };
        return names[Mathf.RoundToInt(angle / 45f) % 8];
    }
}
