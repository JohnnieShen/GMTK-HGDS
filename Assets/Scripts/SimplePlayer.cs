using UnityEngine;

/// <summary>
/// Debug version of Player - use this to test if the basic setup works
/// </summary>
public class SimplePlayer : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 10f;
    
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("SimplePlayer: No Rigidbody2D found!");
        }
        else
        {
            Debug.Log("SimplePlayer: Setup complete!");
        }
    }
    
    void Update()
    {
        // Simple movement for testing
        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            rb.linearVelocity = new Vector2(h * speed, rb.linearVelocity.y);
            Debug.Log($"Moving: {h}");
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                Debug.Log("Jumping!");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Rewind key pressed - full system needed for this!");
        }
    }
}
