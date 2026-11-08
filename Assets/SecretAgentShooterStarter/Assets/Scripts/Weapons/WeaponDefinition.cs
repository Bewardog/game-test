using UnityEngine;

public enum WeaponClass
{
    Pistol,
    SubmachineGun,
    Rifle,
    Shotgun,
    Heavy,
    Gadget
}

/// <summary>What a weapon fires.</summary>
public enum WeaponRound
{
    Dart,       // tranquilizer dart: sedates, instant knockout on a headshot
    Bullet,     // lethal round
    ShockBolt,  // electric stun bolt: sedates
    Grenade,    // arcing explosive round
    Rocket      // straight-flying explosive rocket
}

[CreateAssetMenu(menuName = "Secret Agent/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    public string weaponId = "tranq-pistol";
    public string weaponName = "Tranquilizer Pistol";
    public WeaponClass weaponClass = WeaponClass.Pistol;
    public bool nonlethal = true;
    public float sedationDamage = 35f;
    public float lethalDamage = 0f;
    public float fireRate = 0.45f;
    public float range = 80f;
    public int magazineSize = 12;
    public int startingReserveAmmo = 36;
    public int maxReserveAmmo = 144;
    public int ammoPerPickup = 8;
    public int unlockedAtArmoryLevel = 0;
    public GameObject projectilePrefab;

    [Header("Rounds")]
    public WeaponRound round = WeaponRound.Dart;
    public float reloadDuration = 1.15f;

    [Header("Dual ammo (switch with X)")]
    [Tooltip("The weapon can also load lethal bullets, with their own ammo.")]
    public bool dualAmmo;
    public float bulletDamage;
    public int bulletMagazineSize = 12;
    public int bulletStartingReserveAmmo = 36;
    public int bulletMaxReserveAmmo = 120;
    public int bulletAmmoPerPickup = 12;

    [Header("Explosives")]
    public float blastRadius;
    public float projectileSpeed = 40f;
    [Tooltip("The round drops under gravity (grenades) instead of flying straight (rockets).")]
    public bool projectileArcs;

    public bool IsExplosive => round == WeaponRound.Grenade || round == WeaponRound.Rocket;

    public WeaponDefinition WithRound(WeaponRound value)
    {
        round = value;
        return this;
    }

    public WeaponDefinition WithMagazine(int magazine, int startingReserve, int maxReserve)
    {
        magazineSize = magazine;
        startingReserveAmmo = startingReserve;
        maxReserveAmmo = maxReserve;
        return this;
    }

    public WeaponDefinition WithBulletMode(float damage, int magazine, int startingReserve, int maxReserve, int perPickup)
    {
        dualAmmo = true;
        bulletDamage = damage;
        bulletMagazineSize = magazine;
        bulletStartingReserveAmmo = startingReserve;
        bulletMaxReserveAmmo = maxReserve;
        bulletAmmoPerPickup = perPickup;
        return this;
    }

    public WeaponDefinition WithExplosive(float radius, float speed, bool arcs)
    {
        blastRadius = radius;
        projectileSpeed = speed;
        projectileArcs = arcs;
        return this;
    }

    public WeaponDefinition WithReload(float seconds)
    {
        reloadDuration = seconds;
        return this;
    }

    /// <summary>The model under Resources for a weapon id.</summary>
    public static string GetModelResourcePath(string weaponId)
    {
        switch (weaponId)
        {
            case "dart-smg":
                return "Models/Weapons/SedativeSMG";
            case "tranq-rifle":
                return "Models/Weapons/PrecisionRifle";
            case "shock-pistol":
                return "Models/Weapons/ShockPistol";
            case "assault-rifle":
                return "Models/Weapons/AssaultRifle";
            case "grenade-launcher":
                return "Models/Weapons/GrenadeLauncher";
            case "rocket-launcher":
                return "Models/Weapons/RocketLauncher";
            default:
                return "Models/Weapons/TranquilizerPistol";
        }
    }

    public static WeaponDefinition CreateRuntime(
        string id,
        string displayName,
        WeaponClass weaponClass,
        float sedationDamage,
        float lethalDamage,
        float fireRate,
        float range,
        int ammoPerPickup,
        int armoryLevel)
    {
        WeaponDefinition weapon = CreateInstance<WeaponDefinition>();
        weapon.name = displayName;
        weapon.hideFlags = HideFlags.HideAndDontSave;
        weapon.weaponId = id;
        weapon.weaponName = displayName;
        weapon.weaponClass = weaponClass;
        weapon.nonlethal = lethalDamage <= 0f;
        weapon.sedationDamage = sedationDamage;
        weapon.lethalDamage = lethalDamage;
        weapon.fireRate = fireRate;
        weapon.range = range;
        weapon.ammoPerPickup = ammoPerPickup;
        weapon.unlockedAtArmoryLevel = armoryLevel;

        switch (weaponClass)
        {
            case WeaponClass.SubmachineGun:
                weapon.magazineSize = 30;
                weapon.startingReserveAmmo = 90;
                weapon.maxReserveAmmo = 240;
                break;
            case WeaponClass.Rifle:
                weapon.magazineSize = 8;
                weapon.startingReserveAmmo = 32;
                weapon.maxReserveAmmo = 96;
                break;
            case WeaponClass.Shotgun:
                weapon.magazineSize = 6;
                weapon.startingReserveAmmo = 24;
                weapon.maxReserveAmmo = 72;
                break;
            case WeaponClass.Heavy:
                weapon.magazineSize = 60;
                weapon.startingReserveAmmo = 120;
                weapon.maxReserveAmmo = 300;
                break;
            default:
                weapon.magazineSize = weaponClass == WeaponClass.Gadget ? 4 : 12;
                weapon.startingReserveAmmo = weaponClass == WeaponClass.Gadget ? 12 : 36;
                weapon.maxReserveAmmo = weaponClass == WeaponClass.Gadget ? 36 : 144;
                break;
        }

        return weapon;
    }
}
