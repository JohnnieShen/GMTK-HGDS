using UnityEngine;
using System.Collections;

public class Lever : Interactable, RecordableProp
{
    [Header("Target")]
    public GameObject targetObject;
    public Sprite targetActiveSprite;
    public Sprite targetInactiveSprite;

    [Header("Timing")]
    public float delayOn = 0f;
    public bool defaultPressed = false; // Whether target object's default state is on or off

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;
    public Sprite activatingSprite;

    private bool playerInRange = false;
    private bool isProcessing = false;
    bool isPressed = false; // Lever visual state
    bool targetActive; // Current state of the target object
    PropRecorder recorder;

    void Start()
    {
        isPressed = false; // Lever always starts visually not pressed (off position)
        targetActive = defaultPressed; // Target starts in its default state
        ApplyVisuals();
    }

    void Awake()  => PropManager.Instance?.RegisterGameObject(gameObject);
    void OnDestroy() => PropManager.Instance?.UnregisterGameObject(gameObject);

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isProcessing)
        {
            StartCoroutine(ToggleLever());
        }
    }

    public override void Interact()
    {
        if (!isProcessing)
            StartCoroutine(ToggleLever());
    }

    IEnumerator ToggleLever()
    {
        isProcessing = true;

        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return StartCoroutine(WaitForTimelineSeconds(delayOn));

        // Toggle the lever state permanently
        isPressed = !isPressed; // Toggle lever visual state
        targetActive = isPressed ? !defaultPressed : defaultPressed; // Target matches lever state
        ApplyVisuals();

        isProcessing = false;
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
        return new PropStatusFrame(
            gameObject.GetInstanceID(),
            TimelineManager.Instance.GetCurrentTime(),
            targetActive,
            transform.position
        );
    }

    public void ApplyFrame(PropStatusFrame frame)
    {
        targetActive = frame.active;
        transform.position = frame.position;
        isPressed = targetActive != defaultPressed;
        ApplyVisuals();
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

        // Change lever sprite
        if (spriteRenderer)
            spriteRenderer.sprite = isPressed ? onSprite : offSprite;
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
}
