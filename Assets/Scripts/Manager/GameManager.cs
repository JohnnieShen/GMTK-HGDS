using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject CurrentPlayer { get; private set; }

    [Header("Time Travel Settings")]
    public float selectedSpawnTime = 5f;

    public float timelineDuration = 10f;

    private GameObject playerToRespawn;
    private Vector3 respawnPosition;
    private bool waitingToRespawn = false;

    struct BodyState
    {
        public Rigidbody2D rb;
        public Vector2 vel;
        public float angVel;
    }

    readonly List<BodyState> frozenBodies = new ();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!waitingToRespawn)
        {
            return;
        }

        float currentTime = TimelineManager.Instance.GetCurrentTime();
        float timelineDuration = TimelineManager.Instance.timelineDuration;
        float previousTime = (currentTime - Time.deltaTime * TimelineManager.Instance.timelineSpeed + timelineDuration) % timelineDuration;

        bool crossedSpawnTime =
            previousTime > currentTime
                ? (selectedSpawnTime >= previousTime || selectedSpawnTime <= currentTime)
                : (selectedSpawnTime >= previousTime && selectedSpawnTime <= currentTime);

        if (crossedSpawnTime)
        {
            RespawnPlayer();
        }

    }

    public void RegisterPlayer(GameObject p)  => CurrentPlayer = p;
    public void UnregisterPlayer(GameObject p)
    {
        if (CurrentPlayer == p) CurrentPlayer = null;
    }

    public void HandlePhysicsPause(bool pause)
    {
        if (pause) FreezeAll();
        else UnfreezeAll();
    }

    public void PrepareRespawn(GameObject playerPrefab, Vector3 position)
    {
        playerToRespawn = playerPrefab;
        respawnPosition = position;
        waitingToRespawn = true;

        Debug.Log($"Player hidden. Will respawn when timeline reaches: {selectedSpawnTime}");
    }

    private void RespawnPlayer()
    {
        if (playerToRespawn != null)
        {
            float currentTime = TimelineManager.Instance.GetCurrentTime();
            Debug.Log($"Respawning player! Timeline time: {currentTime}, Target: {selectedSpawnTime}");

            GameObject newPlayer = Instantiate(playerToRespawn, respawnPosition, Quaternion.identity);

            InputRecorder newRecorder = newPlayer.GetComponent<InputRecorder>();
            if (newRecorder != null)
            {
                newRecorder.StartRecording();
                Debug.Log("Input recording started for new player.");
            }
            else
            {
                Debug.LogError("Newly spawned player is missing an InputRecorder!");
            }

            waitingToRespawn = false;
            playerToRespawn = null;
        }
    }
    
    void FreezeAll()
    {
        frozenBodies.Clear();
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            frozenBodies.Add(new BodyState
            {
                rb     = rb,
                vel    = rb.linearVelocity,
                angVel = rb.angularVelocity
            });

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }
    }

    void UnfreezeAll()
    {
        foreach (var b in frozenBodies)
        {
            if (b.rb == null) continue;
            b.rb.simulated = true;
            b.rb.linearVelocity = b.vel;
            b.rb.angularVelocity = b.angVel;
        }
        frozenBodies.Clear();
    }
}
