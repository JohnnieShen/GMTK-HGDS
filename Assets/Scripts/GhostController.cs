using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class GhostController : MonoBehaviour
{
    private MovementController movement;
    private SpriteRenderer sr;
    private List<PlayerInputFrame> inputFrames;
    private int replayIndex = 0;
    private float timeElapsed = 0f;
    bool prevJumpHeld = false;
    
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
    
    void Update()
    {
        timeElapsed += Time.deltaTime;
        ReplayInput();
    }
    
    public void Initialize(List<PlayerInputFrame> frames, Vector3 startPosition)
    {
        inputFrames = new List<PlayerInputFrame>(frames);
        movement.SetPosition(startPosition);
        replayIndex = 0;
        timeElapsed = 0f;
        prevJumpHeld  = false;
        Debug.Log($"GhostController: Initialized with {frames.Count} frames at position {startPosition}");
    }
    
    void ReplayInput()
    {
        if (inputFrames == null || replayIndex >= inputFrames.Count)
        {
            Debug.Log("GhostController → replay finished, destroying ghost");
            Destroy(gameObject);
            return;
        }

        PlayerInputFrame frame = inputFrames[replayIndex];

        if (timeElapsed >= frame.time)
        {
            movement.Move(frame.horizontal);

            bool jumpHeld = frame.jump;
            bool jumpDown = jumpHeld && !prevJumpHeld;
            movement.Jump(jumpDown, jumpHeld);
            prevJumpHeld = jumpHeld;

            replayIndex++;
        }
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
