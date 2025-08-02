using UnityEngine;
using System.Collections;

public class Lever : Interactable, RecordableProp
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Settings")]
    public float delayOn = 0f;

    [Header("Default")]
    public bool defaultActive = true;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;
    public Sprite activatingSprite;

    private bool playerInRange = false;
    private bool isProcessing = false;
    private bool currentState;
    bool isActive;
    PropRecorder recorder;

    void Start()
    {
        isActive = defaultActive;
        ApplyVisuals();
    }

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

        isActive = !isActive;
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
            isActive,
            transform.position
        );
    }

    public void ApplyFrame(PropStatusFrame frame)
    {
        isActive = frame.active;
        transform.position = frame.position;
        ApplyVisuals();
    }
    void ApplyVisuals()
    {
        if (targetObject) targetObject.SetActive(isActive);

        if (spriteRenderer)
            spriteRenderer.sprite = isActive ? onSprite : offSprite;
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
