using UnityEngine;
using UnityEngine.SceneManagement; 

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float gravityScale = 3f;
    public LayerMask groundLayer;

    [Header("Level Completion")]
    public float finishXPosition = 100f; 
    public string successSceneName = "LevelComplete"; 

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isLevelComplete = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
      
        Time.timeScale = 1f; 
    }

    void Update()
    {
       
        if (isLevelComplete) return;

        
        if (transform.position.x >= finishXPosition)
        {
            CompleteLevel();
            return;
        }

      
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.gravityScale = gravityScale;

        bool jumpPressed = Input.GetKeyDown(KeyCode.W) || 
                           Input.GetKeyDown(KeyCode.UpArrow) || 
                           Input.GetKeyDown(KeyCode.Space);

        if (jumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    void CompleteLevel()
    {
        isLevelComplete = true;
        Debug.Log("🏁 Level Complete! Loading Success Scene...");
        SceneManager.LoadScene(successSceneName);
    }

    public bool IsGrounded()
    {
        float extraHeight = 0.1f;
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(finishXPosition, -10, 0), new Vector3(finishXPosition, 10, 0));
    }
}