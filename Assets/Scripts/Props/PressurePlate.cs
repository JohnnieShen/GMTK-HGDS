using UnityEngine;
using System.Collections;

public class PressurePlateHold : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Delays")]
    public float delayOn = 0f;
    public float delayOff = 0f;

    [Header("Default")]
    public bool defaultActive = false;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;
    public Sprite activatingSprite; // optional

    private bool isSomethingOnPlate = false;
    private Coroutine delayCoroutine;
    bool isActive;
    Coroutine delayCo;
    PropRecorder recorder;

    void Start()
    {
        isActive = defaultActive;
        ApplyVisuals();

        recorder = GetComponent<PropRecorder>() ?? gameObject.AddComponent<PropRecorder>();
        PropManager.Instance.Register(recorder);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;
        ChangeStateDelayed(true, delayOn);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;
        ChangeStateDelayed(false, delayOff);
    }

    void ChangeStateDelayed(bool nextState, float delay)
    {
        if (delayCo != null) StopCoroutine(delayCo);
        delayCo = StartCoroutine(SetStateAfterDelay(nextState, delay));
    }

    IEnumerator SetStateAfterDelay(bool nextState, float delay)
    {
        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return new WaitForSeconds(delay);

        isActive = nextState;
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        if (targetObject) targetObject.SetActive(isActive);

        if (spriteRenderer)
            spriteRenderer.sprite = isActive ? pressedSprite : idleSprite;
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
}
