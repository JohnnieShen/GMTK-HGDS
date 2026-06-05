using UnityEngine;

public class SpectralImprintSource : MonoBehaviour
{
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
    [SerializeField] float rollingFadeSpeed = 8f;
    [SerializeField, Range(0f, 1f)] float spatialBlend = 0f;
    [SerializeField] float minDistance = 1.5f;
    [SerializeField] float maxDistance = 18f;

    [Header("Generation Profile")]
    [SerializeField] bool applyGenerationProfile = true;
    [SerializeField, Range(0f, 1f)] float volumeLossPerGeneration = 0.12f;
    [SerializeField, Range(0f, 1f)] float minGenerationVolume = 0.35f;
    [SerializeField, Range(0f, 0.25f)] float pitchLossPerGeneration = 0.035f;
    [SerializeField, Range(0.5f, 1f)] float minGenerationPitch = 0.82f;
    [SerializeField] float maxLowPassCutoff = 22000f;
    [SerializeField] float minLowPassCutoff = 1800f;
    [SerializeField, Range(0.1f, 0.95f)] float lowPassFalloff = 0.65f;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] bool debugLogging = false;
    [SerializeField] KeyCode debugPlayKey = KeyCode.P;
#endif

    AudioSource rollingSource;
    AudioSource oneShotSource;
    AudioLowPassFilter lowPassFilter;
    float rollingTargetVolume;
    float generationGain = 1f;
    float generationPitch = 1f;
    int generationIndex;

    public int GenerationIndex => generationIndex;

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

    void Update()
    {
#if UNITY_EDITOR
        if (debugPlayKey != KeyCode.None && Input.GetKeyDown(debugPlayKey))
            PlayDebugBurst();
#endif

        if (rollingSource == null) return;

        float target = playbackEnabled ? rollingTargetVolume : 0f;
        rollingSource.volume = Mathf.MoveTowards(
            rollingSource.volume,
            target,
            rollingFadeSpeed * Time.deltaTime);

        if (rollingSource.isPlaying && rollingSource.volume <= 0.001f && target <= 0f)
            rollingSource.Stop();
    }

    void OnDisable()
    {
        SetRolling(false);
    }

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

        rollingTargetVolume = active
            ? rollingVolume * Mathf.Clamp01(intensity) * generationGain
            : 0f;

        if (active && playbackEnabled && !rollingSource.isPlaying)
            rollingSource.Play();
    }

    public void PlayJump() => PlayOneShot(jumpClip);
    public void PlayLand() => PlayOneShot(landClip);
    public void PlaySpawn() => PlayOneShot(spawnClip);
    public void PlayInteract() => PlayOneShot(interactClip);

    public void PlayDieDetached()
    {
        if (!playbackEnabled || dieClip == null) return;

        PlayClipDetached(dieClip, transform.position, oneShotVolume * generationGain, spatialBlend, minDistance, maxDistance);
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
            Debug.Log($"SpectralImprintSource playing {clip.name} on {name}. loadState={clip.loadState}, sourceEnabled={oneShotSource.enabled}, volume={oneShotSource.volume}");
#endif

        oneShotSource?.PlayOneShot(clip, oneShotVolume * generationGain);
    }

    void EnsureSources()
    {
        if (rollingSource == null)
        {
            rollingSource = gameObject.AddComponent<AudioSource>();
            ConfigureSource(rollingSource);
            rollingSource.clip = rollingClip;
            rollingSource.loop = true;
            rollingSource.playOnAwake = false;
            rollingSource.volume = 0f;
        }

        if (oneShotSource == null)
        {
            oneShotSource = gameObject.AddComponent<AudioSource>();
            ConfigureSource(oneShotSource);
            oneShotSource.loop = false;
            oneShotSource.playOnAwake = false;
            oneShotSource.volume = 1f;
        }

        ApplyGenerationProfile();
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
        source.pitch = generationPitch;
    }

    void ApplyGenerationProfile()
    {
        generationGain = 1f;
        generationPitch = 1f;

        bool shouldColor = applyGenerationProfile && generationIndex > 0;
        if (shouldColor)
        {
            generationGain = Mathf.Max(minGenerationVolume, 1f - generationIndex * volumeLossPerGeneration);
            generationPitch = Mathf.Max(minGenerationPitch, 1f - generationIndex * pitchLossPerGeneration);
        }

        if (rollingSource != null)
            rollingSource.pitch = generationPitch;

        if (oneShotSource != null)
        {
            oneShotSource.pitch = generationPitch;
            oneShotSource.volume = 1f;
        }

        if (!shouldColor)
        {
            if (lowPassFilter != null)
                lowPassFilter.enabled = false;
            return;
        }

        EnsureLowPassFilter();
        if (lowPassFilter == null) return;

        float ageAmount = 1f - Mathf.Pow(lowPassFalloff, generationIndex);
        lowPassFilter.enabled = true;
        lowPassFilter.cutoffFrequency = Mathf.Lerp(maxLowPassCutoff, minLowPassCutoff, ageAmount);
        lowPassFilter.lowpassResonanceQ = 1f;
    }

    void EnsureLowPassFilter()
    {
        if (lowPassFilter != null) return;

        lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null)
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
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
        float spatialBlend,
        float minDistance,
        float maxDistance)
    {
        var go = new GameObject($"SpectralImprint_{clip.name}");
        go.transform.position = position;

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.Play();

        Destroy(go, clip.length + 0.1f);
    }
}
