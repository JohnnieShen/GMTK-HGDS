using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreenUI : MonoBehaviour
{
    int nextSceneIndex = 1;
    public void LoadGame()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
