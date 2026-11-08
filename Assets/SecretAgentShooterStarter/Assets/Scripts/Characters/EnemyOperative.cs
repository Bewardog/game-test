using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyOperative : MonoBehaviour, IInteractable
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorFactorId = Shader.PropertyToID("_BaseColorFactor");

    public static event Action<EnemyOperative> AnySpawned;
    public static event Action<EnemyOperative> AnyKnockedOut;
    public static event Action<EnemyOperative> AnyKilled;
    public static event Action<EnemyOperative> AnyRecruited;

    [Header("Profile")]
    public OperativeProfile profile;
    public string visualResourcePath;
    public EnemyRank fallbackRank = EnemyRank.Grunt;
    public OperativeSpecialization fallbackSpecialization = OperativeSpecialization.HandToHand;

    [Header("Runtime")]
    public float currentHealth;
    public float sedation;
    public bool isKnockedOut;
    public bool isDead;
    public bool isRecruited;
    public bool isBeingCarried;

    [Header("Carry Pose Offsets")]
    public Vector3 bridalCarryLocalPosition = new Vector3(0.55f, -0.15f, 1.15f);
    public Vector3 bridalCarryLocalEuler = new Vector3(0f, 0f, 90f);
    public Vector3 shoulderCarryLocalPosition = new Vector3(0.45f, 0.35f, 0.45f);
    public Vector3 shoulderCarryFirstPersonLocalPosition = new Vector3(0.75f, 0.35f, 1f);
    public Vector3 shoulderCarryLocalEuler = new Vector3(-90f, 0f, 0f);
    public Vector3 riggedBridalHipsLocalPosition = new Vector3(0.5f, -0.08f, 0.75f);
    public Vector3 riggedShoulderHipsLocalPosition = new Vector3(0.22f, 0.45f, 0f);
    public Vector3 riggedShoulderFirstPersonHipsLocalPosition = new Vector3(0.3f, 0.45f, 0.65f);

    [Header("Disable On Knockout")]
    public MonoBehaviour[] aiBehavioursToDisable = new MonoBehaviour[0];
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;

    private Renderer[] cachedRenderers;
    private Color[] baseRendererColors;
    private Coroutine flashRoutine;
    private readonly Dictionary<string, Transform> carryBones = new Dictionary<string, Transform>();
    private readonly List<CachedBonePose> bindPose = new List<CachedBonePose>();
    private Vector3 bindBodyAxisLocal;
    private Vector3 bindForwardAxisLocal;
    private Vector3 bindHeadForwardAxisLocal;
    private bool hasBindAxes;
    private bool hasBindHeadForwardAxis;
    private CarryStyle activeCarryStyle;
    private bool carryUsesFirstPersonPresentation;
    private Transform parentBeforeCarry;
    private CharacterAnimationDriver animationDriver;
    private float difficultyHealthMultiplier = 1f;
    private float difficultySedationThresholdMultiplier = 1f;

    private sealed class CachedBonePose
    {
        public Transform bone;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    public EnemyRank Rank => profile != null ? profile.rank : fallbackRank;
    public OperativeSpecialization Specialization => profile != null ? profile.specialization : fallbackSpecialization;
    public float MaxHealth => (profile != null ? profile.maxHealth : GetFallbackMaxHealth()) *
        difficultyHealthMultiplier;
    public float SedationThreshold => (profile != null ? profile.sedationThreshold : GetFallbackSedationThreshold()) *
        difficultySedationThresholdMultiplier;
    public float SedationResistance => profile != null ? profile.sedationResistance : GetFallbackSedationResistance();
    public int RecruitmentValue => profile != null ? profile.recruitmentValue : GetFallbackRecruitmentValue();

    public string InteractionPrompt
    {
        get
        {
            if (isRecruited) return string.Empty;
            if (isDead) return string.Empty;
            if (isKnockedOut && !isBeingCarried) return "Carry operative";
            return string.Empty;
        }
    }

    private void Awake()
    {
        currentHealth = MaxHealth;
        CacheCarryRig();
        CacheRagdollIfNeeded();
        CacheRendererColors();
        SetRagdoll(false);
    }

    private void Start()
    {
        AnySpawned?.Invoke(this);
    }

    public void ApplySedation(float amount)
    {
        if (isDead || isKnockedOut || isRecruited) return;

        if (amount > 0f) MissionManager.Instance?.TriggerAlert($"{name} was attacked");
        sedation += Mathf.Max(0f, amount - SedationResistance);
        PlayFlash(new Color(0.2f, 0.95f, 1f), 0.1f);
        GetAnimationDriver()?.PlayAction(CharacterActionAnimation.Hit);

        if (sedation >= SedationThreshold)
        {
            KnockOut();
        }
    }

    private Transform headBone;
    private bool headBoneSearched;

    // Unaware: the mission isn't on alert and this operative hasn't spotted anyone.
    public bool IsUnaware()
    {
        if (isDead || isKnockedOut || isRecruited) return false;
        if (MissionManager.Instance != null && MissionManager.Instance.IsAlerted) return false;
        EnemyCombatController combat = GetComponent<EnemyCombatController>();
        return combat == null || !combat.IsAlerted;
    }

    // A hit counts as a headshot when it lands at or above the base of the
    // skull. Uses the Head bone when the model has one, otherwise the top of
    // the body collider.
    public bool IsHeadshot(Vector3 hitPoint)
    {
        if (!headBoneSearched)
        {
            headBoneSearched = true;
            headBone = FindNamedBone(GetComponentsInChildren<Transform>(true), "Head");
        }

        if (headBone != null)
        {
            return hitPoint.y >= headBone.position.y - 0.05f * transform.lossyScale.y;
        }

        Collider body = GetComponent<Collider>();
        if (body == null) return false;
        Bounds bounds = body.bounds;
        return hitPoint.y >= bounds.max.y - bounds.size.y * 0.15f;
    }

    // Tranquilizer dart hit. A headshot sedates instantly (commanders take
    // triple sedation instead). A hit on an unaware target counts double, and
    // knocking out an unaware target doesn't raise the alarm.
    public bool ReceiveTranquilizerDart(float amount, Vector3 hitPoint, out bool headshot)
    {
        headshot = false;
        if (isDead || isKnockedOut || isRecruited) return false;

        headshot = IsHeadshot(hitPoint);
        bool unaware = IsUnaware();
        if (headshot && Rank != EnemyRank.Commander)
        {
            if (!unaware) MissionManager.Instance?.TriggerAlert($"{name} was attacked");
            sedation = SedationThreshold;
            GetAnimationDriver()?.PlayAction(CharacterActionAnimation.Hit);
            KnockOut();
            return true;
        }

        float multiplier = (headshot ? 3f : 1f) * (unaware ? 2f : 1f);
        ApplySedation(amount * multiplier);
        return isKnockedOut;
    }

    public bool CanReceiveSilentTakedown(SpyPlayerController player)
    {
        if (player == null || isDead || isKnockedOut || isRecruited || isBeingCarried) return false;
        if (Rank == EnemyRank.Commander) return false;
        if (MissionManager.Instance != null && MissionManager.Instance.IsAlerted) return false;

        EnemyCombatController combat = GetComponent<EnemyCombatController>();
        if (combat != null && combat.IsAlerted) return false;

        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return true;
        return Vector3.Dot(transform.forward, toPlayer.normalized) <= -0.42f;
    }

    public bool ReceiveSilentTakedown(SpyPlayerController player)
    {
        if (!CanReceiveSilentTakedown(player)) return false;

        sedation = SedationThreshold;
        GetAnimationDriver()?.PlayAction(CharacterActionAnimation.Hit);
        KnockOut();
        return true;
    }

    public void ApplyDifficulty(MissionDifficultyProfile difficulty)
    {
        if (difficulty == null) return;
        difficultyHealthMultiplier = difficulty.enemyDurabilityMultiplier;
        difficultySedationThresholdMultiplier = difficulty.enemySedationThresholdMultiplier;
        currentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isRecruited) return;

        if (amount > 0f) MissionManager.Instance?.TriggerAlert($"{name} was attacked");
        currentHealth -= amount;
        GetAnimationDriver()?.PlayAction(CharacterActionAnimation.Hit);

        if (currentHealth <= 0f)
        {
            Kill();
        }
    }

    public void KnockOut()
    {
        if (isDead || isKnockedOut || isRecruited) return;

        isKnockedOut = true;
        DisableAI();
        SetRagdoll(true);

        if (HasRagdoll())
        {
            ApplyKnockoutImpulse();
        }
        else
        {
            ApplyPrototypeKnockoutPose();
        }

        PlayFlash(new Color(1f, 0.25f, 0.7f), 0.22f);
        Debug.Log($"{name} is sedated and ready to carry.");
        AnyKnockedOut?.Invoke(this);
    }

    public void Kill()
    {
        if (isDead || isRecruited) return;

        isDead = true;
        DisableAI();
        SetRagdoll(true);
        ApplyKnockoutImpulse();
        AnyKilled?.Invoke(this);
    }

    public bool BeginCarry(Transform carrySocket, CarryStyle style)
    {
        if (!isKnockedOut || isDead || isRecruited || carrySocket == null) return false;

        isBeingCarried = true;
        parentBeforeCarry = transform.parent;
        SetRagdoll(false);
        RestoreBindPose();
        SetColliders(false);

        transform.SetParent(carrySocket, false);
        UpdateCarryPose(style);
        return true;
    }

    public void ConfigureRagdoll(Rigidbody[] bodies, Collider[] colliders)
    {
        ragdollBodies = bodies ?? new Rigidbody[0];
        ragdollColliders = colliders ?? new Collider[0];
        CacheCarryRig();
        SetRagdoll(false);
    }

    public void UpdateCarryPose(CarryStyle style)
    {
        if (!isBeingCarried) return;

        activeCarryStyle = style;

        if (HasRiggedCarryPose())
        {
            RestoreBindPose();
            ApplyRiggedRootPose(style);
        }
        else if (style == CarryStyle.BridalCarry)
        {
            transform.localPosition = bridalCarryLocalPosition;
            Quaternion bridalRotation = Quaternion.Euler(bridalCarryLocalEuler);
            Quaternion faceUpRotation = Quaternion.AngleAxis(90f, Vector3.up);
            transform.localRotation = bridalRotation * faceUpRotation;
        }
        else
        {
            transform.localPosition = carryUsesFirstPersonPresentation
                ? shoulderCarryFirstPersonLocalPosition
                : shoulderCarryLocalPosition;
            Quaternion shoulderRotation = Quaternion.Euler(shoulderCarryLocalEuler);
            Quaternion stomachDownRotation = Quaternion.AngleAxis(180f, Vector3.up);
            transform.localRotation = shoulderRotation * stomachDownRotation;
        }

        ApplyRiggedCarryPose(style);
    }

    public void SetCarryCameraMode(bool firstPerson)
    {
        carryUsesFirstPersonPresentation = firstPerson;

        if (cachedRenderers == null) return;

        foreach (Renderer bodyRenderer in cachedRenderers)
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.forceRenderingOff = false;
            }
        }

        if (isBeingCarried)
        {
            UpdateCarryPose(activeCarryStyle);
        }
    }

    public void EndCarry(Vector3 dropPosition)
    {
        EndCarry(dropPosition, Vector3.zero, Vector3.zero);
    }

    public void EndCarry(Vector3 dropPosition, Vector3 impulse, Vector3 torque)
    {
        if (!isBeingCarried) return;

        isBeingCarried = false;
        SetCarryCameraMode(false);
        transform.SetParent(parentBeforeCarry, true);
        parentBeforeCarry = null;
        transform.position = dropPosition;

        if (HasRagdoll())
        {
            RestoreBindPose();
            SetRagdoll(true);
            ApplyCarryReleaseImpulse(impulse, torque);
        }
        else
        {
            ApplyPrototypeKnockoutPose();
            SetColliders(true);
        }
    }

    private void ApplyCarryReleaseImpulse(Vector3 impulse, Vector3 torque)
    {
        if (impulse.sqrMagnitude < 0.000001f && torque.sqrMagnitude < 0.000001f) return;

        Rigidbody releaseBody = null;
        if (carryBones.TryGetValue("Hips", out Transform hips))
        {
            releaseBody = hips.GetComponent<Rigidbody>();
        }

        if (releaseBody == null && ragdollBodies != null)
        {
            foreach (Rigidbody body in ragdollBodies)
            {
                if (body == null || body.isKinematic) continue;
                if (releaseBody == null || body.mass > releaseBody.mass) releaseBody = body;
            }
        }

        if (releaseBody == null || releaseBody.isKinematic) return;
        releaseBody.AddForce(impulse, ForceMode.VelocityChange);
        releaseBody.AddTorque(torque, ForceMode.VelocityChange);
    }

    public void Recruit()
    {
        if (!isKnockedOut || isDead || isRecruited) return;

        isRecruited = true;
        isBeingCarried = false;
        SetCarryCameraMode(false);
        gameObject.SetActive(false);
        AnyRecruited?.Invoke(this);
    }

    public void Interact(SpyPlayerController player)
    {
        if (player == null || !isKnockedOut || isBeingCarried || isDead || isRecruited) return;
        player.StartCarrying(this);
    }

    private void DisableAI()
    {
        if (aiBehavioursToDisable == null) return;

        foreach (MonoBehaviour behaviour in aiBehavioursToDisable)
        {
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }
    }

    private float GetFallbackMaxHealth()
    {
        float rankBonus = (int)Rank * 20f;
        float roleBonus = Specialization == OperativeSpecialization.Tank ? 80f : 0f;
        return 100f + rankBonus + roleBonus;
    }

    private float GetFallbackSedationThreshold()
    {
        float rankBonus = (int)Rank * 15f;
        float roleBonus = Specialization == OperativeSpecialization.Tank ? 55f :
            Specialization == OperativeSpecialization.Medic ? 20f : 0f;
        return 100f + rankBonus + roleBonus;
    }

    private float GetFallbackSedationResistance()
    {
        float roleResistance = Specialization == OperativeSpecialization.Tank ? 8f :
            Specialization == OperativeSpecialization.Stealth ? 3f : 0f;
        return roleResistance + (int)Rank * 1.5f;
    }

    private int GetFallbackRecruitmentValue()
    {
        return 100 + (int)Rank * 75;
    }

    private void CacheRagdollIfNeeded()
    {
        if (ragdollBodies == null || ragdollBodies.Length == 0)
        {
            ragdollBodies = GetComponentsInChildren<Rigidbody>();
        }

        if (ragdollColliders == null || ragdollColliders.Length == 0)
        {
            List<Collider> bodyColliders = new List<Collider>();
            foreach (Rigidbody body in ragdollBodies)
            {
                if (body != null)
                {
                    bodyColliders.AddRange(body.GetComponents<Collider>());
                }
            }

            ragdollColliders = bodyColliders.ToArray();
        }
    }

    private void SetRagdoll(bool active)
    {
        if (ragdollBodies != null)
        {
            foreach (Rigidbody body in ragdollBodies)
            {
                if (body == null) continue;

                if (active)
                {
                    body.interpolation = RigidbodyInterpolation.Interpolate;
                    body.detectCollisions = true;
                    body.isKinematic = false;
                    body.WakeUp();
                }
                else
                {
                    if (!body.isKinematic)
                    {
                        body.linearVelocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                    }

                    body.isKinematic = true;
                    body.detectCollisions = false;
                    body.interpolation = RigidbodyInterpolation.None;
                }
            }
        }

        HashSet<Collider> ragdollSet = new HashSet<Collider>();
        if (ragdollColliders != null)
        {
            foreach (Collider ragdollCollider in ragdollColliders)
            {
                if (ragdollCollider == null) continue;
                ragdollSet.Add(ragdollCollider);
                ragdollCollider.enabled = active;
            }
        }

        foreach (Collider bodyCollider in GetComponentsInChildren<Collider>(true))
        {
            if (bodyCollider != null && !ragdollSet.Contains(bodyCollider))
            {
                bodyCollider.enabled = !active && !isBeingCarried;
            }
        }

        GetAnimationDriver()?.SetSuspended(active || isBeingCarried);
    }

    private CharacterAnimationDriver GetAnimationDriver()
    {
        if (animationDriver == null)
        {
            animationDriver = GetComponent<CharacterAnimationDriver>();
        }

        return animationDriver;
    }

    private void CacheCarryRig()
    {
        if (bindPose.Count > 0) return;

        string[] boneNames =
        {
            "Root", "Hips", "Spine", "Chest", "UpperChest", "Neck", "Head",
            "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand",
            "RightShoulder", "RightUpperArm", "RightLowerArm", "RightHand",
            "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "LeftToes",
            "RightUpperLeg", "RightLowerLeg", "RightFoot", "RightToes"
        };

        Transform[] candidates = GetComponentsInChildren<Transform>(true);
        foreach (string boneName in boneNames)
        {
            Transform bone = FindNamedBone(candidates, boneName);
            if (bone == null) continue;

            carryBones[boneName] = bone;
            bindPose.Add(new CachedBonePose
            {
                bone = bone,
                localPosition = bone.localPosition,
                localRotation = bone.localRotation,
                localScale = bone.localScale
            });
        }

        CacheBindAxes();
    }

    private void CacheBindAxes()
    {
        if (!carryBones.TryGetValue("Hips", out Transform hips)) return;
        if (!carryBones.TryGetValue("Head", out Transform head)) return;
        if (!carryBones.TryGetValue("LeftFoot", out Transform foot)) return;
        if (!carryBones.TryGetValue("LeftToes", out Transform toes)) return;

        bindBodyAxisLocal = transform.InverseTransformDirection(head.position - hips.position).normalized;
        bindForwardAxisLocal = transform.InverseTransformDirection(toes.position - foot.position).normalized;
        bindForwardAxisLocal = Vector3.ProjectOnPlane(bindForwardAxisLocal, bindBodyAxisLocal).normalized;
        hasBindAxes = bindBodyAxisLocal.sqrMagnitude > 0.9f && bindForwardAxisLocal.sqrMagnitude > 0.9f;

        if (hasBindAxes)
        {
            Vector3 bindForwardWorld = transform.TransformDirection(bindForwardAxisLocal);
            bindHeadForwardAxisLocal = head.InverseTransformDirection(bindForwardWorld).normalized;
            hasBindHeadForwardAxis = bindHeadForwardAxisLocal.sqrMagnitude > 0.9f;
        }
    }

    private void RestoreBindPose()
    {
        foreach (CachedBonePose pose in bindPose)
        {
            if (pose.bone == null) continue;
            pose.bone.localPosition = pose.localPosition;
            pose.bone.localRotation = pose.localRotation;
            pose.bone.localScale = pose.localScale;
        }

        Physics.SyncTransforms();
    }

    private void ApplyRiggedCarryPose(CarryStyle style)
    {
        if (!HasRiggedCarryPose()) return;

        if (style == CarryStyle.BridalCarry)
        {
            AimBone("Neck", "Head", new Vector3(-0.38f, -0.92f, 0f));
            AimBone("LeftUpperArm", "LeftLowerArm", new Vector3(-0.12f, -1f, -0.2f));
            AimBone("LeftLowerArm", "LeftHand", new Vector3(0.12f, -1f, -0.08f));
            AimBone("RightUpperArm", "RightLowerArm", new Vector3(0.08f, -1f, 0.24f));
            AimBone("RightLowerArm", "RightHand", new Vector3(-0.1f, -1f, 0.06f));

            AimBone("LeftUpperLeg", "LeftLowerLeg", new Vector3(0.9f, 0.32f, -0.08f));
            AimBone("LeftLowerLeg", "LeftFoot", new Vector3(0.35f, -0.94f, -0.04f));
            AimBone("RightUpperLeg", "RightLowerLeg", new Vector3(0.92f, 0.18f, 0.08f));
            AimBone("RightLowerLeg", "RightFoot", new Vector3(0.48f, -0.88f, 0.04f));
            OrientHead(Vector3.up);
        }
        else
        {
            AimBone("Neck", "Head", new Vector3(0f, -0.82f, -0.58f));

            AimBone("LeftUpperArm", "LeftLowerArm", new Vector3(-0.18f, -1f, -0.12f));
            AimBone("LeftLowerArm", "LeftHand", new Vector3(-0.08f, -1f, -0.18f));
            AimBone("RightUpperArm", "RightLowerArm", new Vector3(0.18f, -1f, -0.12f));
            AimBone("RightLowerArm", "RightHand", new Vector3(0.08f, -1f, -0.18f));

            AimBone("LeftUpperLeg", "LeftLowerLeg", new Vector3(-0.12f, -0.72f, 0.68f));
            AimBone("LeftLowerLeg", "LeftFoot", new Vector3(-0.06f, -1f, 0.2f));
            AimBone("RightUpperLeg", "RightLowerLeg", new Vector3(0.12f, -0.72f, 0.68f));
            AimBone("RightLowerLeg", "RightFoot", new Vector3(0.06f, -1f, 0.2f));
            OrientHead(new Vector3(0f, 0.2f, -1f));
        }

        Physics.SyncTransforms();
    }

    private void ApplyRiggedRootPose(CarryStyle style)
    {
        Vector3 targetBodyAxis = style == CarryStyle.BridalCarry
            ? Vector3.left
            : new Vector3(0f, -0.65f, -0.76f).normalized;
        Vector3 targetForwardAxis = style == CarryStyle.BridalCarry
            ? Vector3.up
            : new Vector3(0f, -0.76f, 0.65f).normalized;

        transform.localPosition = Vector3.zero;
        transform.localRotation = AlignAxes(
            bindBodyAxisLocal,
            bindForwardAxisLocal,
            targetBodyAxis,
            targetForwardAxis);
        Physics.SyncTransforms();

        Transform hips = carryBones["Hips"];
        Vector3 currentHipsPosition = transform.parent.InverseTransformPoint(hips.position);
        Vector3 targetHipsPosition = style == CarryStyle.BridalCarry
            ? riggedBridalHipsLocalPosition
            : carryUsesFirstPersonPresentation
                ? riggedShoulderFirstPersonHipsLocalPosition
                : riggedShoulderHipsLocalPosition;
        transform.localPosition += targetHipsPosition - currentHipsPosition;
        Physics.SyncTransforms();
    }

    private bool HasRiggedCarryPose()
    {
        return hasBindAxes && carryBones.ContainsKey("Hips");
    }

    private static Quaternion AlignAxes(
        Vector3 sourceBodyAxis,
        Vector3 sourceForwardAxis,
        Vector3 targetBodyAxis,
        Vector3 targetForwardAxis)
    {
        Vector3 sourceUp = sourceBodyAxis.normalized;
        Vector3 sourceForward = Vector3.ProjectOnPlane(sourceForwardAxis, sourceUp).normalized;
        Vector3 targetUp = targetBodyAxis.normalized;
        Vector3 targetForward = Vector3.ProjectOnPlane(targetForwardAxis, targetUp).normalized;

        Quaternion sourceOrientation = Quaternion.LookRotation(sourceForward, sourceUp);
        Quaternion targetOrientation = Quaternion.LookRotation(targetForward, targetUp);
        return targetOrientation * Quaternion.Inverse(sourceOrientation);
    }

    private void AimBone(string boneName, string childName, Vector3 carryLocalDirection)
    {
        if (!carryBones.TryGetValue(boneName, out Transform bone)) return;
        if (!carryBones.TryGetValue(childName, out Transform child)) return;

        Vector3 currentDirection = child.position - bone.position;
        if (currentDirection.sqrMagnitude < 0.000001f) return;

        Transform carryFrame = transform.parent != null ? transform.parent : transform;
        Vector3 targetDirection = carryFrame.TransformDirection(carryLocalDirection.normalized);
        bone.rotation = Quaternion.FromToRotation(currentDirection, targetDirection) * bone.rotation;
    }

    private void OrientHead(Vector3 carryLocalForward)
    {
        if (!hasBindHeadForwardAxis) return;
        if (!carryBones.TryGetValue("Head", out Transform head)) return;

        Transform carryFrame = transform.parent != null ? transform.parent : transform;
        Vector3 currentForward = head.TransformDirection(bindHeadForwardAxisLocal);
        Vector3 targetForward = carryFrame.TransformDirection(carryLocalForward.normalized);
        head.rotation = Quaternion.FromToRotation(currentForward, targetForward) * head.rotation;
    }

    private static Transform FindNamedBone(Transform[] candidates, string boneName)
    {
        foreach (Transform candidate in candidates)
        {
            if (candidate.name == boneName || candidate.name.EndsWith(":" + boneName) || candidate.name.EndsWith("|" + boneName))
            {
                return candidate;
            }
        }

        return null;
    }

    private void SetColliders(bool active)
    {
        foreach (Collider bodyCollider in GetComponentsInChildren<Collider>(true))
        {
            if (bodyCollider != null)
            {
                bodyCollider.enabled = active;
            }
        }
    }

    private bool HasRagdoll()
    {
        if (ragdollBodies == null) return false;

        foreach (Rigidbody body in ragdollBodies)
        {
            if (body != null)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyKnockoutImpulse()
    {
        Rigidbody impulseBody = null;
        if (ragdollBodies != null)
        {
            foreach (Rigidbody body in ragdollBodies)
            {
                if (body == null || body.isKinematic) continue;
                if (impulseBody == null || body.mass > impulseBody.mass)
                {
                    impulseBody = body;
                }
            }
        }

        if (impulseBody == null || impulseBody.isKinematic) return;

        impulseBody.AddForce(
            transform.forward * 0.55f + Vector3.up * 0.15f,
            ForceMode.VelocityChange);
        impulseBody.AddTorque(
            transform.right * 1.6f + transform.forward * 0.25f,
            ForceMode.VelocityChange);
    }

    private void ApplyPrototypeKnockoutPose()
    {
        float facing = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, facing, 90f);

        Renderer targetRenderer = GetComponentInChildren<Renderer>();
        float bottomOffset = 0.5f;
        if (targetRenderer != null)
        {
            bottomOffset = transform.position.y - targetRenderer.bounds.min.y;
        }

        SetColliders(false);
        Vector3 rayOrigin = transform.position + Vector3.up * 3f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, 10f, ~0, QueryTriggerInteraction.Ignore))
        {
            Vector3 groundedPosition = transform.position;
            groundedPosition.y = groundHit.point.y + Mathf.Max(0.05f, bottomOffset) + 0.01f;
            transform.position = groundedPosition;
        }

        SetColliders(true);
    }

    private void CacheRendererColors()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>();
        baseRendererColors = new Color[cachedRenderers.Length];

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
            {
                baseRendererColors[i] = GetRendererColor(cachedRenderers[i]);
            }
        }
    }

    private void PlayFlash(Color flashColor, float duration)
    {
        if (cachedRenderers == null || baseRendererColors == null || cachedRenderers.Length != baseRendererColors.Length)
        {
            CacheRendererColors();
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            RestoreRendererColors();
        }

        flashRoutine = StartCoroutine(FlashColor(flashColor, duration));
    }

    private IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (cachedRenderers == null || baseRendererColors == null)
        {
            yield break;
        }

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
            {
                SetRendererColor(cachedRenderers[i], flashColor);
            }
        }

        yield return new WaitForSeconds(duration);

        RestoreRendererColors();
        flashRoutine = null;
    }

    private void RestoreRendererColors()
    {
        if (cachedRenderers == null || baseRendererColors == null) return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
            {
                SetRendererColor(cachedRenderers[i], baseRendererColors[i]);
            }
        }
    }

    private Color GetRendererColor(Renderer targetRenderer)
    {
        Material material = targetRenderer.material;
        if (material.HasProperty(BaseColorId)) return material.GetColor(BaseColorId);
        if (material.HasProperty(ColorId)) return material.GetColor(ColorId);
        if (material.HasProperty(BaseColorFactorId)) return material.GetColor(BaseColorFactorId);
        return Color.white;
    }

    private void SetRendererColor(Renderer targetRenderer, Color color)
    {
        Material material = targetRenderer.material;
        if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, color);
        else if (material.HasProperty(ColorId)) material.SetColor(ColorId, color);
        else if (material.HasProperty(BaseColorFactorId)) material.SetColor(BaseColorFactorId, color);
    }
}
