using UnityEngine;
using System.Collections.Generic;

public class PropManager : MonoBehaviour
{
    public static PropManager Instance { get; private set; }

    readonly Dictionary<int, GameObject> props = new();
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

    public void RegisterGameObject(GameObject go)
    {
        if (go == null) return;
        props[go.GetInstanceID()] = go;
    }

    public void UnregisterGameObject(GameObject go)
    {
        if (go == null) return;
        props.Remove(go.GetInstanceID());
    }

    public GameObject GetProp(int instanceId)
        => props.TryGetValue(instanceId, out var go) ? go : null;

    public void Register(PropRecorder r)
    {
        if (r == null) return;

        if (!recorders.Contains(r))
            recorders.Add(r);

        props[r.gameObject.GetInstanceID()] = r.gameObject;
    }

    public void Unregister(PropRecorder r)
    {
        if (r == null) return;

        recorders.Remove(r);
        props.Remove(r.gameObject.GetInstanceID());
    }

    void OnTick(float t)
    {
        float spd = TimelineManager.Instance.timelineSpeed;
        bool record   = spd > 0f;   // record on any forward speed
        bool playback = true;

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
