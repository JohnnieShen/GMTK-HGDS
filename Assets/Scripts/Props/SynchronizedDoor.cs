using UnityEngine;
using System.Collections.Generic;

public class SynchronizedDoor : MonoBehaviour, RecordableProp
{
    [Header("Required Pressure Plates")]
    public PressurePlateHold[] requiredPlates;

    [Header("Target")]
    public GameObject targetObject;
    public Sprite targetActiveSprite;
    public Sprite targetInactiveSprite;

    [Header("Synchronization")]
    public float synchronizationWindow = 0.1f; // Time window for plates to be considered synchronized
    public bool requireExactSync = true; // If true, plates must be active at exactly the same timeline moment

    [Header("Default")]
    public bool defaultPressed = false; // Whether door's default state is open or closed

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite closedSprite;
    public Sprite openSprite;

    private bool targetActive; // Current state of the door
    private Dictionary<PressurePlateHold, float> plateActivationTimes = new Dictionary<PressurePlateHold, float>();

    void Start()
    {
        targetActive = defaultPressed; // Door starts in its default state
        ApplyVisuals();
    }

    void Update()
    {
        CheckSynchronizedActivation();
    }

    void CheckSynchronizedActivation()
    {
        if (requiredPlates == null || requiredPlates.Length == 0) return;

        float currentTime = TimelineManager.Instance.GetCurrentTime();
        bool shouldBeActive = false;

        if (requireExactSync)
        {
            // Check if all plates are currently active
            bool allPlatesActive = true;
            foreach (var plate in requiredPlates)
            {
                if (plate == null) continue;
                
                var frame = plate.CaptureFrame();
                if (!frame.active)
                {
                    allPlatesActive = false;
                    break;
                }
            }
            shouldBeActive = allPlatesActive;
        }
        else
        {
            // Check if all plates were active within the synchronization window
            List<float> activationTimes = new List<float>();
            bool allPlatesHaveActivation = true;

            foreach (var plate in requiredPlates)
            {
                if (plate == null) continue;
                
                // This would require tracking when each plate was last activated
                // For now, we'll use the simpler exact sync approach
                var frame = plate.CaptureFrame();
                if (frame.active)
                {
                    activationTimes.Add(currentTime);
                }
                else
                {
                    allPlatesHaveActivation = false;
                    break;
                }
            }

            if (allPlatesHaveActivation && activationTimes.Count == requiredPlates.Length)
            {
                // Check if all activation times are within the synchronization window
                float minTime = Mathf.Min(activationTimes.ToArray());
                float maxTime = Mathf.Max(activationTimes.ToArray());
                shouldBeActive = (maxTime - minTime) <= synchronizationWindow;
            }
        }

        // Update door state
        bool newTargetActive = shouldBeActive ? !defaultPressed : defaultPressed;
        
        if (newTargetActive != targetActive)
        {
            targetActive = newTargetActive;
            ApplyVisuals();
        }
    }

    void ApplyVisuals()
    {
        // Change target object sprite and collider state
        if (targetObject)
        {
            SpriteRenderer targetSpriteRenderer = targetObject.GetComponent<SpriteRenderer>();
            Collider2D targetCollider = targetObject.GetComponent<Collider2D>();
            
            if (targetSpriteRenderer)
            {
                targetSpriteRenderer.sprite = targetActive ? targetActiveSprite : targetInactiveSprite;
            }
            
            if (targetCollider)
            {
                targetCollider.enabled = targetActive;
            }
        }

        // Change door sprite
        if (spriteRenderer)
            spriteRenderer.sprite = targetActive ? openSprite : closedSprite;
    }

    public PropStatusFrame CaptureFrame()
    {
        return new PropStatusFrame(
            gameObject.GetInstanceID(),
            TimelineManager.Instance.GetCurrentTime(),
            targetActive,
            transform.position
        );
    }

    public void ApplyFrame(PropStatusFrame frame)
    {
        targetActive = frame.active;
        transform.position = frame.position;
        ApplyVisuals();
    }

    void OnDrawGizmosSelected()
    {
        if (requiredPlates == null) return;

        // Draw lines to show which pressure plates this door depends on
        Gizmos.color = requireExactSync ? Color.red : Color.yellow;
        foreach (var plate in requiredPlates)
        {
            if (plate != null)
            {
                Gizmos.DrawLine(transform.position, plate.transform.position);
            }
        }

        // Draw synchronization window info
        if (!requireExactSync)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, synchronizationWindow);
        }
    }
}
