using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderManager : MonoBehaviour
{
    public static LevelLoaderManager Instance { get; private set; }

    [Header("Transition")]
    [SerializeField] private float transitionTime = 0.2f;

    public bool loading;

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
            return;
        }
    }

    void OnEnable()
    {
        LevelLoader.OnLevelLoadRequested += HandleLoadRequest;
        SceneManager.sceneLoaded         += OnSceneLoaded;
    }

    void OnDisable()
    {
        LevelLoader.OnLevelLoadRequested -= HandleLoadRequest;
        SceneManager.sceneLoaded         -= OnSceneLoaded;
    }

    private void HandleLoadRequest(int levelIndex)
    {
        if (loading) return;
        StartCoroutine(TransitionAndLoad(levelIndex));
    }

    IEnumerator TransitionAndLoad(int levelIndex)
    {
        loading = true;

        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LifeManager.Instance?.FullReset();
        loading = false;
    }
}
