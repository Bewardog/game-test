using UnityEngine;

public class EnemyLootDrop : MonoBehaviour
{
    public EnemyOperative operative;
    public WeaponDefinition droppedWeapon;
    public int ammoAmount = 12;
    public int healthAmount;

    private bool dropped;

    private void OnEnable()
    {
        EnemyOperative.AnyKnockedOut += HandleNeutralized;
        EnemyOperative.AnyKilled += HandleNeutralized;
    }

    private void OnDisable()
    {
        EnemyOperative.AnyKnockedOut -= HandleNeutralized;
        EnemyOperative.AnyKilled -= HandleNeutralized;
    }

    public void Configure(EnemyOperative source, WeaponDefinition weapon, int ammo, int health)
    {
        operative = source;
        droppedWeapon = weapon;
        ammoAmount = ammo;
        healthAmount = health;
    }

    private void HandleNeutralized(EnemyOperative neutralized)
    {
        if (dropped || neutralized == null || neutralized != operative) return;
        dropped = true;

        Vector3 origin = neutralized.transform.position + Vector3.up * 0.65f;
        if (droppedWeapon != null)
        {
            CreatePickup(
                $"Dropped {droppedWeapon.weaponName}",
                origin + Vector3.left * 0.45f,
                PickupType.Weapon,
                droppedWeapon.ammoPerPickup,
                droppedWeapon,
                GetWeaponColor(droppedWeapon.weaponClass),
                new Vector3(0.28f, 0.28f, 0.9f));
        }
        else if (ammoAmount > 0)
        {
            CreatePickup(
                "Dropped Ammo",
                origin,
                PickupType.Ammo,
                ammoAmount,
                null,
                new Color(0.18f, 0.75f, 0.9f),
                new Vector3(0.55f, 0.35f, 0.45f));
        }

        if (healthAmount > 0)
        {
            CreatePickup(
                "Dropped Medical Kit",
                origin + Vector3.right * 0.45f,
                PickupType.Health,
                healthAmount,
                null,
                new Color(0.2f, 0.85f, 0.35f),
                new Vector3(0.45f, 0.45f, 0.45f));
        }
    }

    private static void CreatePickup(
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

        SphereCollider trigger = pickupObject.AddComponent<SphereCollider>();
        trigger.radius = weapon != null ? 0.65f : 0.55f;
        trigger.isTrigger = true;

        string resourcePath = weapon != null
            ? GetWeaponModelResourcePath(weapon.weaponId)
            : "Models/Environment/SupplyCrate_A";
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab != null)
        {
            GameObject visual = Instantiate(prefab, pickupObject.transform);
            visual.name = $"{pickupName} Model";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = weapon != null
                ? Quaternion.Euler(0f, 90f, 18f)
                : Quaternion.identity;
            visual.transform.localScale = weapon != null
                ? Vector3.one * 0.42f
                : Vector3.one * 0.5f;
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
            Renderer renderer = fallback.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
        }

        FieldPickup pickup = pickupObject.AddComponent<FieldPickup>();
        pickup.pickupType = type;
        pickup.amount = amount;
        pickup.weaponDefinition = weapon;
    }

    private static string GetWeaponModelResourcePath(string weaponId)
    {
        return WeaponDefinition.GetModelResourcePath(weaponId);
    }

    private static Color GetWeaponColor(WeaponClass weaponClass)
    {
        switch (weaponClass)
        {
            case WeaponClass.SubmachineGun: return new Color(0.9f, 0.25f, 0.2f);
            case WeaponClass.Rifle: return new Color(0.25f, 0.7f, 0.32f);
            case WeaponClass.Shotgun: return new Color(0.88f, 0.6f, 0.16f);
            case WeaponClass.Heavy: return new Color(0.45f, 0.28f, 0.22f);
            default: return new Color(0.15f, 0.75f, 0.9f);
        }
    }
}
