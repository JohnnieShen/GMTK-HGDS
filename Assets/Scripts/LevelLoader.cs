using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelLoader : MonoBehaviour
{
    public static event Action<int> OnLevelLoadRequested;
    public ParticleSystem portalBurst;

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

            if (portalBurst != null)
            {
                portalBurst.Play(); 
            }

            int nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
            OnLevelLoadRequested?.Invoke(nextLevel);
        }
    }

}
