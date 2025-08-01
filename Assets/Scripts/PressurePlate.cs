using UnityEngine;
using System.Collections;

public class PressurePlate_ReversedLogic : MonoBehaviour
{
    public GameObject connectedObject;
    public float reactivateDelay = 2f;

    private int objectsOnPlate = 0;
    private Coroutine reactivateCoroutine;

    void Start()
    {
        if (connectedObject != null)
            connectedObject.SetActive(true); // Default is ON
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValidObject(other))
        {
            objectsOnPlate++;

            // Deactivate immediately when something steps on
            if (connectedObject != null)
                connectedObject.SetActive(false);

            // Cancel pending reactivation
            if (reactivateCoroutine != null)
            {
                StopCoroutine(reactivateCoroutine);
                reactivateCoroutine = null;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValidObject(other))
        {
            objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);

            if (objectsOnPlate == 0)
            {
                // Begin reactivation delay
                reactivateCoroutine = StartCoroutine(ReactivateAfterDelay());
            }
        }
    }

    private IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(reactivateDelay);

        if (objectsOnPlate == 0 && connectedObject != null)
            connectedObject.SetActive(true);
    }

    private bool IsValidObject(Collider2D other)
    {
        return other.CompareTag("Player") || other.attachedRigidbody != null;
    }
}
