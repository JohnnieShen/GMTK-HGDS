using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(InputRecorder))]
public class PlayerInputHandler : MonoBehaviour
{
    MovementController movement;
    InputRecorder      recorder;
    SpectralImprintSource spectralImprint;
    private bool jumpDownBuffered     = false;
    private bool interactDownBuffered = false;
    bool wasGroundedLastFrame = true;

    void Awake()
    {
        movement = GetComponent<MovementController>();
        recorder = GetComponent<InputRecorder>();
        spectralImprint = GetComponent<SpectralImprintSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpDownBuffered = true;

        if (Input.GetKeyDown(KeyCode.T))
            interactDownBuffered = true;
    }

    void FixedUpdate()
    {
        if (!recorder.IsRecording)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        bool  jumpHeld   = Input.GetKey(KeyCode.Space);

        bool jumpDown   = jumpDownBuffered;
        bool interact   = interactDownBuffered;
        bool groundedNow = movement.IsGrounded();
        bool landed = groundedNow && !wasGroundedLastFrame;
        wasGroundedLastFrame = groundedNow;

        jumpDownBuffered     = false;
        interactDownBuffered = false;

        int interactPropId = -1;
        if (interact)
        {
            const float radius = 0.5f;
            var hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var c in hits)
            {
                var i = c.GetComponent<Interactable>();
                if (i != null)
                {
                    interactPropId = c.gameObject.GetInstanceID();
                    GetSpectralImprint()?.PlayInteract();
                    break;
                }
            }
        }

        movement.Move(horizontal);
        movement.Jump(jumpDown, jumpHeld);

        Debug.Log($"Recording input: {horizontal}, {jumpHeld}, {interact}, {interactPropId}");
        recorder.RecordInput(horizontal,
                             jumpHeld,
                             jumpDown,
                             landed,
                             interact,
                             interactPropId);
    }

    SpectralImprintSource GetSpectralImprint()
    {
        if (spectralImprint == null)
            spectralImprint = GetComponent<SpectralImprintSource>();

        return spectralImprint;
    }
}
