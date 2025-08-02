using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(RecordableProp))]
public class PropRecorder : MonoBehaviour
{
    public List<PropStatusFrame> frames = new();

    RecordableProp recordable;
    int propId;

    void Awake()
    {
        recordable = GetComponent<RecordableProp>();
        propId = gameObject.GetInstanceID();
        PropManager.Instance.Register(this);

        RecordFrame(TimelineManager.Instance.GetCurrentTime());
    }

    void OnDestroy()
    {
        if (PropManager.Exists)
            PropManager.Instance.Unregister(this);
    }

    public void RecordFrame(float t)
    {
        frames.RemoveAll(f => f.time > t);

        var snap = recordable.CaptureFrame();
        snap.propId = propId;
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

        recordable.ApplyFrame(f);
    }
}
