using UnityEngine;
using System.Collections;

public class Lever : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Settings")]
    public float delayOn = 0f;
    public bool defaultStateIsOn = true;

    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;
    public Sprite activatingSprite;

    private bool playerInRange = false;
    private bool isProcessing = false;
    private bool currentState;

    void Start()
    {
        currentState = defaultStateIsOn;
        targetObject.SetActive(currentState);

        if (spriteRenderer)
            spriteRenderer.sprite = currentState ? onSprite : offSprite;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isProcessing)
        {
            StartCoroutine(ToggleLever());
        }
    }

    IEnumerator ToggleLever()
    {
        isProcessing = true;

        if (spriteRenderer && activatingSprite)
            spriteRenderer.sprite = activatingSprite;

        yield return new WaitForSeconds(delayOn);

        // Flip state
        currentState = !currentState;
        targetObject.SetActive(currentState);

        if (spriteRenderer)
            spriteRenderer.sprite = currentState ? onSprite : offSprite;

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
