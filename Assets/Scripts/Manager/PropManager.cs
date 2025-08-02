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
        TimelineManager.Instance.OnTimelineLoop += OnLoop;
    }

    void OnDestroy()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTimelineTick -= OnTick;
            TimelineManager.Instance.OnTimelineLoop -= OnLoop;
        }
        if (Instance == this) Instance = null;
    }

    public void Register   (PropRecorder r) => recorders.Add(r);
    public void Unregister (PropRecorder r) => recorders.Remove(r);

    void OnTick(float t)
    {
        var mgr    = TimelineManager.Instance;
        float spd  = mgr.timelineSpeed;
        bool record   = spd >  0f && !mgr.IsFastForwarding;
        bool playback = spd <  0f || mgr.IsFastForwarding;

        foreach (var r in recorders) {
            if (record)   r.RecordFrame(t);
            if (playback) r.ApplyAtTime(t);
        }
    }

    void OnLoop()
    {
        SeekAll(0f);
    }

    public void SeekAll(float time)
    {
        foreach (var r in recorders) r.ApplyAtTime(time);
    }
}
