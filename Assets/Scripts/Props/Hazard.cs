using UnityEngine;

public class HazardInstantKill : MonoBehaviour, RecordableProp
{
    PropRecorder recorder;

    void Start()
    {
        recorder = GetComponent<PropRecorder>() ?? gameObject.AddComponent<PropRecorder>();
        PropManager.Instance.Register(recorder);
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
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }

    }
}
