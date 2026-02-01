using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RusherEnemy : MonoBehaviour
{
    [Header("Movement Constraints")]
    public float minX = -10f; 
    public float maxX = 10f;  

    [Header("Movement Stats")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6f; 
    public float detectionRange = 5f;
    public float verticalDetectionRange = 1.5f; 

    [Header("Combat Stats")]
    public float damage = 10f;
    public float knockbackForceOnPlayer = 20f; 
    public float knockbackForceOnSelf = 8f;   
    public float impactStunDuration = 0.5f; 

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.5f; 

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Color patrolColor = Color.white;
    public Color aggroColor = Color.red;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private bool isAggro = false;
    private float stunTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        
        if (spriteRenderer == null) 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
                Debug.Log($"[{gameObject.name}] Found SpriteRenderer on: {spriteRenderer.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] No SpriteRenderer found! Color changes won't work.");
            }
        }
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) 
        {
            playerTransform = p.transform;
            Debug.Log($"[{gameObject.name}] Found player: {p.name}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Player not found! Make sure player has 'Player' tag.");
        }
        if (spriteRenderer != null) 
        {
            spriteRenderer.color = patrolColor;
            Debug.Log($"[{gameObject.name}] Set initial color to patrol color");
        }
    }

    void Update()
    {
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return; 
        }

        if (playerTransform == null) return;
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float yDiff = Mathf.Abs(playerTransform.position.y - transform.position.y);

        bool wasAggro = isAggro;
        isAggro = (distToPlayer < detectionRange && yDiff < verticalDetectionRange);

        if (isAggro != wasAggro)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isAggro ? aggroColor : patrolColor;
                Debug.Log($"[{gameObject.name}] Aggro changed: {isAggro} - Color set to {(isAggro ? "RED" : "WHITE")}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Can't change color - spriteRenderer is null!");
            }
        }
    }

    void FixedUpdate()
    {
        if (stunTimer > 0) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = 0f;
        float targetX = transform.position.x;
        if (isAggro && playerTransform != null)
        {
            targetX = playerTransform.position.x;
            currentSpeed = chaseSpeed;
        }
        else
        {
            if (patrolPoints.Length > 0)
            {
                targetX = patrolPoints[currentWaypointIndex].position.x;
                currentSpeed = patrolSpeed;
            }
        }
        float clampedTargetX = Mathf.Clamp(targetX, minX, maxX);
        float distToTarget = clampedTargetX - transform.position.x;

        if (Mathf.Abs(distToTarget) < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            float directionX = Mathf.Sign(distToTarget);
            rb.linearVelocity = new Vector2(directionX * currentSpeed, 0); 
        }
        if (!isAggro && patrolPoints.Length > 0)
        {
            if (Mathf.Abs(transform.position.x - targetX) < waypointTolerance)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (patrolPoints.Length > 0)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
            }
            stunTimer = 0.2f;
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
            Vector2 pushDir = (collision.transform.position - transform.position).normalized;
            KnockbackReceiver receiver = collision.gameObject.GetComponent<KnockbackReceiver>();
            if (receiver != null)
            {
                receiver.ApplyKnockback(pushDir * knockbackForceOnPlayer);
            }
            else
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.AddForce(pushDir * knockbackForceOnPlayer, ForceMode2D.Impulse);
                }
            }
            rb.linearVelocity = Vector2.zero; 
            rb.AddForce(-pushDir * knockbackForceOnSelf, ForceMode2D.Impulse);
            stunTimer = impactStunDuration;
        }
    }
    
    void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            Vector3 scaler = spriteRenderer.transform.localScale;
            scaler.x *= -1;
            spriteRenderer.transform.localScale = scaler;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 start = new Vector3(minX, transform.position.y, 0);
        Vector3 end = new Vector3(maxX, transform.position.y, 0);
        Gizmos.DrawLine(start + Vector3.up, start + Vector3.down);
        Gizmos.DrawLine(end + Vector3.up, end + Vector3.down);   
        Gizmos.DrawLine(start, end);
        Gizmos.color = isAggro ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange * 2, verticalDetectionRange * 2, 0));
    }
}