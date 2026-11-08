using System;
using System.Collections.Generic;
using UnityEngine;

public class SecretLairManager : MonoBehaviour
{
    public static SecretLairManager Instance { get; private set; }

    public int currency;
    public List<RoomState> rooms = new List<RoomState>();
    public List<string> unlockedWeaponIds = new List<string> { "tranq-pistol" };
    public string equippedWeaponId = "tranq-pistol";
    public int tranquilizerResearchLevel;
    public int armorResearchLevel;
    public int weaponsResearchLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureDefaultRooms();
        CampaignPersistence.LoadInto(this);
        RefreshRoomVisuals();
    }

    public int GetRoomLevel(RoomType type)
    {
        RoomState room = FindRoom(type);
        return room != null ? room.level : 0;
    }

    public bool CanUpgrade(RoomType type)
    {
        RoomState room = FindRoom(type);
        return room != null && room.level < room.maxLevel && currency >= room.CurrentUpgradeCost;
    }

    public bool TryUpgrade(RoomType type)
    {
        RoomState room = FindRoom(type);
        if (room == null || room.level >= room.maxLevel) return false;
        if (currency < room.CurrentUpgradeCost) return false;

        currency -= room.CurrentUpgradeCost;
        room.level++;
        RefreshRoomVisuals();
        CampaignPersistence.SaveCurrent();
        return true;
    }

    public void RegisterRoomVisual(RoomType type, GameObject contentRoot, GameObject[] levelVisuals)
    {
        RoomState room = FindRoom(type);
        if (room == null) return;

        room.roomRoot = contentRoot;
        room.levelVisuals = levelVisuals ?? new GameObject[0];
        RefreshRoomVisuals();
    }

    public void AddCurrency(int amount)
    {
        currency += Mathf.Max(0, amount);
        CampaignPersistence.SaveCurrent();
    }

    public bool IsWeaponUnlocked(string weaponId)
    {
        return !string.IsNullOrEmpty(weaponId) && unlockedWeaponIds.Contains(weaponId);
    }

    public void UnlockWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || string.IsNullOrEmpty(weapon.weaponId)) return;
        if (!unlockedWeaponIds.Contains(weapon.weaponId)) unlockedWeaponIds.Add(weapon.weaponId);
        equippedWeaponId = weapon.weaponId;
        CampaignPersistence.SaveCurrent();
    }

    public void EquipWeapon(WeaponDefinition weapon)
    {
        if (weapon == null || !IsWeaponUnlocked(weapon.weaponId)) return;
        equippedWeaponId = weapon.weaponId;
        CampaignPersistence.SaveCurrent();
    }

    public float GetTranquilizerSpeedMultiplier()
    {
        return 1f + tranquilizerResearchLevel * 0.12f;
    }

    public int GetResearchLevel(ResearchTrack track)
    {
        switch (track)
        {
            case ResearchTrack.Armor: return armorResearchLevel;
            case ResearchTrack.Weapons: return weaponsResearchLevel;
            default: return tranquilizerResearchLevel;
        }
    }

    public int GetResearchCost(ResearchTrack track)
    {
        return 175 * (GetResearchLevel(track) + 1);
    }

    public bool TryResearch(ResearchTrack track)
    {
        int currentLevel = GetResearchLevel(track);
        int labLevel = GetRoomLevel(RoomType.ResearchLab);
        int cost = GetResearchCost(track);
        if (labLevel <= currentLevel || currency < cost) return false;

        currency -= cost;
        switch (track)
        {
            case ResearchTrack.Armor:
                armorResearchLevel++;
                break;
            case ResearchTrack.Weapons:
                weaponsResearchLevel++;
                break;
            default:
                tranquilizerResearchLevel++;
                break;
        }

        CampaignPersistence.SaveCurrent();
        return true;
    }

    public int GetStartingArmor()
    {
        return GetRoomLevel(RoomType.Armory) * 12 + armorResearchLevel * 15;
    }

    public float GetWeaponEffectivenessMultiplier()
    {
        return 1f + weaponsResearchLevel * 0.08f;
    }

    public int GetCompanionSlotCount()
    {
        return 1 + Mathf.FloorToInt(GetRoomLevel(RoomType.TrainingRoom) / 2f);
    }

    private RoomState FindRoom(RoomType type)
    {
        return rooms.Find(room => room.type == type);
    }

    private void EnsureDefaultRooms()
    {
        AddRoomIfMissing(RoomType.Armory, 250);
        AddRoomIfMissing(RoomType.ResearchLab, 300);
        AddRoomIfMissing(RoomType.OperativeCustomization, 225);
        AddRoomIfMissing(RoomType.TrainingRoom, 225);
        AddRoomIfMissing(RoomType.IntelCenter, 275);
        AddRoomIfMissing(RoomType.MedicalBay, 200);
        AddRoomIfMissing(RoomType.Garage, 350);
    }

    private void AddRoomIfMissing(RoomType type, int baseCost)
    {
        if (FindRoom(type) != null) return;

        rooms.Add(new RoomState
        {
            type = type,
            baseUpgradeCost = baseCost,
            maxLevel = 5
        });
    }

    private void RefreshRoomVisuals()
    {
        foreach (RoomState room in rooms)
        {
            if (room.roomRoot != null)
            {
                room.roomRoot.SetActive(room.level > 0);
            }

            for (int i = 0; i < room.levelVisuals.Length; i++)
            {
                if (room.levelVisuals[i] != null)
                {
                    room.levelVisuals[i].SetActive(i < room.level);
                }
            }
        }
    }
}

[Serializable]
public class RoomState
{
    public RoomType type;
    public int level;
    public int maxLevel = 5;
    public int baseUpgradeCost = 250;
    public GameObject roomRoot;
    public GameObject[] levelVisuals = new GameObject[0];

    public int CurrentUpgradeCost => baseUpgradeCost * (level + 1);
}
