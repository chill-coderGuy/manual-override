using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RushEnemy : MonoBehaviour
{
    [Header("Movement Stats")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6f; 
    public float detectionRange = 5f;

    [Header("Combat Stats")]
    public float damage = 10f;
    
    // --- SEPARATE KNOCKBACK VARIABLES ---
    [Tooltip("How hard the PLAYER gets pushed away.")]
    public float knockbackForceOnPlayer = 20f; // High value so you feel the hit
    
    [Tooltip("How hard THIS ENEMY bounces back.")]
    public float knockbackForceOnSelf = 8f;    // Lower value for a slight recoil
    
    [Tooltip("How long the enemy stops moving after hitting (Stun).")]
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
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (spriteRenderer != null) spriteRenderer.color = patrolColor;
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
        bool wasAggro = isAggro;
        isAggro = (distToPlayer < detectionRange);

        if (isAggro != wasAggro && spriteRenderer != null)
        {
            spriteRenderer.color = isAggro ? aggroColor : patrolColor;
        }
    }

    void FixedUpdate()
    {
        // DO NOT move if stunned. This allows the knockback physics to work.
        if (stunTimer > 0) return;

        if (isAggro && playerTransform != null)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void ChasePlayer()
    {
        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionX * chaseSpeed, rb.linearVelocity.y);
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Transform targetPoint = patrolPoints[currentWaypointIndex];
        float directionX = Mathf.Sign(targetPoint.position.x - transform.position.x);
        
        rb.linearVelocity = new Vector2(directionX * patrolSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - targetPoint.position.x) < waypointTolerance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
        }
    }

    void OnCollision2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Deal Damage
            HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);

            // 2. CALCULATE FORCE DIRECTION
            // Vector pointing FROM Enemy TO Player
            Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
            
            // 3. KNOCKBACK PLAYER (Force A)
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Reset velocity to zero first so the knockback is consistent
                playerRb.linearVelocity = Vector2.zero; 
                playerRb.AddForce(pushDirection * knockbackForceOnPlayer, ForceMode2D.Impulse);
            }

            // 4. KNOCKBACK SELF (Force B)
            // Reverse direction (-pushDirection)
            rb.linearVelocity = Vector2.zero; 
            rb.AddForce(-pushDirection * knockbackForceOnSelf, ForceMode2D.Impulse);

            // 5. STUN SELF
            stunTimer = impactStunDuration;
        }
    }
}