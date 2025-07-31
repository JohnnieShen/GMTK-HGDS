using UnityEngine;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager Instance;

    [Header("References")]
    [Tooltip("Assign your Player Prefab here.")]
    public GameObject playerPrefab;
    [Tooltip("Assign your Ghost Prefab here.")]
    public GameObject ghostPrefab;

    [Header("Settings")]
    public KeyCode rewindKey = KeyCode.R;

    [Header("Physics Layers")]
    public int playerLayer = 8;
    public int ghostLayer = 9;

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
        if (Input.GetKeyDown(rewindKey))
        {
            TriggerRewind();
        }
    }

    public void TriggerRewind()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("Rewind triggered, but no active player found to rewind!");
            return;
        }

        InputRecorder recorder = playerObject.GetComponent<InputRecorder>();
        if (recorder == null)
        {
            Debug.LogError("Player object is missing an InputRecorder component!");
            return;
        }

        Debug.Log("*** Rewind key pressed! ***");

        recorder.StopRecording();
        CreateGhost(recorder);

        GameManager.Instance.PrepareRespawn(playerPrefab, recorder.GetSpawnPosition());

        Destroy(playerObject);
    }

    void CreateGhost(InputRecorder sourceRecorder)
    {
        if (sourceRecorder.FrameCount == 0)
        {
            Debug.LogWarning("No input frames recorded, skipping ghost creation.");
            return;
        }

        Vector3 spawnPos = sourceRecorder.GetSpawnPosition();
        GameObject ghostObj = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);
        GhostController ghost = ghostObj.GetComponent<GhostController>();

        if (ghost == null)
        {
            ghost = ghostObj.AddComponent<GhostController>();
        }

        ghost.ghostLayer = ghostLayer;
        ghost.Initialize(sourceRecorder.InputHistory, spawnPos);
        Debug.Log("Ghost initialized and ready to replay.");
    }
}
