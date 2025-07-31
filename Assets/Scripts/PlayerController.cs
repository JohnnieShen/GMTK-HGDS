using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public bool isGhost = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private List<PlayerInputFrame> inputHistory = new();
    private float timeElapsed = 0f;
    private Vector3 spawnPosition;

    private int replayIndex = 0;
    private bool isReplaying = false;
    private bool isRecording = true;

    [Header("Rewind")]
    public GameObject playerPrefab;
    
    [Header("Physics Layers")]
    public int playerLayer = 8;  // Layer for live player
    public int ghostLayer = 9;   // Layer for ghosts

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        // Record spawn position for live players
        if (!isGhost)
        {
            spawnPosition = transform.position;
            // Set live player to player layer
            gameObject.layer = playerLayer;
        }
        else
        {
            // Set ghost to ghost layer
            gameObject.layer = ghostLayer;
        }
        
        // Ghosts should start replaying immediately
        if (isGhost)
        {
            isReplaying = true;
            isRecording = false;
        }
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (!isGhost)
        {
            if (isRecording)
            {
                HandleInput();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Rewind();
            }
        }
        else
        {
            Replay();
        }
    }

    void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        bool jump = Input.GetKeyDown(KeyCode.Space);

        // Move
        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);
        if (jump && Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Record
        inputHistory.Add(new PlayerInputFrame(timeElapsed, h, jump));
    }

    void Replay()
    {
        if (replayIndex >= inputHistory.Count)
        {
            // Replay finished, destroy the ghost
            Destroy(gameObject);
            return;
        }

        PlayerInputFrame frame = inputHistory[replayIndex];

        // Check if it's time to execute this frame
        if (timeElapsed >= frame.time)
        {
            float h = frame.horizontal;
            bool jump = frame.jump;

            rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);
            if (jump && Mathf.Abs(rb.linearVelocity.y) < 0.05f)
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            replayIndex++;
        }
    }

    void Rewind()
    {
        // Stop recording for this player
        isRecording = false;

        // Spawn a ghost at the original spawn position
        GameObject ghost = Instantiate(gameObject, spawnPosition, Quaternion.identity);
        Destroy(ghost.GetComponent<PlayerController>()); // remove original controller
        PlayerController ghostController = ghost.AddComponent<PlayerController>();
        ghostController.isGhost = true;
        ghostController.inputHistory = new List<PlayerInputFrame>(inputHistory);
        ghostController.moveSpeed = moveSpeed;
        ghostController.jumpForce = jumpForce;
        ghostController.playerPrefab = playerPrefab;
        ghostController.spawnPosition = spawnPosition; // Pass the spawn position to ghost
        ghostController.playerLayer = playerLayer; // Pass layer settings
        ghostController.ghostLayer = ghostLayer;
        
        // Make ghost slightly transparent to distinguish it
        ghostController.sr = ghost.GetComponent<SpriteRenderer>();
        Color ghostColor = ghostController.sr.color;
        ghostColor.a = 0.7f;
        ghostController.sr.color = ghostColor;

        // Reset this player to start fresh at spawn position
        transform.position = spawnPosition;
        rb.linearVelocity = Vector2.zero;
        timeElapsed = 0f;
        inputHistory.Clear();
        
        // Start recording again
        isRecording = true;
        
        // Configure physics layers to prevent collision between player and ghosts
        ConfigurePhysicsLayers();
    }
    
    void ConfigurePhysicsLayers()
    {
        // Disable collision between player layer and ghost layer
        Physics2D.IgnoreLayerCollision(playerLayer, ghostLayer, true);
        // Ghosts can still collide with each other and the environment
        // Player can still collide with the environment
    }
}

[System.Serializable]
public class PlayerInputFrame
{
    public float time;
    public float horizontal;
    public bool jump;

    public PlayerInputFrame(float time, float horizontal, bool jump)
    {
        this.time = time;
        this.horizontal = horizontal;
        this.jump = jump;
    }
}
