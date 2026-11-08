using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyCombatController : MonoBehaviour
{
    public static event System.Action<Vector3, float, bool, string> NoiseBroadcast;

    public CharacterAnimationDriver animationDriver;
    public float detectionRange = 14f;
    public float disengageRange = 22f;
    public float moveSpeed = 3.5f;
    public float attackRange = 10f;
    public float attackDamage = 9f;
    public float attackCooldown = 1.1f;
    public float patrolRadius = 3f;
    public float instantConfirmationRange = 4.5f;
    public float investigationDuration = 4f;
    public float investigationSpeedMultiplier = 0.65f;
    public float bodyDiscoveryRange = 9f;
    public float bodyConfirmationRange = 5.5f;
    public float abilityCooldown = 5f;

    private EnemyOperative operative;
    private SpyPlayerController player;
    private Vector3 patrolOrigin;
    private Vector3 patrolTarget;
    private readonly List<Vector3> patrolRoute = new List<Vector3>();
    private int patrolRouteIndex;
    private float patrolHoldUntil;
    private float patrolLegStartedAt;
    private float patrolPauseDuration = 1.15f;
    private float nextAttackTime;
    private float nextSupportTime;
    private float suspicion;
    private float investigateUntil;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 searchAnchor;
    private Vector3 searchTarget;
    private Vector3 searchForward;
    private int searchStep;
    private float nextSearchStepTime;
    private bool searchSweepStarted;
    private bool alerted;
    private Renderer alertRenderer;
    private Renderer roleRenderer;
    private PrototypeAudioEmitter audioEmitter;
    private int attackSequence;
    private float nextAbilityTime;
    private float flankSign = 1f;
    private bool charging;
    private float suspicionGainMultiplier = 1f;
    private float nextBodyScanTime;
    private bool hasBeenObserved;
    private readonly HashSet<EnemyOperative> discoveredBodies = new HashSet<EnemyOperative>();
    private bool attackPending;
    private bool pendingAttackIsMelee;
    private float pendingAttackDamage;
    private float pendingAttackImpactAt;
    private float attackRecoveryUntil;
    private Color pendingAttackTraceColor;

    public bool IsAlerted => alerted;
    public bool IsInvestigating => !alerted && Time.time < investigateUntil;
    public bool HasBeenObserved => hasBeenObserved;
    public EnemyAwarenessState AwarenessState
    {
        get
        {
            if (alerted) return EnemyAwarenessState.Alerted;
            if (searchSweepStarted && Time.time < investigateUntil)
            {
                return EnemyAwarenessState.Searching;
            }
            if (suspicion > 0.03f || Time.time < investigateUntil)
            {
                return EnemyAwarenessState.Suspicious;
            }
            return EnemyAwarenessState.Calm;
        }
    }

    private void OnEnable()
    {
        NoiseBroadcast += HandleBroadcastNoise;
    }

    private void OnDisable()
    {
        NoiseBroadcast -= HandleBroadcastNoise;
    }

    public static void BroadcastNoise(
        Vector3 sourcePosition,
        float hearingRange,
        bool canTriggerImmediateAlert,
        string sourceLabel)
    {
        NoiseBroadcast?.Invoke(
            sourcePosition,
            Mathf.Max(0f, hearingRange),
            canTriggerImmediateAlert,
            sourceLabel);
    }

    public void Configure(EnemyOperative enemy)
    {
        operative = enemy;
        if (animationDriver == null) animationDriver = GetComponent<CharacterAnimationDriver>();
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(1f);
        patrolOrigin = transform.position;
        flankSign = Random.value < 0.5f ? -1f : 1f;
        TuneForRole();
        ChoosePatrolTarget();
        CreateAlertIndicator();
        CreateRoleIndicator();
    }

    public void ConfigurePatrolRoute(IList<Vector3> waypoints, float pauseDuration = 1.15f)
    {
        patrolRoute.Clear();
        if (waypoints != null)
        {
            for (int index = 0; index < waypoints.Count; index++)
            {
                Vector3 waypoint = waypoints[index];
                waypoint.y = transform.position.y;
                patrolRoute.Add(waypoint);
            }
        }

        patrolPauseDuration = Mathf.Max(0.2f, pauseDuration);
        patrolRouteIndex = 0;
        patrolHoldUntil = Time.time + Random.Range(0.2f, patrolPauseDuration);
        patrolLegStartedAt = Time.time;
        if (patrolRoute.Count > 0)
        {
            patrolOrigin = patrolRoute[0];
            patrolTarget = patrolRoute[0];
        }
        else
        {
            patrolOrigin = transform.position;
            ChoosePatrolTarget();
        }
    }

    private void Start()
    {
        if (operative == null) Configure(GetComponent<EnemyOperative>());
        player = FindAnyObjectByType<SpyPlayerController>();
    }

    public void HearNoise(
        Vector3 sourcePosition,
        float hearingRange,
        bool canTriggerImmediateAlert = true,
        string sourceLabel = "weapon discharge")
    {
        if (operative == null) Configure(GetComponent<EnemyOperative>());
        if (operative == null || operative.isDead || operative.isKnockedOut || operative.isRecruited) return;

        float distance = Vector3.Distance(transform.position, sourcePosition);
        if (distance > hearingRange) return;

        SetInvestigationTarget(sourcePosition, investigationDuration);
        suspicion = Mathf.Max(suspicion, Mathf.Lerp(0.24f, 0.68f, 1f - distance / Mathf.Max(0.1f, hearingRange)));

        if (alertRenderer != null)
        {
            alertRenderer.gameObject.SetActive(true);
            alertRenderer.material.color = new Color(1f, 0.62f, 0.08f);
        }

        if (!canTriggerImmediateAlert) return;

        float closeConfirmationRange = Mathf.Min(3.5f, hearingRange * 0.22f);
        if (distance > closeConfirmationRange) return;

        string alertSource = string.IsNullOrWhiteSpace(sourceLabel)
            ? "suspicious noise"
            : sourceLabel;
        MissionManager.Instance?.TriggerAlert(
            $"{operative.name} heard {alertSource} at close range");
        SetAlerted(true);
    }

    private void HandleBroadcastNoise(
        Vector3 sourcePosition,
        float hearingRange,
        bool canTriggerImmediateAlert,
        string sourceLabel)
    {
        HearNoise(sourcePosition, hearingRange, canTriggerImmediateAlert, sourceLabel);
    }

    private void Update()
    {
        if (operative == null || operative.isDead || operative.isKnockedOut || operative.isRecruited) return;
        if (player == null) player = FindAnyObjectByType<SpyPlayerController>();
        if (player == null) return;
        UpdatePlayerObservation();
        if (player.IsIncapacitated) return;
        ResolvePendingAttack();
        if (Time.time < attackRecoveryUntil)
        {
            Face(player.transform.position);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        MissionManager mission = MissionManager.Instance;
        if (mission != null && !mission.IsAlerted)
        {
            UpdateBodyAwareness(mission);
            if (!UpdateInvestigation(distance, mission)) return;
        }

        suspicion = 0f;
        investigateUntil = 0f;
        if (distance <= disengageRange)
        {
            SetAlerted(true);
        }
        else
        {
            SetAlerted(false);
            Patrol();
            return;
        }

        Engage(distance);
        if (operative.Specialization == OperativeSpecialization.Medic) SupportAllies();
    }

    private void UpdatePlayerObservation()
    {
        if (hasBeenObserved || player == null) return;
        Camera activeCamera = player.ActiveCamera;
        if (activeCamera == null || !activeCamera.enabled) return;

        Vector3 target = transform.position + Vector3.up * 1.2f;
        Vector3 direction = target - activeCamera.transform.position;
        float distance = direction.magnitude;
        if (distance > 45f || distance < 0.01f) return;
        if (Vector3.Dot(activeCamera.transform.forward, direction.normalized) < 0.22f) return;

        RaycastHit[] hits = Physics.RaycastAll(
            activeCamera.transform.position,
            direction.normalized,
            distance + 0.2f,
            ~0,
            QueryTriggerInteraction.Ignore);
        float closestDistance = float.MaxValue;
        Collider closestCollider = null;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(player.transform)) continue;
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestCollider = hit.collider;
            }
        }

        hasBeenObserved = closestCollider == null ||
            closestCollider.transform.IsChildOf(transform);
    }

    private void UpdateBodyAwareness(MissionManager mission)
    {
        if (Time.time < nextBodyScanTime) return;
        nextBodyScanTime = Time.time + Random.Range(0.55f, 0.8f);

        EnemyOperative[] candidates = FindObjectsByType<EnemyOperative>();
        foreach (EnemyOperative body in candidates)
        {
            if (body == null ||
                body == operative ||
                body.isRecruited ||
                body.isBeingCarried ||
                (!body.isKnockedOut && !body.isDead))
            {
                continue;
            }

            if (discoveredBodies.Contains(body)) continue;

            Vector3 toBody = body.transform.position - transform.position;
            float distance = toBody.magnitude;
            if (distance > bodyDiscoveryRange) continue;

            Vector3 flatDirection = new Vector3(toBody.x, 0f, toBody.z);
            if (flatDirection.sqrMagnitude > 0.001f &&
                Vector3.Dot(transform.forward, flatDirection.normalized) < 0.05f)
            {
                continue;
            }
            if (!HasLineOfSightTo(body.transform, 0.35f)) continue;

            SetInvestigationTarget(body.transform.position, investigationDuration + 1.5f);
            suspicion = Mathf.Max(suspicion, 0.78f);
            if (alertRenderer != null)
            {
                alertRenderer.gameObject.SetActive(true);
                alertRenderer.material.color = new Color(1f, 0.42f, 0.08f);
            }

            if (distance <= bodyConfirmationRange)
            {
                discoveredBodies.Add(body);
                mission.TriggerAlert($"{operative.name} found an operative down");
                SetAlerted(true);
            }
            return;
        }
    }

    private bool UpdateInvestigation(float distance, MissionManager mission)
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        float viewDot = toPlayer.sqrMagnitude > 0.001f
            ? Vector3.Dot(transform.forward, toPlayer.normalized)
            : 1f;
        float effectiveDetectionRange = detectionRange * player.StealthVisibilityMultiplier;
        bool canSeePlayer = distance <= effectiveDetectionRange &&
            viewDot >= 0.2f &&
            HasLineOfSight();

        if (canSeePlayer)
        {
            SetInvestigationTarget(player.transform.position, investigationDuration);

            if (distance <= instantConfirmationRange)
            {
                mission.TriggerAlert($"{operative.name} confirmed the agent at close range");
                SetAlerted(true);
                return true;
            }

            float distancePressure = Mathf.InverseLerp(
                effectiveDetectionRange,
                instantConfirmationRange,
                distance);
            float suspicionRate = Mathf.Lerp(0.18f, 0.55f, distancePressure) *
                suspicionGainMultiplier;
            suspicion = Mathf.Min(0.85f, suspicion + Time.deltaTime * suspicionRate);
        }
        else
        {
            suspicion = Mathf.Clamp01(suspicion - Time.deltaTime * 0.22f);
        }

        if (alertRenderer != null)
        {
            bool showInvestigation = suspicion > 0.03f || Time.time < investigateUntil;
            alertRenderer.gameObject.SetActive(showInvestigation);
            alertRenderer.material.color = Color.Lerp(
                new Color(0.95f, 0.75f, 0.18f),
                new Color(1f, 0.48f, 0.1f),
                suspicion / 0.85f);
        }

        if (Time.time < investigateUntil)
        {
            Vector3 flatLastKnown = new Vector3(
                lastKnownPlayerPosition.x,
                transform.position.y,
                lastKnownPlayerPosition.z);
            float distanceToLastKnown = Vector3.Distance(transform.position, flatLastKnown);
            if (distanceToLastKnown > 0.6f)
            {
                MoveToward(flatLastKnown, moveSpeed * investigationSpeedMultiplier);
            }
            else
            {
                SearchLastKnownArea(flatLastKnown);
            }
        }
        else
        {
            Patrol();
        }

        return false;
    }

    private void SetInvestigationTarget(Vector3 target, float duration)
    {
        bool investigationWasInactive = Time.time >= investigateUntil;
        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
        Vector3 flatLastKnown = new Vector3(
            lastKnownPlayerPosition.x,
            transform.position.y,
            lastKnownPlayerPosition.z);
        bool targetChanged = Vector3.SqrMagnitude(flatTarget - flatLastKnown) > 0.25f;

        lastKnownPlayerPosition = target;
        investigateUntil = Mathf.Max(investigateUntil, Time.time + duration);
        if (!investigationWasInactive && !targetChanged) return;

        searchAnchor = flatTarget;
        searchTarget = flatTarget;
        searchForward = transform.forward;
        searchForward.y = 0f;
        if (searchForward.sqrMagnitude < 0.01f) searchForward = Vector3.forward;
        searchForward.Normalize();
        searchStep = 0;
        nextSearchStepTime = 0f;
        searchSweepStarted = false;
    }

    private void SearchLastKnownArea(Vector3 flatLastKnown)
    {
        if (!searchSweepStarted)
        {
            searchSweepStarted = true;
            searchAnchor = flatLastKnown;
            searchTarget = flatLastKnown;
            searchForward = transform.forward;
            searchForward.y = 0f;
            if (searchForward.sqrMagnitude < 0.01f) searchForward = Vector3.forward;
            searchForward.Normalize();
            nextSearchStepTime = Time.time + 0.3f;
            investigateUntil = Mathf.Max(investigateUntil, Time.time + 2.4f);
        }

        float distanceToSearchTarget = Vector3.Distance(transform.position, searchTarget);
        if (Time.time >= nextSearchStepTime || distanceToSearchTarget < 0.38f)
        {
            Vector3 right = Vector3.Cross(Vector3.up, searchForward).normalized;
            searchStep = (searchStep + 1) % 4;
            switch (searchStep)
            {
                case 0:
                    searchTarget = searchAnchor + searchForward * 1.65f;
                    break;
                case 1:
                    searchTarget = searchAnchor + right * 1.45f;
                    break;
                case 2:
                    searchTarget = searchAnchor - searchForward * 1.35f;
                    break;
                default:
                    searchTarget = searchAnchor - right * 1.45f;
                    break;
            }
            searchTarget.y = transform.position.y;
            nextSearchStepTime = Time.time + Random.Range(0.95f, 1.35f);
            distanceToSearchTarget = Vector3.Distance(transform.position, searchTarget);
        }

        if (distanceToSearchTarget > 0.4f)
        {
            MoveToward(searchTarget, moveSpeed * investigationSpeedMultiplier * 0.72f);
        }
        else
        {
            Face(searchAnchor + searchForward);
        }
    }

    private void Patrol()
    {
        Vector3 flatTarget = new Vector3(patrolTarget.x, transform.position.y, patrolTarget.z);
        if (patrolRoute.Count > 0)
        {
            if (Time.time < patrolHoldUntil)
            {
                Face(flatTarget);
                return;
            }

            bool reachedWaypoint = Vector3.Distance(transform.position, flatTarget) < 0.45f;
            bool routeBlocked = Time.time - patrolLegStartedAt > 9f;
            if (reachedWaypoint || routeBlocked)
            {
                AdvancePatrolRoute();
                return;
            }

            MoveToward(flatTarget, moveSpeed * 0.45f);
            return;
        }

        if (Vector3.Distance(transform.position, flatTarget) < 0.35f)
        {
            ChoosePatrolTarget();
            return;
        }

        MoveToward(flatTarget, moveSpeed * 0.45f);
    }

    private void AdvancePatrolRoute()
    {
        if (patrolRoute.Count == 0)
        {
            ChoosePatrolTarget();
            return;
        }

        patrolRouteIndex = (patrolRouteIndex + 1) % patrolRoute.Count;
        patrolTarget = patrolRoute[patrolRouteIndex];
        patrolHoldUntil = Time.time + Random.Range(
            patrolPauseDuration * 0.72f,
            patrolPauseDuration * 1.28f);
        patrolLegStartedAt = patrolHoldUntil;
    }

    private void Engage(float distance)
    {
        bool melee = operative.Specialization == OperativeSpecialization.HandToHand ||
            operative.Specialization == OperativeSpecialization.Stealth ||
            operative.Specialization == OperativeSpecialization.Tank;

        switch (operative.Specialization)
        {
            case OperativeSpecialization.Stealth:
                EngageStealth(distance);
                break;
            case OperativeSpecialization.Tank:
                EngageTank(distance);
                break;
            case OperativeSpecialization.Marksman:
                MaintainRange(distance, attackRange * 0.62f, attackRange * 0.94f);
                break;
            case OperativeSpecialization.Hacker:
                MaintainRange(distance, attackRange * 0.48f, attackRange * 0.84f);
                break;
            case OperativeSpecialization.Medic:
                MaintainRange(distance, attackRange * 0.5f, attackRange * 0.82f);
                break;
            default:
                ChaseMelee(distance);
                break;
        }

        if (distance <= attackRange && Time.time >= nextAttackTime && HasLineOfSight())
        {
            Attack(melee);
        }
    }

    private void ChaseMelee(float distance)
    {
        if (distance > attackRange * 0.86f)
        {
            MoveToward(player.transform.position, moveSpeed);
        }
        else
        {
            Face(player.transform.position);
        }
    }

    private void EngageStealth(float distance)
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        Vector3 flankTarget = player.transform.position -
            player.transform.forward * 1.15f +
            player.transform.right * flankSign * 1.5f;

        if (distance > attackRange * 0.8f)
        {
            MoveToward(flankTarget, moveSpeed);
        }
        else
        {
            Face(player.transform.position);
        }

        if (toPlayer.sqrMagnitude < 1f) flankSign *= -1f;
    }

    private void EngageTank(float distance)
    {
        if (!charging && Time.time >= nextAbilityTime && distance > attackRange * 1.35f && distance < 9f)
        {
            charging = true;
            nextAbilityTime = Time.time + abilityCooldown;
            audioEmitter?.PlayAbility();
            SpawnRolePulse(GetRoleColor(), 0.55f);
        }

        if (charging)
        {
            MoveToward(player.transform.position, moveSpeed * 2.2f);
            if (distance <= attackRange || distance > 10f)
            {
                charging = false;
            }
        }
        else
        {
            ChaseMelee(distance);
        }
    }

    private void MaintainRange(float distance, float preferredMinimum, float preferredMaximum)
    {
        if (distance > preferredMaximum)
        {
            MoveToward(player.transform.position, moveSpeed);
        }
        else if (distance < preferredMinimum)
        {
            Vector3 away = transform.position - player.transform.position;
            away.y = 0f;
            Vector3 retreatTarget = transform.position + away.normalized * 2.5f +
                transform.right * flankSign * 0.8f;
            MoveToward(retreatTarget, moveSpeed * 0.8f);
        }
        else
        {
            Face(player.transform.position);
        }
    }

    private void Attack(bool melee)
    {
        nextAttackTime = Time.time + attackCooldown;
        CharacterActionAnimation action = CharacterActionAnimation.Shoot;
        if (melee)
        {
            bool useBlade = operative != null &&
                (operative.Specialization == OperativeSpecialization.Stealth ||
                 (operative.Specialization == OperativeSpecialization.HandToHand &&
                  (int)operative.Rank >= (int)EnemyRank.Lieutenant &&
                  attackSequence % 2 == 1));
            action = useBlade ? CharacterActionAnimation.Stab : CharacterActionAnimation.Punch;
        }
        attackSequence++;
        animationDriver?.PlayAction(action);
        audioEmitter?.PlayEnemyAttack(melee);

        pendingAttackDamage = attackDamage;
        if (operative.Specialization == OperativeSpecialization.Tank && charging)
        {
            pendingAttackDamage *= 1.35f;
            charging = false;
        }

        pendingAttackIsMelee = melee;
        pendingAttackTraceColor = melee ? new Color(1f, 0.2f, 0.12f) : GetRoleColor();
        float impactDelay = action == CharacterActionAnimation.Stab
            ? 0.24f
            : action == CharacterActionAnimation.Punch
                ? 0.18f
                : 0.08f;
        float recoveryDuration = action == CharacterActionAnimation.Stab
            ? 0.62f
            : action == CharacterActionAnimation.Punch
                ? 0.5f
                : 0.34f;
        pendingAttackImpactAt = Time.time + impactDelay;
        attackRecoveryUntil = Time.time + recoveryDuration;
        attackPending = true;
    }

    private void ResolvePendingAttack()
    {
        if (!attackPending || Time.time < pendingAttackImpactAt) return;
        attackPending = false;
        if (player == null || player.IsIncapacitated) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        float allowedRange = attackRange * (pendingAttackIsMelee ? 1.08f : 1.2f);
        if (distance > allowedRange || !HasLineOfSight()) return;

        player.ApplyDamage(
            pendingAttackDamage,
            operative.Specialization.ToString(),
            transform.position);

        if (operative.Specialization == OperativeSpecialization.Hacker &&
            Time.time >= nextAbilityTime)
        {
            nextAbilityTime = Time.time + abilityCooldown;
            player.ApplyMovementDisruption(0.5f, 2.5f, "MOVEMENT SYSTEM JAMMED");
            audioEmitter?.PlayAbility();
            SpawnRolePulse(GetRoleColor(), 0.8f);
        }

        Vector3 start = transform.position + Vector3.up * 1.25f;
        Vector3 end = player.transform.position + Vector3.up * 0.75f;
        SpawnAttackTrace(start, end, pendingAttackTraceColor);
    }

    private void SupportAllies()
    {
        if (Time.time < nextSupportTime) return;
        nextSupportTime = Time.time + 4f;

        EnemyOperative[] enemies = FindObjectsByType<EnemyOperative>();
        foreach (EnemyOperative ally in enemies)
        {
            if (ally == null || ally == operative || ally.isDead || ally.isKnockedOut || ally.isRecruited) continue;
            if (Vector3.Distance(transform.position, ally.transform.position) > 7f) continue;
            ally.sedation = Mathf.Max(0f, ally.sedation - 18f);
            ally.currentHealth = Mathf.Min(ally.MaxHealth, ally.currentHealth + 15f);
        }
        operative.currentHealth = Mathf.Min(operative.MaxHealth, operative.currentHealth + 10f);
        audioEmitter?.PlayAbility();
        SpawnRolePulse(GetRoleColor(), 0.75f);
    }

    private void MoveToward(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        direction.Normalize();
        Face(transform.position + direction);

        float distance = speed * Time.deltaTime;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(origin, 0.35f, direction, distance + 0.1f, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (player != null && hit.collider.transform.IsChildOf(player.transform)) continue;
            return;
        }

        transform.position += direction * distance;
    }

    private void Face(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 target = player.transform.position + Vector3.up * 0.75f;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        if (distance < 0.01f) return true;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, distance + 0.2f, ~0, QueryTriggerInteraction.Ignore);
        float closestDistance = float.MaxValue;
        Collider closestCollider = null;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestCollider = hit.collider;
            }
        }

        return closestCollider == null || closestCollider.transform.IsChildOf(player.transform);
    }

    private bool HasLineOfSightTo(Transform targetTransform, float targetHeight)
    {
        if (targetTransform == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 target = targetTransform.position + Vector3.up * targetHeight;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        if (distance < 0.01f) return true;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction.normalized,
            distance + 0.2f,
            ~0,
            QueryTriggerInteraction.Ignore);
        float closestDistance = float.MaxValue;
        Collider closestCollider = null;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestCollider = hit.collider;
            }
        }

        return closestCollider == null || closestCollider.transform.IsChildOf(targetTransform);
    }

    private void SetAlerted(bool value)
    {
        if (alerted == value && (alertRenderer == null || alertRenderer.gameObject.activeSelf == value)) return;
        alerted = value;
        if (alertRenderer != null)
        {
            alertRenderer.material.color = value ? new Color(1f, 0.15f, 0.12f) : new Color(0.95f, 0.75f, 0.18f);
            alertRenderer.gameObject.SetActive(value);
        }
    }

    private void TuneForRole()
    {
        if (operative == null) return;

        switch (operative.Specialization)
        {
            case OperativeSpecialization.HandToHand:
                moveSpeed = 5.2f;
                attackRange = 1.8f;
                attackDamage = 12f;
                attackCooldown = 0.8f;
                break;
            case OperativeSpecialization.Stealth:
                moveSpeed = 5.8f;
                detectionRange = 13f;
                attackRange = 2.15f;
                attackDamage = 15f;
                attackCooldown = 0.75f;
                abilityCooldown = 4f;
                break;
            case OperativeSpecialization.Tank:
                moveSpeed = 2.7f;
                detectionRange = 16f;
                attackRange = 2.2f;
                attackDamage = 22f;
                attackCooldown = 1.35f;
                abilityCooldown = 5.5f;
                break;
            case OperativeSpecialization.Marksman:
                moveSpeed = 3.2f;
                detectionRange = 24f;
                disengageRange = 30f;
                attackRange = 18f;
                attackDamage = 16f;
                attackCooldown = 1.5f;
                abilityCooldown = 6f;
                break;
            case OperativeSpecialization.Hacker:
                moveSpeed = 3.6f;
                detectionRange = 18f;
                attackRange = 12f;
                attackDamage = 11f;
                attackCooldown = 1.05f;
                abilityCooldown = 5f;
                break;
            case OperativeSpecialization.Medic:
                moveSpeed = 3.8f;
                detectionRange = 16f;
                attackRange = 10f;
                attackDamage = 8f;
                attackCooldown = 1.2f;
                abilityCooldown = 4f;
                break;
        }

        float rankMultiplier = 1f + (int)operative.Rank * 0.12f;
        attackDamage *= rankMultiplier;
        detectionRange += (int)operative.Rank;

        MissionDifficultyProfile difficulty = GameFlowManager.Instance != null
            ? GameFlowManager.Instance.GetDifficultyProfile()
            : MissionDifficultyCatalog.Get(MissionDifficulty.SecretAgent);
        attackDamage *= difficulty.enemyDamageMultiplier;
        detectionRange *= difficulty.enemyAwarenessMultiplier;
        instantConfirmationRange *= Mathf.Lerp(
            0.9f,
            1.15f,
            Mathf.InverseLerp(0.75f, 1.3f, difficulty.enemyAwarenessMultiplier));
        suspicionGainMultiplier = difficulty.enemyAwarenessMultiplier;
        disengageRange = Mathf.Max(disengageRange, detectionRange + 7f);
    }

    private void ChoosePatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = patrolOrigin + new Vector3(offset.x, 0f, offset.y);
        patrolLegStartedAt = Time.time;
    }

    private void CreateAlertIndicator()
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "Alert Indicator";
        indicator.transform.SetParent(transform, false);
        indicator.transform.localPosition = new Vector3(0f, 2.45f, 0f);
        indicator.transform.localScale = Vector3.one * 0.18f;
        Collider indicatorCollider = indicator.GetComponent<Collider>();
        if (indicatorCollider != null) Destroy(indicatorCollider);
        alertRenderer = indicator.GetComponent<Renderer>();
        indicator.SetActive(false);
    }

    private void CreateRoleIndicator()
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = $"{operative.Specialization} Role Indicator";
        indicator.transform.SetParent(transform, false);
        indicator.transform.localPosition = new Vector3(0f, 0.035f, 0f);
        indicator.transform.localScale = new Vector3(0.72f, 0.025f, 0.72f);
        Collider indicatorCollider = indicator.GetComponent<Collider>();
        if (indicatorCollider != null) Destroy(indicatorCollider);
        roleRenderer = indicator.GetComponent<Renderer>();
        if (roleRenderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = GetRoleColor();
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", GetRoleColor() * 0.8f);
            roleRenderer.material = material;
        }
    }

    private Color GetRoleColor()
    {
        return operative != null
            ? GetSpecializationColor(operative.Specialization)
            : new Color(1f, 0.72f, 0.15f);
    }

    public static Color GetSpecializationColor(OperativeSpecialization specialization)
    {
        switch (specialization)
        {
            case OperativeSpecialization.HandToHand: return new Color(0.95f, 0.22f, 0.16f);
            case OperativeSpecialization.Stealth: return new Color(0.55f, 0.28f, 0.9f);
            case OperativeSpecialization.Tank: return new Color(0.95f, 0.55f, 0.12f);
            case OperativeSpecialization.Marksman: return new Color(0.2f, 0.72f, 1f);
            case OperativeSpecialization.Hacker: return new Color(0.15f, 1f, 0.72f);
            case OperativeSpecialization.Medic: return new Color(0.35f, 1f, 0.42f);
            default: return Color.white;
        }
    }

    private void SpawnRolePulse(Color color, float duration)
    {
        GameObject pulseObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pulseObject.name = $"{operative.Specialization} Ability Pulse";
        pulseObject.transform.position = transform.position + Vector3.up * 1f;
        pulseObject.transform.localScale = Vector3.one * 0.35f;
        Collider pulseCollider = pulseObject.GetComponent<Collider>();
        if (pulseCollider != null) Destroy(pulseCollider);
        Renderer pulseRenderer = pulseObject.GetComponent<Renderer>();
        if (pulseRenderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            material.color = color;
            pulseRenderer.material = material;
            Destroy(material, duration);
        }

        AbilityPulse pulse = pulseObject.AddComponent<AbilityPulse>();
        pulse.Configure(duration, 3.5f);
    }

    private void SpawnAttackTrace(Vector3 start, Vector3 end, Color color)
    {
        GameObject traceObject = new GameObject("Enemy Attack Trace");
        LineRenderer trace = traceObject.AddComponent<LineRenderer>();
        trace.useWorldSpace = true;
        trace.positionCount = 2;
        trace.SetPosition(0, start);
        trace.SetPosition(1, end);
        trace.startWidth = 0.045f;
        trace.endWidth = 0.015f;
        trace.startColor = color;
        trace.endColor = color;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            trace.material = new Material(shader);
            trace.material.color = color;
            Destroy(trace.material, 0.2f);
        }
        Destroy(traceObject, 0.16f);
    }
}

public enum EnemyAwarenessState
{
    Calm,
    Suspicious,
    Searching,
    Alerted
}
