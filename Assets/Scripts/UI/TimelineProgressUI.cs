using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class TimelineProgressUI : MonoBehaviour,
                                   IPointerDownHandler,
                                   IPointerUpHandler,
                                   IPointerClickHandler
{
    public Slider slider;
    bool   dragging;

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    void OnEnable()
    {
        TimelineManager.Instance.OnTimelineTick += SyncDisplay;
    }
    void OnDisable()
    {
        if (TimelineManager.Instance != null)
            TimelineManager.Instance.OnTimelineTick -= SyncDisplay;
    }


    public void OnPointerDown(PointerEventData e)
    {
        if (!TimelineManager.Instance.IsPaused) return;
        dragging = true;
        ApplyPointer(e);
    }

    public void OnDrag(IDragHandler e) { }

    public void OnPointerClick(PointerEventData e)
    {
        if (!TimelineManager.Instance.IsPaused) return;
        ApplyPointer(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!dragging) return;
        ApplyPointer(e);
        dragging = false;
    }


    void ApplyPointer(PointerEventData e)
    {
        RectTransform rt = (RectTransform)slider.fillRect.parent;
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out localPos);

        float pct = Mathf.InverseLerp(rt.rect.xMin, rt.rect.xMax, localPos.x);
        pct = Mathf.Clamp01(pct);

        slider.SetValueWithoutNotify(pct);
        JumpToTime(pct);
    }

    void JumpToTime(float fraction)
    {
        var tm   = TimelineManager.Instance;
        float t  = fraction * tm.timelineDuration;
        tm.currentTime = t;

        foreach (var g in FindObjectsOfType<GhostController>())
            g.Seek(t);
    }


    void SyncDisplay(float current)
    {
        if (TimelineManager.Instance.IsPaused) return;
        slider.SetValueWithoutNotify(current / TimelineManager.Instance.timelineDuration);
    }


    void Update()
    {
        slider.interactable = TimelineManager.Instance.IsPaused;
    }
}
