using UnityEngine;
using System.Collections;

public class PressurePlateHold : MonoBehaviour, RecordableProp
{
    [Header("Target")]
    public GameObject targetObject;
    public Sprite targetActiveSprite;
    public Sprite targetInactiveSprite;

    [Header("Timing")]
    public float delayOn = 0f;
    public float delayOff = 0f;
    public bool defaultPressed = false; // Whether target object's default state is on or off

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;
    public Sprite activatingSprite; // optional

    private bool isSomethingOnPlate = false;
    private bool isPressed = false; // Pressure plate visual state
    private bool targetActive; // Current state of the target object
    private Coroutine delayCo;

    void Start()
    {
        isPressed = false; // Pressure plate always starts visually not pressed
        targetActive = defaultPressed; // Target starts in its default state
        ApplyVisuals();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;
        isSomethingOnPlate = true;
        ActivatePlate();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;
        
        // Check if there are still valid objects on the plate
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, GetComponent<Collider2D>().bounds.size, 0f);
        bool stillHasValidObject = false;
        
        foreach (Collider2D col in colliders)
        {
            if (col != GetComponent<Collider2D>() && IsValidObject(col))
            {
                stillHasValidObject = true;
                break;
            }
        }
        
        if (!stillHasValidObject)
        {
            isSomethingOnPlate = false;
            DeactivatePlate();
        }
    }

    void ActivatePlate()
    {
        if (delayCo != null) StopCoroutine(delayCo);
        delayCo = StartCoroutine(ActivateSequence());
    }

    void DeactivatePlate()
    {
        if (delayCo != null) StopCoroutine(delayCo);
        delayCo = StartCoroutine(DeactivateSequence());
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

    IEnumerator ActivateSequence()
    {
        // Show activating sprite
        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return StartCoroutine(WaitForTimelineSeconds(delayOn));

        // Switch to non-default state
        isPressed = true; // Pressure plate becomes visually pressed
        targetActive = !defaultPressed; // Target goes to opposite of default
        ApplyVisuals();
    }

    IEnumerator DeactivateSequence()
    {
        // Show activating sprite
        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return StartCoroutine(WaitForTimelineSeconds(delayOff));

        // Return to default state
        isPressed = false; // Pressure plate returns to not pressed
        targetActive = defaultPressed; // Target returns to default
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

        // Change pressure plate sprite
        if (spriteRenderer)
            spriteRenderer.sprite = isPressed ? pressedSprite : idleSprite;
    }

    private bool IsValidObject(Collider2D col)
    {
        return col.CompareTag("Player") || col.CompareTag("Box");
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
}
