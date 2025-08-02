using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour, RecordableProp
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Timing")]
    public float delayOn = 0f;
    public float delayOff = 1f;
    public bool defaultPressed = false;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;
    public Sprite activatingSprite;

    private bool playerInRange = false;
    private bool isProcessing = false;
    bool isPressed;
    PropRecorder recorder;

    void Start()
    {
        isPressed = defaultPressed;

        ApplyVisuals();

        recorder = GetComponent<PropRecorder>() ?? gameObject.AddComponent<PropRecorder>();
        PropManager.Instance.Register(recorder);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isProcessing)
            StartCoroutine(PressSequence());
    }

    IEnumerator PressSequence()
    {
        isProcessing = true;

        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return new WaitForSeconds(delayOn);

        isPressed = !isPressed;
        ApplyVisuals();

        yield return new WaitForSeconds(delayOff);

        isPressed = defaultPressed;
        ApplyVisuals();

        isProcessing = false;
    }

    void ApplyVisuals()
    {
        if (targetObject) targetObject.SetActive(isPressed);

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
        return new PropStatusFrame(gameObject.GetInstanceID(), TimelineManager.Instance.GetCurrentTime(), isPressed, transform.position);
    }

    public void ApplyFrame(PropStatusFrame frame)
    {
        isPressed = frame.active;
        transform.position = frame.position;
        ApplyVisuals();
    }
}
