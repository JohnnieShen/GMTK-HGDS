using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    class GhostWindow
    {
        public GhostController gc;
        public float start;
        public float end;
        public bool active;
    }

    public static GameManager Instance;

    public GameObject CurrentPlayer { get; private set; }

    [Header("Time Travel Settings")]
    public float selectedSpawnTime = 5f;

    public float timelineDuration = 10f;
    [Header("Pause UI")]
    [SerializeField] GameObject pausePanel;

    private GameObject playerToRespawn;
    private Vector3 respawnPosition;
    private bool waitingToRespawn = false;

    struct BodyState
    {
        public Rigidbody2D rb;
        public Vector2 vel;
        public float angVel;
    }

    readonly List<GhostWindow> ghosts = new();

    readonly List<BodyState> frozenBodies = new();

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
        TriggerNotifier.PlayerTriggerEvent += HandlePlayerTrigger;
    }

    void OnDestroy()
    {
        TriggerNotifier.PlayerTriggerEvent -= HandlePlayerTrigger;
    }

    void Update()
    {
        foreach (var g in ghosts)
            UpdateOneGhost(g);

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

    public void RegisterPlayer(GameObject p) => CurrentPlayer = p;
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
                rb = rb,
                vel = rb.linearVelocity,
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

    public void RegisterGhost(GhostController gc, float start, float end)
    {
        Debug.Log($"Registering ghost from {start:0.00} to {end:0.00}");
        ghosts.Add(new GhostWindow { gc = gc, start = start, end = end });
        UpdateOneGhost(ghosts[^1]);
    }

    public void UnregisterGhost(GhostController gc)
    {
        ghosts.RemoveAll(g => g.gc == gc);
    }

    void UpdateOneGhost(GhostWindow g)
    {
        float t = TimelineManager.Instance.GetCurrentTime();
        float len = TimelineManager.Instance.timelineDuration;

        bool inside = (g.start <= g.end)
                    ? (t >= g.start && t <= g.end)
                    : (t >= g.start || t <= g.end);

        bool isEnabled = g.gc.gameObject.activeSelf;

        if (inside && !isEnabled)
        {
            g.gc.gameObject.SetActive(true);
            g.gc.Seek(t);
            g.active = true;
        }
        else if (!inside && isEnabled)
        {
            g.gc.gameObject.SetActive(false);
            g.active = false;
        }
    }
    
    void HandlePlayerTrigger(bool entered)
    {
        Debug.Log($"Player trigger event: {(entered ? "Entered" : "Exited")}");
        Time.timeScale = entered ? 0f : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(entered);
    }
}
