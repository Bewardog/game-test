using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpyPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5.5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 3.25f;
    public float carrySpeedMultiplier = 0.65f;
    public float jumpHeight = 1.2f;
    public float gravity = -18f;
    public float crouchHeight = 1.2f;
    public float crouchCameraDrop = 0.52f;
    public float crouchTransitionSpeed = 10f;
    [Range(0.35f, 1f)] public float crouchVisibilityMultiplier = 0.62f;
    [Range(1f, 1.5f)] public float sprintVisibilityMultiplier = 1.15f;

    [Header("Movement Noise")]
    public float crouchNoiseRange = 1.4f;
    public float walkNoiseRange = 4.25f;
    public float sprintNoiseRange = 10.5f;
    public float carryNoiseRange = 6.5f;
    public float crouchStepInterval = 0.82f;
    public float walkStepInterval = 0.58f;
    public float sprintStepInterval = 0.38f;

    [Header("Look")]
    public float mouseSensitivity = 2.5f;
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public Transform firstPersonPitchRoot;
    public Transform thirdPersonPitchRoot;
    public Renderer playerBodyRenderer;
    public GameObject playerBodyVisual;
    public FirstPersonViewModel firstPersonViewModel;
    public Vector3 normalFirstPersonCameraLocalPosition = new Vector3(0f, 0f, 0.25f);
    public Vector3 shoulderCarryFirstPersonCameraLocalPosition = new Vector3(0f, 0f, 0.25f);
    public Vector3 shoulderCarryFirstPersonCameraLocalEuler = Vector3.zero;

    [Header("Interaction")]
    public float interactRange = 3.5f;
    public LayerMask interactMask = ~0;
    public Transform carrySocket;
    public CarryStyle selectedCarryStyle = CarryStyle.BridalCarry;

    [Header("Carry Mechanics")]
    public float carryTransitionDuration = 0.25f;
    public float throwForce = 6.5f;
    public float throwLift = 2.25f;
    public LayerMask carryCollisionMask = ~0;
    public Vector3 bridalCollisionCenter = new Vector3(0.45f, 0.75f, 0.65f);
    public Vector3 bridalCollisionHalfExtents = new Vector3(0.95f, 0.55f, 0.65f);
    public Vector3 shoulderCollisionCenter = new Vector3(0f, 0.9f, 0.2f);
    public Vector3 shoulderCollisionHalfExtents = new Vector3(0.7f, 0.8f, 0.75f);

    [Header("Carry Animation Hooks")]
    public Animator characterAnimator;
    public CharacterAnimationDriver animationDriver;
    public Transform leftHandCarryTarget;
    public Transform rightHandCarryTarget;
    [Range(0f, 1f)] public float carryHandIKWeight = 1f;
    public bool useProceduralCarryArmIK = true;
    public Vector3 bridalLeftHandLocalPosition = new Vector3(-0.45f, 0.05f, 0.62f);
    public Vector3 bridalLeftHandLocalEuler = new Vector3(0f, 90f, 0f);
    public Vector3 bridalRightHandLocalPosition = new Vector3(0.45f, 0.05f, 0.72f);
    public Vector3 bridalRightHandLocalEuler = new Vector3(0f, -90f, 0f);
    public Vector3 shoulderRightHandLocalPosition = new Vector3(0.3f, 0.12f, 0.08f);
    public Vector3 shoulderRightHandLocalEuler = new Vector3(15f, -80f, -15f);

    [Header("Systems")]
    public SpyWeaponController weaponController;
    public RecruitRoster recruitRoster;
    public CompanionSummoner companionSummoner;

    [Header("Vitals")]
    public float maxHealth = 100f;
    public int maxArmor = 100;

    [Header("Melee")]
    public float meleeRange = 2.25f;
    public float silentTakedownRange = 2.1f;
    public float punchSedation = 24f;
    public float knifeDamage = 52f;
    public float meleeCooldown = 0.58f;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;
    private bool firstPerson = true;
    private EnemyOperative carriedOperative;
    private int armor;
    private float currentHealth;
    private bool incapacitated;
    private bool controlsLocked;
    private float carryTransitionUntil;
    private float movementModifier = 1f;
    private float movementModifierUntil;
    private float nextMeleeTime;
    private int missionClearance;
    private float standingControllerHeight;
    private Vector3 standingControllerCenter;
    private Vector3 standingFirstPersonPitchPosition;
    private Vector3 standingThirdPersonPitchPosition;
    private bool stanceCameraPositionsCached;
    private float nextMovementNoiseTime;
    private PrototypeAudioEmitter audioEmitter;
    private Transform leftUpperArmBone;
    private Transform leftLowerArmBone;
    private Transform leftHandBone;
    private Transform rightUpperArmBone;
    private Transform rightLowerArmBone;
    private Transform rightHandBone;
    private bool carryArmBonesCached;
    private float carryPoseBlend;

    public SpyWeaponController WeaponController => weaponController;
    public bool IsCarrying => carriedOperative != null;
    public EnemyOperative CarriedOperative => carriedOperative;
    public bool IsCarryTransitioning => Time.time < carryTransitionUntil;
    public float CurrentHealth => currentHealth;
    public int Armor => armor;
    public bool IsIncapacitated => incapacitated;
    public bool ControlsLocked => controlsLocked;
    public bool IsFirstPerson => firstPerson;
    public Camera ActiveCamera => firstPerson ? firstPersonCamera : thirdPersonCamera;
    public bool IsCrouching { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsGrounded => controller == null || controller.isGrounded;
    public float StealthVisibilityMultiplier => IsSprinting
        ? sprintVisibilityMultiplier
        : IsCrouching ? crouchVisibilityMultiplier : 1f;
    public int MissionClearance => missionClearance;
    public float DamageFlashUntil { get; private set; }
    public Vector3 LastDamageDirection { get; private set; }
    public float DamageDirectionUntil { get; private set; }
    public bool CanFireEquippedWeapon => !IsCarryTransitioning && (!IsCarrying ||
        (selectedCarryStyle == CarryStyle.OverShoulder &&
         weaponController != null &&
         weaponController.IsPistolEquipped));
    public string CurrentInteractionPrompt { get; private set; }
    public string ActionMessage { get; private set; }
    public float ActionMessageUntil { get; private set; }

    public bool HasMissionClearance(int requiredLevel)
    {
        return requiredLevel <= 0 || missionClearance >= requiredLevel;
    }

    public void GrantMissionClearance(int level)
    {
        int grantedLevel = Mathf.Clamp(level, 1, 3);
        missionClearance = Mathf.Max(missionClearance, grantedLevel);
        SetActionMessage($"CLEARANCE {missionClearance} ACQUIRED", 1.1f);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        standingControllerHeight = controller.height;
        standingControllerCenter = controller.center;

        if (weaponController == null)
        {
            weaponController = GetComponent<SpyWeaponController>();
        }
        if (animationDriver == null)
        {
            animationDriver = GetComponent<CharacterAnimationDriver>();
        }
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null)
        {
            audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        }

        if (recruitRoster == null)
        {
            recruitRoster = FindAnyObjectByType<RecruitRoster>();
        }

        currentHealth = maxHealth;

        SetCameraMode(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        UpdateStanceCamera();
        UpdateCarryPoseSupport();
    }

    private void Update()
    {
        if (incapacitated || controlsLocked)
        {
            CurrentInteractionPrompt = string.Empty;
            return;
        }

        if (ridingPlatform)
        {
            // Riding the elevator: the car carries the player, who can still look around.
            UpdateLook();
            CurrentInteractionPrompt = string.Empty;
            return;
        }

        UpdateLook();
        UpdateMovement();
        UpdateActions();
        UpdateInteractionPrompt();
    }

    public void AddArmor(int amount)
    {
        armor = Mathf.Clamp(armor + Mathf.Max(0, amount), 0, maxArmor);
    }

    public float Heal(float amount)
    {
        if (amount <= 0f || currentHealth >= maxHealth) return 0f;
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        return currentHealth - previousHealth;
    }

    public void SetStartingVitals(float health, int startingArmor)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        armor = Mathf.Clamp(startingArmor, 0, maxArmor);
    }

    public void RestoreFromCheckpoint(Vector3 position, Quaternion rotation)
    {
        if (IsCarrying) DropCarriedOperative();

        transform.rotation = rotation;
        Teleport(position);
        currentHealth = Mathf.Max(1f, maxHealth * 0.65f);
        armor = 0;
        velocity = Vector3.zero;
        movementModifier = 1f;
        movementModifierUntil = 0f;
        SetCrouching(false, true);
        incapacitated = false;
        controlsLocked = false;
        weaponController?.SetVisible(true);
        weaponController?.SetFirstPersonMode(firstPerson);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetActionMessage("FIELD RECOVERY - CHECKPOINT RESTORED", 1.5f);
    }

    public void ApplyDamage(float amount, string source = null, Vector3? sourcePosition = null)
    {
        if (incapacitated || amount <= 0f) return;

        int absorbed = Mathf.Min(armor, Mathf.CeilToInt(amount * 0.65f));
        armor -= absorbed;
        float healthDamage = Mathf.Max(0f, amount - absorbed);
        currentHealth = Mathf.Max(0f, currentHealth - healthDamage);
        DamageFlashUntil = Time.time + 0.2f;
        if (sourcePosition.HasValue)
        {
            Vector3 direction = sourcePosition.Value - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                LastDamageDirection = direction.normalized;
                DamageDirectionUntil = Time.time + 0.85f;
            }
        }
        animationDriver?.PlayAction(CharacterActionAnimation.Hit);

        if (currentHealth <= 0f)
        {
            StartCoroutine(HandleIncapacitated());
        }
        else
        {
            string sourceLabel = string.IsNullOrEmpty(source) ? "ENEMY FIRE" : source.ToUpperInvariant();
            SetActionMessage($"HIT: {sourceLabel}", 0.45f);
        }
    }

    public void ShowActionMessage(string message, float duration = 1f)
    {
        SetActionMessage(message, duration);
    }

    public void ApplyMovementDisruption(float speedMultiplier, float duration, string source = "SYSTEM DISRUPTION")
    {
        if (incapacitated || duration <= 0f) return;

        movementModifier = Mathf.Min(movementModifier, Mathf.Clamp(speedMultiplier, 0.25f, 1f));
        movementModifierUntil = Mathf.Max(movementModifierUntil, Time.time + duration);
        SetActionMessage(source.ToUpperInvariant(), Mathf.Min(duration, 1.25f));
    }

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;
        CurrentInteractionPrompt = string.Empty;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    private bool ridingPlatform;
    public bool IsRidingPlatform => ridingPlatform;

    // Used by moving platforms such as the lair elevator. The character
    // controller is paused during the ride so the platform can carry the player.
    public void BeginPlatformRide()
    {
        ridingPlatform = true;
        velocity = Vector3.zero;
        if (controller != null) controller.enabled = false;
    }

    public void MoveWithPlatform(Vector3 delta)
    {
        if (!ridingPlatform) return;
        transform.position += delta;
    }

    public void EndPlatformRide()
    {
        if (!ridingPlatform) return;
        ridingPlatform = false;
        velocity = Vector3.zero;
        if (controller != null) controller.enabled = true;
    }

    public void Teleport(Vector3 destination)
    {
        if (controller == null)
        {
            transform.position = destination;
            return;
        }

        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.position = destination;
        velocity = Vector3.zero;
        controller.enabled = wasEnabled;
    }

    public void StartCarrying(EnemyOperative operative)
    {
        if (operative == null || carriedOperative != null || carrySocket == null) return;

        if (!operative.BeginCarry(carrySocket, selectedCarryStyle))
        {
            SetActionMessage("CANNOT CARRY TARGET", 0.8f);
            return;
        }

        carriedOperative = operative;
        BeginCarryTransition();
        carriedOperative.SetCarryCameraMode(firstPerson);
        RefreshAnimatorCarryState();
        RefreshCarryWeaponState();
        RefreshFirstPersonCarryView();
        SetActionMessage($"CARRYING: {operative.name}", 1.25f);
        Debug.Log($"Started carrying {operative.name} using {selectedCarryStyle}.");
    }

    public void DropCarriedOperative()
    {
        if (carriedOperative == null) return;

        Vector3 dropPosition = transform.position + transform.forward * 1.2f;
        carriedOperative.EndCarry(dropPosition);
        carriedOperative = null;
        BeginCarryTransition();
        RefreshAnimatorCarryState();
        RefreshCarryWeaponState();
        RefreshFirstPersonCarryView();
        SetActionMessage("OPERATIVE DROPPED", 0.8f);
    }

    public void ThrowCarriedOperative()
    {
        if (carriedOperative == null || IsCarryTransitioning) return;

        EnemyOperative thrownOperative = carriedOperative;
        carriedOperative = null;

        Vector3 releasePosition = transform.position + transform.forward * 1.1f + Vector3.up * 0.35f;
        Vector3 impulse = transform.forward * throwForce + Vector3.up * throwLift;
        Vector3 torque = transform.right * 2.5f + transform.forward * 0.8f;
        thrownOperative.EndCarry(releasePosition, impulse, torque);

        BeginCarryTransition();
        RefreshAnimatorCarryState("ThrowCarry");
        RefreshCarryWeaponState();
        RefreshFirstPersonCarryView();
        SetActionMessage("OPERATIVE THROWN", 0.8f);
    }

    public void ExtractCarriedOperative()
    {
        if (carriedOperative == null) return;

        EnemyOperative extracted = carriedOperative;
        carriedOperative = null;
        BeginCarryTransition();
        RefreshAnimatorCarryState();
        RefreshCarryWeaponState();
        RefreshFirstPersonCarryView();
        string operativeName = extracted.name;

        if (recruitRoster == null)
        {
            recruitRoster = FindAnyObjectByType<RecruitRoster>();
        }

        if (recruitRoster != null)
        {
            recruitRoster.AddRecruit(extracted);
        }

        extracted.Recruit();
        SetActionMessage($"RECRUITED: {operativeName}", 1.5f);
    }

    private void UpdateLook()
    {
        Vector2 look = Vector2.ClampMagnitude(SpyInput.Look, 4f);
        float mouseX = look.x * mouseSensitivity;
        float mouseY = look.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        if (firstPersonPitchRoot != null)
        {
            firstPersonPitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        if (thirdPersonPitchRoot != null)
        {
            thirdPersonPitchRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void UpdateMovement()
    {
        UpdateStance();

        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        Vector2 moveInput = SpyInput.Move;
        float x = moveInput.x;
        float z = moveInput.y;

        IsSprinting = SpyInput.SprintHeld && !IsCarrying && !IsCrouching;
        float speed = IsCrouching
            ? crouchSpeed
            : IsSprinting ? sprintSpeed : walkSpeed;
        if (IsCarrying)
        {
            speed *= carrySpeedMultiplier;
        }
        if (Time.time < movementModifierUntil)
        {
            speed *= movementModifier;
        }
        else
        {
            movementModifier = 1f;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        move = FilterCarryMovement(move * speed * Time.deltaTime);
        Vector3 beforeHorizontalMove = transform.position;
        controller.Move(move);
        float horizontalDistance = Vector3.ProjectOnPlane(
            transform.position - beforeHorizontalMove,
            Vector3.up).magnitude;
        ReportMovementNoise(grounded, horizontalDistance);

        if (SpyInput.JumpPressed && grounded && !IsCarrying && !IsCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animationDriver?.PlayAction(CharacterActionAnimation.Jump);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ReportMovementNoise(bool grounded, float horizontalDistance)
    {
        if (!grounded || horizontalDistance <= 0.002f || Time.time < nextMovementNoiseTime) return;

        float hearingRange;
        float stepInterval;
        bool forcefulStep;
        if (IsCrouching)
        {
            hearingRange = crouchNoiseRange;
            stepInterval = crouchStepInterval;
            forcefulStep = false;
        }
        else if (IsSprinting)
        {
            hearingRange = sprintNoiseRange;
            stepInterval = sprintStepInterval;
            forcefulStep = true;
        }
        else if (IsCarrying)
        {
            hearingRange = carryNoiseRange;
            stepInterval = walkStepInterval * 0.88f;
            forcefulStep = true;
        }
        else
        {
            hearingRange = walkNoiseRange;
            stepInterval = walkStepInterval;
            forcefulStep = false;
        }

        nextMovementNoiseTime = Time.time + Mathf.Max(0.12f, stepInterval);
        audioEmitter?.PlayFootstep(IsCrouching, forcefulStep);
        EnemyCombatController.BroadcastNoise(
            transform.position,
            hearingRange,
            false,
            "footsteps");
    }

    private void UpdateStance()
    {
        bool wantsCrouch = SpyInput.CrouchHeld && !IsCarrying;
        if (wantsCrouch)
        {
            SetCrouching(true);
        }
        else if (IsCrouching && HasStandingClearance())
        {
            SetCrouching(false);
        }
    }

    private void SetCrouching(bool crouching, bool force = false)
    {
        if (!force && IsCrouching == crouching) return;
        if (!crouching && !force && !HasStandingClearance()) return;

        IsCrouching = crouching;
        IsSprinting = false;

        float targetHeight = crouching
            ? Mathf.Clamp(crouchHeight, controller.radius * 2f + 0.05f, standingControllerHeight)
            : standingControllerHeight;
        float standingBottom = standingControllerCenter.y - standingControllerHeight * 0.5f;
        controller.height = targetHeight;
        controller.center = new Vector3(
            standingControllerCenter.x,
            standingBottom + targetHeight * 0.5f,
            standingControllerCenter.z);
    }

    private bool HasStandingClearance()
    {
        float radius = Mathf.Max(0.05f, controller.radius - 0.04f);
        float halfSegment = Mathf.Max(0f, standingControllerHeight * 0.5f - radius);
        Vector3 center = transform.TransformPoint(standingControllerCenter) + Vector3.up * 0.02f;
        Vector3 top = center + Vector3.up * halfSegment;
        Vector3 bottom = center - Vector3.up * halfSegment;
        Collider[] overlaps = Physics.OverlapCapsule(
            bottom,
            top,
            radius,
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (overlap == null || overlap.transform.IsChildOf(transform)) continue;
            return false;
        }

        return true;
    }

    private void UpdateStanceCamera()
    {
        if (!stanceCameraPositionsCached)
        {
            if (firstPersonPitchRoot == null || thirdPersonPitchRoot == null) return;
            standingFirstPersonPitchPosition = firstPersonPitchRoot.localPosition;
            standingThirdPersonPitchPosition = thirdPersonPitchRoot.localPosition;
            stanceCameraPositionsCached = true;
        }

        float drop = IsCrouching ? crouchCameraDrop : 0f;
        float interpolation = 1f - Mathf.Exp(-Mathf.Max(0.01f, crouchTransitionSpeed) * Time.deltaTime);
        if (firstPersonPitchRoot != null)
        {
            firstPersonPitchRoot.localPosition = Vector3.Lerp(
                firstPersonPitchRoot.localPosition,
                standingFirstPersonPitchPosition + Vector3.down * drop,
                interpolation);
        }
        if (thirdPersonPitchRoot != null)
        {
            thirdPersonPitchRoot.localPosition = Vector3.Lerp(
                thirdPersonPitchRoot.localPosition,
                standingThirdPersonPitchPosition + Vector3.down * drop,
                interpolation);
        }
    }

    private void UpdateActions()
    {
        if (SpyInput.ToggleCameraPressed)
        {
            SetCameraMode(!firstPerson);
        }

        if (SpyInput.ToggleCarryStylePressed)
        {
            CarryStyle nextStyle = selectedCarryStyle == CarryStyle.BridalCarry
                ? CarryStyle.OverShoulder
                : CarryStyle.BridalCarry;
            SetCarryStyle(nextStyle);
        }

        int weaponSlot = SpyInput.WeaponSlotPressed;
        int cycleDirection = SpyInput.WeaponCycleDirection;
        if (weaponSlot >= 0)
        {
            TrySelectWeaponSlot(weaponSlot);
        }
        else if (cycleDirection != 0)
        {
            TryCycleWeapon(cycleDirection);
        }

        if (SpyInput.AmmoSwitchPressed)
        {
            weaponController?.TryToggleAmmoType();
        }

        bool meleeRequested = SpyInput.PunchPressed || SpyInput.KnifePressed;
        if (meleeRequested)
        {
            TryMelee(SpyInput.KnifePressed);
        }

        if (!meleeRequested && SpyInput.FireHeld && weaponController != null && CanFireEquippedWeapon)
        {
            weaponController.aimCamera = firstPerson ? firstPersonCamera : thirdPersonCamera;
            if (weaponController.TryFire())
            {
                animationDriver?.PlayAction(CharacterActionAnimation.Shoot);
            }
        }

        if (SpyInput.InteractPressed)
        {
            TryInteract();
        }

        if (SpyInput.DropPressed)
        {
            DropCarriedOperative();
        }

        if (SpyInput.ReloadPressed)
        {
            if (IsCarrying)
            {
                ThrowCarriedOperative();
            }
            else
            {
                weaponController?.TryReload();
            }
        }

        if (SpyInput.CompanionPressed)
        {
            TryDeployCompanion();
        }
    }

    private void TryMelee(bool useKnife)
    {
        if (Time.time < nextMeleeTime) return;
        if (IsCarrying || IsCarryTransitioning)
        {
            SetActionMessage("HANDS OCCUPIED", 0.65f);
            return;
        }

        nextMeleeTime = Time.time + meleeCooldown;
        CharacterActionAnimation action = useKnife
            ? CharacterActionAnimation.Stab
            : CharacterActionAnimation.Punch;
        animationDriver?.PlayAction(action);
        firstPersonViewModel?.PlayMelee(useKnife);

        Camera attackCamera = firstPerson ? firstPersonCamera : thirdPersonCamera;
        Vector3 origin = attackCamera != null
            ? attackCamera.transform.position
            : transform.position + Vector3.up * 0.9f;
        Vector3 direction = attackCamera != null ? attackCamera.transform.forward : transform.forward;
        bool hitOperative = false;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, meleeRange, interactMask, QueryTriggerInteraction.Ignore))
        {
            EnemyOperative target = hit.collider != null
                ? hit.collider.GetComponentInParent<EnemyOperative>()
                : null;
            if (target != null && !target.isDead && !target.isKnockedOut && !target.isRecruited)
            {
                if (useKnife) target.TakeDamage(knifeDamage);
                else target.ApplySedation(punchSedation);
                hitOperative = true;
            }
        }

        weaponController?.PlayMeleeFeedback(hitOperative, useKnife);
        SetActionMessage(
            hitOperative
                ? (useKnife ? "KNIFE HIT" : "PUNCH CONNECTED")
                : (useKnife ? "KNIFE MISS" : "PUNCH MISS"),
            0.45f);
    }

    private void TrySelectWeaponSlot(int slotIndex)
    {
        if (!CanChangeWeapon()) return;
        bool pistolsOnly = IsCarrying && selectedCarryStyle == CarryStyle.OverShoulder;
        if (weaponController != null && weaponController.TrySelectWeaponSlot(slotIndex, pistolsOnly))
        {
            RefreshCarryWeaponState();
        }
    }

    private void TryCycleWeapon(int direction)
    {
        if (!CanChangeWeapon()) return;
        bool pistolsOnly = IsCarrying && selectedCarryStyle == CarryStyle.OverShoulder;
        if (weaponController != null && weaponController.TryCycleWeapon(direction, pistolsOnly))
        {
            RefreshCarryWeaponState();
        }
    }

    private bool CanChangeWeapon()
    {
        if (!IsCarrying) return true;
        if (selectedCarryStyle == CarryStyle.OverShoulder) return true;

        SetActionMessage("BOTH HANDS OCCUPIED", 0.7f);
        return false;
    }

    private void TryDeployCompanion()
    {
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.State != GameFlowState.Mission)
        {
            SetActionMessage("COMPANIONS DEPLOY DURING MISSIONS", 1f);
            return;
        }

        if (IsCarrying)
        {
            SetActionMessage("DROP THE OPERATIVE FIRST", 1f);
            return;
        }

        if (companionSummoner == null) companionSummoner = GetComponent<CompanionSummoner>();
        if (companionSummoner == null || !companionSummoner.SummonByIndex(0))
        {
            SetActionMessage("NO RECRUIT AVAILABLE", 1f);
            return;
        }

        SetActionMessage("RECRUIT DEPLOYED", 1f);
    }

    private IEnumerator HandleIncapacitated()
    {
        incapacitated = true;
        if (IsCarrying) DropCarriedOperative();
        weaponController?.SetVisible(false);
        SetActionMessage("AGENT DOWN", 1.4f);
        yield return new WaitForSeconds(1.4f);

        MissionManager mission = MissionManager.Instance;
        if (mission != null && mission.TryRecoverPlayer(this))
        {
            yield break;
        }

        SetActionMessage("MISSION FAILED - RETURNING TO LAIR", 1f);
        yield return new WaitForSeconds(1f);
        GameFlowManager.Instance?.ReturnToHub();
    }

    public void SetCameraMode(bool useFirstPerson)
    {
        firstPerson = useFirstPerson;

        if (carriedOperative != null)
        {
            carriedOperative.SetCarryCameraMode(firstPerson);
        }

        if (firstPersonCamera != null)
        {
            firstPersonCamera.enabled = firstPerson;
        }

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = !firstPerson;
        }

        if (playerBodyRenderer != null)
        {
            playerBodyRenderer.enabled = !firstPerson;
        }
        if (playerBodyVisual != null)
        {
            playerBodyVisual.SetActive(!firstPerson);
        }

        firstPersonViewModel?.SetFirstPersonActive(firstPerson);
        weaponController?.SetFirstPersonMode(firstPerson);
        RefreshFirstPersonCarryView();
    }

    public void SetCarryStyle(CarryStyle style)
    {
        selectedCarryStyle = style;
        BeginCarryTransition();

        if (carriedOperative != null)
        {
            carriedOperative.UpdateCarryPose(selectedCarryStyle);
        }

        RefreshCarryWeaponState();
        RefreshFirstPersonCarryView();
        RefreshAnimatorCarryState();

        string carryLabel = selectedCarryStyle == CarryStyle.BridalCarry ? "BRIDAL CARRY" : "SHOULDER CARRY";
        SetActionMessage(carryLabel, 0.8f);
    }

    private void RefreshFirstPersonCarryView()
    {
        if (firstPersonCamera == null) return;

        bool useShoulderView = firstPerson &&
            carriedOperative != null &&
            selectedCarryStyle == CarryStyle.OverShoulder;

        firstPersonCamera.transform.localPosition = useShoulderView
            ? shoulderCarryFirstPersonCameraLocalPosition
            : normalFirstPersonCameraLocalPosition;
        firstPersonCamera.transform.localRotation = useShoulderView
            ? Quaternion.Euler(shoulderCarryFirstPersonCameraLocalEuler)
            : Quaternion.identity;
    }

    private void RefreshCarryWeaponState()
    {
        if (weaponController == null) return;

        bool shoulderPistol = IsCarrying &&
            selectedCarryStyle == CarryStyle.OverShoulder &&
            weaponController.IsPistolEquipped;

        weaponController.SetShoulderCarryPresentation(shoulderPistol);
        weaponController.SetVisible(!IsCarrying || shoulderPistol);
    }

    private Vector3 FilterCarryMovement(Vector3 displacement)
    {
        if (!IsCarrying || displacement.sqrMagnitude < 0.000001f) return displacement;

        Vector3 localCenter = selectedCarryStyle == CarryStyle.BridalCarry
            ? bridalCollisionCenter
            : shoulderCollisionCenter;
        Vector3 halfExtents = selectedCarryStyle == CarryStyle.BridalCarry
            ? bridalCollisionHalfExtents
            : shoulderCollisionHalfExtents;
        Vector3 center = transform.TransformPoint(localCenter);
        Vector3 direction = displacement.normalized;
        float distance = displacement.magnitude + 0.05f;

        RaycastHit[] hits = Physics.BoxCastAll(
            center,
            halfExtents,
            direction,
            transform.rotation,
            distance,
            carryCollisionMask,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (carriedOperative != null && hit.collider.transform.IsChildOf(carriedOperative.transform)) continue;
            return Vector3.zero;
        }

        return displacement;
    }

    private void BeginCarryTransition()
    {
        carryTransitionUntil = Time.time + Mathf.Max(0f, carryTransitionDuration);
        carryPoseBlend = 0f;
    }

    private void RefreshAnimatorCarryState(string triggerName = null)
    {
        if (characterAnimator == null) return;

        SetAnimatorBoolIfPresent("IsCarrying", IsCarrying);
        SetAnimatorIntIfPresent("CarryStyle", (int)selectedCarryStyle);

        if (!string.IsNullOrEmpty(triggerName) && HasAnimatorParameter(triggerName, AnimatorControllerParameterType.Trigger))
        {
            characterAnimator.SetTrigger(triggerName);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (characterAnimator == null || useProceduralCarryArmIK) return;

        float leftWeight = IsCarrying && selectedCarryStyle == CarryStyle.BridalCarry ? carryHandIKWeight : 0f;
        float rightWeight = IsCarrying ? carryHandIKWeight : 0f;
        ApplyHandIK(AvatarIKGoal.LeftHand, leftHandCarryTarget, leftWeight);
        ApplyHandIK(AvatarIKGoal.RightHand, rightHandCarryTarget, rightWeight);
    }

    private void ApplyHandIK(AvatarIKGoal goal, Transform target, float weight)
    {
        float appliedWeight = target != null ? weight : 0f;
        characterAnimator.SetIKPositionWeight(goal, appliedWeight);
        characterAnimator.SetIKRotationWeight(goal, appliedWeight);

        if (appliedWeight <= 0f) return;
        characterAnimator.SetIKPosition(goal, target.position);
        characterAnimator.SetIKRotation(goal, target.rotation);
    }

    private void UpdateCarryPoseSupport()
    {
        float targetBlend = IsCarrying ? carryHandIKWeight : 0f;
        float blendDuration = Mathf.Max(0.05f, carryTransitionDuration);
        carryPoseBlend = Mathf.MoveTowards(
            carryPoseBlend,
            targetBlend,
            Time.deltaTime / blendDuration);

        UpdateCarryHandTargets();
        if (!useProceduralCarryArmIK ||
            carryPoseBlend <= 0.001f ||
            playerBodyVisual == null ||
            !playerBodyVisual.activeInHierarchy)
        {
            return;
        }

        CacheCarryArmBones();
        if (!carryArmBonesCached) return;

        float leftWeight = selectedCarryStyle == CarryStyle.BridalCarry
            ? carryPoseBlend
            : 0f;
        float rightWeight = carryPoseBlend;
        SolveCarryArm(
            leftUpperArmBone,
            leftLowerArmBone,
            leftHandBone,
            leftHandCarryTarget,
            leftWeight);
        SolveCarryArm(
            rightUpperArmBone,
            rightLowerArmBone,
            rightHandBone,
            rightHandCarryTarget,
            rightWeight);
    }

    private void UpdateCarryHandTargets()
    {
        if (leftHandCarryTarget == null || rightHandCarryTarget == null) return;

        Vector3 leftPosition = bridalLeftHandLocalPosition;
        Vector3 leftEuler = bridalLeftHandLocalEuler;
        Vector3 rightPosition = selectedCarryStyle == CarryStyle.BridalCarry
            ? bridalRightHandLocalPosition
            : shoulderRightHandLocalPosition;
        Vector3 rightEuler = selectedCarryStyle == CarryStyle.BridalCarry
            ? bridalRightHandLocalEuler
            : shoulderRightHandLocalEuler;
        float smoothing = 1f - Mathf.Exp(-12f * Time.deltaTime);

        leftHandCarryTarget.localPosition = Vector3.Lerp(
            leftHandCarryTarget.localPosition,
            leftPosition,
            smoothing);
        leftHandCarryTarget.localRotation = Quaternion.Slerp(
            leftHandCarryTarget.localRotation,
            Quaternion.Euler(leftEuler),
            smoothing);
        rightHandCarryTarget.localPosition = Vector3.Lerp(
            rightHandCarryTarget.localPosition,
            rightPosition,
            smoothing);
        rightHandCarryTarget.localRotation = Quaternion.Slerp(
            rightHandCarryTarget.localRotation,
            Quaternion.Euler(rightEuler),
            smoothing);
    }

    private void CacheCarryArmBones()
    {
        if (carryArmBonesCached || characterAnimator == null) return;

        Transform[] bones = characterAnimator.GetComponentsInChildren<Transform>(true);
        leftUpperArmBone = FindNamedTransform(bones, "LeftUpperArm");
        leftLowerArmBone = FindNamedTransform(bones, "LeftLowerArm");
        leftHandBone = FindNamedTransform(bones, "LeftHand");
        rightUpperArmBone = FindNamedTransform(bones, "RightUpperArm");
        rightLowerArmBone = FindNamedTransform(bones, "RightLowerArm");
        rightHandBone = FindNamedTransform(bones, "RightHand");
        carryArmBonesCached = leftUpperArmBone != null &&
            leftLowerArmBone != null &&
            leftHandBone != null &&
            rightUpperArmBone != null &&
            rightLowerArmBone != null &&
            rightHandBone != null;
    }

    private static Transform FindNamedTransform(Transform[] transforms, string targetName)
    {
        if (transforms == null) return null;
        foreach (Transform candidate in transforms)
        {
            if (candidate != null && candidate.name == targetName) return candidate;
        }

        return null;
    }

    private static void SolveCarryArm(
        Transform upperArm,
        Transform lowerArm,
        Transform hand,
        Transform target,
        float weight)
    {
        if (upperArm == null ||
            lowerArm == null ||
            hand == null ||
            target == null ||
            weight <= 0.001f)
        {
            return;
        }

        float upperLength = Vector3.Distance(upperArm.position, lowerArm.position);
        float lowerLength = Vector3.Distance(lowerArm.position, hand.position);
        float maximumReach = Mathf.Max(0.01f, (upperLength + lowerLength) * 0.96f);
        Vector3 shoulderToTarget = target.position - upperArm.position;
        Vector3 reachableTarget = upperArm.position +
            Vector3.ClampMagnitude(shoulderToTarget, maximumReach);

        for (int iteration = 0; iteration < 2; iteration++)
        {
            RotateBoneTowardTarget(upperArm, hand.position, reachableTarget, weight * 0.62f);
            RotateBoneTowardTarget(lowerArm, hand.position, reachableTarget, weight * 0.88f);
        }

        hand.rotation = Quaternion.Slerp(hand.rotation, target.rotation, weight);
    }

    private static void RotateBoneTowardTarget(
        Transform bone,
        Vector3 endPosition,
        Vector3 targetPosition,
        float weight)
    {
        Vector3 currentDirection = endPosition - bone.position;
        Vector3 targetDirection = targetPosition - bone.position;
        if (currentDirection.sqrMagnitude <= 0.000001f ||
            targetDirection.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        Quaternion correction = Quaternion.FromToRotation(currentDirection, targetDirection);
        Quaternion correctedRotation = correction * bone.rotation;
        bone.rotation = Quaternion.Slerp(bone.rotation, correctedRotation, Mathf.Clamp01(weight));
    }

    private void SetAnimatorBoolIfPresent(string parameterName, bool value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            characterAnimator.SetBool(parameterName, value);
        }
    }

    private void SetAnimatorIntIfPresent(string parameterName, int value)
    {
        if (HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Int))
        {
            characterAnimator.SetInteger(parameterName, value);
        }
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
        {
            if (parameter.type == type && parameter.name == parameterName) return true;
        }

        return false;
    }

    private void TryInteract()
    {
        IInteractable interactable = FindInteractionTarget();
        if (interactable != null)
        {
            interactable.Interact(this);
        }
        else if (TrySilentTakedown())
        {
            return;
        }
        else
        {
            SetActionMessage("NO INTERACTION TARGET", 0.7f);
        }
    }

    private void UpdateInteractionPrompt()
    {
        CurrentInteractionPrompt = string.Empty;

        IInteractable interactable = FindInteractionTarget();
        if (interactable != null && !string.IsNullOrEmpty(interactable.InteractionPrompt))
        {
            CurrentInteractionPrompt = $"E  {interactable.InteractionPrompt.ToUpperInvariant()}";
            return;
        }

        if (FindSilentTakedownTarget() != null)
        {
            CurrentInteractionPrompt = "E  SILENT TAKEDOWN";
        }
    }

    private bool TrySilentTakedown()
    {
        if (IsCarrying || IsCarryTransitioning || Time.time < nextMeleeTime) return false;

        EnemyOperative target = FindSilentTakedownTarget();
        if (target == null || !target.ReceiveSilentTakedown(this)) return false;

        nextMeleeTime = Time.time + meleeCooldown;
        animationDriver?.PlayAction(CharacterActionAnimation.Punch);
        firstPersonViewModel?.PlayMelee(false);
        weaponController?.PlayMeleeFeedback(true, false);
        SetActionMessage("SILENT TAKEDOWN", 0.8f);
        return true;
    }

    private EnemyOperative FindSilentTakedownTarget()
    {
        if (IsCarrying || IsCarryTransitioning) return null;
        if (MissionManager.Instance != null && MissionManager.Instance.IsAlerted) return null;

        Camera activeCamera = firstPerson ? firstPersonCamera : thirdPersonCamera;
        Vector3 facing = activeCamera != null ? activeCamera.transform.forward : transform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude > 0.001f) facing.Normalize();

        EnemyOperative[] operatives = FindObjectsByType<EnemyOperative>();
        EnemyOperative bestTarget = null;
        float bestScore = float.MinValue;
        foreach (EnemyOperative operative in operatives)
        {
            if (operative == null || !operative.CanReceiveSilentTakedown(this)) continue;

            Vector3 toTarget = operative.transform.position - transform.position;
            float verticalDifference = Mathf.Abs(toTarget.y);
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            if (distance > silentTakedownRange || verticalDifference > 1.25f || distance < 0.01f) continue;

            float facingScore = Vector3.Dot(facing, toTarget / distance);
            if (facingScore < 0.2f || !HasMeleeLineOfSight(operative)) continue;

            float score = facingScore * 2f - distance * 0.35f;
            if (score <= bestScore) continue;
            bestScore = score;
            bestTarget = operative;
        }

        return bestTarget;
    }

    private bool HasMeleeLineOfSight(EnemyOperative target)
    {
        Vector3 origin = transform.position + Vector3.up * 0.9f;
        Vector3 destination = target.transform.position + Vector3.up * 0.9f;
        Vector3 direction = destination - origin;
        float distance = direction.magnitude;
        if (distance < 0.01f) return true;

        if (!Physics.Raycast(
                origin,
                direction / distance,
                out RaycastHit hit,
                distance + 0.25f,
                interactMask,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        EnemyOperative visibleTarget = hit.collider != null
            ? hit.collider.GetComponentInParent<EnemyOperative>()
            : null;
        return visibleTarget == target;
    }

    private IInteractable FindInteractionTarget()
    {
        Camera activeCamera = firstPerson ? firstPersonCamera : thirdPersonCamera;
        if (activeCamera == null) return null;

        float cameraToPlayer = Vector3.Distance(activeCamera.transform.position, transform.position);
        float cameraRayDistance = cameraToPlayer + interactRange + 1f;
        RaycastHit[] aimHits = Physics.RaycastAll(
            activeCamera.transform.position,
            activeCamera.transform.forward,
            cameraRayDistance,
            interactMask,
            QueryTriggerInteraction.Collide);

        IInteractable aimedTarget = null;
        float aimedDistance = float.MaxValue;

        foreach (RaycastHit hit in aimHits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            IInteractable interactable = FindInteractable(hit.collider);
            if (interactable == null || string.IsNullOrEmpty(interactable.InteractionPrompt))
            {
                continue;
            }

            if (IsWithinInteractionRange(hit.collider) && hit.distance < aimedDistance)
            {
                aimedTarget = interactable;
                aimedDistance = hit.distance;
            }
        }

        if (aimedTarget != null)
        {
            return aimedTarget;
        }

        EnemyOperative nearbyOperative = FindNearbyCarryableOperative(activeCamera);
        if (nearbyOperative != null)
        {
            return nearbyOperative;
        }

        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position + Vector3.up * 0.35f,
            interactRange,
            interactMask,
            QueryTriggerInteraction.Collide);

        IInteractable bestTarget = null;
        float bestScore = float.MinValue;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            if (nearbyCollider == null || nearbyCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            IInteractable interactable = FindInteractable(nearbyCollider);
            if (interactable == null || string.IsNullOrEmpty(interactable.InteractionPrompt))
            {
                continue;
            }

            Vector3 targetPoint = nearbyCollider.bounds.center;
            Vector3 cameraToTarget = targetPoint - activeCamera.transform.position;
            float facingScore = cameraToTarget.sqrMagnitude > 0.001f
                ? Vector3.Dot(activeCamera.transform.forward, cameraToTarget.normalized)
                : 1f;

            if (facingScore < -0.25f)
            {
                continue;
            }

            float playerDistance = Vector3.Distance(transform.position, nearbyCollider.ClosestPoint(transform.position));
            float score = facingScore * 2f - playerDistance * 0.2f;

            if (score > bestScore)
            {
                bestTarget = interactable;
                bestScore = score;
            }
        }

        return bestTarget;
    }

    private EnemyOperative FindNearbyCarryableOperative(Camera activeCamera)
    {
        EnemyOperative[] operatives = FindObjectsByType<EnemyOperative>();
        EnemyOperative bestTarget = null;
        float bestScore = float.MinValue;
        Vector3 playerCenter = transform.position + Vector3.up * 0.35f;

        foreach (EnemyOperative operative in operatives)
        {
            if (operative == null || string.IsNullOrEmpty(operative.InteractionPrompt))
            {
                continue;
            }

            Renderer[] renderers = operative.GetComponentsInChildren<Renderer>();
            Vector3 targetPoint = operative.transform.position;
            Vector3 closestPoint = targetPoint;

            if (renderers.Length > 0)
            {
                Bounds combinedBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }
                }

                targetPoint = combinedBounds.center;
                closestPoint = combinedBounds.ClosestPoint(playerCenter);
            }

            float playerDistance = Vector3.Distance(playerCenter, closestPoint);
            if (playerDistance > interactRange + 0.75f)
            {
                continue;
            }

            Vector3 cameraToTarget = targetPoint - activeCamera.transform.position;
            float facingScore = cameraToTarget.sqrMagnitude > 0.001f
                ? Vector3.Dot(activeCamera.transform.forward, cameraToTarget.normalized)
                : 1f;

            if (facingScore < -0.4f)
            {
                continue;
            }

            float score = facingScore * 2f - playerDistance * 0.2f;
            if (score > bestScore)
            {
                bestTarget = operative;
                bestScore = score;
            }
        }

        return bestTarget;
    }

    private bool IsWithinInteractionRange(Collider targetCollider)
    {
        Vector3 playerCenter = transform.position + Vector3.up * 0.35f;
        Vector3 closestPoint = targetCollider.ClosestPoint(playerCenter);
        return Vector3.Distance(playerCenter, closestPoint) <= interactRange;
    }

    private void SetActionMessage(string message, float duration)
    {
        ActionMessage = message;
        ActionMessageUntil = Time.time + duration;
    }

    private IInteractable FindInteractable(Collider hitCollider)
    {
        if (hitCollider == null) return null;

        MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }
}
