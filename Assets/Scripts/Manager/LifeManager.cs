using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance { get; private set; }

    public TimelineProgressUI timelineProgressUI;

    /*  runtime lists  */
    readonly List<LifeLog> completedLives = new();     // logs only
    readonly List<GhostController> ghosts      = new();     // live ghost objects

    /*  current live player  */
    GameObject          playerGO;
    InputRecorder       currentRec;
    float               lifeStartTime;

    [Header("Prefabs")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject ghostPrefab;

    /* ───────────────  MONO  ─────────────── */
    void Awake() => Instance = this;

    void Start()  => StartNewLife(0f);                      // game begins at t=0

    /* ───────────────  LIFE CYCLE  ─────────────── */

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))          // K → finish this life
            EndCurrentLife();

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (GameManager.Instance.CurrentPlayer != null) return;
            float target = timelineProgressUI.SliderFraction *
                        TimelineManager.Instance.timelineDuration;
            StartNewLife(target);
        }
    }

    public void EndCurrentLife()
    {
        Debug.Log("Ending current life...");
        if (currentRec == null) return;
        Debug.Log($"LifeManager: Ending life at t={TimelineManager.Instance.GetCurrentTime():0.00}s");
        currentRec.StopRecording();

        // 1. store the log
        var log = new LifeLog
        {
            frames = currentRec.InputHistory,
            spawnPos = currentRec.GetSpawnPosition(),
            startTime = lifeStartTime,
            endTime = currentRec.InputHistory[^1].time
        };
        completedLives.Add(log);

        // 2. create a ghost that replays it
        var ghost = SpawnGhost(log);
        ghosts.Add(ghost);

        // 3. remove the player object
        GameManager.Instance.UnregisterPlayer(playerGO);
        Destroy(playerGO);
        playerGO = null;
        currentRec = null;
    }

    public void StartNewLife(float spawnTime)
    {
        playerGO  = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerGO.SetActive(true);

        //  NEW: tell GameManager a player now exists
        GameManager.Instance.RegisterPlayer(playerGO);

        currentRec = playerGO.GetComponent<InputRecorder>();
        currentRec.StartRecording();
        lifeStartTime = spawnTime;
    }

    /* ───────────────  HELPERS  ─────────────── */

    GhostController SpawnGhost(LifeLog log)
    {
        var go = Instantiate(ghostPrefab, log.spawnPos, Quaternion.identity);
        var gc = go.GetComponent<GhostController>() ?? go.AddComponent<GhostController>();
        gc.Initialize(log.frames, log.startTime, log.endTime);     // overload below
        return gc;
    }

    /* expose for UI */
    public float GetTimelineDuration() => TimelineManager.Instance.timelineDuration;
    public IReadOnlyList<LifeLog> Lives => completedLives;
}
