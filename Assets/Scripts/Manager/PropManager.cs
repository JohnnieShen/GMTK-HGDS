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
    float speed = TimelineManager.Instance.timelineSpeed;

    bool record = speed >  0f;
    bool playback = speed <  0f;

    foreach (var r in recorders)
    {
        if (record)
            r.RecordFrame(currentTime);

        if (playback)
            r.ApplyAtTime(currentTime);
    }
}


    public void SeekAll(float time)
    {
        foreach (var r in recorders) r.ApplyAtTime(time);
    }
}
