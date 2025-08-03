using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreenUI : MonoBehaviour
{
    int nextSceneIndex = 1;
    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
