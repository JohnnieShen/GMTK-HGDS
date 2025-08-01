using UnityEngine;
using System.Collections;

public class PressurePlateHold : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Delays")]
    public float delayOn = 0f;
    public float delayOff = 0f;

    [Header("Default State")]
    public bool defaultStateIsOn = false;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;
    public Sprite activatingSprite; // optional

    private bool isSomethingOnPlate = false;
    private Coroutine delayCoroutine;

    void Start()
    {
        targetObject.SetActive(defaultStateIsOn);
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;

        isSomethingOnPlate = true;

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);

        delayCoroutine = StartCoroutine(SetStateAfterDelay(!defaultStateIsOn, delayOn, pressedSprite));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;

        isSomethingOnPlate = false;

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);

        delayCoroutine = StartCoroutine(SetStateAfterDelay(defaultStateIsOn, delayOff, idleSprite));
    }

    private IEnumerator SetStateAfterDelay(bool stateToSet, float delay, Sprite finalSprite)
    {
        if (spriteRenderer != null && activatingSprite != null)
            spriteRenderer.sprite = activatingSprite;

        yield return new WaitForSeconds(delay);

        targetObject.SetActive(stateToSet);

        if (spriteRenderer != null && finalSprite != null)
            spriteRenderer.sprite = finalSprite;
    }

    private bool IsValidObject(Collider2D col)
    {
        return col.CompareTag("Player") || col.CompareTag("Box");
    }
}
