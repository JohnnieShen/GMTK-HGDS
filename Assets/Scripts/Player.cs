using UnityEngine;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(InputRecorder))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Header("Player Info")]
    public string playerName = "Player";
    public bool isGhost = false; // Set to true if this is a ghost player

    void Start()
    {
        gameObject.name = playerName;
        Debug.Log($"Player: '{playerName}' started with modular component system");
    }

    // Expose common functionality for external scripts
    public MovementController Movement => GetComponent<MovementController>();
    public InputRecorder InputRecorder => GetComponent<InputRecorder>();
    public PlayerInputHandler InputHandler => GetComponent<PlayerInputHandler>();
}
