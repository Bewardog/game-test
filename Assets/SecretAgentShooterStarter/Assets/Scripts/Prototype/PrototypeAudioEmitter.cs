using UnityEngine;

[DisallowMultipleComponent]
public class PrototypeAudioEmitter : MonoBehaviour
{
    private const int SampleRate = 22050;

    private static AudioClip pistolShot;
    private static AudioClip smgShot;
    private static AudioClip rifleShot;
    private static AudioClip heavyShot;
    private static AudioClip dryFire;
    private static AudioClip reloadStart;
    private static AudioClip reloadComplete;
    private static AudioClip meleeHit;
    private static AudioClip abilityPulse;
    private static AudioClip doorOpen;
    private static AudioClip doorClose;
    private static AudioClip securityAlarm;
    private static AudioClip securityDisabled;
    private static AudioClip accessDenied;
    private static AudioClip softFootstep;
    private static AudioClip hardFootstep;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.18f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 28f;
    }

    public void PlayWeaponFire(WeaponClass weaponClass)
    {
        EnsureClips();
        switch (weaponClass)
        {
            case WeaponClass.SubmachineGun:
                Play(smgShot, 0.72f, 1.05f);
                break;
            case WeaponClass.Rifle:
                Play(rifleShot, 0.82f, 0.92f);
                break;
            case WeaponClass.Shotgun:
            case WeaponClass.Heavy:
                Play(heavyShot, 0.9f, 0.84f);
                break;
            default:
                Play(pistolShot, 0.68f, 1f);
                break;
        }
    }

    public void SetSpatialBlend(float spatialBlend)
    {
        if (audioSource == null) Awake();
        audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
    }

    public void PlayDryFire()
    {
        EnsureClips();
        Play(dryFire, 0.52f, 1f);
    }

    public void PlayReloadStart()
    {
        EnsureClips();
        Play(reloadStart, 0.52f, 1f);
    }

    public void PlayReloadComplete()
    {
        EnsureClips();
        Play(reloadComplete, 0.58f, 1.04f);
    }

    public void PlayEnemyAttack(bool melee)
    {
        EnsureClips();
        Play(melee ? meleeHit : pistolShot, melee ? 0.72f : 0.45f, melee ? 0.9f : 0.82f);
    }

    public void PlayAbility()
    {
        EnsureClips();
        Play(abilityPulse, 0.6f, 1f);
    }

    public void PlayDoor(bool opening)
    {
        EnsureClips();
        Play(opening ? doorOpen : doorClose, 0.46f, opening ? 1.05f : 0.92f);
    }

    public void PlaySecurityAlarm()
    {
        EnsureClips();
        Play(securityAlarm, 0.78f, 1f);
    }

    public void PlaySecurityDisabled()
    {
        EnsureClips();
        Play(securityDisabled, 0.54f, 0.96f);
    }

    public void PlayAccessDenied()
    {
        EnsureClips();
        Play(accessDenied, 0.48f, 0.82f);
    }

    public void PlayFootstep(bool crouching, bool forceful)
    {
        EnsureClips();
        AudioClip clip = crouching ? softFootstep : hardFootstep;
        float volume = crouching ? 0.1f : forceful ? 0.28f : 0.2f;
        float pitch = crouching ? 1.14f : forceful ? 0.88f : 1f;
        Play(clip, volume, pitch);
    }

    private void Play(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;
        if (audioSource == null) Awake();
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);
    }

    private static void EnsureClips()
    {
        if (pistolShot != null) return;
        pistolShot = CreateTransient("Suppressed Pistol", 880f, 210f, 0.11f, 0.2f, 3.2f);
        smgShot = CreateTransient("Sedative SMG", 760f, 180f, 0.085f, 0.28f, 3.8f);
        rifleShot = CreateTransient("Precision Rifle", 620f, 120f, 0.17f, 0.34f, 2.7f);
        heavyShot = CreateTransient("Heavy Weapon", 380f, 72f, 0.24f, 0.48f, 2.1f);
        dryFire = CreateTransient("Dry Fire", 2100f, 1450f, 0.035f, 0.08f, 8f);
        reloadStart = CreateTransient("Reload Start", 560f, 310f, 0.07f, 0.12f, 5f);
        reloadComplete = CreateTransient("Reload Complete", 420f, 920f, 0.09f, 0.08f, 4.5f);
        meleeHit = CreateTransient("Melee Impact", 180f, 65f, 0.13f, 0.58f, 3.1f);
        abilityPulse = CreateTransient("Ability Pulse", 260f, 980f, 0.24f, 0.12f, 1.7f);
        doorOpen = CreateTransient("Security Door Open", 170f, 520f, 0.34f, 0.08f, 1.25f);
        doorClose = CreateTransient("Security Door Close", 480f, 130f, 0.3f, 0.12f, 1.45f);
        securityAlarm = CreateTransient("Security Alarm", 760f, 1280f, 0.42f, 0.05f, 0.55f);
        securityDisabled = CreateTransient("Security Disabled", 920f, 170f, 0.28f, 0.09f, 1.5f);
        accessDenied = CreateTransient("Access Denied", 290f, 155f, 0.24f, 0.04f, 1.1f);
        softFootstep = CreateTransient("Soft Footstep", 115f, 54f, 0.09f, 0.64f, 3.8f);
        hardFootstep = CreateTransient("Hard Footstep", 150f, 62f, 0.12f, 0.58f, 3.25f);
    }

    private static AudioClip CreateTransient(
        string clipName,
        float startFrequency,
        float endFrequency,
        float duration,
        float noiseAmount,
        float decayPower)
    {
        int sampleCount = Mathf.Max(128, Mathf.CeilToInt(duration * SampleRate));
        float[] samples = new float[sampleCount];
        float phase = 0f;
        uint noiseState = (uint)(clipName.GetHashCode() * 747796405);
        for (int i = 0; i < sampleCount; i++)
        {
            float normalized = i / (float)(sampleCount - 1);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, normalized);
            phase += Mathf.PI * 2f * frequency / SampleRate;
            noiseState = noiseState * 1664525u + 1013904223u;
            float noise = ((noiseState >> 8) / 16777215f) * 2f - 1f;
            float envelope = Mathf.Pow(1f - normalized, decayPower);
            samples[i] = (Mathf.Sin(phase) * (1f - noiseAmount) + noise * noiseAmount) * envelope * 0.72f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
