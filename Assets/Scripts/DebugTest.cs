using UnityEngine;

public class DebugTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("DebugTest: Game started!");
        
        // Check for PlayerController components in scene
        PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Debug.Log($"Found {controllers.Length} PlayerController(s) in scene");
        for (int i = 0; i < controllers.Length; i++)
        {
            Debug.Log($"PlayerController {i}: {controllers[i].gameObject.name}");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("DebugTest: R key was pressed!");
        }
        
        if (Input.anyKeyDown)
        {
            Debug.Log($"DebugTest: Some key was pressed!");
        }
    }
}
