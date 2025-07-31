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
    
    void Start()
    {

        gameObject.name = playerName;
    }
    
    // Expose common functionality for external scripts
    public MovementController Movement => GetComponent<MovementController>();
    public InputRecorder InputRecorder => GetComponent<InputRecorder>();
    public TimeRewindManager TimeRewind => GetComponent<TimeRewindManager>();
    public PlayerInputHandler InputHandler => GetComponent<PlayerInputHandler>();
}
