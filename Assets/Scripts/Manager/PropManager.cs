using UnityEngine;
using System.Collections.Generic;

public class PropManager : MonoBehaviour
{
    public static PropManager Instance { get; private set; }
    public static bool Exists => Instance != null;

    readonly List<PropRecorder> recorders = new();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        TimelineManager.Instance.OnTimelineTick += OnTick;
    }

    void OnDestroy()
    {
        if (TimelineManager.Instance != null)
            TimelineManager.Instance.OnTimelineTick -= OnTick;
        if (Instance == this) Instance = null;
    }

    public void Register   (PropRecorder r) => recorders.Add(r);
    public void Unregister (PropRecorder r) => recorders.Remove(r);

    void OnTick(float currentTime)
    {
        foreach (var r in recorders)
        {
            r.RecordFrame(currentTime);
            r.ApplyAtTime(currentTime);
        }
    }

    public void SeekAll(float time)
    {
        foreach (var r in recorders) r.ApplyAtTime(time);
    }
}
