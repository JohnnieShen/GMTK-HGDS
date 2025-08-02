using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    [Header("Timeline Settings")]
    public float timelineDuration = 10f;
    public float currentTime;

    [Header("Fast-Forward / Rewind UI")]
    public Button fastForwardButton;
    public Button fastBackwardButton;
    [Tooltip("Multiplier for timeline speed")]
    public float timelineSpeed = 1f;
    public float fastSpeedMultiplier = 3f;

    public event System.Action<float> OnTimelineTick;
    public event System.Action OnTimelineLoop;

    float defaultSpeed;
    bool holdForward;
    bool holdBackward;

    bool isPaused;
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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        defaultSpeed = timelineSpeed;

        if (fastForwardButton != null)
        {
            AddTrigger(fastForwardButton.gameObject, EventTriggerType.PointerDown, OnForwardDown);
            AddTrigger(fastForwardButton.gameObject, EventTriggerType.PointerUp, OnForwardUp);
        }

        if (fastBackwardButton != null)
        {
            AddTrigger(fastBackwardButton.gameObject, EventTriggerType.PointerDown, OnBackwardDown);
            AddTrigger(fastBackwardButton.gameObject, EventTriggerType.PointerUp, OnBackwardUp);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !holdForward && !holdBackward)
            TogglePause();

        if (Input.GetKeyDown(KeyCode.E)) OnForwardDown(null);
        if (Input.GetKeyUp(KeyCode.E)) OnForwardUp(null);
        if (Input.GetKeyDown(KeyCode.Q)) OnBackwardDown(null);
        if (Input.GetKeyUp(KeyCode.Q)) OnBackwardUp(null);

        if (isPaused) return;

        float prev = currentTime;
        currentTime += Time.deltaTime * timelineSpeed;

        bool looped = false;
        if (currentTime > timelineDuration) {                 
            currentTime -= timelineDuration; looped = true;
        }
        if (currentTime < 0f) {
            currentTime += timelineDuration; looped = true;
        }

        if (looped) OnTimelineLoop?.Invoke();

        OnTimelineTick?.Invoke(currentTime);
    }

    void OnForwardDown(BaseEventData _)
    {
        if (holdForward) return;
        holdForward = true;
        LifeManager.Instance.EndCurrentLife();
        SetPaused(false);
        timelineSpeed = Mathf.Abs(defaultSpeed) * fastSpeedMultiplier;
    }

    void OnForwardUp(BaseEventData _)
    {
        if (!holdForward) return;
        holdForward = false;
        SetPaused(true);
        timelineSpeed = defaultSpeed;
    }

    void OnBackwardDown(BaseEventData _)
    {
        if (holdBackward) return;
        holdBackward = true;
        LifeManager.Instance.EndCurrentLife();
        SetPaused(false);
        timelineSpeed = -Mathf.Abs(defaultSpeed) * fastSpeedMultiplier;
    }

    void OnBackwardUp(BaseEventData _)
    {
        if (!holdBackward) return;
        holdBackward = false;
        SetPaused(true);
        timelineSpeed = defaultSpeed;
    }

    public float GetCurrentTime() => currentTime;

    void OnFastForward()
    {
        LifeManager.Instance.EndCurrentLife();

        SetPaused(true);
        timelineSpeed = Mathf.Abs(timelineSpeed) * fastSpeedMultiplier;
    }

    void OnFastBackward()
    {
        LifeManager.Instance.EndCurrentLife();

        SetPaused(true);
        timelineSpeed = -Mathf.Abs(timelineSpeed) * fastSpeedMultiplier;
    }

    void AddTrigger(GameObject go, EventTriggerType type, System.Action<BaseEventData> callback)
    {
        var trig = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => callback(data));
        trig.triggers.Add(entry);
    }
    
    void OnDestroy()
    {
        if (fastForwardButton != null)
        {
            var ft = fastForwardButton.gameObject.GetComponent<EventTrigger>();
            if (ft != null)
                ft.triggers.Clear();
        }
        if (fastBackwardButton != null)
        {
            var bt = fastBackwardButton.gameObject.GetComponent<EventTrigger>();
            if (bt != null)
                bt.triggers.Clear();
        }

        if (Instance == this)
            Instance = null;
    }
}
