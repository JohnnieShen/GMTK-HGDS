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

    readonly List<Transform> playersOnGhost = new ();

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

        var frame = inputFrames.OrderBy(f => Mathf.Abs(f.time - t)).First();

        // Store previous position for movement calculation
        Vector2 prevPos = transform.position;

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

        // Calculate movement delta and move players riding the ghost
        Vector2 currentPos = transform.position;
        Vector2 delta = currentPos - prevPos;
        
        // Move all players riding on the ghost
        foreach (var p in playersOnGhost)
        {
            if (p != null)
            {
                // Move the player by the same amount the ghost moved
                p.position += (Vector3)delta;
                
                // Optional: Match ghost's horizontal velocity to prevent sliding
                Rigidbody2D playerRb = p.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 playerVel = playerRb.linearVelocity;
                    playerVel.x = rb.linearVelocity.x; // Match ghost's horizontal velocity
                    playerRb.linearVelocity = playerVel;
                }
            }
        }
    }

    void TryInteract()
    {
        Debug.Log("Ghost trying to interact");
        float radius = 0.7f;
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
        startTime = start;
        endTime = end;

        Seek(start);
        GameManager.Instance?.RegisterGhost(this, startTime, endTime);
    }

    public void Initialize(List<PlayerInputFrame> frames, float start)
    {
        float end = frames is { Count: > 0 } ? frames[^1].time : start;
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

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.transform.CompareTag("Player"))
        {
            // Check if player is landing on top of the ghost (not hitting from the side)
            bool isOnTop = false;
            foreach (ContactPoint2D contact in c.contacts)
            {
                if (contact.normal.y < -0.5f) // Normal pointing down means player is on top
                {
                    isOnTop = true;
                    break;
                }
            }
            
            if (isOnTop && !playersOnGhost.Contains(c.transform))
            {
                playersOnGhost.Add(c.transform);
                Debug.Log($"Player {c.transform.name} is now riding ghost");
            }
        }
    }
    
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.transform.CompareTag("Player"))
        {
            playersOnGhost.Remove(c.transform);
            Debug.Log($"Player {c.transform.name} stopped riding ghost");
        }
    }

    
    
}