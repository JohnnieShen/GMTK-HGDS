using UnityEngine;

public class TimeRewindManager : MonoBehaviour
{
    [Header("Rewind Settings")]
    public GameObject ghostPrefab;
    public KeyCode rewindKey = KeyCode.R;
    
    [Header("Physics Layers")]
    public int playerLayer = 8;
    public int ghostLayer = 9;
    
    private Vector3 spawnPosition;
    private MovementController movement;
    private InputRecorder inputRecorder;
    
    void Awake()
    {
        movement = GetComponent<MovementController>();
        inputRecorder = GetComponent<InputRecorder>();
    }
    
    void Start()
    {
        spawnPosition = transform.position;
        gameObject.layer = playerLayer;
        ConfigurePhysicsLayers();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(rewindKey))
        {
            TriggerRewind();
        }
    }
    
    public void TriggerRewind()
    {
        // Stop recording
        inputRecorder.StopRecording();
        
        // Create ghost with recorded input
        CreateGhost();
        
        // Reset player
        ResetPlayer();
        
        // Start recording again
        inputRecorder.StartRecording();
    }
    
    void CreateGhost()
    {
        if (inputRecorder.FrameCount == 0) return;
        
        GameObject ghostObj;
        
        if (ghostPrefab != null)
        {
            ghostObj = Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // Use current object as template
            ghostObj = Instantiate(gameObject, spawnPosition, Quaternion.identity);
            // Remove player-specific components
            Destroy(ghostObj.GetComponent<TimeRewindManager>());
            Destroy(ghostObj.GetComponent<PlayerInputHandler>());
            Destroy(ghostObj.GetComponent<InputRecorder>());
        }
        
        // Add and configure ghost controller
        GhostController ghost = ghostObj.GetComponent<GhostController>();
        if (ghost == null)
        {
            ghost = ghostObj.AddComponent<GhostController>();
        }
        
        ghost.ghostLayer = ghostLayer;
        ghost.Initialize(inputRecorder.InputHistory, spawnPosition);
    }
    
    void ResetPlayer()
    {
        movement.SetPosition(spawnPosition);
        inputRecorder.ClearHistory();
    }
    
    void ConfigurePhysicsLayers()
    {
        // Disable collision between player layer and ghost layer
        Physics2D.IgnoreLayerCollision(playerLayer, ghostLayer, true);
    }
    
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }
}
