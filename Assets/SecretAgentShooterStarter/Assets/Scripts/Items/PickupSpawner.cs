using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public FieldPickup pickupPrefab;
    public Transform[] spawnPoints;
    public List<WeaponDefinition> weaponPool = new List<WeaponDefinition>();
    public bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnWeapons();
        }
    }

    public void SpawnWeapons()
    {
        if (pickupPrefab == null || spawnPoints == null) return;

        int armoryLevel = SecretLairManager.Instance != null
            ? SecretLairManager.Instance.GetRoomLevel(RoomType.Armory)
            : 0;

        List<WeaponDefinition> unlocked = weaponPool.FindAll(weapon => weapon != null && weapon.unlockedAtArmoryLevel <= armoryLevel);
        if (unlocked.Count == 0) return;

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            WeaponDefinition weapon = unlocked[Random.Range(0, unlocked.Count)];
            FieldPickup pickup = Instantiate(pickupPrefab, point.position, point.rotation);
            pickup.pickupType = PickupType.Weapon;
            pickup.weaponDefinition = weapon;
            pickup.amount = weapon.ammoPerPickup;
        }
    }
}

