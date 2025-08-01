using UnityEngine;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject ghostPrefab;

    [Header("Keys")]
    public KeyCode rewindKey = KeyCode.R;
    public KeyCode playbackKey = KeyCode.V;

    [Header("Physics Layers")]
    public int playerLayer = 8;
    public int ghostLayer = 9;

    [Header("Ghost")]
    public float playbackStartTime = 1f;

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

    void Start()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, ghostLayer, true);
    }

    void Update()
    {
        if (Input.GetKeyDown(rewindKey)) TriggerRewind();
        if (Input.GetKeyDown(playbackKey)) TriggerPlayback();
    }

    public void TriggerRewind()
    {
        var player = GameObject.FindWithTag("Player");
        if (!TryGetRecorder(player, out var rec)) return;

        rec.StopRecording();
        SpawnGhost(rec, playbackStartTime);

        GameManager.Instance.PrepareRespawn(playerPrefab, rec.GetSpawnPosition());
        Destroy(player);
    }

    public void TriggerPlayback()
    {
        var player = GameObject.FindWithTag("Player");
        if (!TryGetRecorder(player, out var rec)) return;

        SpawnGhost(rec, playbackStartTime);
        TimelineManager.Instance.currentTime = playbackStartTime;
    }

    bool TryGetRecorder(GameObject go, out InputRecorder recorder)
    {
        recorder = null;
        if (go == null)
        {
            Debug.LogWarning("No active player found.");
            return false;
        }

        recorder = go.GetComponent<InputRecorder>();
        if (recorder == null)
        {
            Debug.LogError("Player missing InputRecorder component.");
            return false;
        }

        if (recorder.FrameCount == 0)
        {
            Debug.LogWarning("No input frames recorded; cannot create ghost.");
            return false;
        }

        return true;
    }

    void SpawnGhost(InputRecorder recorder, float startTime)
    {
        Vector3 pos = recorder.GetSpawnPosition();
        var     go  = Instantiate(ghostPrefab, pos, Quaternion.identity);

        go.name = $"Ghost @{startTime:0.00}s ({pos})";
        go.tag = "Ghost";
        go.layer = ghostLayer;

        var ghost = go.GetComponent<GhostController>() ??
                    go.AddComponent<GhostController>();

        ghost.ghostLayer = ghostLayer;
        ghost.Initialize(recorder.InputHistory, startTime);

        Debug.Log($"<color=cyan>[TimeRewind]</color> Spawned ghost at {pos} (t={startTime:0.00}s)");
    }
}
