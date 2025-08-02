using UnityEngine;
using System.Collections.Generic;

public class PlatformMover : MonoBehaviour, RecordableProp
{
    [Header("Path")]
    public Vector2 pointA;
    public Vector2 pointB;
    public float   speed = 2f;

    Vector2  target;
    bool directionBA;
    bool paused;

    readonly List<Transform> playersOnPlatform = new ();

    PropRecorder recorder;

    void Start()
    {
        directionBA = false;
        target = pointB;
    }

    void Update()
    {
        paused = TimelineManager.Instance.IsPaused;
        if (paused) return;

        bool simulate = TimelineManager.Instance.timelineSpeed > 0f;

        if (!simulate) return;

        float dt = Time.deltaTime * TimelineManager.Instance.timelineSpeed;

        Vector2 prevPos = transform.position;
        transform.position = Vector2.MoveTowards(prevPos, target, speed * dt);

        Vector2 delta = (Vector2)transform.position - prevPos;
        foreach (var p in playersOnPlatform)
            if (p) p.position += (Vector3)delta;

        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            directionBA = !directionBA;
            target = directionBA ? pointA : pointB;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.transform.CompareTag("Player") && c.contacts[0].normal.y < -0.5f
            && !playersOnPlatform.Contains(c.transform))
            playersOnPlatform.Add(c.transform);
    }
    void OnCollisionExit2D (Collision2D c)
    {
        if (c.transform.CompareTag("Player"))
            playersOnPlatform.Remove(c.transform);
    }

    public PropStatusFrame CaptureFrame()
    {
        Debug.Log($"Capturing frame for {gameObject.name} at time {TimelineManager.Instance.GetCurrentTime()}, active: {directionBA}, position: {transform.position}");
        return new PropStatusFrame(
            gameObject.GetInstanceID(),
            TimelineManager.Instance.GetCurrentTime(),
            directionBA,
            transform.position
        );
    }

    public void ApplyFrame(PropStatusFrame f)
    {
        Debug.Log($"Applying frame to {gameObject.name} at time {f.time}, active: {f.active}, position: {f.position}");
        directionBA = f.active;
        transform.position = f.position;
        target = directionBA ? pointA : pointB;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA, pointB);
    }
}
