using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MultiPressureDoor : MonoBehaviour, RecordableProp
{
    [Header("Required Pressure Plates")]
    public PressurePlateHold[] requiredPlates;

    [Header("Target")]
    public GameObject targetObject;
    public Sprite targetActiveSprite;
    public Sprite targetInactiveSprite;

    [Header("Default")]
    public bool defaultPressed = false; // Whether door's default state is open or closed

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite closedSprite;
    public Sprite openSprite;

    private bool targetActive; // Current state of the door
    private bool lastCheckResult = false;

    void Start()
    {
        targetActive = defaultPressed; // Door starts in its default state
        ApplyVisuals();
    }

    void Update()
    {
        CheckPlatesStatus();
    }

    void CheckPlatesStatus()
    {
        if (requiredPlates == null || requiredPlates.Length == 0) return;

        // Check if all required plates are active at the current timeline point
        bool allPlatesActive = true;
        
        foreach (var plate in requiredPlates)
        {
            if (plate == null) continue;
            
            // Get the plate's current frame to check its state at this timeline point
            var frame = plate.CaptureFrame();
            if (!frame.active)
            {
                allPlatesActive = false;
                break;
            }
        }

        // Only change state if the result is different from last check
        if (allPlatesActive != lastCheckResult)
        {
            lastCheckResult = allPlatesActive;
            
            if (allPlatesActive)
            {
                // All plates are active - switch to non-default state
                targetActive = !defaultPressed;
            }
            else
            {
                // Not all plates are active - return to default state
                targetActive = defaultPressed;
            }
            
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
        
        // Update the last check result to match the applied state
        lastCheckResult = targetActive != defaultPressed;
    }

    void OnDrawGizmosSelected()
    {
        if (requiredPlates == null) return;

        // Draw lines to show which pressure plates this door depends on
        Gizmos.color = Color.yellow;
        foreach (var plate in requiredPlates)
        {
            if (plate != null)
            {
                Gizmos.DrawLine(transform.position, plate.transform.position);
            }
        }
    }
}
