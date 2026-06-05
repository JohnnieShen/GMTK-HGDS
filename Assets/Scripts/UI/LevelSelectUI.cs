using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    public void Load1_1()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(2);
    }
    public void Load1_2()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(3);
    }
    public void Load1_3()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(4);
    }
    public void Load1_4()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(5);
    }
    public void Load1_5()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(6);
    }
    public void Load2_1()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(7);
    }
    public void Load2_2()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(8);
    }
    public void Load2_3()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(9);
    }
    public void Load2_4()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(10);
    }
    public void Load3_1()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(11);
    }
    public void Load3_2()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(12);
    }
    public void Load3_3()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(13);
    }
    public void Load4_1()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(14);
    }
    public void Load4_2()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(15);
    }
    public void Load5_1()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(16);
    }
    public void backToMainMenu()
    {
        AkSoundEngine.PostEvent("Play_ClickUI", gameObject);
        SceneManager.LoadScene(0);
    }
}
