using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelLoader : MonoBehaviour
{
    public float transitionTime = 1f;

    private Animator transition;
    private bool loading = false;

    public void Start()
    {
        transition = GetComponent<Animator>();
        
        // Make sure the collider is set as a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public bool isLoading()
    {
        return loading;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !loading)
        {
            Debug.Log("Player hit level transition trigger");
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        loading = true;
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        // Start animation
        transition.SetTrigger("Start");
        Debug.Log("Trigger set");

        // Wait for animation
        yield return new WaitForSeconds(transitionTime);   

        // Load next scene
        SceneManager.LoadScene(levelIndex);
    }
}
