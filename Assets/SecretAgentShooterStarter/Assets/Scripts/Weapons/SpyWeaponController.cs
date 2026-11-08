using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpyWeaponController : MonoBehaviour
{
    public WeaponDefinition currentWeapon;
    public Transform muzzle;
    public Transform weaponVisualRoot;
    public Transform thirdPersonWeaponRoot;
    public Transform thirdPersonShoulderWeaponRoot;
    public Camera aimCamera;

    [Header("Ammunition")]
    public int magazineAmmo;
    public int reserveAmmo;
    public float reloadDuration = 1.15f;

    [Header("Carry Presentation")]
    public Vector3 leftHandMuzzleLocalPosition = new Vector3(-0.35f, 0.5f, 0.7f);
    public Vector3 leftHandMuzzleLocalEuler = Vector3.zero;

    [Header("Prototype Feedback")]
    public Color tracerColor = new Color(0.2f, 0.95f, 1f);
    public Color enemyHitColor = new Color(1f, 0.25f, 0.7f);
    public float tracerDuration = 0.16f;
    public float recoilDistance = 0.12f;
    public float recoilAngle = 7f;
    public float recoilReturnSpeed = 9f;

    public string FeedbackMessage { get; private set; }
    public float FeedbackUntil { get; private set; }
    public bool EnemyHitRecently { get; private set; }
    public int MissionShotsFired { get; private set; }
    public int MissionShotsHit { get; private set; }

    private float nextFireTime;
    private Coroutine reloadRoutine;
    private bool ammoInitialized;
    private string loadedWeaponId;
    private bool muzzlePoseCached;
    private Vector3 normalMuzzleLocalPosition;
    private Quaternion normalMuzzleLocalRotation;
    private GameObject currentWeaponVisual;
    private GameObject thirdPersonWeaponVisual;
    private GameObject thirdPersonShoulderWeaponVisual;
    private PrototypeAudioEmitter audioEmitter;
    private float recoilAmount;
    private bool firstPersonPresentation = true;
    private bool presentationVisible = true;
    private bool shoulderCarryPresentation;
    private readonly Dictionary<string, AmmoState> ammoByWeapon = new Dictionary<string, AmmoState>();

    public bool IsPistolEquipped => currentWeapon == null || currentWeapon.weaponClass == WeaponClass.Pistol;
    public bool IsReloading => reloadRoutine != null;
    public int MagazineSize => currentWeapon != null ? Mathf.Max(1, currentWeapon.magazineSize) : 12;
    public int MaxReserveAmmo => currentWeapon != null ? Mathf.Max(0, currentWeapon.maxReserveAmmo) : 144;
    public IReadOnlyList<WeaponDefinition> AvailableWeapons => GetAvailableWeapons(false);
    public float RecoilAmount => recoilAmount;

    private void Start()
    {
        audioEmitter = GetComponent<PrototypeAudioEmitter>();
        if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
        audioEmitter.SetSpatialBlend(0.08f);
        EnsureAmmoInitialized();
        RefreshWeaponVisual();
    }

    private void Update()
    {
        if (currentWeaponVisual == null) return;
        recoilAmount = Mathf.MoveTowards(recoilAmount, 0f, Time.deltaTime * recoilReturnSpeed);
        currentWeaponVisual.transform.localPosition = Vector3.back * (recoilDistance * recoilAmount);
        currentWeaponVisual.transform.localRotation = Quaternion.Euler(-recoilAngle * recoilAmount, 0f, 0f);
    }

    public void SetWeapon(WeaponDefinition weapon, bool grantPickupAmmo = true)
    {
        if (weapon == null) return;

        EnsureAmmoInitialized();
        SaveCurrentAmmoState();
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }

        currentWeapon = weapon;
        loadedWeaponId = weapon.weaponId;
        RefreshWeaponVisual();

        if (ammoByWeapon.TryGetValue(weapon.weaponId, out AmmoState savedState))
        {
            magazineAmmo = Mathf.Clamp(savedState.magazine, 0, MagazineSize);
            reserveAmmo = Mathf.Clamp(savedState.reserve, 0, MaxReserveAmmo);
        }
        else
        {
            magazineAmmo = MagazineSize;
            reserveAmmo = Mathf.Clamp(weapon.startingReserveAmmo, 0, MaxReserveAmmo);
        }

        ammoInitialized = true;
        if (grantPickupAmmo)
        {
            AddAmmo(weapon.ammoPerPickup);
        }
        SaveCurrentAmmoState();
    }

    public bool TrySelectWeaponSlot(int slotIndex, bool pistolsOnly)
    {
        List<WeaponDefinition> available = GetAvailableWeapons(pistolsOnly);
        if (slotIndex < 0 || slotIndex >= available.Count)
        {
            SetFeedback("WEAPON SLOT EMPTY", false, 0.65f);
            return false;
        }

        return EquipFromLoadout(available[slotIndex]);
    }

    public bool TryCycleWeapon(int direction, bool pistolsOnly)
    {
        if (direction == 0) return false;

        List<WeaponDefinition> available = GetAvailableWeapons(pistolsOnly);
        if (available.Count == 0)
        {
            SetFeedback(pistolsOnly ? "NO PISTOL AVAILABLE" : "NO WEAPON AVAILABLE", false, 0.7f);
            return false;
        }

        int currentIndex = available.FindIndex(
            weapon => currentWeapon != null && weapon.weaponId == currentWeapon.weaponId);
        if (currentIndex < 0) currentIndex = direction > 0 ? -1 : 0;
        int nextIndex = (currentIndex + (direction > 0 ? 1 : -1) + available.Count) % available.Count;
        return EquipFromLoadout(available[nextIndex]);
    }

    public int AddAmmo(int amount)
    {
        EnsureAmmoInitialized();
        int previousReserve = reserveAmmo;
        reserveAmmo = Mathf.Clamp(reserveAmmo + Mathf.Max(0, amount), 0, MaxReserveAmmo);
        SaveCurrentAmmoState();
        return reserveAmmo - previousReserve;
    }

    public bool TryReload()
    {
        EnsureAmmoInitialized();
        if (IsReloading) return false;
        if (magazineAmmo >= MagazineSize)
        {
            SetFeedback("MAGAZINE FULL", false, 0.65f);
            return false;
        }

        if (reserveAmmo <= 0)
        {
            SetFeedback("NO RESERVE AMMO", false, 0.75f);
            return false;
        }

        reloadRoutine = StartCoroutine(ReloadRoutine());
        audioEmitter?.PlayReloadStart();
        SetFeedback("RELOADING", false, reloadDuration);
        return true;
    }

    public void PlayMeleeFeedback(bool hit, bool useKnife)
    {
        if (audioEmitter == null)
        {
            audioEmitter = GetComponent<PrototypeAudioEmitter>();
            if (audioEmitter == null) audioEmitter = gameObject.AddComponent<PrototypeAudioEmitter>();
            audioEmitter.SetSpatialBlend(0.08f);
        }
        audioEmitter.PlayEnemyAttack(true);
        if (hit)
        {
            SetFeedback(useKnife ? "KNIFE IMPACT" : "MELEE IMPACT", true, 0.35f);
        }
    }

    public void ResetMissionPerformance()
    {
        MissionShotsFired = 0;
        MissionShotsHit = 0;
    }

    public void RecordProjectileHit()
    {
        MissionShotsHit++;
    }

    public void SetVisible(bool visible)
    {
        presentationVisible = visible;
        RefreshPresentationVisibility();
    }

    public void SetFirstPersonMode(bool active)
    {
        firstPersonPresentation = active;
        RefreshPresentationVisibility();
    }

    public void SetShoulderCarryPresentation(bool active)
    {
        shoulderCarryPresentation = active;
        CacheMuzzlePose();
        Transform presentation = weaponVisualRoot != null ? weaponVisualRoot : muzzle;
        if (presentation == null) return;

        presentation.localPosition = active ? leftHandMuzzleLocalPosition : normalMuzzleLocalPosition;
        presentation.localRotation = active
            ? Quaternion.Euler(leftHandMuzzleLocalEuler)
            : normalMuzzleLocalRotation;
        RefreshPresentationVisibility();
    }

    private void CacheMuzzlePose()
    {
        Transform presentation = weaponVisualRoot != null ? weaponVisualRoot : muzzle;
        if (muzzlePoseCached || presentation == null) return;

        normalMuzzleLocalPosition = presentation.localPosition;
        normalMuzzleLocalRotation = presentation.localRotation;
        muzzlePoseCached = true;
    }

    private void RefreshWeaponVisual()
    {
        if (weaponVisualRoot == null || currentWeapon == null) return;

        if (currentWeaponVisual != null)
        {
            Destroy(currentWeaponVisual);
        }
        if (thirdPersonWeaponVisual != null)
        {
            Destroy(thirdPersonWeaponVisual);
        }
        if (thirdPersonShoulderWeaponVisual != null)
        {
            Destroy(thirdPersonShoulderWeaponVisual);
        }

        string resourcePath = GetWeaponModelResourcePath(currentWeapon.weaponId);
        GameObject prefab = string.IsNullOrEmpty(resourcePath)
            ? null
            : Resources.Load<GameObject>(resourcePath);
        if (prefab == null) return;

        currentWeaponVisual = Instantiate(prefab, weaponVisualRoot);
        currentWeaponVisual.name = $"{currentWeapon.weaponName} View Model";
        currentWeaponVisual.transform.localPosition = Vector3.zero;
        currentWeaponVisual.transform.localRotation = Quaternion.identity;
        currentWeaponVisual.transform.localScale = Vector3.one * 0.38f;
        SetLayerRecursively(currentWeaponVisual, gameObject.layer);

        thirdPersonWeaponVisual = CreateThirdPersonWeaponVisual(
            prefab,
            thirdPersonWeaponRoot,
            $"{currentWeapon.weaponName} Third Person View");
        thirdPersonShoulderWeaponVisual = CreateThirdPersonWeaponVisual(
            prefab,
            thirdPersonShoulderWeaponRoot,
            $"{currentWeapon.weaponName} Left Hand View");
        RefreshPresentationVisibility();
    }

    private GameObject CreateThirdPersonWeaponVisual(GameObject prefab, Transform parent, string visualName)
    {
        if (prefab == null || parent == null) return null;

        GameObject visual = Instantiate(prefab, parent);
        visual.name = visualName;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * 0.18f;
        SetLayerRecursively(visual, gameObject.layer);
        return visual;
    }

    private void RefreshPresentationVisibility()
    {
        bool showFirstPerson = presentationVisible && firstPersonPresentation;
        bool showThirdPerson = presentationVisible && !firstPersonPresentation;
        if (weaponVisualRoot != null) weaponVisualRoot.gameObject.SetActive(showFirstPerson);
        if (thirdPersonWeaponRoot != null)
        {
            thirdPersonWeaponRoot.gameObject.SetActive(showThirdPerson && !shoulderCarryPresentation);
        }
        if (thirdPersonShoulderWeaponRoot != null)
        {
            thirdPersonShoulderWeaponRoot.gameObject.SetActive(showThirdPerson && shoulderCarryPresentation);
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

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        if (target == null) return;
        target.layer = layer;
        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public bool TryFire()
    {
        EnsureAmmoInitialized();
        if (Time.time < nextFireTime) return false;
        if (IsReloading) return false;

        if (magazineAmmo <= 0)
        {
            audioEmitter?.PlayDryFire();
            SetFeedback(reserveAmmo > 0 ? "EMPTY - PRESS R TO RELOAD" : "OUT OF AMMO", false, 0.9f);
            return false;
        }

        float fireRate = currentWeapon != null ? currentWeapon.fireRate : 0.45f;
        float range = currentWeapon != null ? currentWeapon.range : 80f;
        float sedation = currentWeapon != null ? currentWeapon.sedationDamage : 35f;
        float lethal = currentWeapon != null ? currentWeapon.lethalDamage : 0f;
        SecretLairManager lair = SecretLairManager.Instance;
        if (lair != null)
        {
            sedation *= lair.GetTranquilizerSpeedMultiplier();
            lethal *= lair.GetWeaponEffectivenessMultiplier();
        }

        nextFireTime = Time.time + fireRate;
        magazineAmmo--;
        MissionShotsFired++;
        recoilAmount = 1f;
        audioEmitter?.PlayWeaponFire(currentWeapon != null ? currentWeapon.weaponClass : WeaponClass.Pistol);
        SaveCurrentAmmoState();

        Vector3 origin = aimCamera != null ? aimCamera.transform.position : transform.position + Vector3.up;
        Vector3 direction = aimCamera != null ? aimCamera.transform.forward : transform.forward;
        ReportWeaponNoise(origin);

        GameObject projectilePrefab = currentWeapon != null ? currentWeapon.projectilePrefab : null;
        if (projectilePrefab != null && muzzle != null)
        {
            SpawnMuzzleFlash();
            GameObject projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(direction));
            TranquilizerDart dart = projectile.GetComponent<TranquilizerDart>();
            if (dart != null)
            {
                dart.Launch(direction, sedation, lethal, this);
            }

            return true;
        }

        Vector3 shotStart = muzzle != null ? muzzle.position : origin + direction * 0.4f;
        Vector3 shotEnd = origin + direction * range;
        bool hitSomething = TryGetShotHit(origin, direction, range, out RaycastHit hit);
        bool firesDarts = FiresDarts(currentWeapon);
        EnemyOperative enemy = null;
        MissionSecurityCamera securityCamera = null;
        Transform stickTo = null;

        if (hitSomething)
        {
            shotEnd = hit.point;
            enemy = hit.collider.GetComponentInParent<EnemyOperative>();
            if (enemy != null)
            {
                MissionShotsHit++;
                stickTo = enemy.transform;
            }
            else
            {
                securityCamera = hit.collider.GetComponentInParent<MissionSecurityCamera>();
            }
        }

        // Effects land when the round arrives (instantly for bullets, after the
        // short flight for darts). There is no hit/miss readout; the sedation
        // gauge and camera messages still show because they carry information.
        Vector3 impactPoint = shotEnd;
        System.Action applyHit = () =>
        {
            if (enemy != null)
            {
                if (sedation > 0f) enemy.ApplySedation(sedation);
                if (lethal > 0f) enemy.TakeDamage(lethal);
                string message = enemy.isKnockedOut
                    ? "TARGET SEDATED"
                    : $"SEDATION {Mathf.CeilToInt(enemy.sedation)}/{Mathf.CeilToInt(enemy.SedationThreshold)}";
                SetFeedback(message, true, 0.8f);
            }
            else if (securityCamera != null && securityCamera.ReceiveWeaponHit(impactPoint))
            {
                MissionShotsHit++;
                SetFeedback("CAMERA DISABLED", true, 0.8f);
            }
        };

        SpawnMuzzleFlash();
        if (firesDarts)
        {
            ShotProjectileVisual.Fire(shotStart, shotEnd, true, hitSomething, stickTo, applyHit);
        }
        else
        {
            applyHit();
            ShotProjectileVisual.Fire(shotStart, shotEnd, false, hitSomething, null);
        }

        return true;
    }

    private void ReportWeaponNoise(Vector3 sourcePosition)
    {
        WeaponClass weaponClass = currentWeapon != null
            ? currentWeapon.weaponClass
            : WeaponClass.Pistol;
        float hearingRange;
        switch (weaponClass)
        {
            case WeaponClass.SubmachineGun:
                hearingRange = 18f;
                break;
            case WeaponClass.Rifle:
                hearingRange = 12f;
                break;
            case WeaponClass.Shotgun:
            case WeaponClass.Heavy:
                hearingRange = 30f;
                break;
            case WeaponClass.Gadget:
                hearingRange = 4f;
                break;
            default:
                hearingRange = 7f;
                break;
        }

        EnemyCombatController.BroadcastNoise(
            sourcePosition,
            hearingRange,
            true,
            "a weapon discharge");
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(reloadDuration);

        int needed = MagazineSize - magazineAmmo;
        int transferred = Mathf.Min(needed, reserveAmmo);
        magazineAmmo += transferred;
        reserveAmmo -= transferred;
        reloadRoutine = null;
        audioEmitter?.PlayReloadComplete();
        SaveCurrentAmmoState();
        SetFeedback("RELOADED", false, 0.55f);
    }

    private void EnsureAmmoInitialized()
    {
        if (currentWeapon == null || (ammoInitialized && loadedWeaponId == currentWeapon.weaponId)) return;

        loadedWeaponId = currentWeapon.weaponId;
        magazineAmmo = MagazineSize;
        reserveAmmo = Mathf.Clamp(currentWeapon.startingReserveAmmo, 0, MaxReserveAmmo);
        ammoInitialized = true;
        SaveCurrentAmmoState();
    }

    private void SaveCurrentAmmoState()
    {
        if (!ammoInitialized || currentWeapon == null || string.IsNullOrEmpty(currentWeapon.weaponId)) return;
        ammoByWeapon[currentWeapon.weaponId] = new AmmoState
        {
            magazine = magazineAmmo,
            reserve = reserveAmmo
        };
    }

    private List<WeaponDefinition> GetAvailableWeapons(bool pistolsOnly)
    {
        List<WeaponDefinition> available = new List<WeaponDefinition>();
        GameFlowManager flow = GameFlowManager.Instance;
        SecretLairManager lair = SecretLairManager.Instance;
        if (flow == null)
        {
            if (currentWeapon != null && (!pistolsOnly || currentWeapon.weaponClass == WeaponClass.Pistol))
            {
                available.Add(currentWeapon);
            }

            return available;
        }

        foreach (WeaponDefinition weapon in flow.weaponCatalog)
        {
            if (weapon == null) continue;
            if (pistolsOnly && weapon.weaponClass != WeaponClass.Pistol) continue;
            bool unlocked = lair == null
                ? currentWeapon != null && weapon.weaponId == currentWeapon.weaponId
                : lair.IsWeaponUnlocked(weapon.weaponId);
            if (unlocked) available.Add(weapon);
        }

        return available;
    }

    private bool EquipFromLoadout(WeaponDefinition weapon)
    {
        if (weapon == null) return false;
        if (currentWeapon != null && currentWeapon.weaponId == weapon.weaponId)
        {
            SetFeedback(weapon.weaponName.ToUpperInvariant(), false, 0.5f);
            return true;
        }

        SetWeapon(weapon, false);
        SecretLairManager.Instance?.EquipWeapon(weapon);
        SetFeedback($"EQUIPPED: {weapon.weaponName.ToUpperInvariant()}", false, 0.8f);
        return true;
    }

    private struct AmmoState
    {
        public int magazine;
        public int reserve;
    }

    private bool TryGetShotHit(Vector3 origin, Vector3 direction, float range, out RaycastHit closestHit)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, range, ~0, QueryTriggerInteraction.Ignore);
        closestHit = new RaycastHit();
        float closestDistance = float.MaxValue;
        bool foundHit = false;

        foreach (RaycastHit candidate in hits)
        {
            if (candidate.collider == null || candidate.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (candidate.distance < closestDistance)
            {
                closestHit = candidate;
                closestDistance = candidate.distance;
                foundHit = true;
            }
        }

        return foundHit;
    }

    // Tranquilizer weapons fire visible darts; everything else fires bullets.
    private static bool FiresDarts(WeaponDefinition weapon)
    {
        if (weapon == null) return true;
        string id = weapon.weaponId ?? string.Empty;
        return id.Contains("tranq") || id.Contains("dart");
    }

    private void SetFeedback(string message, bool hitEnemy, float duration)
    {
        FeedbackMessage = message;
        FeedbackUntil = Time.time + duration;
        EnemyHitRecently = hitEnemy;
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzle == null) return;

        Color flashColor = FiresDarts(currentWeapon) ? new Color(0.75f, 0.95f, 1f) : new Color(1f, 0.78f, 0.35f);
        GameObject flash = CreateEffectSphere("Muzzle Flash", muzzle.position + muzzle.forward * 0.25f, flashColor, FiresDarts(currentWeapon) ? 0.09f : 0.16f);
        Destroy(flash, 0.1f);
    }

    private void SpawnTracer(Vector3 start, Vector3 end, Color color)
    {
        GameObject tracerObject = new GameObject("Dart Tracer");
        LineRenderer line = tracerObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.055f;
        line.endWidth = 0.018f;
        line.numCapVertices = 3;
        line.startColor = color;
        line.endColor = color;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader != null)
        {
            Material material = new Material(shader);
            material.color = color;
            line.material = material;
            Destroy(material, tracerDuration + 0.1f);
        }

        Destroy(tracerObject, tracerDuration);
    }

    private void SpawnImpact(Vector3 position, Color color)
    {
        GameObject impact = CreateEffectSphere("Dart Impact", position, color, 0.13f);
        Destroy(impact, 0.25f);
    }

    private GameObject CreateEffectSphere(string effectName, Vector3 position, Color color, float size)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.name = effectName;
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * size;

        Collider effectCollider = effect.GetComponent<Collider>();
        if (effectCollider != null)
        {
            effectCollider.enabled = false;
            Destroy(effectCollider);
        }

        Renderer effectRenderer = effect.GetComponent<Renderer>();
        if (effectRenderer != null)
        {
            effectRenderer.material.color = color;
        }

        return effect;
    }
}
