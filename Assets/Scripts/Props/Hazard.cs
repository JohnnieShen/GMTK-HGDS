using UnityEngine;

public class HazardInstantKill : MonoBehaviour, RecordableProp
{
    PropRecorder recorder;

    public bool isActive = true;

    void Start()
    {
        isActive = true;
    }

    void Update()
    {
        isActive = !TimelineManager.Instance.IsPaused;
    }

    public PropStatusFrame CaptureFrame()
    {
        return new PropStatusFrame(gameObject.GetInstanceID(), TimelineManager.Instance.GetCurrentTime(), true, transform.position);
    }

    public void ApplyFrame(PropStatusFrame frame)
    {
        gameObject.SetActive(frame.active);
        transform.position = frame.position;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")&& isActive)
        {
            Destroy(other.gameObject);
        }

    }
}
