using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class GhostController : MonoBehaviour
{
    private MovementController movement;
    private SpriteRenderer sr;
    private Collider2D col;
    private List<PlayerInputFrame> inputFrames;
    private int replayIndex = 0;
    private bool prevJumpHeld = false;
    
    [Header("Ghost Settings")]
    public int ghostLayer = 9;
    public float transparency = 0.7f;
    Rigidbody2D rb;
    

    float startTime;
    float endTime;

    void Awake()
    {
        Debug.Log($"GhostController: Awake on layer {ghostLayer} with transparency {transparency}");
        movement = GetComponent<MovementController>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        gameObject.layer = ghostLayer;
        SetTransparency(transparency);
        Debug.Log($"GhostController: Awake complete. Layer={gameObject.layer}, Transparency={sr.color.a}");
        float timelineTime = TimelineManager.Instance.GetCurrentTime();
        bool visible = timelineTime >= startTime && timelineTime <= endTime;
        sr.enabled = visible;
        col.isTrigger = !visible;
    }
       
    void FixedUpdate()
    {
        if (inputFrames == null || inputFrames.Count == 0) return;
        float timelineTime = TimelineManager.Instance.GetCurrentTime();
        float speed = TimelineManager.Instance.timelineSpeed;
        bool visible = timelineTime >= startTime && timelineTime <= endTime;
        sr.enabled = visible;
        col.isTrigger = !visible;
        if (!visible) return;
        const float tol = 0.02f;
        if (speed >= 0f)
        {
            if (timelineTime > endTime + tol) { Destroy(gameObject); return; }
        }
        else
        {
            if (timelineTime < startTime - tol) { Destroy(gameObject); return; }
        }
        
        var frame = inputFrames
            .OrderBy(f => Mathf.Abs(f.time - timelineTime))
            .First();

        if (speed < 0f || speed > 1f)
        {
            movement.SetPosition(frame.position);

            rb.linearVelocity = (speed < 0f)
                ? -frame.velocity
                : frame.velocity;

            prevJumpHeld = frame.jump;
        }
        else
        {
            movement.Move(frame.horizontal);
            bool jumpDown = frame.jump && !prevJumpHeld;
            movement.Jump(jumpDown, frame.jump);
            prevJumpHeld = frame.jump;
        }
    }
    
    public void Seek(float time)
    {
        if (inputFrames == null || inputFrames.Count == 0) return;

        PlayerInputFrame frame = inputFrames
                                .OrderBy(f => Mathf.Abs(f.time - time))
                                .First();

        movement.SetPosition(frame.position);
        rb.linearVelocity = frame.velocity;

        prevJumpHeld = frame.jump;
        replayIndex  = inputFrames.IndexOf(frame);
    }

    public void Initialize(List<PlayerInputFrame> frames, float start, float end)
    {
        startTime = start;
        endTime   = end;

        Debug.Log($"GhostController141: Initializing with {frames.Count} frames at start time {start:0.00}s");
        inputFrames = new List<PlayerInputFrame>(frames);

        PlayerInputFrame frame = inputFrames
                                 .OrderBy(f => Mathf.Abs(f.time - start))
                                 .First();

        movement.SetPosition(frame.position);
        rb.linearVelocity = frame.velocity;

        prevJumpHeld = frame.jump;

        replayIndex = inputFrames.IndexOf(frame);

        Debug.Log(
            $"Ghost init @t={frame.time:0.00}s  pos={frame.position}  vel={frame.velocity}");
    }

    public void Initialize(List<PlayerInputFrame> frames, float start)
    {
        float end = frames != null && frames.Count > 0 ? frames[^1].time : start;
        Initialize(frames, start, end);
    }
    
    void SetTransparency(float alpha)
    {
        if (sr != null)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }
    }
}