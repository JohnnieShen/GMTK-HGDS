using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    

    [Header("Time Travel Settings")]
    public float selectedSpawnTime = 5f; // Player can set this to control when in the timeline to spawn

    public float timelineDuration = 10f; // Total timeline length in seconds
    
    private GameObject playerToRespawn;
    private Vector3 respawnPosition;
    private bool waitingToRespawn = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep GameManager persistent
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        float timelineDuration = TimelineManager.Instance.timelineDuration;
        float previousTime = (TimelineManager.Instance.GetCurrentTime() - Time.deltaTime * TimelineManager.Instance.timelineSpeed + timelineDuration) % timelineDuration;

        // Handle timeline wraparound:
        bool crossedSpawnTime =
            previousTime > TimelineManager.Instance.GetCurrentTime()
                ? (selectedSpawnTime >= previousTime || selectedSpawnTime <= TimelineManager.Instance.GetCurrentTime())
                : (selectedSpawnTime >= previousTime && selectedSpawnTime <= TimelineManager.Instance.GetCurrentTime());

        if (crossedSpawnTime)
        {
            RespawnPlayer();
        }

    }
    
    // Called when R is pressed - hide the player and set up for respawn
    public void HidePlayerAndPrepareRespawn(GameObject playerPrefab, Vector3 position)
    {
        playerToRespawn = playerPrefab;
        respawnPosition = position;
        waitingToRespawn = true;
        
        Debug.Log($"Player hidden. Will respawn when timeline reaches: {selectedSpawnTime}");
    }
    
    // Respawn the player when timeline reaches the selected time
    private void RespawnPlayer()
    {
        if (playerToRespawn != null)
        {
            float currentTime = TimelineManager.Instance.GetCurrentTime();
            Debug.Log($"Respawning player! Timeline time: {currentTime}, Target: {selectedSpawnTime}");
            
            GameObject newPlayer = Instantiate(playerToRespawn, respawnPosition, Quaternion.identity);
            waitingToRespawn = false;
            playerToRespawn = null;
        }
    }
}
