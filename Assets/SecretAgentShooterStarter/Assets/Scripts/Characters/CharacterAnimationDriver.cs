using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public enum CharacterActionAnimation
{
    None,
    Jump,
    Shoot,
    Punch,
    Stab,
    Hit
}

[DisallowMultipleComponent]
public class CharacterAnimationDriver : MonoBehaviour
{
    public Animator animator;
    public Transform motionSource;
    public string clipResourcePath = "Models/RiggedGrunts/HandToHandGrunt01_Animated";
    public float walkSpeedReference = 3.5f;
    public float runSpeedReference = 6f;
    public float crossFadeDuration = 0.12f;
    public float shootPlaybackSpeed = 1.15f;
    public float meleePlaybackSpeed = 1.05f;
    public float hitPlaybackSpeed = 1.12f;

    private readonly Dictionary<string, AnimationClip> clips =
        new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;
    private AnimationClipPlayable locomotionPlayable;
    private AnimationClipPlayable previousLocomotionPlayable;
    private AnimationClipPlayable actionPlayable;
    private string activeLocomotion;
    private float locomotionDuration;
    private bool locomotionLoops;
    private float locomotionTransitionStartedAt;
    private float locomotionBlend = 1f;
    private float actionStartedAt;
    private float actionDuration;
    private float actionWeight;
    private bool graphReady;
    private bool suspended;
    private Vector3 previousPosition;
    private CharacterController characterController;
    private SpyPlayerController playerController;

    public bool HasAnimationLibrary => clips.Count > 0;
    public bool IsSuspended => suspended;

    private void Awake()
    {
        if (motionSource == null) motionSource = transform;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<SpyPlayerController>();
        previousPosition = motionSource.position;
        TryInitializeGraph();
    }

    private void OnEnable()
    {
        if (graphReady && !suspended && !graph.IsPlaying())
        {
            graph.Play();
        }
    }

    private void Update()
    {
        if (suspended || motionSource == null) return;
        if (!graphReady) TryInitializeGraph();

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 displacement = motionSource.position - previousPosition;
        previousPosition = motionSource.position;
        float horizontalSpeed = new Vector2(displacement.x, displacement.z).magnitude / deltaTime;
        bool grounded = characterController == null || characterController.isGrounded;
        UpdateLocomotion(horizontalSpeed, grounded);
        UpdateLocomotionBlend();
        UpdateActionBlend();
        ApplyMixerWeights();
    }

    private void OnDestroy()
    {
        if (graphReady && graph.IsValid())
        {
            graph.Destroy();
        }
    }

    public void Initialize(Animator targetAnimator, string resourcePath, Transform source = null)
    {
        if (targetAnimator != null) animator = targetAnimator;
        if (!string.IsNullOrWhiteSpace(resourcePath)) clipResourcePath = resourcePath;
        if (source != null) motionSource = source;
        if (motionSource == null) motionSource = transform;
        previousPosition = motionSource.position;
        TryInitializeGraph();
    }

    public void SetSuspended(bool value)
    {
        suspended = value;
        if (animator != null) animator.enabled = !value;
        if (!graphReady || !graph.IsValid()) return;

        if (value)
        {
            graph.Stop();
        }
        else
        {
            previousPosition = motionSource != null ? motionSource.position : transform.position;
            graph.Play();
        }
    }

    public void PlayAction(CharacterActionAnimation action)
    {
        if (action == CharacterActionAnimation.None || suspended) return;
        string clipName = $"Agent_{action}";
        if (!TryGetClip(clipName, out AnimationClip clip))
        {
            SetAnimatorTriggerIfPresent(action.ToString());
            return;
        }

        if (!graphReady) TryInitializeGraph();
        if (!graphReady) return;

        DisconnectPlayable(1, ref actionPlayable);
        actionPlayable = AnimationClipPlayable.Create(graph, clip);
        actionPlayable.SetApplyFootIK(true);
        actionPlayable.SetTime(0d);
        float playbackSpeed = GetActionPlaybackSpeed(action);
        actionPlayable.SetSpeed(playbackSpeed);
        graph.Connect(actionPlayable, 0, mixer, 1);
        actionWeight = Mathf.Clamp(actionWeight, 0f, 0.72f);
        actionStartedAt = Time.time;
        actionDuration = Mathf.Max(0.08f, clip.length / playbackSpeed);
        ApplyMixerWeights();
    }

    private void TryInitializeGraph()
    {
        if (graphReady || animator == null || string.IsNullOrWhiteSpace(clipResourcePath)) return;

        AnimationClip[] loadedClips = Resources.LoadAll<AnimationClip>(clipResourcePath);
        foreach (AnimationClip clip in loadedClips)
        {
            if (clip == null || string.IsNullOrWhiteSpace(clip.name)) continue;
            clips[clip.name] = clip;
        }

        if (clips.Count == 0) return;

        graph = PlayableGraph.Create($"{name} Animation Graph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, 3);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Character Animation", animator);
        output.SetSourcePlayable(mixer);
        graph.Play();
        graphReady = true;
        SetLocomotionClip("Agent_Idle");
    }

    private void UpdateLocomotion(float horizontalSpeed, bool grounded)
    {
        if (!graphReady) return;

        string desiredClip;
        bool crouching = playerController != null && playerController.IsCrouching;
        if (crouching &&
            horizontalSpeed >= 0.12f &&
            TryGetClip("Agent_CrouchWalk", out _))
        {
            desiredClip = "Agent_CrouchWalk";
        }
        else if (crouching && TryGetClip("Agent_CrouchIdle", out _))
        {
            desiredClip = "Agent_CrouchIdle";
        }
        else if (!grounded && TryGetClip("Agent_Jump", out _))
        {
            desiredClip = "Agent_Jump";
        }
        else if (horizontalSpeed >= Mathf.Lerp(walkSpeedReference, runSpeedReference, 0.55f))
        {
            desiredClip = "Agent_Run";
        }
        else if (horizontalSpeed >= 0.12f)
        {
            desiredClip = "Agent_Walk";
        }
        else
        {
            desiredClip = "Agent_Idle";
        }

        if (!string.Equals(activeLocomotion, desiredClip, StringComparison.OrdinalIgnoreCase))
        {
            SetLocomotionClip(desiredClip);
        }

        if (!locomotionPlayable.IsValid()) return;
        float playbackSpeed = desiredClip == "Agent_Run"
            ? Mathf.Clamp(horizontalSpeed / Mathf.Max(0.1f, runSpeedReference), 0.8f, 1.4f)
            : desiredClip == "Agent_Walk" || desiredClip == "Agent_CrouchWalk"
                ? Mathf.Clamp(horizontalSpeed / Mathf.Max(0.1f, walkSpeedReference), 0.65f, 1.35f)
                : 1f;
        locomotionPlayable.SetSpeed(playbackSpeed);
        if (locomotionLoops &&
            locomotionDuration > 0.01f &&
            locomotionPlayable.GetTime() >= locomotionDuration)
        {
            locomotionPlayable.SetTime(locomotionPlayable.GetTime() % locomotionDuration);
        }
    }

    private void SetLocomotionClip(string clipName)
    {
        if (!TryGetClip(clipName, out AnimationClip clip)) return;

        DisconnectPlayable(2, ref previousLocomotionPlayable);
        if (locomotionPlayable.IsValid())
        {
            if (mixer.IsValid()) graph.Disconnect(mixer, 0);
            previousLocomotionPlayable = locomotionPlayable;
            locomotionPlayable = default;
            graph.Connect(previousLocomotionPlayable, 0, mixer, 2);
            locomotionBlend = 0f;
            locomotionTransitionStartedAt = Time.time;
        }
        else
        {
            locomotionBlend = 1f;
        }

        locomotionPlayable = AnimationClipPlayable.Create(graph, clip);
        locomotionPlayable.SetApplyFootIK(true);
        locomotionPlayable.SetTime(GetMatchedLocomotionTime(clip));
        graph.Connect(locomotionPlayable, 0, mixer, 0);
        activeLocomotion = clipName;
        locomotionDuration = Mathf.Max(0.01f, clip.length);
        locomotionLoops = clipName != "Agent_Jump";
        ApplyMixerWeights();
    }

    private double GetMatchedLocomotionTime(AnimationClip incomingClip)
    {
        if (!previousLocomotionPlayable.IsValid() ||
            incomingClip == null ||
            incomingClip.length <= 0.01f)
        {
            return 0d;
        }

        double previousDuration = previousLocomotionPlayable.GetAnimationClip().length;
        if (previousDuration <= 0.01d)
        {
            return 0d;
        }

        double normalizedTime = previousLocomotionPlayable.GetTime() / previousDuration;
        normalizedTime -= Math.Floor(normalizedTime);
        return normalizedTime * incomingClip.length;
    }

    private void UpdateLocomotionBlend()
    {
        if (!previousLocomotionPlayable.IsValid())
        {
            locomotionBlend = 1f;
            return;
        }

        float duration = Mathf.Max(0.02f, crossFadeDuration);
        locomotionBlend = Mathf.Clamp01((Time.time - locomotionTransitionStartedAt) / duration);
        locomotionBlend = locomotionBlend * locomotionBlend * (3f - 2f * locomotionBlend);
        if (locomotionBlend < 0.999f)
        {
            return;
        }

        DisconnectPlayable(2, ref previousLocomotionPlayable);
        locomotionBlend = 1f;
    }

    private void UpdateActionBlend()
    {
        if (!graphReady || !actionPlayable.IsValid())
        {
            actionWeight = 0f;
            return;
        }

        float elapsed = Time.time - actionStartedAt;
        float fadeDuration = Mathf.Clamp(crossFadeDuration, 0.02f, actionDuration * 0.45f);
        actionWeight = elapsed < fadeDuration
            ? elapsed / fadeDuration
            : elapsed > actionDuration - fadeDuration
                ? Mathf.Clamp01((actionDuration - elapsed) / fadeDuration)
                : 1f;

        if (elapsed < actionDuration) return;

        DisconnectPlayable(1, ref actionPlayable);
        actionWeight = 0f;
    }

    private void ApplyMixerWeights()
    {
        if (!graphReady || !mixer.IsValid()) return;

        float baseWeight = 1f - actionWeight * 0.35f;
        mixer.SetInputWeight(0, locomotionPlayable.IsValid() ? baseWeight * locomotionBlend : 0f);
        mixer.SetInputWeight(1, actionPlayable.IsValid() ? actionWeight : 0f);
        mixer.SetInputWeight(
            2,
            previousLocomotionPlayable.IsValid() ? baseWeight * (1f - locomotionBlend) : 0f);
    }

    private float GetActionPlaybackSpeed(CharacterActionAnimation action)
    {
        switch (action)
        {
            case CharacterActionAnimation.Shoot:
                return Mathf.Max(0.1f, shootPlaybackSpeed);
            case CharacterActionAnimation.Punch:
            case CharacterActionAnimation.Stab:
                return Mathf.Max(0.1f, meleePlaybackSpeed);
            case CharacterActionAnimation.Hit:
                return Mathf.Max(0.1f, hitPlaybackSpeed);
            default:
                return 1f;
        }
    }

    private bool TryGetClip(string requestedName, out AnimationClip clip)
    {
        if (clips.TryGetValue(requestedName, out clip) && clip != null) return true;

        foreach (KeyValuePair<string, AnimationClip> candidate in clips)
        {
            if (candidate.Value != null &&
                candidate.Key.EndsWith(requestedName, StringComparison.OrdinalIgnoreCase))
            {
                clip = candidate.Value;
                return true;
            }
        }

        clip = null;
        return false;
    }

    private void DisconnectPlayable(int inputIndex, ref AnimationClipPlayable playable)
    {
        if (!playable.IsValid()) return;
        if (mixer.IsValid()) graph.Disconnect(mixer, inputIndex);
        playable.Destroy();
        playable = default;
    }

    private void SetAnimatorTriggerIfPresent(string triggerName)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == triggerName)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }
    }
}
