using UnityEngine;

/// <summary>
/// The visible round for a shot: a tranquilizer dart (body, needle, fletching)
/// or a bullet with a short streak. Purely visual — hits are resolved
/// instantly by the weapon — it flies from the muzzle to the hit point, then a
/// dart sticks where it landed and a bullet leaves a small puff.
/// </summary>
public class ShotProjectileVisual : MonoBehaviour
{
    private const float DartSpeed = 55f;
    private const float BulletSpeed = 260f;
    private const float StuckDartLifetime = 6f;

    private Vector3 target;
    private float speed;
    private bool isDart;
    private bool hitSurface;
    private Transform stickTo;
    private System.Action onArrive;

    private static Shader trailShader;

    public static void Fire(Vector3 start, Vector3 end, bool dart, bool hitSurface, Transform stickTo, System.Action onArrive = null)
    {
        Vector3 direction = end - start;
        if (direction.sqrMagnitude < 0.0001f)
        {
            onArrive?.Invoke();
            return;
        }

        GameObject root = new GameObject(dart ? "Tranquilizer Dart Round" : "Bullet Round");
        root.transform.position = start;
        root.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        if (dart)
        {
            BuildDart(root.transform);
            root.transform.localScale = Vector3.one * 1.4f;
        }
        else BuildBullet(root.transform);

        ShotProjectileVisual visual = root.AddComponent<ShotProjectileVisual>();
        visual.target = end;
        visual.speed = dart ? DartSpeed : BulletSpeed;
        visual.isDart = dart;
        visual.hitSurface = hitSurface;
        visual.stickTo = stickTo;
        visual.onArrive = onArrive;
    }

    /// <summary>
    /// The shock pistol's electric bolt: a jagged blue arc from the muzzle to
    /// the hit point that flickers for a moment, with a spark where it lands.
    /// </summary>
    public static void FireShockBolt(Vector3 start, Vector3 end, bool hitSurface)
    {
        GameObject bolt = new GameObject("Shock Bolt");
        LineRenderer line = bolt.AddComponent<LineRenderer>();
        Vector3 span = end - start;
        float length = span.magnitude;
        int segments = Mathf.Clamp(Mathf.CeilToInt(length / 0.8f), 4, 40);
        Vector3 side = Vector3.Cross(span.normalized, Vector3.up);
        if (side.sqrMagnitude < 0.01f) side = Vector3.right;
        side.Normalize();
        Vector3 up = Vector3.Cross(side, span.normalized);

        line.useWorldSpace = true;
        line.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float jitter = (i == 0 || i == segments) ? 0f : 0.12f;
            Vector3 offset = side * Random.Range(-jitter, jitter) + up * Random.Range(-jitter, jitter);
            line.SetPosition(i, Vector3.Lerp(start, end, t) + offset);
        }

        line.startWidth = 0.05f;
        line.endWidth = 0.025f;
        line.numCapVertices = 2;
        line.startColor = new Color(0.75f, 0.9f, 1f, 1f);
        line.endColor = new Color(0.3f, 0.6f, 1f, 0.8f);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (trailShader == null)
        {
            trailShader = Shader.Find("Sprites/Default");
            if (trailShader == null) trailShader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (trailShader != null) line.material = new Material(trailShader);
        Destroy(bolt, 0.09f);

        if (!hitSurface) return;
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.name = "Shock Bolt Spark";
        Collider sparkCollider = spark.GetComponent<Collider>();
        if (sparkCollider != null)
        {
            sparkCollider.enabled = false;
            Destroy(sparkCollider);
        }

        spark.transform.position = end;
        spark.transform.localScale = Vector3.one * 0.16f;
        Renderer sparkRenderer = spark.GetComponent<Renderer>();
        if (sparkRenderer != null)
        {
            sparkRenderer.material.color = new Color(0.55f, 0.8f, 1f);
            sparkRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        Light glow = spark.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(0.45f, 0.7f, 1f);
        glow.range = 3f;
        glow.intensity = 2.5f;
        glow.shadows = LightShadows.None;
        Destroy(spark, 0.15f);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if ((transform.position - target).sqrMagnitude > 0.0001f) return;

        enabled = false;
        onArrive?.Invoke();
        onArrive = null;
        if (!hitSurface)
        {
            Destroy(gameObject);
            return;
        }

        if (isDart)
        {
            // Bury the needle slightly and leave the dart where it landed.
            transform.position = target - transform.forward * 0.02f;
            if (stickTo != null) transform.SetParent(stickTo, true);
            Destroy(gameObject, StuckDartLifetime);
        }
        else
        {
            SpawnImpactPuff(target);
            Destroy(gameObject);
        }
    }

    private static void BuildDart(Transform root)
    {
        Color barrel = new Color(0.78f, 0.8f, 0.83f);
        Color needle = new Color(0.92f, 0.93f, 0.95f);
        Color fletching = new Color(1f, 0.45f, 0.08f);
        Color band = new Color(0.15f, 0.85f, 0.9f);

        // Barrel and needle run along the root's forward (+Z) axis.
        CreatePart(root, PrimitiveType.Cylinder, "Dart Barrel", new Vector3(0f, 0f, -0.04f), new Vector3(0.016f, 0.05f, 0.016f), Quaternion.Euler(90f, 0f, 0f), barrel);
        CreatePart(root, PrimitiveType.Cylinder, "Dart Band", new Vector3(0f, 0f, -0.005f), new Vector3(0.018f, 0.008f, 0.018f), Quaternion.Euler(90f, 0f, 0f), band);
        CreatePart(root, PrimitiveType.Cylinder, "Dart Needle", new Vector3(0f, 0f, 0.035f), new Vector3(0.004f, 0.03f, 0.004f), Quaternion.Euler(90f, 0f, 0f), needle);
        CreatePart(root, PrimitiveType.Cube, "Dart Fletching A", new Vector3(0f, 0f, -0.1f), new Vector3(0.045f, 0.003f, 0.035f), Quaternion.identity, fletching);
        CreatePart(root, PrimitiveType.Cube, "Dart Fletching B", new Vector3(0f, 0f, -0.1f), new Vector3(0.003f, 0.045f, 0.035f), Quaternion.identity, fletching);
    }

    private static void BuildBullet(Transform root)
    {
        Color brass = new Color(0.78f, 0.6f, 0.26f);
        CreatePart(root, PrimitiveType.Capsule, "Bullet Slug", Vector3.zero, new Vector3(0.016f, 0.028f, 0.016f), Quaternion.Euler(90f, 0f, 0f), brass);

        TrailRenderer trail = root.gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.045f;
        trail.startWidth = 0.02f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        trail.startColor = new Color(1f, 0.85f, 0.5f, 0.75f);
        trail.endColor = new Color(1f, 0.7f, 0.3f, 0f);
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (trailShader == null)
        {
            trailShader = Shader.Find("Sprites/Default");
            if (trailShader == null) trailShader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (trailShader != null) trail.material = new Material(trailShader);
    }

    private static void CreatePart(Transform root, PrimitiveType type, string partName, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Color color)
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
    }

    private static void SpawnImpactPuff(Vector3 position)
    {
        GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        puff.name = "Bullet Impact Puff";
        Collider puffCollider = puff.GetComponent<Collider>();
        if (puffCollider != null)
        {
            puffCollider.enabled = false;
            Destroy(puffCollider);
        }

        puff.transform.position = position;
        puff.transform.localScale = Vector3.one * 0.09f;
        Renderer puffRenderer = puff.GetComponent<Renderer>();
        if (puffRenderer != null) puffRenderer.material.color = new Color(0.55f, 0.5f, 0.42f);
        Destroy(puff, 0.18f);
    }
}
