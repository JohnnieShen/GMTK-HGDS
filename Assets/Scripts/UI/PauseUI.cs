using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject pausePanel;

    bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        TimelineManager.Instance.SetPaused(true);
    }

    public void GoToMainMenu()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(0);
    }

    public void GoToLevelSelect()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(1);
    }
}
