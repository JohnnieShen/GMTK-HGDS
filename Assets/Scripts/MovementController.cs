using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Horizontal")]
    [SerializeField] float moveSpeed      = 10f;
    [SerializeField] float accelTime      = 0.10f;
    [SerializeField] float airAccelTime   = 0.20f;

    [Header("Jump & Gravity")]
    [SerializeField] float jumpHeight     = 4f;
    [SerializeField] float timeToApex     = 0.4f;
    [SerializeField] float upGravityMult  = 1f;
    [SerializeField] float downGravityMult= 2f;

    [Header("Forgiveness")]
    [SerializeField] float coyoteTime     = 0.10f;
    [SerializeField] float jumpBuffer     = 0.10f;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundMask;

    [SerializeField] LayerMask playerMask;
    [SerializeField] LayerMask ghostMask;
    [SerializeField] Vector2  groundCheckSize = new(0.8f, 0.1f);
    [SerializeField] Vector3  groundCheckOffset = new(0f, -0.51f, 0f);

    [Header("Audio Events")]
    [SerializeField] float landEventCooldown = 0.15f;

    Rigidbody2D rb;
    Vector2 velocitySmooth;
    float  gravity;
    float  jumpVel;

    float  horizontalInput;
    bool   jumpHeld;

    float  timeSinceLeftGround;
    float  timeSinceJumpPressed;
    
    private bool isRollingSoundPlaying = false;
    private bool wasGroundedLastFrame = true;
    bool jumpPerformedSinceLastConsume;
    bool landedSinceLastConsume;
    float lastLandEventTime = -999f;
    SpectralImprintSource spectralImprint;


    public Vector2 CurrentVelocity => rb.linearVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spectralImprint = GetComponent<SpectralImprintSource>();
        rb.gravityScale = 0f;

        gravity = -(2f * jumpHeight) / (timeToApex * timeToApex);
        jumpVel = Mathf.Abs(gravity) * timeToApex;

        timeSinceJumpPressed = jumpBuffer + 1f;
        timeSinceLeftGround  = coyoteTime + 1f;
    }

    void Start()
    {
        wasGroundedLastFrame = IsGrounded();
    }

    void Update()
    {
        timeSinceJumpPressed += Time.deltaTime;
        if (IsGrounded())
        {
            timeSinceLeftGround = 0;
        }
        else
        {
            timeSinceLeftGround += Time.deltaTime;
        }

        if (timeSinceJumpPressed <= jumpBuffer &&
            timeSinceLeftGround <= coyoteTime)
        {
            PerformJump();
        }
        
        HandleRollingSFX(); 
        HandleJumpLandSFX();
    }

    void FixedUpdate()
    {
        float targetX = horizontalInput * moveSpeed;
        float accel   = IsGrounded() ? 1f / accelTime : 1f / airAccelTime;

        float newVelX = Mathf.SmoothDamp(rb.linearVelocity.x,
                                         targetX,
                                         ref velocitySmooth.x,
                                         1f / accel,
                                         Mathf.Infinity,
                                         Time.fixedDeltaTime);

        float g = (rb.linearVelocity.y > 0f ? upGravityMult : downGravityMult) * gravity;
        if (rb.linearVelocity.y > 0f && !jumpHeld) g *= 1.5f;

        float newVelY = rb.linearVelocity.y + g * Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(newVelX, newVelY);
    }

    public void Move(float horizontal) => horizontalInput = horizontal;

    public void Jump(bool jumpDown, bool jumpHeld)
    {
        if (jumpDown) timeSinceJumpPressed = 0f;
        this.jumpHeld = jumpHeld;
    }

    public void ResetVelocity() => rb.linearVelocity = Vector2.zero;

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        rb.linearVelocity  = Vector2.zero;
    }

    public bool IsGrounded() =>
        Physics2D.OverlapBox(transform.position + groundCheckOffset,
                             groundCheckSize, 0f, groundMask | playerMask | ghostMask);

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);
        timeSinceJumpPressed = jumpBuffer + 1f;
        jumpPerformedSinceLastConsume = true;
        GetSpectralImprint()?.PlayJump();
        WwiseAudioGate.PostCharacterLifetimeEvent("Play_Jumping", gameObject);
    }
    
    void HandleRollingSFX()
    {
        var ghost = GetComponent<GhostController>();
        if (ghost != null && ghost.enabled)
            return;

        bool isMovingHorizontally = Mathf.Abs(rb.linearVelocity.x) > 0.05f;
        GetSpectralImprint()?.SetRolling(isMovingHorizontally);

        if (isMovingHorizontally && !isRollingSoundPlaying)
        {
            WwiseAudioGate.PostCharacterLifetimeEvent("Play_Rolling", gameObject);
            isRollingSoundPlaying = true;
        }
        else if (!isMovingHorizontally && isRollingSoundPlaying)
        {
            WwiseAudioGate.PostCharacterLifetimeEvent("Stop_Rolling", gameObject);
            isRollingSoundPlaying = false;
        }
    }


    
    void OnDestroy()
    {
        var ghost = GetComponent<GhostController>();
        GetSpectralImprint()?.SetRolling(false);
        if (ghost != null && ghost.enabled)
            return;

        if (isRollingSoundPlaying)
        {
            WwiseAudioGate.PostCharacterLifetimeEvent("Stop_Rolling", gameObject);
            isRollingSoundPlaying = false;
        }
    }

    
    void OnDisable()
    {
        var ghost = GetComponent<GhostController>();
        GetSpectralImprint()?.SetRolling(false);
        if (ghost != null && ghost.enabled)
            return;

        if (isRollingSoundPlaying)
        {
            WwiseAudioGate.PostCharacterLifetimeEvent("Stop_Rolling", gameObject);
            isRollingSoundPlaying = false;
        }
    }
    
    void HandleJumpLandSFX()
    {
        var ghost = GetComponent<GhostController>();
        if (ghost != null && ghost.enabled)
            return;

        bool isGroundedNow = IsGrounded();

        if (isGroundedNow && !wasGroundedLastFrame && Time.time >= lastLandEventTime + landEventCooldown)
        {
            landedSinceLastConsume = true;
            lastLandEventTime = Time.time;
            GetSpectralImprint()?.PlayLand();
            WwiseAudioGate.PostCharacterLifetimeEvent("Play_Landing", gameObject);
        }

        wasGroundedLastFrame = isGroundedNow;
    }

    public bool ConsumeJumpPerformed()
    {
        bool value = jumpPerformedSinceLastConsume;
        jumpPerformedSinceLastConsume = false;
        return value;
    }

    public bool ConsumeLanded()
    {
        bool value = landedSinceLastConsume;
        landedSinceLastConsume = false;
        return value;
    }

    SpectralImprintSource GetSpectralImprint()
    {
        if (spectralImprint == null)
            spectralImprint = GetComponent<SpectralImprintSource>();

        return spectralImprint;
    }




#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + groundCheckOffset,
                            new Vector3(groundCheckSize.x, groundCheckSize.y, 0));
    }
#endif
}
