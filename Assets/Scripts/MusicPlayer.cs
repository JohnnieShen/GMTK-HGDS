using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    [Header("Advanced")]
    public SceneMusicDatabase sceneMusicDatabase;
    public string switchGroupName = "BGM";
    public string playEventName = "Play_BGM";

    private string currentScene = "";
    private ChapterMusic? lastSwitchSet = null;
    private bool isMusicPlaying = false;

    private static MusicPlayer instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateMusicForScene();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene();
    }

    void UpdateMusicForScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == currentScene)
            return;

        currentScene = sceneName;
        ChapterMusic? targetSwitch = LookupMusicSwitchForScene(sceneName);

        if (targetSwitch.HasValue && targetSwitch != lastSwitchSet)
        {
            AkSoundEngine.SetSwitch(switchGroupName, targetSwitch.ToString(), gameObject);
            Debug.Log($"[MusicPlayer] Set BGM switch to {targetSwitch} for scene {sceneName}");
            lastSwitchSet = targetSwitch;

            if (!isMusicPlaying)
            {
                AkSoundEngine.PostEvent(playEventName, gameObject);
                isMusicPlaying = true;
            }
        }
    }

    ChapterMusic? LookupMusicSwitchForScene(string sceneName)
    {
        foreach (var pair in sceneMusicDatabase.sceneMusicPairs)
        {
            if (pair.sceneName == sceneName)
                return pair.musicSwitch;
        }

        Debug.LogWarning($"[MusicPlayer] No music switch defined for scene: {sceneName}");
        return null;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
