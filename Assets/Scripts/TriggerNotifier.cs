using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class TriggerNotifier : MonoBehaviour
{
    public static event Action<bool> PlayerTriggerEvent;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerTriggerEvent?.Invoke(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerTriggerEvent?.Invoke(false);
    }
}
