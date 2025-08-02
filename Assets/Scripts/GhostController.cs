using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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
        movement = GetComponent<MovementController>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        gameObject.layer = ghostLayer;
        SetTransparency(transparency);
    }

    void FixedUpdate()
    {
        if (inputFrames == null || inputFrames.Count == 0) return;

        float t = TimelineManager.Instance.GetCurrentTime();
        float speed = TimelineManager.Instance.timelineSpeed;

        // const float tol = 0.02f;
        // if (speed >= 0f && t > endTime + tol) { Destroy(gameObject); return; }
        // if (speed <  0f && t < startTime - tol) { Destroy(gameObject); return; }

        var frame = inputFrames.OrderBy(f => Mathf.Abs(f.time - t)).First();

        // if (speed < 0f || speed > 1f)
        // {
        //     movement.SetPosition(frame.position);
        //     rb.linearVelocity = (speed < 0f) ? -frame.velocity : frame.velocity;
        //     prevJumpHeld = frame.jump;
        // }
        // else
        // {
        //     movement.Move(frame.horizontal);
        //     bool jumpDown = frame.jump && !prevJumpHeld;
        //     movement.Jump(jumpDown, frame.jump);
        //     prevJumpHeld = frame.jump;
        // }

        movement.SetPosition(frame.position);
        rb.linearVelocity = (speed < 0f) ? -frame.velocity : frame.velocity;
        prevJumpHeld = frame.jump;
        
        Debug.Log($"Ghost frame at t={frame.time:0.00}s: pos={frame.position}, vel={rb.linearVelocity}, jump={frame.jump}, interact={frame.interact}, propId={frame.interactPropId}");
        if (frame.interact && frame.interactPropId != -1)
        {
            Debug.Log($"Ghost interacting with prop ID: {frame.interactPropId}");
            var go = EditorUtility.InstanceIDToObject(frame.interactPropId) as GameObject;
            if (go != null)
            {
                var i = go.GetComponent<Interactable>();
                if (i != null) i.Interact();
            }
        }
    }
    
    void TryInteract()
    {
        Debug.Log("Ghost trying to interact");
        float radius = 0.5f;
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var c in hits)
        {
            var interactable = c.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact();
                break;
            }
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
        replayIndex = inputFrames.IndexOf(frame);
    }

    public void Initialize(List<PlayerInputFrame> frames, float start, float end)
    {
        inputFrames = new List<PlayerInputFrame>(frames);
        startTime   = start;
        endTime     = end;

        Seek(start);
        GameManager.Instance?.RegisterGhost(this, startTime, endTime);
    }

    public void Initialize(List<PlayerInputFrame> frames, float start)
    {
        float end = frames is { Count: >0 } ? frames[^1].time : start;
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