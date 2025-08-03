using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance { get; private set; }

    public TimelineProgressUI timelineProgressUI;

    readonly List<LifeLog> completedLives = new();
    readonly List<GhostController> ghosts = new();

    GameObject playerGO;
    InputRecorder currentRec;
    float lifeStartTime;

    [Header("Prefabs")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject ghostPrefab;

    [Header("Player Spawn Settings")]
    [SerializeField] Transform spawnPoint;

    [Header("Time-Budget")]
    public float totalTimeBudget = 30f;
    [SerializeField] Slider timeBudgetSlider;

    float timeRemaining;
    float lifeStartClock;
    float lastShownValue;
    public static float PersistentBudget;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        TimelineManager.Instance.OnTimelineLoop += HandleLoop;
    }

    void Start()
    {
        if (PersistentBudget <= 0f)
            timeRemaining = totalTimeBudget;
        else
            timeRemaining = PersistentBudget;
        lastShownValue = totalTimeBudget;
        UpdateSlider(timeRemaining);
        TimelineManager.Instance.SetPaused(true);
    }

    void Update()
    {
        float effectiveRemaining = GetCurrentTimeRemaining();
        if (!Mathf.Approximately(effectiveRemaining, lastShownValue))
        {
            lastShownValue = effectiveRemaining;
            UpdateSlider(effectiveRemaining);
        }

        if (Input.GetKeyDown(KeyCode.K) || (GameManager.Instance.CurrentPlayer != null && effectiveRemaining <= 0f))
            EndCurrentLife();

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (GameManager.Instance.CurrentPlayer != null) return;
            float target = timelineProgressUI.SliderFraction *
                        TimelineManager.Instance.timelineDuration;
            StartNewLife(target);
        }
    }

    void OnDestroy()
    {
        PersistentBudget = timeRemaining;
        if (TimelineManager.Instance != null)
            TimelineManager.Instance.OnTimelineLoop -= HandleLoop;
    }

    public void EndCurrentLife()
    {
        AkSoundEngine.PostEvent("Play_Die", gameObject);
        Debug.Log("Ending current life...");
        if (currentRec == null) return;
        Debug.Log($"LifeManager: Ending life at t={TimelineManager.Instance.GetCurrentTime():0.00}s");

        float lifeEndClock = TimelineManager.Instance.GetCurrentTime();
        float lifeDuration = Mathf.Abs(lifeEndClock - lifeStartClock);
        Debug.Log($"Life duration: {lifeDuration:0.00}s, remaining time: {timeRemaining:0.00}s, total budget: {totalTimeBudget:0.00}s");

        timeRemaining = Mathf.Max(0f, timeRemaining - lifeDuration);
        UpdateSlider(timeRemaining);

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

    public void EndCurrentLifePostLoop()
    {
        Debug.Log("Ending current life...");
        if (currentRec == null) return;
        Debug.Log($"LifeManager: Ending life at t={TimelineManager.Instance.GetCurrentTime():0.00}s");

        float lifeEndClock = TimelineManager.Instance.GetCurrentTime() + TimelineManager.Instance.timelineDuration;
        float lifeDuration = Mathf.Abs(lifeEndClock - lifeStartClock);
        Debug.Log($"Life duration: {lifeDuration:0.00}s, remaining time: {timeRemaining:0.00}s, total budget: {totalTimeBudget:0.00}s");

        timeRemaining = Mathf.Max(0f, timeRemaining - lifeDuration);
        UpdateSlider(timeRemaining);

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
        if (timeRemaining <= 0f) return;

        lifeStartClock = TimelineManager.Instance.GetCurrentTime();

        playerGO = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        var anim = playerGO.GetComponent<Animator>();
        var col = playerGO.GetComponent<Collider2D>();
        col.isTrigger = false;
        var sr = playerGO.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        anim.Rebind();
        anim.Update(0f);
        playerGO.SetActive(true);
        
        AkSoundEngine.PostEvent("Play_Spawn", gameObject);

        GameManager.Instance.RegisterPlayer(playerGO);

        currentRec = playerGO.GetComponent<InputRecorder>();
        currentRec.StartRecording();
        lifeStartTime = spawnTime;
        TimelineManager.Instance.SetPaused(false);
    }


    GhostController SpawnGhost(LifeLog log)
    {
        var go = Instantiate(ghostPrefab, log.spawnPos, Quaternion.identity);
        
        // AkSoundEngine.PostEvent("Play_Spawn", gameObject);
        
        var gc = go.GetComponent<GhostController>() ?? go.AddComponent<GhostController>();
        gc.Initialize(log.frames, log.startTime, log.endTime);
        return gc;
    }

    public float GetTimelineDuration() => TimelineManager.Instance.timelineDuration;
    public IReadOnlyList<LifeLog> Lives => completedLives;

    void HandleLoop()
    {
        if (currentRec != null) EndCurrentLifePostLoop();
    }

    float GetCurrentTimeRemaining()
    {
        if (currentRec == null) return timeRemaining;

        float elapsed = Mathf.Abs(
            TimelineManager.Instance.GetCurrentTime() - lifeStartClock);

        return Mathf.Max(0f, timeRemaining - elapsed);
    }

    void UpdateSlider(float value)
    {
        if (!timeBudgetSlider) return;

        timeBudgetSlider.maxValue = totalTimeBudget;
        timeBudgetSlider.SetValueWithoutNotify(value);
    }
    
    public void FullReset()
    {
        foreach (var g in ghosts)
            if (g != null)
                Destroy(g.gameObject);
        ghosts.Clear();

        completedLives.Clear();
        timeRemaining = totalTimeBudget;
        lastShownValue = totalTimeBudget;
        UpdateSlider(timeRemaining);

        if (playerGO != null)
            Destroy(playerGO);
        playerGO = null;
        currentRec  = null;

        TimelineManager.Instance.SetPaused(true);
    }
}
