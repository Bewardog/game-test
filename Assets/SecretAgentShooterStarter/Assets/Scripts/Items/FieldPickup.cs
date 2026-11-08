using UnityEngine;

public class FieldPickup : MonoBehaviour, IInteractable
{
    public PickupType pickupType = PickupType.Weapon;
    public WeaponDefinition weaponDefinition;
    public int amount = 1;
    public float bobSpeed = 2f;
    public float bobHeight = 0.25f;
    public float rotateSpeed = 90f;

    private Vector3 startPosition;

    public string InteractionPrompt => "Pick up " + pickupType;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        SpyPlayerController player = other.GetComponentInParent<SpyPlayerController>();
        if (player != null)
        {
            Collect(player);
        }
    }

    public void Interact(SpyPlayerController player)
    {
        Collect(player);
    }

    private void Collect(SpyPlayerController player)
    {
        if (player == null) return;
        bool collected = true;

        switch (pickupType)
        {
            case PickupType.Weapon:
                if (player.WeaponController != null && weaponDefinition != null)
                {
                    SecretLairManager.Instance?.UnlockWeapon(weaponDefinition);
                    player.WeaponController.SetWeapon(weaponDefinition);
                    player.ShowActionMessage($"ACQUIRED: {weaponDefinition.weaponName.ToUpperInvariant()}", 1.25f);
                }
                break;

            case PickupType.Ammo:
                if (player.WeaponController != null)
                {
                    int addedAmmo = player.WeaponController.AddAmmo(amount);
                    if (addedAmmo <= 0)
                    {
                        player.ShowActionMessage("AMMO RESERVE FULL", 0.8f);
                        collected = false;
                    }
                    else
                    {
                        player.ShowActionMessage($"+{addedAmmo} RESERVE AMMO", 0.8f);
                    }
                }
                break;

            case PickupType.Health:
                float restoredHealth = player.Heal(amount);
                if (restoredHealth <= 0f)
                {
                    player.ShowActionMessage("HEALTH FULL", 0.8f);
                    collected = false;
                }
                else
                {
                    player.ShowActionMessage($"+{Mathf.CeilToInt(restoredHealth)} HEALTH", 0.8f);
                }
                break;

            case PickupType.Armor:
                player.AddArmor(amount);
                break;

            case PickupType.Currency:
                if (SecretLairManager.Instance != null)
                {
                    SecretLairManager.Instance.AddCurrency(amount);
                }
                break;

            case PickupType.Intel:
                if (MissionManager.Instance != null)
                {
                    MissionManager.Instance.NotifyIntelStolen();
                }
                break;

            case PickupType.PrototypeWeapon:
                if (MissionManager.Instance != null)
                {
                    MissionManager.Instance.NotifyPrototypeRecovered();
                }
                break;
        }

        if (collected)
        {
            Destroy(gameObject);
        }
    }
}
