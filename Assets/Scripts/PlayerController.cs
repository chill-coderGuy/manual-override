using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 1. Declare these at the VERY TOP of the class
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float gravityScale = 3f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider; // <--- This was missing!

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>(); // <--- This assigns it
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.gravityScale = gravityScale;

        // The jump logic
        bool jumpPressed =  Input.GetKeyDown(KeyCode.W) || 
                            Input.GetKeyDown(KeyCode.UpArrow);
        if (jumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        float extraHeight = 0.1f;
        
        // This 'BoxCast' ensures you can jump even if only a sliver of the player is on a cliff
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            boxCollider.bounds.center, 
            boxCollider.bounds.size, 
            0f, 
            Vector2.down, 
            extraHeight, 
            groundLayer
        );

        return raycastHit.collider != null;
    }
}