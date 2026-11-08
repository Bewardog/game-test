using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PrototypeEnvironmentArt
{
    private enum SurfaceKind
    {
        Concrete,
        WallPanel,
        Metal,
        FloorTile,
        Asphalt,
        Terrain,
        Ceiling,
        Emissive
    }

    private struct EnvironmentTheme
    {
        public Color sky;
        public Color horizon;
        public Color ground;
        public Color fog;
        public Color ambient;
        public Color sunlight;
        public Color accent;
        public float fogDensity;
        public float sunIntensity;
        public float exposure;
        public bool night;
    }

    private static readonly Dictionary<SurfaceKind, Texture2D> Textures =
        new Dictionary<SurfaceKind, Texture2D>();
    private static readonly Dictionary<SurfaceKind, Texture2D> NormalTextures =
        new Dictionary<SurfaceKind, Texture2D>();
    private static readonly Dictionary<SurfaceKind, Material> Materials =
        new Dictionary<SurfaceKind, Material>();

    private static EnvironmentTheme currentTheme;
    private static MissionEnvironmentType currentEnvironment;
    private static bool isHubTheme;
    private static float currentSunPitch;
    private static float currentSunYaw;
    private static Texture2D skylineWindowTexture;
    private static Texture2D atmosphereParticleTexture;

    public static void BeginHub()
    {
        isHubTheme = true;
        currentEnvironment = MissionEnvironmentType.CorporateAnnex;
        currentTheme = new EnvironmentTheme
        {
            sky = new Color(0.075f, 0.14f, 0.21f),
            horizon = new Color(0.28f, 0.43f, 0.49f),
            ground = new Color(0.035f, 0.055f, 0.07f),
            fog = new Color(0.09f, 0.16f, 0.19f),
            ambient = new Color(0.21f, 0.29f, 0.32f),
            sunlight = new Color(0.72f, 0.86f, 0.9f),
            accent = new Color(0.16f, 0.72f, 0.72f),
            fogDensity = 0.0045f,
            sunIntensity = 0.72f,
            exposure = 0.08f,
            night = false
        };
        currentSunPitch = 38f;
        currentSunYaw = -24f;
        ResetMaterials();
    }

    public static void BeginMission(MissionEnvironmentType environment, int contractNumber = 1)
    {
        isHubTheme = false;
        currentEnvironment = environment;
        switch (environment)
        {
            case MissionEnvironmentType.WeaponsLaboratory:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.16f, 0.26f, 0.3f),
                    horizon = new Color(0.48f, 0.61f, 0.62f),
                    ground = new Color(0.055f, 0.085f, 0.09f),
                    fog = new Color(0.2f, 0.32f, 0.34f),
                    ambient = new Color(0.25f, 0.34f, 0.35f),
                    sunlight = new Color(0.68f, 0.88f, 0.86f),
                    accent = new Color(0.12f, 0.78f, 0.7f),
                    fogDensity = 0.0065f,
                    sunIntensity = 0.9f,
                    exposure = 0.04f,
                    night = false
                };
                break;
            case MissionEnvironmentType.Dockyard:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.035f, 0.07f, 0.12f),
                    horizon = new Color(0.22f, 0.34f, 0.42f),
                    ground = new Color(0.025f, 0.045f, 0.055f),
                    fog = new Color(0.08f, 0.15f, 0.19f),
                    ambient = new Color(0.13f, 0.22f, 0.27f),
                    sunlight = new Color(0.55f, 0.72f, 0.86f),
                    accent = new Color(1f, 0.54f, 0.12f),
                    fogDensity = 0.008f,
                    sunIntensity = 0.48f,
                    exposure = -0.02f,
                    night = true
                };
                break;
            case MissionEnvironmentType.WoodlandAnnex:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.21f, 0.42f, 0.58f),
                    horizon = new Color(0.64f, 0.74f, 0.7f),
                    ground = new Color(0.075f, 0.13f, 0.085f),
                    fog = new Color(0.32f, 0.43f, 0.35f),
                    ambient = new Color(0.3f, 0.39f, 0.3f),
                    sunlight = new Color(1f, 0.88f, 0.68f),
                    accent = new Color(0.2f, 0.58f, 0.72f),
                    fogDensity = 0.004f,
                    sunIntensity = 1.2f,
                    exposure = 0.12f,
                    night = false
                };
                break;
            case MissionEnvironmentType.ArcticRelay:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.4f, 0.58f, 0.74f),
                    horizon = new Color(0.82f, 0.9f, 0.93f),
                    ground = new Color(0.62f, 0.7f, 0.73f),
                    fog = new Color(0.72f, 0.82f, 0.86f),
                    ambient = new Color(0.58f, 0.69f, 0.74f),
                    sunlight = new Color(0.82f, 0.91f, 1f),
                    accent = new Color(0.28f, 0.78f, 1f),
                    fogDensity = 0.006f,
                    sunIntensity = 1.05f,
                    exposure = 0.16f,
                    night = false
                };
                break;
            case MissionEnvironmentType.ExecutiveStronghold:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.025f, 0.035f, 0.085f),
                    horizon = new Color(0.22f, 0.11f, 0.16f),
                    ground = new Color(0.018f, 0.02f, 0.03f),
                    fog = new Color(0.08f, 0.045f, 0.075f),
                    ambient = new Color(0.12f, 0.11f, 0.18f),
                    sunlight = new Color(0.48f, 0.58f, 0.85f),
                    accent = new Color(0.86f, 0.12f, 0.2f),
                    fogDensity = 0.009f,
                    sunIntensity = 0.38f,
                    exposure = -0.08f,
                    night = true
                };
                break;
            case MissionEnvironmentType.Embassy:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.035f, 0.045f, 0.095f),
                    horizon = new Color(0.25f, 0.14f, 0.2f),
                    ground = new Color(0.025f, 0.025f, 0.04f),
                    fog = new Color(0.09f, 0.06f, 0.1f),
                    ambient = new Color(0.16f, 0.13f, 0.2f),
                    sunlight = new Color(0.62f, 0.68f, 0.92f),
                    accent = new Color(0.88f, 0.2f, 0.34f),
                    fogDensity = 0.0075f,
                    sunIntensity = 0.45f,
                    exposure = -0.03f,
                    night = true
                };
                break;
            case MissionEnvironmentType.DataCenter:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.07f, 0.14f, 0.19f),
                    horizon = new Color(0.25f, 0.39f, 0.43f),
                    ground = new Color(0.025f, 0.045f, 0.055f),
                    fog = new Color(0.08f, 0.16f, 0.19f),
                    ambient = new Color(0.15f, 0.24f, 0.27f),
                    sunlight = new Color(0.62f, 0.85f, 0.9f),
                    accent = new Color(0.1f, 0.9f, 1f),
                    fogDensity = 0.0055f,
                    sunIntensity = 0.58f,
                    exposure = 0.01f,
                    night = false
                };
                break;
            default:
                currentTheme = new EnvironmentTheme
                {
                    sky = new Color(0.19f, 0.34f, 0.48f),
                    horizon = new Color(0.62f, 0.68f, 0.67f),
                    ground = new Color(0.09f, 0.11f, 0.13f),
                    fog = new Color(0.35f, 0.42f, 0.44f),
                    ambient = new Color(0.34f, 0.37f, 0.39f),
                    sunlight = new Color(0.92f, 0.9f, 0.82f),
                    accent = new Color(0.14f, 0.48f, 0.72f),
                    fogDensity = 0.0035f,
                    sunIntensity = 1.05f,
                    exposure = 0.09f,
                    night = false
                };
                break;
        }

        ApplyContractAtmosphere(contractNumber);
        ResetMaterials();
    }

    private static void ApplyContractAtmosphere(int contractNumber)
    {
        int resolvedContract = Mathf.Max(1, contractNumber);
        int operationProfile = (resolvedContract - 1) % 4;
        float campaignProgress = Mathf.Clamp01((resolvedContract - 1f) / 19f);
        float returnVisit = Mathf.Clamp01((contractNumber - 5f) / 15f);

        currentTheme.fogDensity *= Mathf.Lerp(1f, 1.34f, campaignProgress);
        currentTheme.exposure -= campaignProgress * 0.08f;
        currentTheme.accent = Color.Lerp(currentTheme.accent, new Color(0.95f, 0.18f, 0.2f), campaignProgress * 0.18f);
        currentSunPitch = currentTheme.night ? 28f : 46f;
        currentSunYaw = -42f + Mathf.Repeat(resolvedContract * 47f, 104f);

        switch (operationProfile)
        {
            case 1:
                currentTheme.horizon = Color.Lerp(
                    currentTheme.horizon,
                    new Color(0.92f, 0.5f, 0.3f),
                    currentTheme.night ? 0.12f : 0.24f);
                currentTheme.sunlight = Color.Lerp(
                    currentTheme.sunlight,
                    new Color(1f, 0.68f, 0.44f),
                    currentTheme.night ? 0.12f : 0.32f);
                currentTheme.sunIntensity *= currentTheme.night ? 0.92f : 0.86f;
                currentTheme.exposure += currentTheme.night ? 0f : 0.03f;
                currentSunPitch = currentTheme.night ? 22f : 24f;
                break;
            case 2:
                currentTheme.sky = Color.Lerp(
                    currentTheme.sky,
                    new Color(0.3f, 0.36f, 0.4f),
                    currentTheme.night ? 0.1f : 0.34f);
                currentTheme.horizon = Color.Lerp(
                    currentTheme.horizon,
                    new Color(0.48f, 0.52f, 0.54f),
                    currentTheme.night ? 0.08f : 0.3f);
                currentTheme.fog = Color.Lerp(
                    currentTheme.fog,
                    currentTheme.horizon,
                    0.24f);
                currentTheme.fogDensity *= 1.18f;
                currentTheme.sunIntensity *= 0.72f;
                currentTheme.exposure -= 0.025f;
                currentSunPitch = currentTheme.night ? 31f : 56f;
                break;
            case 3:
                currentTheme.sky = Color.Lerp(
                    currentTheme.sky,
                    new Color(0.018f, 0.028f, 0.07f),
                    currentTheme.night ? 0.28f : 0.58f);
                currentTheme.horizon = Color.Lerp(
                    currentTheme.horizon,
                    new Color(0.24f, 0.1f, 0.15f),
                    currentTheme.night ? 0.22f : 0.42f);
                currentTheme.ambient = Color.Lerp(
                    currentTheme.ambient,
                    new Color(0.11f, 0.12f, 0.19f),
                    currentTheme.night ? 0.2f : 0.46f);
                currentTheme.sunlight = Color.Lerp(
                    currentTheme.sunlight,
                    new Color(0.46f, 0.56f, 0.82f),
                    0.48f);
                currentTheme.sunIntensity *= currentTheme.night ? 0.8f : 0.55f;
                currentTheme.exposure -= currentTheme.night ? 0.035f : 0.11f;
                currentTheme.night = true;
                currentSunPitch = 18f;
                break;
        }

        if (currentEnvironment == MissionEnvironmentType.ArcticRelay)
        {
            currentTheme.fog = Color.Lerp(currentTheme.fog, Color.white, returnVisit * 0.2f);
            currentTheme.horizon = Color.Lerp(currentTheme.horizon, Color.white, returnVisit * 0.16f);
            currentTheme.fogDensity *= Mathf.Lerp(1f, 1.45f, returnVisit);
            currentTheme.sunIntensity *= Mathf.Lerp(1f, 0.72f, returnVisit);
        }
        else if (currentEnvironment == MissionEnvironmentType.Dockyard)
        {
            currentTheme.sky = Color.Lerp(currentTheme.sky, new Color(0.012f, 0.02f, 0.045f), returnVisit * 0.45f);
            currentTheme.horizon = Color.Lerp(currentTheme.horizon, new Color(0.16f, 0.23f, 0.31f), returnVisit * 0.35f);
            currentTheme.sunIntensity *= Mathf.Lerp(1f, 0.7f, returnVisit);
        }
        else if (currentEnvironment == MissionEnvironmentType.Embassy)
        {
            currentTheme.horizon = Color.Lerp(currentTheme.horizon, new Color(0.36f, 0.08f, 0.12f), returnVisit * 0.28f);
            currentTheme.ambient = Color.Lerp(currentTheme.ambient, new Color(0.18f, 0.08f, 0.12f), returnVisit * 0.24f);
        }
        else if (currentEnvironment == MissionEnvironmentType.DataCenter)
        {
            currentTheme.ambient = Color.Lerp(currentTheme.ambient, new Color(0.05f, 0.17f, 0.2f), returnVisit * 0.3f);
            currentTheme.accent = Color.Lerp(currentTheme.accent, new Color(0.08f, 0.72f, 1f), returnVisit * 0.32f);
        }
    }

    public static void StyleBlock(GameObject target, string objectName, Vector3 scale, Color tint)
    {
        if (target == null) return;
        Renderer targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer == null) return;

        SurfaceKind kind = ClassifySurface(objectName);
        Material surfaceMaterial = GetMaterial(kind);
        if (surfaceMaterial != null)
        {
            targetRenderer.sharedMaterial = surfaceMaterial;
        }
        else
        {
            targetRenderer.material.color = tint;
            return;
        }

        Vector2 tiling = GetSurfaceTiling(scale);
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(properties);
        properties.SetColor("_BaseColor", tint);
        properties.SetColor("_Color", tint);
        properties.SetVector("_BaseMap_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
        properties.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
        if (kind == SurfaceKind.Emissive)
        {
            Color emission = tint * (isHubTheme ? 2.15f : 1.8f);
            properties.SetColor("_EmissionColor", emission);
        }

        targetRenderer.SetPropertyBlock(properties);
    }

    public static void StyleKitInstance(GameObject instance, Color accent)
    {
        if (instance == null) return;

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null) continue;

            string sourceMaterialName = targetRenderer.sharedMaterial != null
                ? targetRenderer.sharedMaterial.name
                : targetRenderer.gameObject.name;
            SurfaceKind kind = ClassifyKitMaterial(sourceMaterialName);
            targetRenderer.sharedMaterial = GetMaterial(kind);

            Color tint = GetKitTint(sourceMaterialName, accent);
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", tint);
            properties.SetColor("_Color", tint);
            properties.SetVector("_BaseMap_ST", new Vector4(1f, 1f, 0f, 0f));
            properties.SetVector("_MainTex_ST", new Vector4(1f, 1f, 0f, 0f));
            if (kind == SurfaceKind.Emissive)
            {
                properties.SetColor("_EmissionColor", tint * 2.4f);
            }

            targetRenderer.SetPropertyBlock(properties);
        }
    }

    public static void FinalizeHub(Transform runtimeRoot)
    {
        FinalizeEnvironment(runtimeRoot, MissionEnvironmentType.CorporateAnnex, true);
    }

    public static void FinalizeMission(Transform runtimeRoot, MissionEnvironmentType environment)
    {
        FinalizeEnvironment(runtimeRoot, environment, false);
    }

    private static void FinalizeEnvironment(
        Transform runtimeRoot,
        MissionEnvironmentType environment,
        bool hub)
    {
        if (runtimeRoot == null) return;
        ConfigureSkybox();
        ConfigureFogAndAmbient();
        ConfigureSun(runtimeRoot);
        ConfigureReflectionProbe(runtimeRoot, hub);
        ConfigurePostProcessing(runtimeRoot);
        // The lair sits in open, endless grounds that fade into fog, so it
        // gets no ring of ridges, skyline, or backdrop wrapped around it.
        if (!hub)
        {
            CreateMountainRings(runtimeRoot, environment, hub);
            CreateEnvironmentSignatureBackdrop(runtimeRoot, environment, hub);
        }

        CreateAmbientAtmospherics(runtimeRoot, environment, hub);

        if (environment == MissionEnvironmentType.ExecutiveStronghold ||
            environment == MissionEnvironmentType.Embassy)
        {
            if (!hub) CreateDistantSkyline(runtimeRoot, 135f, false);
        }

        if (environment == MissionEnvironmentType.WeaponsLaboratory ||
            environment == MissionEnvironmentType.Dockyard)
        {
            CreateIndustrialBackdrop(runtimeRoot);
        }
    }

    private static SurfaceKind ClassifySurface(string objectName)
    {
        string lowerName = string.IsNullOrEmpty(objectName)
            ? string.Empty
            : objectName.ToLowerInvariant();

        if (ContainsAny(lowerName, "light", "accent", "strip", "window", "screen", "beacon", "scanner", "guide", "threshold"))
        {
            return SurfaceKind.Emissive;
        }

        if (ContainsAny(lowerName, "grass", "woodland", "earth", "trail", "arctic", "snow"))
        {
            return SurfaceKind.Terrain;
        }

        if (ContainsAny(lowerName, "road", "approach", "landing pad", "courtyard floor"))
        {
            return SurfaceKind.Asphalt;
        }

        if (ContainsAny(lowerName, "ceiling", "roof", "canopy"))
        {
            return SurfaceKind.Ceiling;
        }

        if (ContainsAny(lowerName, "floor", "bridge", "platform", "dais", "stairs", "step"))
        {
            return SurfaceKind.FloorTile;
        }

        if (ContainsAny(lowerName, "wall", "perimeter", "facade", "divider", "door frame"))
        {
            return SurfaceKind.WallPanel;
        }

        if (ContainsAny(lowerName, "cover", "terminal", "bench", "rail", "post", "pillar", "shaft", "gate"))
        {
            return SurfaceKind.Metal;
        }

        return SurfaceKind.Concrete;
    }

    private static SurfaceKind ClassifyKitMaterial(string materialName)
    {
        string lowerName = string.IsNullOrEmpty(materialName)
            ? string.Empty
            : materialName.ToLowerInvariant();

        if (ContainsAny(lowerName, "emissive", "light", "lens", "status", "accent"))
        {
            return SurfaceKind.Emissive;
        }

        if (ContainsAny(lowerName, "needles", "bark", "rock"))
        {
            return SurfaceKind.Terrain;
        }

        if (ContainsAny(lowerName, "floor", "plate"))
        {
            return SurfaceKind.FloorTile;
        }

        if (ContainsAny(lowerName, "wall", "panel"))
        {
            return SurfaceKind.WallPanel;
        }

        return SurfaceKind.Metal;
    }

    private static Color GetKitTint(string materialName, Color accent)
    {
        string lowerName = string.IsNullOrEmpty(materialName)
            ? string.Empty
            : materialName.ToLowerInvariant();

        if (ContainsAny(lowerName, "emissive", "light", "lens", "status", "accent"))
        {
            return Color.Lerp(accent, Color.white, 0.12f);
        }

        if (lowerName.Contains("needles"))
        {
            return currentEnvironment == MissionEnvironmentType.ArcticRelay
                ? new Color(0.17f, 0.31f, 0.32f)
                : new Color(0.075f, 0.24f, 0.12f);
        }
        if (lowerName.Contains("bark")) return new Color(0.21f, 0.12f, 0.055f);
        if (lowerName.Contains("rock"))
        {
            return currentEnvironment == MissionEnvironmentType.ArcticRelay
                ? new Color(0.58f, 0.66f, 0.68f)
                : new Color(0.28f, 0.31f, 0.29f);
        }
        if (lowerName.Contains("glass")) return new Color(0.035f, 0.12f, 0.15f);
        if (lowerName.Contains("dark")) return new Color(0.075f, 0.095f, 0.105f);
        if (lowerName.Contains("brushed")) return new Color(0.28f, 0.32f, 0.34f);
        if (lowerName.Contains("floor")) return new Color(0.17f, 0.2f, 0.21f);
        if (lowerName.Contains("wall")) return new Color(0.34f, 0.38f, 0.4f);
        return new Color(0.23f, 0.27f, 0.29f);
    }

    private static bool ContainsAny(string value, params string[] fragments)
    {
        foreach (string fragment in fragments)
        {
            if (value.Contains(fragment)) return true;
        }

        return false;
    }

    private static Vector2 GetSurfaceTiling(Vector3 scale)
    {
        const float tileSize = 2.4f;
        if (scale.y <= scale.x && scale.y <= scale.z)
        {
            return new Vector2(
                Mathf.Max(0.5f, scale.x / tileSize),
                Mathf.Max(0.5f, scale.z / tileSize));
        }

        if (scale.x <= scale.z)
        {
            return new Vector2(
                Mathf.Max(0.5f, scale.z / tileSize),
                Mathf.Max(0.5f, scale.y / tileSize));
        }

        return new Vector2(
            Mathf.Max(0.5f, scale.x / tileSize),
            Mathf.Max(0.5f, scale.y / tileSize));
    }

    private static Material GetMaterial(SurfaceKind kind)
    {
        if (Materials.TryGetValue(kind, out Material cached) && cached != null)
        {
            return cached;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) return null;

        Material material = new Material(shader)
        {
            name = $"Runtime {kind} Material",
            hideFlags = HideFlags.HideAndDontSave
        };

        Texture2D texture = GetTexture(kind);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        Texture2D normalTexture = GetNormalTexture(kind);
        if (normalTexture != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normalTexture);
            material.SetFloat("_BumpScale", kind == SurfaceKind.Terrain ? 0.52f : 0.78f);
            material.EnableKeyword("_NORMALMAP");
        }
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", kind == SurfaceKind.Metal || kind == SurfaceKind.Emissive ? 0.62f : 0.08f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            float smoothness = kind == SurfaceKind.FloorTile
                ? 0.42f
                : kind == SurfaceKind.Metal || kind == SurfaceKind.Emissive ? 0.57f : 0.22f;
            material.SetFloat("_Smoothness", smoothness);
        }

        if (kind == SurfaceKind.Emissive)
        {
            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.white);
        }

        Materials[kind] = material;
        return material;
    }

    private static Texture2D GetTexture(SurfaceKind kind)
    {
        if (Textures.TryGetValue(kind, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            name = $"Procedural {kind} Texture",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 4,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color[] pixels = new Color[size * size];
        int seed = 37 + (int)kind * 61;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise((x + seed) * 0.075f, (y + seed * 0.4f) * 0.075f);
                float detail = Mathf.PerlinNoise((x + seed * 2f) * 0.22f, (y + seed) * 0.22f);
                float value = 0.72f + (noise - 0.5f) * 0.22f + (detail - 0.5f) * 0.08f;
                value *= GetPatternMultiplier(kind, x, y);
                pixels[y * size + x] = new Color(value, value, value, 1f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(true, false);
        Textures[kind] = texture;
        return texture;
    }

    private static Texture2D GetNormalTexture(SurfaceKind kind)
    {
        if (kind == SurfaceKind.Emissive) return null;
        if (NormalTextures.TryGetValue(kind, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        Texture2D heightTexture = GetTexture(kind);
        if (heightTexture == null) return null;

        int width = heightTexture.width;
        int height = heightTexture.height;
        Color[] heights = heightTexture.GetPixels();
        Color[] normals = new Color[heights.Length];
        float strength = kind == SurfaceKind.Concrete || kind == SurfaceKind.Terrain ? 2.2f : 3.4f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float left = heights[y * width + ((x - 1 + width) % width)].grayscale;
                float right = heights[y * width + ((x + 1) % width)].grayscale;
                float down = heights[((y - 1 + height) % height) * width + x].grayscale;
                float up = heights[((y + 1) % height) * width + x].grayscale;
                Vector3 normal = new Vector3((left - right) * strength, (down - up) * strength, 1f).normalized;
                float encodedX = normal.x * 0.5f + 0.5f;
                normals[y * width + x] = new Color(
                    encodedX,
                    normal.y * 0.5f + 0.5f,
                    normal.z * 0.5f + 0.5f,
                    encodedX);
            }
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, true)
        {
            name = $"Procedural {kind} Normal",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 4,
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels(normals);
        texture.Apply(true, false);
        NormalTextures[kind] = texture;
        return texture;
    }

    private static float GetPatternMultiplier(SurfaceKind kind, int x, int y)
    {
        switch (kind)
        {
            case SurfaceKind.WallPanel:
                if (x % 64 < 2 || y % 32 < 2) return 0.48f;
                if ((x % 64 == 5 || x % 64 == 58) && (y % 32 == 5 || y % 32 == 27)) return 1.3f;
                return 1f;
            case SurfaceKind.FloorTile:
                if (x % 32 < 2 || y % 32 < 2) return 0.42f;
                if ((x % 32 < 5 || x % 32 > 27) && (y % 32 < 5 || y % 32 > 27)) return 0.8f;
                return 1f;
            case SurfaceKind.Metal:
                if (x % 64 < 2 || y % 64 < 2) return 0.52f;
                if ((x + y) % 31 == 0) return 1.22f;
                return 1f;
            case SurfaceKind.Ceiling:
                return x % 64 < 2 || y % 64 < 2 ? 0.5f : 0.93f;
            case SurfaceKind.Asphalt:
                return (x * 17 + y * 31) % 43 == 0 ? 1.35f : 0.86f;
            case SurfaceKind.Terrain:
                return 0.78f + Mathf.PerlinNoise(x * 0.035f, y * 0.035f) * 0.35f;
            case SurfaceKind.Emissive:
                return x % 16 < 2 || y % 16 < 2 ? 0.72f : 1.18f;
            default:
                return 0.9f + Mathf.PerlinNoise(x * 0.025f, y * 0.025f) * 0.18f;
        }
    }

    private static void ConfigureSkybox()
    {
        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader == null)
        {
            RenderSettings.skybox = null;
            return;
        }

        Material skybox = new Material(skyShader)
        {
            name = "Secret Agent Runtime Skybox",
            hideFlags = HideFlags.HideAndDontSave
        };
        skybox.SetColor("_SkyTint", currentTheme.sky);
        skybox.SetColor("_GroundColor", currentTheme.ground);
        skybox.SetFloat("_AtmosphereThickness", currentTheme.night ? 0.32f : 0.82f);
        skybox.SetFloat("_Exposure", currentTheme.night ? 0.48f : 0.82f);
        skybox.SetFloat("_SunSize", currentTheme.night ? 0.012f : 0.045f);
        skybox.SetFloat("_SunSizeConvergence", 5f);
        RenderSettings.skybox = skybox;
        DynamicGI.UpdateEnvironment();
    }

    private static void ConfigureFogAndAmbient()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = currentTheme.fog;
        RenderSettings.fogDensity = currentTheme.fogDensity;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = currentTheme.ambient;
        RenderSettings.ambientEquatorColor = Color.Lerp(currentTheme.ambient, currentTheme.horizon, 0.5f);
        RenderSettings.ambientGroundColor = currentTheme.ground;
        RenderSettings.reflectionIntensity = currentTheme.night ? 0.38f : 0.72f;
    }

    private static void ConfigureSun(Transform runtimeRoot)
    {
        Light sunlight = null;
        foreach (Light candidate in UnityEngine.Object.FindObjectsByType<Light>())
        {
            if (candidate.type != LightType.Directional) continue;
            sunlight = candidate;
            break;
        }

        if (sunlight == null)
        {
            GameObject sunObject = new GameObject("Environment Directional Light");
            sunObject.transform.SetParent(runtimeRoot, false);
            sunlight = sunObject.AddComponent<Light>();
            sunlight.type = LightType.Directional;
        }

        sunlight.color = currentTheme.sunlight;
        sunlight.intensity = currentTheme.sunIntensity;
        sunlight.shadows = LightShadows.Soft;
        sunlight.shadowStrength = currentTheme.night ? 0.72f : 0.82f;
        sunlight.transform.rotation = Quaternion.Euler(
            currentSunPitch,
            currentSunYaw,
            0f);
        RenderSettings.sun = sunlight;
    }

    private static void ConfigureReflectionProbe(Transform runtimeRoot, bool hub)
    {
        GameObject probeObject = new GameObject("Environment Reflection Probe");
        probeObject.transform.SetParent(runtimeRoot, false);
        probeObject.transform.position = hub
            ? new Vector3(0f, 18f, -6f)
            : new Vector3(0f, 14f, 4f);

        ReflectionProbe probe = probeObject.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        probe.size = hub
            ? new Vector3(180f, 58f, 180f)
            : new Vector3(220f, 46f, 260f);
        probe.intensity = currentTheme.night ? 0.62f : 0.78f;
        probe.resolution = 128;
        probe.boxProjection = true;
    }

    private static void ConfigurePostProcessing(Transform runtimeRoot)
    {
        GameObject volumeObject = new GameObject("Environment Post Processing");
        volumeObject.transform.SetParent(runtimeRoot, false);
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 20f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.hideFlags = HideFlags.HideAndDontSave;
        volume.sharedProfile = profile;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(currentTheme.night ? 0.36f : 0.18f);
        bloom.threshold.Override(currentTheme.night ? 0.78f : 1.08f);
        bloom.scatter.Override(0.62f);

        ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
        colorAdjustments.postExposure.Override(currentTheme.exposure);
        colorAdjustments.contrast.Override(8f);
        colorAdjustments.saturation.Override(-4f);
        colorAdjustments.colorFilter.Override(Color.Lerp(Color.white, currentTheme.horizon, 0.08f));

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.16f);
        vignette.smoothness.Override(0.54f);

        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>())
        {
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            camera.clearFlags = CameraClearFlags.Skybox;
        }
    }

    private static void CreateMountainRings(
        Transform runtimeRoot,
        MissionEnvironmentType environment,
        bool hub)
    {
        Color nearColor = environment == MissionEnvironmentType.ArcticRelay
            ? new Color(0.48f, 0.58f, 0.62f)
            : environment == MissionEnvironmentType.WoodlandAnnex
            ? new Color(0.075f, 0.15f, 0.09f)
            : Color.Lerp(currentTheme.ground, currentTheme.horizon, 0.2f);
        Color farColor = Color.Lerp(nearColor, currentTheme.fog, 0.52f);
        float radius = hub ? 135f : 155f;
        float height = environment == MissionEnvironmentType.WoodlandAnnex ||
            environment == MissionEnvironmentType.ArcticRelay ? 54f : 42f;

        CreateRidgeMesh(runtimeRoot, "Near Distant Ridge", radius, 48, height, 11f, nearColor, 137);
        CreateRidgeMesh(runtimeRoot, "Far Distant Ridge", radius + 55f, 56, height * 1.18f, 14f, farColor, 281);
    }

    private static void CreateRidgeMesh(
        Transform parent,
        string objectName,
        float radius,
        int segments,
        float averageHeight,
        float variation,
        Color color,
        int seed)
    {
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];
        for (int segment = 0; segment <= segments; segment++)
        {
            float t = segment / (float)segments;
            float angle = t * Mathf.PI * 2f;
            float heightNoise = Mathf.PerlinNoise(seed * 0.017f + t * 5.4f, seed * 0.011f);
            float peak = averageHeight + (heightNoise - 0.5f) * variation * 2f;
            Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[segment * 2] = radial * radius + Vector3.down * 8f;
            vertices[segment * 2 + 1] = radial * (radius - 16f) + Vector3.up * peak;
            uvs[segment * 2] = new Vector2(t * 8f, 0f);
            uvs[segment * 2 + 1] = new Vector2(t * 8f, 1f);

            if (segment == segments) continue;
            int triangle = segment * 6;
            int bottom = segment * 2;
            triangles[triangle] = bottom;
            triangles[triangle + 1] = bottom + 3;
            triangles[triangle + 2] = bottom + 1;
            triangles[triangle + 3] = bottom;
            triangles[triangle + 4] = bottom + 2;
            triangles[triangle + 5] = bottom + 3;
        }

        Mesh mesh = new Mesh { name = $"{objectName} Mesh" };
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject ridge = new GameObject(objectName);
        ridge.transform.SetParent(parent, false);
        ridge.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer ridgeRenderer = ridge.AddComponent<MeshRenderer>();
        ridgeRenderer.sharedMaterial = CreateBackdropMaterial($"{objectName} Material", color, false);
        ridgeRenderer.shadowCastingMode = ShadowCastingMode.Off;
        ridgeRenderer.receiveShadows = false;
    }

    private static void CreateDistantSkyline(Transform parent, float centerZ, bool hub)
    {
        int towerCount = hub ? 17 : 12;
        for (int index = 0; index < towerCount; index++)
        {
            float t = towerCount <= 1 ? 0f : index / (float)(towerCount - 1);
            float x = Mathf.Lerp(-76f, 76f, t);
            float width = 5f + Mathf.PerlinNoise(index * 0.43f, 1.7f) * 8f;
            float depth = 6f + Mathf.PerlinNoise(index * 0.31f, 3.2f) * 9f;
            float height = 18f + Mathf.PerlinNoise(index * 0.27f, 7.4f) * 43f;
            Mesh towerMesh = CreateTaperedTowerMesh(width, depth, height, 0.72f);
            GameObject tower = new GameObject($"Distant Tower {index + 1}");
            tower.transform.SetParent(parent, false);
            tower.transform.position = new Vector3(x, -3f, centerZ + Mathf.Sin(index * 1.7f) * 14f);
            tower.AddComponent<MeshFilter>().sharedMesh = towerMesh;
            MeshRenderer towerRenderer = tower.AddComponent<MeshRenderer>();
            towerRenderer.sharedMaterial = CreateBackdropMaterial(
                $"Distant Tower Material {index + 1}",
                new Color(0.025f, 0.04f, 0.06f),
                true);
            towerRenderer.shadowCastingMode = ShadowCastingMode.Off;
            towerRenderer.receiveShadows = false;
        }
    }

    private static Mesh CreateTaperedTowerMesh(float width, float depth, float height, float taper)
    {
        float halfWidth = width * 0.5f;
        float halfDepth = depth * 0.5f;
        float topWidth = halfWidth * taper;
        float topDepth = halfDepth * taper;
        Vector3[] vertices =
        {
            new Vector3(-halfWidth, 0f, -halfDepth),
            new Vector3(halfWidth, 0f, -halfDepth),
            new Vector3(halfWidth, 0f, halfDepth),
            new Vector3(-halfWidth, 0f, halfDepth),
            new Vector3(-topWidth, height, -topDepth),
            new Vector3(topWidth, height, -topDepth),
            new Vector3(topWidth, height, topDepth),
            new Vector3(-topWidth, height, topDepth)
        };
        int[] triangles =
        {
            0, 5, 1, 0, 4, 5,
            1, 6, 2, 1, 5, 6,
            2, 7, 3, 2, 6, 7,
            3, 4, 0, 3, 7, 4,
            4, 6, 5, 4, 7, 6,
            0, 2, 3, 0, 1, 2
        };
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2((i % 4) * 0.33f, i < 4 ? 0f : height / 8f);
        }

        Mesh mesh = new Mesh { name = "Distant Tapered Tower Mesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void CreateIndustrialBackdrop(Transform parent)
    {
        for (int index = 0; index < 7; index++)
        {
            GameObject tower = new GameObject($"Distant Industrial Stack {index + 1}");
            tower.transform.SetParent(parent, false);
            tower.transform.position = new Vector3(-72f + index * 24f, -2f, 122f + Mathf.Abs(index - 3) * 4f);

            GameObject stack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stack.name = "Stack";
            stack.transform.SetParent(tower.transform, false);
            stack.transform.localPosition = Vector3.up * (18f + index % 3 * 4f);
            stack.transform.localScale = new Vector3(2.2f, 18f + index % 3 * 4f, 2.2f);
            Collider collider = stack.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.Destroy(collider);
            MeshRenderer stackRenderer = stack.GetComponent<MeshRenderer>();
            stackRenderer.sharedMaterial = CreateBackdropMaterial(
                $"Industrial Stack Material {index + 1}",
                new Color(0.06f, 0.085f, 0.09f),
                false);
            stackRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    private static void CreateEnvironmentSignatureBackdrop(
        Transform parent,
        MissionEnvironmentType environment,
        bool hub)
    {
        if (parent.Find("Environment Signature Backdrop") != null)
        {
            return;
        }

        GameObject backdropRoot = new GameObject("Environment Signature Backdrop");
        backdropRoot.transform.SetParent(parent, false);
        float backdropZ = hub ? -146f : 148f;

        if (hub)
        {
            CreateBackdropCampus(backdropRoot.transform, backdropZ, true);
            return;
        }

        switch (environment)
        {
            case MissionEnvironmentType.WeaponsLaboratory:
                CreateBackdropLaboratory(backdropRoot.transform, backdropZ);
                break;
            case MissionEnvironmentType.Dockyard:
                CreateBackdropDockyard(backdropRoot.transform, backdropZ);
                break;
            case MissionEnvironmentType.WoodlandAnnex:
                CreateBackdropWoodland(backdropRoot.transform, backdropZ);
                break;
            case MissionEnvironmentType.ArcticRelay:
                CreateBackdropRelay(backdropRoot.transform, backdropZ, true);
                break;
            case MissionEnvironmentType.ExecutiveStronghold:
                CreateBackdropExecutiveCompound(backdropRoot.transform, backdropZ, true);
                break;
            case MissionEnvironmentType.Embassy:
                CreateBackdropExecutiveCompound(backdropRoot.transform, backdropZ, false);
                break;
            case MissionEnvironmentType.DataCenter:
                CreateBackdropDataCenter(backdropRoot.transform, backdropZ);
                break;
            default:
                CreateBackdropCampus(backdropRoot.transform, backdropZ, false);
                break;
        }
    }

    private static void CreateBackdropCampus(Transform parent, float z, bool hub)
    {
        float facing = hub ? 0f : 180f;
        for (int index = -1; index <= 1; index++)
        {
            float scale = index == 0 ? 4.2f : 3.4f;
            CreateBackdropModel(
                parent,
                "GuardBooth_A",
                $"Distant Operations Pavilion {index + 2}",
                new Vector3(index * 42f, -3f, z + Mathf.Abs(index) * 8f),
                Quaternion.Euler(0f, facing, 0f),
                Vector3.one * scale);
        }

        CreateBackdropModel(
            parent,
            "EmbassyPortal_A",
            "Distant Facility Gate",
            new Vector3(0f, -2.6f, z - 7f),
            Quaternion.Euler(0f, facing, 0f),
            Vector3.one * 4.5f);
        CreateBackdropModel(
            parent,
            "RelayAntenna_A",
            "Distant West Communications Mast",
            new Vector3(-30f, -2f, z - 2f),
            Quaternion.identity,
            Vector3.one * 3.1f);
        CreateBackdropModel(
            parent,
            "RelayAntenna_A",
            "Distant East Communications Mast",
            new Vector3(30f, -2f, z - 2f),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 3.1f);
    }

    private static void CreateBackdropLaboratory(Transform parent, float z)
    {
        for (int index = -1; index <= 1; index++)
        {
            CreateBackdropModel(
                parent,
                "ArcticBunker_A",
                $"Distant Laboratory Wing {index + 2}",
                new Vector3(index * 45f, -3f, z + Mathf.Abs(index) * 8f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(3.7f, 3.2f, 3.7f));
            CreateBackdropModel(
                parent,
                "CryoCore_A",
                $"Distant Reactor Stack {index + 2}",
                new Vector3(index * 45f + 14f, -2f, z - 2f),
                Quaternion.identity,
                Vector3.one * 4.2f);
        }

        CreateBackdropModel(
            parent,
            "RelayAntenna_A",
            "Distant Laboratory Beacon",
            new Vector3(0f, -2f, z - 13f),
            Quaternion.identity,
            Vector3.one * 4.2f);
    }

    private static void CreateBackdropDockyard(Transform parent, float z)
    {
        for (int index = -1; index <= 1; index++)
        {
            CreateBackdropModel(
                parent,
                "DockCrane_A",
                $"Distant Cargo Crane {index + 2}",
                new Vector3(index * 57f, -3f, z + Mathf.Abs(index) * 10f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * (index == 0 ? 3.2f : 2.7f));
        }

        for (int index = 0; index < 9; index++)
        {
            float x = -70f + index * 17.5f;
            float tier = index % 3;
            CreateBackdropModel(
                parent,
                "CargoContainer_A",
                $"Distant Container Stack {index + 1}",
                new Vector3(x, -2.5f + tier * 3.6f, z + 14f + tier * 4f),
                Quaternion.Euler(0f, index % 2 == 0 ? 0f : 180f, 0f),
                Vector3.one * 2.6f);
        }
    }

    private static void CreateBackdropWoodland(Transform parent, float z)
    {
        for (int index = 0; index < 29; index++)
        {
            float t = index / 28f;
            float x = Mathf.Lerp(-112f, 112f, t);
            float depthOffset = Mathf.Sin(index * 1.83f) * 17f;
            float scale = 2.45f + Mathf.PerlinNoise(index * 0.41f, 8.2f) * 1.65f;
            CreateBackdropModel(
                parent,
                "PineTree_A",
                $"Distant Pine {index + 1}",
                new Vector3(x, -4f, z + depthOffset),
                Quaternion.Euler(0f, index * 47f, 0f),
                Vector3.one * scale);
        }

        for (int index = 0; index < 8; index++)
        {
            CreateBackdropModel(
                parent,
                "RockFormation_A",
                $"Distant Rock Outcrop {index + 1}",
                new Vector3(-90f + index * 26f, -4f, z - 10f + Mathf.Sin(index) * 8f),
                Quaternion.Euler(0f, index * 31f, 0f),
                Vector3.one * (3.2f + index % 3 * 0.65f));
        }
    }

    private static void CreateBackdropRelay(Transform parent, float z, bool arctic)
    {
        for (int index = -1; index <= 1; index++)
        {
            CreateBackdropModel(
                parent,
                "ArcticBunker_A",
                $"Distant {(arctic ? "Arctic" : "Relay")} Bunker {index + 2}",
                new Vector3(index * 48f, -4f, z + Mathf.Abs(index) * 9f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(3.4f, 3f, 3.4f));
        }

        for (int index = -2; index <= 2; index++)
        {
            CreateBackdropModel(
                parent,
                "RelayAntenna_A",
                $"Distant Relay Mast {index + 3}",
                new Vector3(index * 27f, -3f, z - 10f + Mathf.Abs(index) * 3f),
                Quaternion.Euler(0f, index * 28f, 0f),
                Vector3.one * (index == 0 ? 4.4f : 3.4f));
        }
    }

    private static void CreateBackdropExecutiveCompound(Transform parent, float z, bool stronghold)
    {
        for (int index = -2; index <= 2; index++)
        {
            float scale = index == 0 ? 5.2f : 3.8f;
            CreateBackdropModel(
                parent,
                "EmbassyPortal_A",
                $"Distant {(stronghold ? "Stronghold" : "Embassy")} Arcade {index + 3}",
                new Vector3(index * 32f, -3f, z + Mathf.Abs(index) * 5f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * scale);
        }

        CreateBackdropModel(
            parent,
            "GuardBooth_A",
            "Distant Executive Security Pavilion",
            new Vector3(0f, -2.5f, z + 16f),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 4.7f);
        if (!stronghold)
        {
            CreateBackdropModel(
                parent,
                "EmbassyFountain_A",
                "Distant Embassy Fountain",
                new Vector3(0f, -2.5f, z - 8f),
                Quaternion.identity,
                Vector3.one * 4f);
        }
    }

    private static void CreateBackdropDataCenter(Transform parent, float z)
    {
        CreateBackdropRelay(parent, z, false);
        for (int index = -3; index <= 3; index++)
        {
            CreateBackdropModel(
                parent,
                "VentUnit_A",
                $"Distant Cooling Tower {index + 4}",
                new Vector3(index * 22f, -2.5f, z + 17f + Mathf.Abs(index) * 2f),
                Quaternion.Euler(0f, index * 35f, 0f),
                Vector3.one * (5.5f + index % 2 * 0.8f));
        }
    }

    private static GameObject CreateBackdropModel(
        Transform parent,
        string resourceName,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        GameObject prefab = Resources.Load<GameObject>($"Models/Environment/{resourceName}");
        if (prefab == null)
        {
            Debug.LogWarning($"Could not load backdrop model Models/Environment/{resourceName}.");
            return null;
        }

        GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
        instance.name = objectName;
        instance.transform.localScale = scale;
        StyleKitInstance(instance, currentTheme.accent);

        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (Renderer targetRenderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            targetRenderer.shadowCastingMode = ShadowCastingMode.Off;
            targetRenderer.receiveShadows = false;
            targetRenderer.lightProbeUsage = LightProbeUsage.Off;
            targetRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        return instance;
    }

    private static void CreateAmbientAtmospherics(
        Transform parent,
        MissionEnvironmentType environment,
        bool hub)
    {
        if (hub || parent.Find("Mission Ambient Atmospherics") != null)
        {
            return;
        }

        GameObject atmosphereRoot = new GameObject("Mission Ambient Atmospherics");
        atmosphereRoot.transform.SetParent(parent, false);
        Transform atmosphereParent = atmosphereRoot.transform;

        switch (environment)
        {
            case MissionEnvironmentType.Dockyard:
                CreateAtmosphereField(
                    atmosphereParent,
                    "Harbor Rain",
                    new Vector3(0f, 27f, -6f),
                    new Vector3(104f, 3f, 220f),
                    310f,
                    new Vector3(0.35f, -10.5f, 0.2f),
                    new Color(0.52f, 0.72f, 0.82f, 0.42f),
                    new Color(0.72f, 0.82f, 0.88f, 0.26f),
                    2.8f,
                    0.035f,
                    0.07f,
                    true);
                break;
            case MissionEnvironmentType.WoodlandAnnex:
                CreateAtmosphereField(
                    atmosphereParent,
                    "Woodland Airborne Leaves",
                    new Vector3(0f, 13f, -8f),
                    new Vector3(80f, 12f, 145f),
                    22f,
                    new Vector3(0.75f, -0.7f, 0.3f),
                    new Color(0.28f, 0.44f, 0.12f, 0.68f),
                    new Color(0.82f, 0.58f, 0.16f, 0.58f),
                    8f,
                    0.09f,
                    0.2f,
                    false);
                break;
            case MissionEnvironmentType.ArcticRelay:
                CreateAtmosphereField(
                    atmosphereParent,
                    "Polar Snow",
                    new Vector3(0f, 21f, -8f),
                    new Vector3(90f, 4f, 155f),
                    175f,
                    new Vector3(1.1f, -2.4f, 0.35f),
                    new Color(0.92f, 0.97f, 1f, 0.82f),
                    new Color(0.68f, 0.84f, 0.94f, 0.58f),
                    6.5f,
                    0.055f,
                    0.17f,
                    false);
                break;
            case MissionEnvironmentType.Embassy:
                CreateAtmosphereField(
                    atmosphereParent,
                    "Embassy Courtyard Petals",
                    new Vector3(0f, 10f, 15f),
                    new Vector3(48f, 5f, 62f),
                    13f,
                    new Vector3(0.42f, -0.55f, -0.18f),
                    new Color(0.82f, 0.12f, 0.24f, 0.65f),
                    new Color(0.95f, 0.48f, 0.54f, 0.52f),
                    9f,
                    0.07f,
                    0.16f,
                    false);
                break;
            case MissionEnvironmentType.ExecutiveStronghold:
                CreateAtmosphereField(
                    atmosphereParent,
                    "Citadel Ash",
                    new Vector3(0f, 14f, 12f),
                    new Vector3(54f, 5f, 86f),
                    27f,
                    new Vector3(-0.28f, -0.9f, 0.16f),
                    new Color(0.34f, 0.08f, 0.09f, 0.62f),
                    new Color(0.52f, 0.3f, 0.28f, 0.46f),
                    7.5f,
                    0.045f,
                    0.13f,
                    false);
                break;
            default:
                UnityEngine.Object.Destroy(atmosphereRoot);
                break;
        }
    }

    private static void CreateAtmosphereField(
        Transform parent,
        string fieldName,
        Vector3 position,
        Vector3 volume,
        float emissionRate,
        Vector3 velocity,
        Color colorA,
        Color colorB,
        float lifetime,
        float minimumSize,
        float maximumSize,
        bool stretched)
    {
        GameObject field = new GameObject(fieldName);
        field.transform.SetParent(parent, false);
        field.transform.position = position;

        ParticleSystem particles = field.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(minimumSize, maximumSize);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.maxParticles = Mathf.CeilToInt(emissionRate * lifetime * 1.35f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = emissionRate;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = volume;

        ParticleSystem.VelocityOverLifetimeModule velocityModule = particles.velocityOverLifetime;
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.World;
        velocityModule.x = velocity.x;
        velocityModule.y = velocity.y;
        velocityModule.z = velocity.z;

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = stretched ? 0.08f : 0.28f;
        noise.frequency = stretched ? 0.16f : 0.32f;
        noise.scrollSpeed = 0.22f;
        noise.damping = true;

        ParticleSystem.CollisionModule collision = particles.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.quality = ParticleSystemCollisionQuality.Medium;
        collision.dampen = 0.82f;
        collision.lifetimeLoss = 0.9f;
        collision.radiusScale = 0.1f;

        ParticleSystemRenderer particleRenderer = field.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sharedMaterial = CreateAtmosphereMaterial($"{fieldName} Material");
        particleRenderer.renderMode = stretched
            ? ParticleSystemRenderMode.Stretch
            : ParticleSystemRenderMode.Billboard;
        particleRenderer.velocityScale = stretched ? 0.12f : 0f;
        particleRenderer.lengthScale = stretched ? 0.9f : 0f;
        particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;
        particleRenderer.lightProbeUsage = LightProbeUsage.Off;
        particleRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        particles.Play();
    }

    private static Material CreateAtmosphereMaterial(string materialName)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        Material material = new Material(shader)
        {
            name = materialName,
            hideFlags = HideFlags.HideAndDontSave,
            renderQueue = (int)RenderQueue.Transparent
        };

        Texture2D particleTexture = GetAtmosphereParticleTexture();
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", particleTexture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", particleTexture);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
        if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        return material;
    }

    private static Texture2D GetAtmosphereParticleTexture()
    {
        if (atmosphereParticleTexture != null)
        {
            return atmosphereParticleTexture;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Atmosphere Soft Particle",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.48f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 1.8f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        atmosphereParticleTexture = texture;
        return atmosphereParticleTexture;
    }

    private static Material CreateBackdropMaterial(string materialName, Color color, bool emissiveWindows)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader)
        {
            name = materialName,
            hideFlags = HideFlags.HideAndDontSave
        };
        Texture2D texture = emissiveWindows
            ? GetSkylineWindowTexture()
            : GetTexture(SurfaceKind.Terrain);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
        return material;
    }

    private static Texture2D GetSkylineWindowTexture()
    {
        if (skylineWindowTexture != null)
        {
            return skylineWindowTexture;
        }

        const int width = 64;
        const int height = 128;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, true)
        {
            name = "Distant Skyline Windows",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };
        Color dark = new Color(0.018f, 0.025f, 0.04f);
        Color lit = currentTheme.night
            ? new Color(0.8f, 0.52f, 0.24f)
            : new Color(0.16f, 0.34f, 0.4f);
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool insideWindow = x % 12 > 3 && x % 12 < 9 && y % 14 > 3 && y % 14 < 10;
                bool powered = ((x / 12) * 7 + (y / 14) * 11) % 5 != 0;
                pixels[y * width + x] = insideWindow && powered ? lit : dark;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(true, false);
        skylineWindowTexture = texture;
        return skylineWindowTexture;
    }

    private static void ResetMaterials()
    {
        foreach (Material material in Materials.Values)
        {
            if (material != null) UnityEngine.Object.Destroy(material);
        }

        Materials.Clear();
        if (skylineWindowTexture != null)
        {
            UnityEngine.Object.Destroy(skylineWindowTexture);
            skylineWindowTexture = null;
        }
    }
}
