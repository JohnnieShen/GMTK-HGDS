using UnityEngine;
public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    public float timelineDuration = 10f;
    public float currentTime;
    public float timelineSpeed = 1f;

    public event System.Action<float> OnTimelineTick;

    bool  isPaused;
    public bool IsPaused => isPaused;
    public void Pause(bool v) => isPaused = v;
    public void SetPaused(bool pause)
    {
        if (isPaused == pause) return;
        isPaused = pause;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePhysicsPause(pause);
        }
    }
    public void TogglePause() => SetPaused(!isPaused);

    void Awake() => Instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) TogglePause();

        if (isPaused) return;

        currentTime += Time.deltaTime * timelineSpeed;
        if (currentTime > timelineDuration)
            currentTime -= timelineDuration;

        OnTimelineTick?.Invoke(currentTime);
    }

    public float GetCurrentTime() => currentTime;
}
