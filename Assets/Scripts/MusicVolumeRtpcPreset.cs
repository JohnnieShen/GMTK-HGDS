using UnityEngine;

[CreateAssetMenu(
    fileName = "MusicVolumeRtpcPreset",
    menuName = "Deja You/Audio/Music Volume RTPC Preset")]
public class MusicVolumeRtpcPreset : ScriptableObject
{
    [SerializeField] string rtpcName = "MusicVolume";
    [SerializeField] bool applyGlobally = true;
    [SerializeField, Range(0f, 100f)] float normalValue = 100f;
    [SerializeField, Range(0f, 100f)] float showcaseValue = 35f;
    [SerializeField, Min(0)] int fadeDurationMs = 250;
    [SerializeField] AkCurveInterpolation fadeCurve = AkCurveInterpolation.AkCurveInterpolation_Linear;

    public string RtpcName => rtpcName;
    public bool ApplyGlobally => applyGlobally;
    public float NormalValue => normalValue;
    public float ShowcaseValue => showcaseValue;

    public AKRESULT ApplyNormal(GameObject target) => ApplyValue(target, normalValue);
    public AKRESULT ApplyShowcase(GameObject target) => ApplyValue(target, showcaseValue);

    public AKRESULT ApplyValue(GameObject target, float value)
    {
        string resolvedRtpcName = string.IsNullOrWhiteSpace(rtpcName) ? "MusicVolume" : rtpcName;
        float clampedValue = Mathf.Clamp(value, 0f, 100f);

        if (applyGlobally)
            return AkSoundEngine.SetRTPCValue(resolvedRtpcName, clampedValue);

        return AkSoundEngine.SetRTPCValue(
            resolvedRtpcName,
            clampedValue,
            target,
            fadeDurationMs,
            fadeCurve);
    }
}
