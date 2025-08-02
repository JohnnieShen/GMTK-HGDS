using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

    public Animator transition;
    public float transitionTime = 1f;

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        // Start animation
        transition.SetTrigger("Start");

        // Wait for animation
        yield return new WaitForSeconds(transitionTime);   

        // Load next scene
        SceneManager.LoadScene(levelIndex);
    }
}
