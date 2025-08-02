using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class AudioTest : MonoBehaviour
{
    public string sceneToLoad;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
