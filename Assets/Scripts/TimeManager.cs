using UnityEngine;
public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    public float timelineDuration = 10f;
    public float currentTime;
    public float timelineSpeed = 1f;

    public event System.Action<float> OnTimelineTick;

    void Awake() => Instance = this;

    void Update()
    {
        currentTime += Time.deltaTime * timelineSpeed;
        if (currentTime > timelineDuration)
            currentTime -= timelineDuration;

        OnTimelineTick?.Invoke(currentTime);
    }

    public float GetCurrentTime() => currentTime;
}
