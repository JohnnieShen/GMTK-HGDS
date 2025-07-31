using System.Collections.Generic;
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
        if (inputFrames == null || replayIndex >= inputFrames.Count)
        {
            // Add a null check to prevent errors if destroyed before initialization.
            if (inputFrames != null)
            {
                Debug.Log("GhostController → replay finished, destroying ghost");
                Destroy(gameObject);
            }
            return;
        }

        // Get the current frame for this physics step.
        PlayerInputFrame frame = inputFrames[replayIndex];

        // Apply the recorded input directly to the MovementController.
        movement.Move(frame.horizontal);

        // Derive the "jumpDown" state from the change in the "jumpHeld" state.
        bool jumpHeld = frame.jump;
        bool jumpDown = jumpHeld && !prevJumpHeld;
        movement.Jump(jumpDown, jumpHeld);
        prevJumpHeld = jumpHeld;

        // Advance to the next frame for the next FixedUpdate call.
        replayIndex++;
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