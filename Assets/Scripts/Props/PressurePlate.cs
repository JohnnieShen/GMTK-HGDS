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

    private bool isSomethingOnPlate = false;
    private bool isProcessing = false;
    private Coroutine delayCoroutine;

    void Start()
    {
        targetObject.SetActive(defaultStateIsOn);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;

        isSomethingOnPlate = true;

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);

        delayCoroutine = StartCoroutine(SetStateAfterDelay(!defaultStateIsOn, delayOn));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidObject(other)) return;

        isSomethingOnPlate = false;

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);

        delayCoroutine = StartCoroutine(SetStateAfterDelay(defaultStateIsOn, delayOff));
    }

    private IEnumerator SetStateAfterDelay(bool stateToSet, float delay)
    {
        isProcessing = true;
        yield return new WaitForSeconds(delay);
        targetObject.SetActive(stateToSet);
        isProcessing = false;
    }

    private bool IsValidObject(Collider2D col)
    {
        return col.CompareTag("Player") || col.CompareTag("Box");
    }
}
