using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TranquilizerDart : MonoBehaviour
{
    public float speed = 55f;
    public float lifeSeconds = 4f;
    public float sedationAmount = 35f;
    public float lethalDamage;

    private Vector3 direction = Vector3.forward;
    private float destroyAt;
    private SpyWeaponController owner;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        destroyAt = Time.time + lifeSeconds;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Time.time >= destroyAt)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(
        Vector3 launchDirection,
        float sedation,
        float damage,
        SpyWeaponController weaponOwner = null)
    {
        direction = launchDirection.normalized;
        sedationAmount = sedation;
        lethalDamage = damage;
        owner = weaponOwner;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyOperative enemy = other.GetComponentInParent<EnemyOperative>();

        if (enemy != null)
        {
            owner?.RecordProjectileHit();
            if (sedationAmount > 0f)
            {
                enemy.ReceiveTranquilizerDart(sedationAmount, transform.position, out _);
            }

            if (lethalDamage > 0f)
            {
                enemy.TakeDamage(lethalDamage);
            }
        }
        else
        {
            MissionSecurityCamera securityCamera = other.GetComponentInParent<MissionSecurityCamera>();
            if (securityCamera != null && securityCamera.ReceiveWeaponHit(transform.position))
            {
                owner?.RecordProjectileHit();
            }
        }

        Destroy(gameObject);
    }
}
