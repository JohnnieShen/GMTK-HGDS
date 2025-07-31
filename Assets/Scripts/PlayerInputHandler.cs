using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(InputRecorder))]
public class PlayerInputHandler : MonoBehaviour
{
    MovementController movement;
    InputRecorder      recorder;

    private bool jumpDownBuffered = false;

    void Awake()
    {
        movement = GetComponent<MovementController>();
        recorder = GetComponent<InputRecorder>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpDownBuffered = true;
        }
    }

    void FixedUpdate()
    {
        if (!recorder.IsRecording) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        bool jumpHeld = Input.GetKey(KeyCode.Space);

        movement.Move(horizontal);
        movement.Jump(jumpDownBuffered, jumpHeld);

        recorder.RecordInput(horizontal, jumpHeld);
        
        jumpDownBuffered = false;
    }
}