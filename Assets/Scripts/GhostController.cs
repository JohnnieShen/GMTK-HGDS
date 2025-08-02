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
    int frameIndex;

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

        float   t     = TimelineManager.Instance.GetCurrentTime();
        float   speed = TimelineManager.Instance.timelineSpeed;

        if (speed >= 0f)
        {
            while (frameIndex < inputFrames.Count - 1 &&
                   inputFrames[frameIndex + 1].time <= t)
            {
                frameIndex++;
                ProcessInteraction(inputFrames[frameIndex]);
            }
        }
        else
        {
            while (frameIndex > 0 &&
                   inputFrames[frameIndex - 1].time >= t)
            {
                frameIndex--;
                ProcessInteraction(inputFrames[frameIndex]);
            }
        }

        ApplyFrame(inputFrames[frameIndex], speed);
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

    void ProcessInteraction(PlayerInputFrame f)
    {
        if (f.interact && f.interactPropId != -1)
        {
            var go = UnityEditor.EditorUtility.InstanceIDToObject(f.interactPropId) as GameObject;
            go?.GetComponent<Interactable>()?.Interact();
        }
    }


    public void Seek(float time)
    {
        if (inputFrames == null || inputFrames.Count == 0) return;

        frameIndex = inputFrames.FindLastIndex(f => f.time <= time);
        if (frameIndex < 0) frameIndex = 0;

        var f = inputFrames[frameIndex];
        movement.SetPosition(f.position);
        rb.linearVelocity = f.velocity;
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

    void ApplyFrame(PlayerInputFrame f, float speed)
    {
        Vector2 prevPos = transform.position;

        movement.SetPosition(f.position);
        rb.linearVelocity = (speed < 0f) ? -f.velocity : f.velocity;

        Vector2 delta = (Vector2)transform.position - prevPos;
        foreach (var p in playersOnGhost)
            if (p) p.position += (Vector3)delta;
    }
    
}