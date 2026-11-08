using UnityEngine;

/// <summary>
/// A grenade or rocket in flight. Grenades drop under gravity; rockets fly
/// straight with a smoke trail. Either explodes on the first thing it hits
/// (or when it runs out of range) and damages everything in the blast.
/// </summary>
public class ExplosiveProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private bool arcs;
    private float blastRadius;
    private float damage;
    private float expiresAt;
    private Transform owner;
    private SpyWeaponController weapon;
    private bool exploded;

    private static Shader trailShader;

    public static ExplosiveProjectile Launch(
        Vector3 start,
        Vector3 direction,
        float speed,
        bool arcs,
        float blastRadius,
        float damage,
        bool rocket,
        float range,
        Transform owner,
        SpyWeaponController weapon)
    {
        GameObject root = new GameObject(rocket ? "Rocket Round" : "Grenade Round");
        root.transform.position = start;
        root.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        if (rocket) BuildRocket(root.transform);
        else BuildGrenade(root.transform);

        ExplosiveProjectile projectile = root.AddComponent<ExplosiveProjectile>();
        projectile.velocity = direction.normalized * Mathf.Max(1f, speed);
        projectile.arcs = arcs;
        projectile.blastRadius = blastRadius;
        projectile.damage = damage;
        projectile.owner = owner;
        projectile.weapon = weapon;
        projectile.expiresAt = Time.time + (arcs ? 6f : Mathf.Max(0.5f, range / Mathf.Max(1f, speed)));
        return projectile;
    }

    private void Update()
    {
        if (exploded) return;

        if (arcs) velocity += Physics.gravity * Time.deltaTime;
        Vector3 step = velocity * Time.deltaTime;
        if (TryFindImpact(transform.position, step, out Vector3 impact))
        {
            Explode(impact);
            return;
        }

        transform.position += step;
        if (velocity.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        if (Time.time >= expiresAt) Explode(transform.position);
    }

    private bool TryFindImpact(Vector3 from, Vector3 step, out Vector3 impact)
    {
        impact = Vector3.zero;
        float distance = step.magnitude;
        if (distance < 0.0001f) return false;

        RaycastHit[] hits = Physics.SphereCastAll(from, 0.08f, step / distance, distance, ~0, QueryTriggerInteraction.Ignore);
        float closest = float.MaxValue;
        bool found = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            if (owner != null && hit.collider.transform.IsChildOf(owner)) continue;
            if (hit.collider.transform.IsChildOf(transform)) continue;
            // The player shoots out through the lair's one-way defense glass.
            if (hit.collider.name.Contains("Defense Glass")) continue;
            if (hit.distance >= closest) continue;
            closest = hit.distance;
            impact = hit.distance > 0f ? hit.point : from;
            found = true;
        }

        return found;
    }

    private void Explode(Vector3 point)
    {
        exploded = true;
        // Pull the blast back off the surface slightly so walls don't hide it.
        Vector3 back = velocity.sqrMagnitude > 0.01f ? -velocity.normalized * 0.25f : Vector3.zero;
        ExplosionEffect.Detonate(point + back, blastRadius, damage, owner, weapon);
        Destroy(gameObject);
    }

    private static void BuildGrenade(Transform root)
    {
        CreatePart(root, PrimitiveType.Sphere, "Grenade Shell", Vector3.zero, new Vector3(0.09f, 0.09f, 0.12f), Quaternion.identity, new Color(0.24f, 0.28f, 0.18f));
        CreatePart(root, PrimitiveType.Cylinder, "Grenade Band", Vector3.zero, new Vector3(0.095f, 0.01f, 0.095f), Quaternion.Euler(90f, 0f, 0f), new Color(0.95f, 0.75f, 0.1f));
    }

    private static void BuildRocket(Transform root)
    {
        Color body = new Color(0.25f, 0.29f, 0.2f);
        CreatePart(root, PrimitiveType.Cylinder, "Rocket Body", Vector3.zero, new Vector3(0.11f, 0.22f, 0.11f), Quaternion.Euler(90f, 0f, 0f), body);
        CreatePart(root, PrimitiveType.Sphere, "Rocket Nose", new Vector3(0f, 0f, 0.24f), new Vector3(0.11f, 0.11f, 0.2f), Quaternion.identity, body);
        CreatePart(root, PrimitiveType.Cylinder, "Rocket Band", new Vector3(0f, 0f, 0.12f), new Vector3(0.115f, 0.015f, 0.115f), Quaternion.Euler(90f, 0f, 0f), new Color(0.85f, 0.16f, 0.12f));
        for (int i = 0; i < 4; i++)
        {
            CreatePart(root, PrimitiveType.Cube, $"Rocket Fin {i + 1}", Quaternion.Euler(0f, 0f, i * 90f) * new Vector3(0.07f, 0f, -0.2f), new Vector3(0.06f, 0.006f, 0.08f), Quaternion.Euler(0f, 0f, i * 90f), body);
        }

        GameObject flame = CreatePart(root, PrimitiveType.Sphere, "Rocket Exhaust Flame", new Vector3(0f, 0f, -0.27f), new Vector3(0.08f, 0.08f, 0.16f), Quaternion.identity, new Color(1f, 0.65f, 0.2f));
        Light glow = flame.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.6f, 0.25f);
        glow.range = 6f;
        glow.intensity = 3f;
        glow.shadows = LightShadows.None;

        TrailRenderer smoke = root.gameObject.AddComponent<TrailRenderer>();
        smoke.time = 0.9f;
        smoke.startWidth = 0.18f;
        smoke.endWidth = 0.7f;
        smoke.minVertexDistance = 0.2f;
        smoke.startColor = new Color(0.8f, 0.78f, 0.74f, 0.7f);
        smoke.endColor = new Color(0.5f, 0.5f, 0.5f, 0f);
        smoke.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (trailShader == null)
        {
            trailShader = Shader.Find("Sprites/Default");
            if (trailShader == null) trailShader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (trailShader != null) smoke.material = new Material(trailShader);
    }

    private static GameObject CreatePart(Transform root, PrimitiveType type, string partName, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        Collider partCollider = part.GetComponent<Collider>();
        if (partCollider != null)
        {
            partCollider.enabled = false;
            Destroy(partCollider);
        }

        part.transform.SetParent(root, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;
        Renderer partRenderer = part.GetComponent<Renderer>();
        if (partRenderer != null)
        {
            partRenderer.material.color = color;
            partRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        return part;
    }
}
