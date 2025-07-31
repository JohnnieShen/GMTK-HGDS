using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Time Travel Settings")]
    public float selectedSpawnTime = 5f; // Set manually for now

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
    
    // Method to handle delayed spawn from PlayerController
    public void SpawnPlayerAfterDelay(GameObject prefab, Vector3 position, float delay)
    {
        Debug.Log($"SpawnPlayerAfterDelay called with delay: {delay} seconds");
        StartCoroutine(DelayedPlayerSpawn(prefab, position, delay));
    }
    
    private IEnumerator DelayedPlayerSpawn(GameObject prefab, Vector3 position, float delay)
    {
        Debug.Log($"Starting delayed spawn, waiting {delay} seconds...");
        yield return new WaitForSeconds(delay);
        Debug.Log("Delay finished, spawning new player now!");
        GameObject newPlayer = Instantiate(prefab, position, Quaternion.identity);
    }
}
