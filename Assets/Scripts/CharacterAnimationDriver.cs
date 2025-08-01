using UnityEngine;

[RequireComponent(typeof(MovementController), typeof(Animator))]
public class CharacterAnimationDriver : MonoBehaviour
{
    const float faceThreshold = 0.05f;

    MovementController movement;
    Animator animator;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        movement = GetComponent<MovementController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 vel = movement.CurrentVelocity;

        animator.SetFloat("Speed", Mathf.Abs(vel.x));
        animator.SetFloat("VerticalVel", vel.y);
        animator.SetBool("IsGrounded", movement.IsGrounded());
        
        if (vel.x >  faceThreshold) spriteRenderer.flipX = false;
        if (vel.x < -faceThreshold) spriteRenderer.flipX = true;
    }
}
