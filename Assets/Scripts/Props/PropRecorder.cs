using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(RecordableProp))]
public class PropRecorder : MonoBehaviour
{
    public IReadOnlyList<PropStatusFrame> Frames => frames;
    public int FrameCount => frames.Count;
    public bool IsRecording { get; private set; } = true;

    readonly List<PropStatusFrame> frames = new();

    RecordableProp prop;
    int id;

    void Awake()
    {
        prop = GetComponent<RecordableProp>();
        id = gameObject.GetInstanceID();
    }

    void Start()
    {
        if (PropManager.Exists)
            PropManager.Instance.Register(this);
        else
            Debug.LogError($"{name}: PropManager not found in scene!");
    }

    void OnDestroy()
    {
        if (PropManager.Exists)
            PropManager.Instance.Unregister(this);
    }

    public void StartRecording() { IsRecording = true;  frames.Clear(); }
    public void StopRecording () { IsRecording = false; }

    public void RecordFrame(float t)
    {
        if (!IsRecording) return;

        if (frames.Count   > 0 && Mathf.Approximately(frames[^1].time, t))
            return;

        frames.RemoveAll(f => f.time > t);

        var snap = prop.CaptureFrame();
        snap.propId = id;
        snap.time = t;
        frames.Add(snap);
    }

    public void ApplyAtTime(float t)
    {
        if (frames.Count == 0) return;

        PropStatusFrame f = frames
            .Where(fr => fr.time <= t)
            .OrderByDescending(fr => fr.time)
            .FirstOrDefault() ?? frames[0];

        prop.ApplyFrame(f);
    }
}
