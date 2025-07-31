using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
            Debug.Log($"LIVE PLAYER created at position: {transform.position}");
        }
        else
        {
            // Set ghost to ghost layer
            gameObject.layer = ghostLayer;
            Debug.Log($"GHOST created at position: {transform.position}");
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
                Debug.Log("R key pressed! Calling Rewind()");
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
        Debug.Log("Rewind() called!");
        
        // Spawn ghost (past lifetime) at spawn position, not current position
        GameObject ghost = Instantiate(gameObject, spawnPosition, Quaternion.identity);
        Destroy(ghost.GetComponent<PlayerController>());
        PlayerController ghostController = ghost.AddComponent<PlayerController>();
        ghostController.isGhost = true;
        ghostController.inputHistory = new List<PlayerInputFrame>(inputHistory);
        ghostController.moveSpeed = moveSpeed;
        ghostController.jumpForce = jumpForce;
        ghostController.playerPrefab = playerPrefab;

        Debug.Log($"Ghost created. Now requesting delayed spawn with prefab: {playerPrefab?.name}, position: {spawnPosition}, delay: {GameManager.Instance.selectedSpawnTime}");

        // Use GameManager to handle delayed spawn
        if (playerPrefab != null)
        {
            GameManager.Instance.SpawnPlayerAfterDelay(playerPrefab, spawnPosition, GameManager.Instance.selectedSpawnTime);
        }
        else
        {
            Debug.LogError("playerPrefab is null! Cannot spawn new player.");
        }

        Debug.Log("Destroying current player...");
        // Destroy this player
        Destroy(gameObject);
    }


    void ConfigurePhysicsLayers()
    {
        // Disable collision between player layer and ghost layer
        Physics2D.IgnoreLayerCollision(playerLayer, ghostLayer, true);
        // Ghosts can still collide with each other and the environment
        // Player can still collide with the environment
    }
}

