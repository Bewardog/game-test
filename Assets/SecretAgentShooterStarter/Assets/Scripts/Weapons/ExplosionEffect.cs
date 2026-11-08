using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An explosion: damages every enemy in the blast (less toward the edge, and
/// not through walls), knocks out security cameras, pushes loose bodies,
/// makes a lot of noise, and shows a fireball, smoke and a flash of light.
/// The player is hurt too if caught in their own blast.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    private const float PlayerSelfDamageShare = 0.5f;

    private Transform fireball;
    private Transform smoke;
    private Renderer fireballRenderer;
    private Renderer smokeRenderer;
    private Light flash;
    private float radius;
    private float startedAt;

    private static Shader effectShader;

    public static void Detonate(Vector3 point, float radius, float damage, Transform owner, SpyWeaponController weapon)
    {
        radius = Mathf.Max(0.5f, radius);
        bool hitAnyone = false;
        HashSet<EnemyOperative> damaged = new HashSet<EnemyOperative>();
        HashSet<MissionSecurityCamera> cameras = new HashSet<MissionSecurityCamera>();
        HashSet<Rigidbody> bodies = new HashSet<Rigidbody>();

        foreach (Collider hit in Physics.OverlapSphere(point, radius, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit == null) continue;
            EnemyOperative enemy = hit.GetComponentInParent<EnemyOperative>();
            if (enemy != null && !enemy.isDead && damaged.Add(enemy))
            {
                Vector3 center = enemy.transform.position + Vector3.up * 1f;
                if (!IsShielded(point, center, enemy.transform))
                {
                    float falloff = Mathf.Lerp(1f, 0.3f, Mathf.Clamp01(Vector3.Distance(point, center) / radius));
                    enemy.TakeDamage(damage * falloff);
                    hitAnyone = true;
                }
            }

            MissionSecurityCamera securityCamera = hit.GetComponentInParent<MissionSecurityCamera>();
            if (securityCamera != null) cameras.Add(securityCamera);
            if (hit.attachedRigidbody != null && !hit.attachedRigidbody.isKinematic) bodies.Add(hit.attachedRigidbody);
        }

        foreach (MissionSecurityCamera securityCamera in cameras) securityCamera.ReceiveWeaponHit(point);
        foreach (Rigidbody body in bodies) body.AddExplosionForce(900f, point, radius, 0.6f, ForceMode.Impulse);

        if (owner != null)
        {
            SpyPlayerController player = owner.GetComponentInParent<SpyPlayerController>();
            if (player != null)
            {
                Vector3 chest = player.transform.position + Vector3.up * 1f;
                float distance = Vector3.Distance(point, chest);
                if (distance < radius && !IsShielded(point, chest, player.transform))
                {
                    float falloff = Mathf.Lerp(1f, 0.3f, distance / radius);
                    player.ApplyDamage(damage * falloff * PlayerSelfDamageShare, "Explosion", point);
                }
            }
        }

        if (hitAnyone) weapon?.RecordProjectileHit();
        EnemyCombatController.BroadcastNoise(point, 45f, true, "an explosion");
        Spawn(point, radius);
    }

    // True when a wall or other solid object stands between the blast and the target.
    private static bool IsShielded(Vector3 from, Vector3 to, Transform target)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        if (distance < 0.05f) return false;

        foreach (RaycastHit hit in Physics.RaycastAll(from, direction / distance, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(target)) continue;
            if (hit.collider.GetComponentInParent<EnemyOperative>() != null) continue;
            if (hit.collider.GetComponentInParent<SpyPlayerController>() != null) continue;
            return true;
        }

        return false;
    }

    private static void Spawn(Vector3 point, float radius)
    {
        GameObject root = new GameObject("Explosion");
        root.transform.position = point;
        ExplosionEffect effect = root.AddComponent<ExplosionEffect>();
        effect.radius = radius;
        effect.startedAt = Time.time;
        effect.fireball = CreateSphere(root.transform, "Explosion Fireball", new Color(1f, 0.62f, 0.2f, 0.95f), out effect.fireballRenderer);
        effect.smoke = CreateSphere(root.transform, "Explosion Smoke", new Color(0.3f, 0.28f, 0.26f, 0.7f), out effect.smokeRenderer);

        GameObject lightObject = new GameObject("Explosion Flash");
        lightObject.transform.SetParent(root.transform, false);
        effect.flash = lightObject.AddComponent<Light>();
        effect.flash.type = LightType.Point;
        effect.flash.color = new Color(1f, 0.65f, 0.3f);
        effect.flash.range = radius * 3.5f;
        effect.flash.intensity = 8f;
        effect.flash.shadows = LightShadows.None;
        Destroy(root, 1.6f);
    }

    private static Transform CreateSphere(Transform parent, string sphereName, Color color, out Renderer sphereRenderer)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = sphereName;
        Collider sphereCollider = sphere.GetComponent<Collider>();
        if (sphereCollider != null)
        {
            sphereCollider.enabled = false;
            Destroy(sphereCollider);
        }

        sphere.transform.SetParent(parent, false);
        sphere.transform.localScale = Vector3.zero;
        sphereRenderer = sphere.GetComponent<Renderer>();
        if (effectShader == null) effectShader = Shader.Find("Sprites/Default");
        if (effectShader != null) sphereRenderer.material = new Material(effectShader);
        sphereRenderer.material.color = color;
        sphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return sphere.transform;
    }

    private void Update()
    {
        float t = Time.time - startedAt;

        // Fireball: bursts out fast, then fades.
        float fireT = Mathf.Clamp01(t / 0.35f);
        fireball.localScale = Vector3.one * (radius * 1.5f * Mathf.Sqrt(fireT));
        Color fire = fireballRenderer.material.color;
        fire.a = Mathf.Lerp(0.95f, 0f, fireT);
        fireballRenderer.material.color = fire;

        // Smoke: grows and rises more slowly.
        float smokeT = Mathf.Clamp01(t / 1.5f);
        smoke.localScale = Vector3.one * (radius * 1.8f * Mathf.Sqrt(smokeT));
        smoke.localPosition = Vector3.up * (smokeT * radius * 0.5f);
        Color grey = smokeRenderer.material.color;
        grey.a = Mathf.Lerp(0.7f, 0f, smokeT);
        smokeRenderer.material.color = grey;

        if (flash != null) flash.intensity = Mathf.Lerp(8f, 0f, Mathf.Clamp01(t / 0.3f));
    }
}
