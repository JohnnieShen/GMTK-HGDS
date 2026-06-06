using System.Collections.Generic;
using UnityEngine;

public struct SpectralImprintDisplaySnapshot
{
    public int generation;
    public bool dspActive;
    public bool quietMemory;
    public float outputGain;
    public float muffleAmount;
    public float echoAmount;
    public float crushAmount;
    public float instabilityAmount;
}

public class SpectralImprintSource : MonoBehaviour
{
    static readonly List<SpectralImprintSource> instances = new();

    [Header("Clips")]
    [SerializeField] AudioClip rollingClip;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip landClip;
    [SerializeField] AudioClip spawnClip;
    [SerializeField] AudioClip dieClip;
    [SerializeField] AudioClip interactClip;

    [Header("Playback")]
    [SerializeField] bool playbackEnabled = true;
    [SerializeField] float rollingVolume = 0.6f;
    [SerializeField] float oneShotVolume = 1f;
    [SerializeField, Range(0.05f, 2f)] float rollingFadeDuration = 0.75f;
    [SerializeField, Range(0f, 1f)] float spatialBlend = 0f;
    [SerializeField] float minDistance = 1.5f;
    [SerializeField] float maxDistance = 18f;

    [Header("Playback Variation")]
    [SerializeField] bool randomizePlayback = true;
    [SerializeField, Range(0f, 3f)] float pitchRandomSemitones = 0.35f;
    [SerializeField, Range(0f, 0.35f)] float volumeRandomAmount = 0.08f;

    [Header("Generation Profile")]
    [SerializeField] bool applyGenerationProfile = true;
    [SerializeField, Range(0f, 1f)] float volumeLossPerGeneration = 0.1f;
    [SerializeField, Range(0f, 1f)] float minGenerationVolume = 0.5f;
    [SerializeField] int quietGeneration = 3;
    [SerializeField, Range(0f, 0.2f)] float quietGenerationVolume = 0.04f;
    [SerializeField] bool muteQuietGenerations = false;

    [Header("Custom DSP")]
    [SerializeField] bool enableCustomDsp = true;
    [SerializeField] float maxLowPassCutoff = 16000f;
    [SerializeField] float minLowPassCutoff = 700f;
    [SerializeField, Range(0.1f, 0.95f)] float lowPassFalloff = 0.45f;
    [SerializeField, Range(0f, 0.6f)] float maxDelayWet = 0.5f;
    [SerializeField, Range(0f, 0.8f)] float maxDelayFeedback = 0.55f;
    [SerializeField, Range(0.02f, 0.5f)] float delayTimeSeconds = 0.12f;
    [SerializeField, Range(4f, 24f)] float cleanBitDepth = 24f;
    [SerializeField, Range(4f, 24f)] float minBitDepth = 4f;
    [SerializeField, Range(1, 12)] int maxSampleHold = 8;
    [SerializeField, Range(0.001f, 0.2f)] float parameterSmoothTime = 0.035f;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] bool debugLogging = false;
    [SerializeField] KeyCode debugPlayKey = KeyCode.P;
    [SerializeField] bool forceAuditionGeneration = false;
    [SerializeField, Range(0, 5)] int auditionGeneration = 2;
#endif

    AudioSource rollingSource;
    AudioSource oneShotSource;
    SpectralImprintDspFilter rollingFilter;
    SpectralImprintDspFilter oneShotFilter;

    float rollingTargetVolume;
    float rollingPlaybackVolumeScale = 1f;
    SpectralImprintDspProfile dspProfile;
    bool generationDspActive;
    int generationIndex;

    public static bool ForceCleanShowcaseMode { get; private set; }
    public int GenerationIndex => generationIndex;

    public static void ToggleCleanShowcaseMode() => SetCleanShowcaseMode(!ForceCleanShowcaseMode);

    public static void SetCleanShowcaseMode(bool enabled)
    {
        if (ForceCleanShowcaseMode == enabled)
            return;

        ForceCleanShowcaseMode = enabled;

        for (int i = instances.Count - 1; i >= 0; i--)
        {
            if (instances[i] == null)
            {
                instances.RemoveAt(i);
                continue;
            }

            instances[i].ApplyGenerationProfile();
        }
    }

    public SpectralImprintDisplaySnapshot GetDisplaySnapshot()
    {
        int dspGeneration = GetEffectiveGenerationIndex();
        bool active = enableCustomDsp && applyGenerationProfile && dspGeneration > 0;
        float ageAmount = active
            ? 1f - Mathf.Pow(lowPassFalloff, Mathf.Max(1, dspGeneration))
            : 0f;
        bool quietMemory = quietGeneration > 0 && dspGeneration >= quietGeneration;

        return new SpectralImprintDisplaySnapshot
        {
            generation = dspGeneration,
            dspActive = active,
            quietMemory = quietMemory,
            outputGain = active
                ? (quietMemory
                    ? (muteQuietGenerations ? 0f : quietGenerationVolume)
                    : Mathf.Max(minGenerationVolume, 1f - Mathf.Max(1, dspGeneration) * volumeLossPerGeneration))
                : 1f,
            muffleAmount = ageAmount,
            echoAmount = Mathf.Lerp(0f, maxDelayWet, ageAmount) / Mathf.Max(0.001f, maxDelayWet),
            crushAmount = active ? Mathf.InverseLerp(cleanBitDepth, minBitDepth, Mathf.Lerp(cleanBitDepth, minBitDepth, ageAmount)) : 0f,
            instabilityAmount = randomizePlayback ? Mathf.Clamp01((pitchRandomSemitones / 3f + volumeRandomAmount / 0.35f) * 0.5f) : 0f
        };
    }

    void Awake()
    {
        EnsureAudioListener();
        AutoAssignMissingClipsInEditor();
        LoadClips();
        EnsureSources();
        ApplyGenerationProfile();

#if UNITY_EDITOR
        if (debugLogging)
            Debug.Log($"SpectralImprintSource ready on {name}. Clips assigned: {DescribeClips()}");
#endif
    }

    void OnEnable()
    {
        if (!instances.Contains(this))
            instances.Add(this);

        ApplyGenerationProfile();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (debugPlayKey != KeyCode.None && Input.GetKeyDown(debugPlayKey))
            PlayDebugBurst();
#endif

        if (rollingSource == null) return;

        float target = playbackEnabled ? rollingTargetVolume : 0f;
        float fadeSpeed = GetRollingFadeSpeed();
        rollingSource.volume = Mathf.MoveTowards(
            rollingSource.volume,
            target,
            fadeSpeed * Time.deltaTime);

        if (rollingSource.isPlaying && rollingSource.volume <= 0.001f && target <= 0f)
            rollingSource.Stop();
    }

    void OnDisable()
    {
        instances.Remove(this);
        SetRolling(false);
        rollingFilter?.ResetDspState();
        oneShotFilter?.ResetDspState();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            ApplyGenerationProfile();
    }
#endif

    public void SetGenerationIndex(int value)
    {
        generationIndex = Mathf.Max(0, value);
        ApplyGenerationProfile();
    }

    public void SetRolling(bool active, float intensity = 1f)
    {
        if (!active && rollingSource == null)
        {
            rollingTargetVolume = 0f;
            return;
        }

        EnsureSources();

        if (rollingSource == null || rollingClip == null)
            return;

        bool startRolling = active && playbackEnabled && !rollingSource.isPlaying;

        if (startRolling)
            ApplyRollingPlaybackVariation();

        rollingTargetVolume = active
            ? rollingVolume * Mathf.Clamp01(intensity) * rollingPlaybackVolumeScale
            : 0f;

        if (startRolling)
            rollingSource.Play();
    }

    float GetRollingFadeSpeed()
    {
        float maxFadeVolume = Mathf.Max(rollingVolume, rollingTargetVolume, 0.001f);
        return maxFadeVolume / Mathf.Max(0.05f, rollingFadeDuration);
    }

    public void PlayJump() => PlayOneShot(jumpClip);
    public void PlayLand() => PlayOneShot(landClip);
    public void PlaySpawn() => PlayOneShot(spawnClip);
    public void PlayInteract() => PlayOneShot(interactClip);

    public void PlayDieDetached()
    {
        if (!playbackEnabled || dieClip == null) return;

        PlayClipDetached(
            dieClip,
            transform.position,
            oneShotVolume * GetRandomVolumeScale(),
            GetRandomPitchScale(),
            spatialBlend,
            minDistance,
            maxDistance);
    }

#if UNITY_EDITOR
    public void PlayDebugBurst()
    {
        if (debugLogging)
            Debug.Log($"SpectralImprintSource debug burst on {name}. driver={AudioSettings.driverCapabilities}, listener={FindObjectOfType<AudioListener>() != null}");

        PlayOneShot(spawnClip != null ? spawnClip : jumpClip);
    }
#endif

    void PlayOneShot(AudioClip clip)
    {
        if (!playbackEnabled)
        {
#if UNITY_EDITOR
            if (debugLogging)
                Debug.LogWarning($"SpectralImprintSource on {name} skipped playback because playback is disabled.");
#endif
            return;
        }

        if (clip == null)
        {
#if UNITY_EDITOR
            if (debugLogging)
                Debug.LogWarning($"SpectralImprintSource on {name} tried to play a missing clip.");
#endif
            return;
        }

        EnsureSources();
#if UNITY_EDITOR
        if (debugLogging)
            Debug.Log($"SpectralImprintSource playing {clip.name} on {name}. loadState={clip.loadState}, sourceEnabled={oneShotSource.enabled}");
#endif

        if (oneShotSource == null)
            return;

        oneShotSource.pitch = GetRandomPitchScale();
        oneShotSource.PlayOneShot(clip, oneShotVolume * GetRandomVolumeScale());
    }

    void EnsureSources()
    {
        if (rollingSource == null)
        {
            rollingSource = CreateFilteredAudioSource("Rolling", out rollingFilter);
            ConfigureSource(rollingSource);
            rollingSource.clip = rollingClip;
            rollingSource.loop = true;
            rollingSource.playOnAwake = false;
            rollingSource.volume = 0f;
        }

        if (oneShotSource == null)
        {
            oneShotSource = CreateFilteredAudioSource("One Shots", out oneShotFilter);
            ConfigureSource(oneShotSource);
            oneShotSource.loop = false;
            oneShotSource.playOnAwake = false;
            oneShotSource.volume = 1f;
        }

        ApplyFilterProfile();
    }

    void LoadClips()
    {
        LoadClip(rollingClip);
        LoadClip(jumpClip);
        LoadClip(landClip);
        LoadClip(spawnClip);
        LoadClip(dieClip);
        LoadClip(interactClip);
    }

    void AutoAssignMissingClipsInEditor()
    {
#if UNITY_EDITOR
        rollingClip ??= UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DSP Samples/DSP-Rolling.wav");
        jumpClip ??= UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DSP Samples/DSP-Jump.wav");
        landClip ??= UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DSP Samples/DSP-Land.wav");
        spawnClip ??= UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DSP Samples/DSP-Spawn.wav");
        dieClip ??= UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DSP Samples/DSP-Die.wav");
        interactClip ??= UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/DSP Samples/DSP-Interact.wav");
#endif
    }

    void LoadClip(AudioClip clip)
    {
        if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
            clip.LoadAudioData();
    }

    static void EnsureAudioListener()
    {
        if (FindObjectOfType<AudioListener>() != null)
            return;

        var camera = Camera.main;
        if (camera != null)
        {
            camera.gameObject.AddComponent<AudioListener>();
#if UNITY_EDITOR
            Debug.Log($"SpectralImprintSource added Unity AudioListener to {camera.name}.");
#endif
            return;
        }

        var listener = new GameObject("Spectral Imprints Audio Listener");
        listener.AddComponent<AudioListener>();
        DontDestroyOnLoad(listener);
#if UNITY_EDITOR
        Debug.Log("SpectralImprintSource created fallback Unity AudioListener.");
#endif
    }

    void ConfigureSource(AudioSource source)
    {
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.pitch = 1f;
    }

    void ApplyRollingPlaybackVariation()
    {
        rollingPlaybackVolumeScale = GetRandomVolumeScale();

        if (rollingSource != null)
            rollingSource.pitch = GetRandomPitchScale();
    }

    float GetRandomPitchScale()
    {
        if (!randomizePlayback || pitchRandomSemitones <= 0f)
            return 1f;

        float semitones = Random.Range(-pitchRandomSemitones, pitchRandomSemitones);
        return Mathf.Pow(2f, semitones / 12f);
    }

    float GetRandomVolumeScale()
    {
        if (!randomizePlayback || volumeRandomAmount <= 0f)
            return 1f;

        return Random.Range(1f - volumeRandomAmount, 1f + volumeRandomAmount);
    }

    AudioSource CreateFilteredAudioSource(string label, out SpectralImprintDspFilter filter)
    {
        var child = new GameObject($"Spectral Imprint {label}");
        child.transform.SetParent(transform, false);

        var source = child.AddComponent<AudioSource>();
        filter = child.AddComponent<SpectralImprintDspFilter>();
        return source;
    }

    void ApplyGenerationProfile()
    {
        int dspGeneration = GetEffectiveGenerationIndex();
        generationDspActive = enableCustomDsp && applyGenerationProfile && dspGeneration > 0;
        dspProfile = SpectralImprintDspProfile.Clean(
            maxLowPassCutoff,
            cleanBitDepth,
            delayTimeSeconds,
            parameterSmoothTime);

        if (!generationDspActive)
        {
            ApplyFilterProfile();
            return;
        }

        int effectiveGeneration = Mathf.Max(1, dspGeneration);
        float ageAmount = 1f - Mathf.Pow(lowPassFalloff, effectiveGeneration);
        bool quietMemory = quietGeneration > 0 && dspGeneration >= quietGeneration;

        dspProfile.active = true;
        dspProfile.gain = quietMemory
            ? (muteQuietGenerations ? 0f : quietGenerationVolume)
            : Mathf.Max(minGenerationVolume, 1f - effectiveGeneration * volumeLossPerGeneration);

        dspProfile.lowPassCutoff = Mathf.Lerp(maxLowPassCutoff, minLowPassCutoff, ageAmount);
        dspProfile.delayWet = Mathf.Lerp(0f, maxDelayWet, ageAmount);
        dspProfile.delayFeedback = Mathf.Lerp(0f, maxDelayFeedback, ageAmount);
        dspProfile.bitDepth = Mathf.Lerp(cleanBitDepth, minBitDepth, ageAmount);
        dspProfile.sampleHold = Mathf.Clamp(
            1 + Mathf.FloorToInt(ageAmount * (maxSampleHold - 1)),
            1,
            maxSampleHold);

        ApplyFilterProfile();
    }

    void ApplyFilterProfile()
    {
        rollingFilter?.SetProfile(dspProfile);
        oneShotFilter?.SetProfile(dspProfile);
    }

    int GetEffectiveGenerationIndex()
    {
        if (ForceCleanShowcaseMode)
            return 0;

#if UNITY_EDITOR
        if (forceAuditionGeneration)
            return Mathf.Max(0, auditionGeneration);
#endif

        return generationIndex;
    }

#if UNITY_EDITOR
    string DescribeClips() =>
        $"rolling={ClipName(rollingClip)}, jump={ClipName(jumpClip)}, land={ClipName(landClip)}, " +
        $"spawn={ClipName(spawnClip)}, die={ClipName(dieClip)}, interact={ClipName(interactClip)}";

    string ClipName(AudioClip clip) =>
        clip != null ? $"{clip.name}/{clip.loadState}" : "missing";
#endif

    static void PlayClipDetached(
        AudioClip clip,
        Vector3 position,
        float volume,
        float pitch,
        float spatialBlend,
        float minDistance,
        float maxDistance)
    {
        var go = new GameObject($"SpectralImprint_{clip.name}");
        go.transform.position = position;

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.Play();

        Destroy(go, clip.length / Mathf.Max(0.01f, pitch) + 0.1f);
    }
}
