using UnityEngine;
using System.Collections;

public class Button : Interactable, RecordableProp
{
    [Header("Target")]
    public GameObject targetObject;
    public Sprite targetActiveSprite;
    public Sprite targetInactiveSprite;

    [Header("Timing")]
    public float delayOn = 0f;
    public float delayOff = 1f;
    public bool defaultPressed = false; // Whether target object's default state is on or off

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;
    public Sprite activatingSprite;

    private bool playerInRange = false;
    private bool isProcessing = false;
    bool isPressed = false; // Button visual state - always starts not pressed
    bool targetActive; // Current state of the target object
    PropRecorder recorder;

    void Start()
    {
        isPressed = false; // Button always starts visually not pressed
        targetActive = defaultPressed; // Target starts in its default state

        ApplyVisuals();
    }

    public override void Interact()
    {
        Debug.Log("Button pressed by interaction");
        if (!isProcessing)
            StartCoroutine(PressSequence());
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.T) && !isProcessing)
            StartCoroutine(PressSequence());
    }

    IEnumerator WaitForTimelineSeconds(float timelineSecs)
    {
        float remaining = timelineSecs;
        float prev = TimelineManager.Instance.GetCurrentTime();
        float duration = TimelineManager.Instance.timelineDuration;

        while (remaining > 0f)
        {
            yield return null;

            float now = TimelineManager.Instance.GetCurrentTime();
            float speed = TimelineManager.Instance.timelineSpeed;

            float delta = speed >= 0f
                ? (now - prev + duration) % duration
                : (prev - now + duration) % duration;

            remaining -= delta;
            prev = now;
        }
    }


    IEnumerator PressSequence()
    {
        isProcessing = true;

        // Show activating sprite
        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return StartCoroutine(WaitForTimelineSeconds(delayOn));

        // Toggle to non-default state
        isPressed = true; // Button becomes visually pressed
        targetActive = !defaultPressed; // Target goes to opposite of default
        ApplyVisuals();

        yield return StartCoroutine(WaitForTimelineSeconds(delayOff));

        // Return to default state
        isPressed = false; // Button returns to not pressed
        targetActive = defaultPressed; // Target returns to default
        ApplyVisuals();

        isProcessing = false;
    }

    void ApplyVisuals()
    {
        // Change target object sprite and collider state
        if (targetObject)
        {
            SpriteRenderer targetSpriteRenderer = targetObject.GetComponent<SpriteRenderer>();
            Collider2D targetCollider = targetObject.GetComponent<Collider2D>();
            
            if (targetSpriteRenderer)
            {
                targetSpriteRenderer.sprite = targetActive ? targetActiveSprite : targetInactiveSprite;
            }
            
            if (targetCollider)
            {
                targetCollider.enabled = targetActive;
            }
        }

        // Change button sprite
        if (spriteRenderer)
            spriteRenderer.sprite = isPressed ? pressedSprite : idleSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public PropStatusFrame CaptureFrame()
    {
        return new PropStatusFrame(gameObject.GetInstanceID(), TimelineManager.Instance.GetCurrentTime(), targetActive, transform.position);
    }

    public void ApplyFrame(PropStatusFrame frame)
    {
        targetActive = frame.active;
        transform.position = frame.position;
        isPressed = targetActive != defaultPressed;
        ApplyVisuals();
    }
}
