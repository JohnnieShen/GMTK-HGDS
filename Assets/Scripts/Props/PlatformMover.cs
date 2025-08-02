using UnityEngine;
using System.Collections.Generic;

public class PlatformMover : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 2f;

    private Vector2 target;
    private Vector2 lastPosition;
    private List<Transform> playersOnPlatform = new List<Transform>();

    bool isActive;
    PropRecorder recorder;

    void Start()
    {
        target = pointB;
        lastPosition = transform.position;

        recorder = GetComponent<PropRecorder>() ?? gameObject.AddComponent<PropRecorder>();
        PropManager.Instance.Register(recorder);
    }

    void Update()
    {
        if (!isActive) return;
        Vector2 previousPosition = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        // Calculate movement delta
        Vector2 movementDelta = (Vector2)transform.position - previousPosition;

        // Move all players on the platform
        foreach (Transform player in playersOnPlatform)
        {
            if (player != null)
            {
                player.position += (Vector3)movementDelta;
            }
        }

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA;
        }
    }

    public void SetActive(bool value) => isActive = value;

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if something landed on top of the platform
        if (collision.transform.CompareTag("Player") && collision.contacts[0].normal.y < -0.5f)
        {
            if (!playersOnPlatform.Contains(collision.transform))
            {
                playersOnPlatform.Add(collision.transform);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Remove player from platform when they leave
        if (collision.transform.CompareTag("Player"))
        {
            playersOnPlatform.Remove(collision.transform);
        }
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

        target = (Vector2.Distance(transform.position, pointA) <
                  Vector2.Distance(transform.position, pointB))
                 ? pointB : pointA;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA, pointB);
    }
}
