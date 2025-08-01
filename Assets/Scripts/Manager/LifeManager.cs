using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance { get; private set; }

    public TimelineProgressUI timelineProgressUI;

    readonly List<LifeLog> completedLives = new();
    readonly List<GhostController> ghosts      = new();

    GameObject          playerGO;
    InputRecorder       currentRec;
    float               lifeStartTime;

    [Header("Prefabs")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject ghostPrefab;

    [Header("Player Spawn Settings")]
    [SerializeField] Transform spawnPoint;

    void Awake() => Instance = this;

    void Start()  => StartNewLife(0f);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
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

        var log = new LifeLog
        {
            frames = currentRec.InputHistory,
            spawnPos = currentRec.GetSpawnPosition(),
            startTime = lifeStartTime,
            endTime = currentRec.InputHistory[^1].time
        };
        completedLives.Add(log);

        var ghost = SpawnGhost(log);
        ghosts.Add(ghost);

        GameManager.Instance.UnregisterPlayer(playerGO);
        Destroy(playerGO);
        playerGO = null;
        currentRec = null;
    }

    public void StartNewLife(float spawnTime)
    {
        playerGO = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        var anim = playerGO.GetComponent<Animator>();
        var col = playerGO.GetComponent<Collider2D>();
        col.isTrigger = false;
        var sr = playerGO.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        anim.Rebind();
        anim.Update(0f);
        playerGO.SetActive(true);

        GameManager.Instance.RegisterPlayer(playerGO);

        currentRec = playerGO.GetComponent<InputRecorder>();
        currentRec.StartRecording();
        lifeStartTime = spawnTime;
    }


    GhostController SpawnGhost(LifeLog log)
    {
        var go = Instantiate(ghostPrefab, log.spawnPos, Quaternion.identity);
        var gc = go.GetComponent<GhostController>() ?? go.AddComponent<GhostController>();
        gc.Initialize(log.frames, log.startTime, log.endTime);
        return gc;
    }

    public float GetTimelineDuration() => TimelineManager.Instance.timelineDuration;
    public IReadOnlyList<LifeLog> Lives => completedLives;
}
