using UnityEngine;
using System.Collections;

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
        Debug.Log("TimeRewindManager: Started and configured physics layers");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(rewindKey))
        {
            Debug.Log("*** TimeRewindManager: Rewind key pressed! ***");
            TriggerRewind();
        }
    }
    
    public void TriggerRewind()
    {
        Debug.Log("TimeRewindManager: TriggerRewind() called!");
        
        // Stop recording
        inputRecorder.StopRecording();
        Debug.Log("TimeRewindManager: Recording stopped");
        
        // Create ghost with recorded input
        CreateGhost();
        
        // Hide/disable the current player and delay the reset
        StartDelayedReset();
        
        Debug.Log("TimeRewindManager: Delayed reset started");
    }
    
    void CreateGhost()
    {
        if (inputRecorder.FrameCount == 0) 
        {
            Debug.Log("TimeRewindManager: No input frames to replay, skipping ghost creation");
            return;
        }
        
        Debug.Log($"TimeRewindManager: Creating ghost with {inputRecorder.FrameCount} recorded frames");
        
        GameObject ghostObj;
        
        if (ghostPrefab != null)
        {
            ghostObj = Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("TimeRewindManager: Ghost created from prefab");
        }
        else
        {
            // Use current object as template
            ghostObj = Instantiate(gameObject, spawnPosition, Quaternion.identity);
            Debug.Log("TimeRewindManager: Ghost created from current player object");
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
        Debug.Log("TimeRewindManager: Ghost initialized and ready to replay");
    }
    
    void ResetPlayer()
    {
        Debug.Log($"TimeRewindManager: Resetting player to spawn position {spawnPosition}");
        movement.SetPosition(spawnPosition);
        inputRecorder.ClearHistory();
    }
    
    void StartDelayedReset()
    {
        // Get the delay from GameManager
        float delay = GameManager.Instance != null ? GameManager.Instance.selectedSpawnTime : 2f;
        Debug.Log($"TimeRewindManager: Starting delayed reset with {delay} second delay");
        
        // Hide/disable player temporarily
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<PlayerInputHandler>().enabled = false;
        movement.ResetVelocity();
        
        // Start the delayed reset coroutine
        StartCoroutine(DelayedResetCoroutine(delay));
    }
    
    IEnumerator DelayedResetCoroutine(float delay)
    {
        Debug.Log("TimeRewindManager: Waiting for delayed reset...");
        yield return new WaitForSeconds(delay);
        
        Debug.Log("TimeRewindManager: Delay finished, resetting player now!");
        
        // Reset player
        ResetPlayer();
        
        // Re-enable player
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<PlayerInputHandler>().enabled = true;
        
        // Start recording again
        inputRecorder.StartRecording();
        Debug.Log("TimeRewindManager: Recording restarted, player is active again!");
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
