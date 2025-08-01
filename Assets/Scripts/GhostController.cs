using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class GhostController : MonoBehaviour
{
    private MovementController movement;
    private SpriteRenderer sr;
    private List<PlayerInputFrame> inputFrames;
    private int replayIndex = 0;
    private bool prevJumpHeld = false;
    
    [Header("Ghost Settings")]
    public int ghostLayer = 9;
    public float transparency = 0.7f;
    Rigidbody2D rb;

    void Awake()
    {
        Debug.Log($"GhostController: Awake on layer {ghostLayer} with transparency {transparency}");
        movement = GetComponent<MovementController>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gameObject.layer = ghostLayer;
        SetTransparency(transparency);
        Debug.Log($"GhostController: Awake complete. Layer={gameObject.layer}, Transparency={sr.color.a}");
    }
       
    // The core replay logic now runs in FixedUpdate.
    void FixedUpdate()
    {
        if (inputFrames == null || inputFrames.Count == 0) return;
        float timelineTime = TimelineManager.Instance.GetCurrentTime();

        if (timelineTime > inputFrames[^1].time + 0.02f)
        {
            Debug.Log("GhostController: finished replay; destroying self.");
            Destroy(gameObject);
            return;
        }
        
        // Find frame that matches current timeline time
        var frame = inputFrames.FirstOrDefault(f => Mathf.Abs(f.time - timelineTime) < 0.02f);
        if (frame == null) return;

        movement.Move(frame.horizontal);
        bool jumpDown = frame.jump && !prevJumpHeld;
        movement.Jump(jumpDown, frame.jump);
        prevJumpHeld = frame.jump;
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

    public void Initialize(List<PlayerInputFrame> frames, float startTime)
    {
        Debug.Log($"GhostController141: Initializing with {frames.Count} frames at start time {startTime:0.00}s");
        inputFrames = new List<PlayerInputFrame>(frames);

        PlayerInputFrame frame = inputFrames
                                 .OrderBy(f => Mathf.Abs(f.time - startTime))
                                 .First();

        movement.SetPosition(frame.position);
        rb.linearVelocity = frame.velocity;

        prevJumpHeld = frame.jump;

        replayIndex = inputFrames.IndexOf(frame);

        Debug.Log(
            $"Ghost init @t={frame.time:0.00}s  pos={frame.position}  vel={frame.velocity}");
    }

    public void Initialize(List<PlayerInputFrame> frames, Vector3 startPosition) => Initialize(frames, 0f);
    
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