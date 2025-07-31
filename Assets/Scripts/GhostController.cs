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
    
    void Awake()
    {
        movement = GetComponent<MovementController>();
        sr = GetComponent<SpriteRenderer>();
    }
    
    void Start()
    {
        // Set up ghost appearance
        gameObject.layer = ghostLayer;
        SetTransparency(transparency);
        Debug.Log($"GhostController: Ghost started on layer {ghostLayer} with transparency {transparency}");
    }
    
    // The core replay logic now runs in FixedUpdate.
    void FixedUpdate()
    {
        float timelineTime = TimelineManager.Instance.GetCurrentTime();
        
        // Find frame that matches current timeline time
        var frame = inputFrames.FirstOrDefault(f => Mathf.Abs(f.time - timelineTime) < 0.02f);
        if (frame == null) return;

        movement.Move(frame.horizontal);
        bool jumpDown = frame.jump && !prevJumpHeld;
        movement.Jump(jumpDown, frame.jump);
        prevJumpHeld = frame.jump;
    }

    public void Initialize(List<PlayerInputFrame> frames, Vector3 startPosition)
    {
        inputFrames = new List<PlayerInputFrame>(frames);
        movement.SetPosition(startPosition);
        replayIndex = 0;
        prevJumpHeld = false;
        Debug.Log($"GhostController: Initialized with {frames.Count} frames at position {startPosition}");
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