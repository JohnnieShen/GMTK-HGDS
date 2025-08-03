using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelLoader : MonoBehaviour
{
    public static event Action<int> OnLevelLoadRequested;

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered level loader trigger.");
            int nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
            OnLevelLoadRequested?.Invoke(nextLevel);
        }
    }
}
