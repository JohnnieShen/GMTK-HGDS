using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(InputRecorder))]
public class PlayerInputHandler : MonoBehaviour
{
    private MovementController movement;
    private InputRecorder inputRecorder;
    
    void Awake()
    {
        movement = GetComponent<MovementController>();
        inputRecorder = GetComponent<InputRecorder>();
    }
    
    void Update()
    {
        if (!inputRecorder.IsRecording) return;
        
        HandleMovementInput();
    }
    
    void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        bool jump = Input.GetKeyDown(KeyCode.Space);
        
        // Apply movement
        movement.Move(horizontal);
        if (jump)
        {
            movement.Jump();
        }
        
        // Record the input
        inputRecorder.RecordInput(horizontal, jump);
    }
}
