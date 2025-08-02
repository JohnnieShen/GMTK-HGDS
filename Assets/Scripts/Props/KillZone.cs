using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) 
            return;

        var current = GameManager.Instance.CurrentPlayer;
        if (other.gameObject != current) 
            return;

        Debug.Log("KillZone: live player entered – ending life");
        LifeManager.Instance.EndCurrentLife();
    }
}
