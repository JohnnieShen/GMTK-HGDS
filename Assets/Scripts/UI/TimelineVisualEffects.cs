using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimelineVisualEffects : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Post-processing Volume with a Chromatic Aberration override")]
    public Volume postProcessVolume;

    [Tooltip("UI panel that appears while fast-forwarding")]
    public GameObject fastForwardPanel;

    [Tooltip("UI panel that appears while rewinding")]
    public GameObject rewindPanel;

    [Header("Effect Settings")]
    [Tooltip("Chromatic aberration intensity while FF / RW is active")]
    [Range(0f, 1f)] public float effectIntensity = 0.5f;

    [Tooltip("Random offset added to intensity each frame (± range)")]
    [Range(0f, 0.5f)] public float jitterRange = 0.1f;

    ChromaticAberration chromatic;
    bool lastFF, lastRW;

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError($"{name}: Post-process Volume reference not set.");
            enabled = false;
            return;
        }

        if (!postProcessVolume.profile.TryGet(out chromatic))
        {
            chromatic = postProcessVolume.profile.Add<ChromaticAberration>();
            chromatic.active = true;
        }

        ApplyState(false, false);
    }

    void Update()
    {
        var tm = TimelineManager.Instance;
        if (tm == null) return;

        bool isFF = tm.IsFastForwarding;
        bool isRW = tm.IsRewinding;

        if (isFF != lastFF || isRW != lastRW)
        {
            ApplyState(isFF, isRW);
            lastFF = isFF;
            lastRW = isRW;
        }

        if (isFF || isRW)
        {
            float jitter = Random.Range(-jitterRange, jitterRange);
            chromatic.intensity.value = Mathf.Clamp01(effectIntensity + jitter);
        }
    }

    void ApplyState(bool isFF, bool isRW)
    {
        if (fastForwardPanel != null)
            fastForwardPanel.SetActive(isFF);

        if (rewindPanel != null)
            rewindPanel.SetActive(isRW);

        if (chromatic != null)
            chromatic.intensity.value = (isFF || isRW) ? effectIntensity : 0f;
    }
}