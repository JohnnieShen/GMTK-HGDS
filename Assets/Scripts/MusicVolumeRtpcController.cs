using System.Collections;
using UnityEngine;

public class MusicVolumeRtpcController : MonoBehaviour
{
    const string DefaultPresetResourcePath = "MusicVolumeRtpcPreset";

    static MusicVolumeRtpcController instance;

    [SerializeField] MusicVolumeRtpcPreset preset;
    [SerializeField] bool applyNormalOnStart = true;
    [SerializeField] KeyCode toggleShowcaseVolumeKey = KeyCode.F6;
    [SerializeField, Min(0f)] float startupApplyDelay = 0.5f;

    bool showcaseVolumeActive;

    public bool ShowcaseVolumeActive => showcaseVolumeActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateRuntimeController()
    {
        if (instance != null) return;

        var go = new GameObject("Music Volume RTPC Controller");
        instance = go.AddComponent<MusicVolumeRtpcController>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        LoadPresetIfNeeded();
    }

    void Start()
    {
        if (applyNormalOnStart)
            StartCoroutine(ApplyNormalAfterStartupDelay());
    }

    void Update()
    {
        if (toggleShowcaseVolumeKey != KeyCode.None && Input.GetKeyDown(toggleShowcaseVolumeKey))
            ToggleShowcaseVolume();
    }

    public void ToggleShowcaseVolume()
    {
        if (showcaseVolumeActive)
            ApplyNormalVolume();
        else
            ApplyShowcaseVolume();
    }

    public void ApplyNormalVolume()
    {
        LoadPresetIfNeeded();
        showcaseVolumeActive = false;
        ApplyPresetValue(false);
    }

    public void ApplyShowcaseVolume()
    {
        LoadPresetIfNeeded();
        showcaseVolumeActive = true;
        ApplyPresetValue(true);
    }

    void LoadPresetIfNeeded()
    {
        if (preset != null) return;

        preset = Resources.Load<MusicVolumeRtpcPreset>(DefaultPresetResourcePath);
        if (preset == null)
            Debug.LogWarning($"MusicVolumeRtpcController could not find Resources/{DefaultPresetResourcePath}.asset.");
    }

    void ApplyPresetValue(bool useShowcaseValue)
    {
        if (preset == null) return;

        AKRESULT result = useShowcaseValue
            ? preset.ApplyShowcase(gameObject)
            : preset.ApplyNormal(gameObject);

#if UNITY_EDITOR
        Debug.Log($"[MusicVolumeRtpcController] {preset.RtpcName}={(useShowcaseValue ? preset.ShowcaseValue : preset.NormalValue):0.##} scope={(preset.ApplyGlobally ? "global" : "gameObject")} result={result}");
#endif
    }

    IEnumerator ApplyNormalAfterStartupDelay()
    {
        ApplyNormalVolume();

        if (startupApplyDelay > 0f)
            yield return new WaitForSecondsRealtime(startupApplyDelay);
        else
            yield return null;

        if (!showcaseVolumeActive)
            ApplyNormalVolume();
    }
}
