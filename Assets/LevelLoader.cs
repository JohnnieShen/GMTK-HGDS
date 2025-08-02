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
        DontDestroyOnLoad(gameObject);
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
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadNextLevel()
    {
        if (loading) return;
        loading = true;
        StartCoroutine(TransitionAndLoad(
            SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator TransitionAndLoad(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
        // don’t try to reset here
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: resetting LifeManager");
        LifeManager.Instance?.FullReset();
        loading = false;
    }
}
