using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(InputRecorder))]
public class PlayerInputHandler : MonoBehaviour
{
    MovementController movement;
    InputRecorder      recorder;

    void Awake()
    {
        movement  = GetComponent<MovementController>();
        recorder  = GetComponent<InputRecorder>();
    }

    void Update()
    {
        if (!recorder.IsRecording) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        bool jumpDown = Input.GetKeyDown(KeyCode.Space);
        bool jumpHeld = Input.GetKey(KeyCode.Space);

        movement.Move(horizontal);
        movement.Jump(jumpDown, jumpHeld);

        recorder.RecordInput(horizontal, jumpDown);
    }
}
