using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour, RecordableProp
{
    [Header("Target")]
    public GameObject targetObject;

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

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.T) && !isProcessing)
            StartCoroutine(PressSequence());
    }

    IEnumerator PressSequence()
    {
        isProcessing = true;

        // Show activating sprite
        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return new WaitForSeconds(delayOn);

        // Toggle to non-default state
        isPressed = true; // Button becomes visually pressed
        targetActive = !defaultPressed; // Target goes to opposite of default
        ApplyVisuals();

        yield return new WaitForSeconds(delayOff);

        // Return to default state
        isPressed = false; // Button returns to not pressed
        targetActive = defaultPressed; // Target returns to default
        ApplyVisuals();

        isProcessing = false;
    }

    void ApplyVisuals()
    {
        if (targetObject) targetObject.SetActive(targetActive);

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
        // Don't change isPressed here - let the visual state be controlled by the press sequence
        transform.position = frame.position;
        ApplyVisuals();
    }
}
