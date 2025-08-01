using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Timing")]
    public float delayOn = 0f;
    public float delayOff = 1f;
    public bool defaultStateIsOn = true;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;
    public Sprite activatingSprite;

    private bool playerInRange = false;
    private bool isProcessing = false;

    void Start()
    {
        targetObject.SetActive(defaultStateIsOn);

        if (spriteRenderer && idleSprite)
            spriteRenderer.sprite = idleSprite;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isProcessing)
        {
            StartCoroutine(PressSequence());
        }
    }

    IEnumerator PressSequence()
    {
        isProcessing = true;

        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return new WaitForSeconds(delayOn);

        // Flip from default state
        bool flippedState = !defaultStateIsOn;
        targetObject.SetActive(flippedState);

        if (spriteRenderer && pressedSprite)
            spriteRenderer.sprite = pressedSprite;

        yield return new WaitForSeconds(delayOff);

        // Revert to default
        targetObject.SetActive(defaultStateIsOn);

        if (spriteRenderer && idleSprite)
            spriteRenderer.sprite = idleSprite;

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
}
