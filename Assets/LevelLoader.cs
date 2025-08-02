using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public float transitionTime = 1f;

    private Animator transition;
    private bool loading = false;

    public void Start()
    {
        transition = GetComponent<Animator>();
    }

    public bool isLoading()
    {
        return loading;
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
