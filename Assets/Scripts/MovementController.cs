using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    private Rigidbody2D rb;
    
    public bool IsGrounded => Mathf.Abs(rb.linearVelocity.y) < 0.05f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void Move(float horizontal)
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }
    
    public void Jump()
    {
        if (IsGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    
    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }
    
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
    }
}
