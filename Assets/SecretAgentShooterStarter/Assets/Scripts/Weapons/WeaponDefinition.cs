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
