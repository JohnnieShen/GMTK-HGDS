using UnityEngine;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(InputRecorder))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(TimeRewindManager))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Header("Player Info")]
    public string playerName = "Player";
    bool isGhost = false; // Set to true if this is a ghost player

    void Start()
    {
        gameObject.name = playerName;
        Debug.Log($"Player: '{playerName}' started with modular component system");
    }

    // Expose common functionality for external scripts
    public MovementController Movement => GetComponent<MovementController>();
    public InputRecorder InputRecorder => GetComponent<InputRecorder>();
    public TimeRewindManager TimeRewind => GetComponent<TimeRewindManager>();
    public PlayerInputHandler InputHandler => GetComponent<PlayerInputHandler>();


}
