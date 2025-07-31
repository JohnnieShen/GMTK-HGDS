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
    }
    
    void ReplayInput()
    {
        if (inputFrames == null || replayIndex >= inputFrames.Count)
        {
            // Replay finished, destroy the ghost
            Destroy(gameObject);
            return;
        }
        
        PlayerInputFrame frame = inputFrames[replayIndex];
        
        // Check if it's time to execute this frame
        if (timeElapsed >= frame.time)
        {
            movement.Move(frame.horizontal);
            if (frame.jump)
            {
                movement.Jump();
            }
            
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
