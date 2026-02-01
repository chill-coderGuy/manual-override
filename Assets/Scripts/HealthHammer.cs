using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthHammer : MonoBehaviour
{
    [Header("References")]
    public Collider2D[] dragColliders; 
    public HealthSystem playerHealth; 
    public Transform handleTransform; 
    public Transform headTransform;   
    public Collider2D hammerCollider; 

    [Header("Calibration")]
    public float handleScaleAtMin = 0.5f; 
    public float handleScaleAtMax = 3.0f; 
    public float headHeightAtMin = 0.5f;
    public float headHeightAtMax = 3.0f;
    public float heartsize = 2.0f;

    [Header("Damage Wave Settings")]
    [Tooltip("Multiplier for wave length based on hammer reach")]
    public float waveLengthMultiplier = 1.5f;
    [Tooltip("Width of the damage wave (perpendicular to attack direction)")]
    public float waveWidth = 2.0f;
    [Tooltip("Should the wave start from player position?")]
    public bool waveStartsFromPlayer = true;

    [Header("Shockwave Settings (Ground Hit)")]
    public float shockwaveRadius = 3.0f; 
    public float shockwaveDamage = 10f;    
    public float shockwaveKnockback = 10f; 
    public LayerMask enemyLayer;
    public LayerMask groundLayer;

    [Header("Combat Stats")]
    public float damage = 25f; 
    public float knockbackForce = 15f;
    public float selfDamageOnHit = 0f; 
    public float swingSpeed = 10f; 
    public float swingDuration = 0.2f;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showWaveGizmos = true;

    private bool isSwinging = false;
    private bool hasTriggeredWave = false;
    private bool hasTriggeredShockwave = false;
    private float lastSwingTime = 0f;
    private Quaternion defaultRotation;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
    private float currentWaveDirection = 1f; // 1 for right, -1 for left
    private Vector2 lastWaveCenter;
    private Vector2 lastWaveSize;

    void Start()
    {
        defaultRotation = Quaternion.Euler(0, 0, 0); 
        if (playerHealth == null) playerHealth = GetComponentInParent<HealthSystem>();
        if (hammerCollider != null) hammerCollider.enabled = false;
    }

    void Update()
    {
        // 1. FAILSAFE
        if (isSwinging && Time.time - lastSwingTime > 1.0f)
        {
            if (showDebugLogs) Debug.LogWarning("[HealthHammer] Swing timeout - resetting");
            ResetSwing();
        }

        // 2. SCALING LOGIC
        if (playerHealth != null && handleTransform != null && headTransform != null)
        {
            float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
            healthPercent = Mathf.Clamp01(healthPercent);

            // Handle Scale
            float currentScale = Mathf.Lerp(handleScaleAtMin, handleScaleAtMax, healthPercent);
            Vector3 newScale = handleTransform.localScale;
            newScale.y = currentScale;
            newScale.x = 0.2f; 
            newScale.z = 1f;
            handleTransform.localScale = newScale;

            // Head Position
            float currentHeadY = Mathf.Lerp(headHeightAtMin, headHeightAtMax, healthPercent);
            headTransform.localPosition = new Vector3(0, currentHeadY, 0);
            headTransform.localScale = Vector3.one / heartsize; 
        }

        // 3. INPUT LOGIC
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverHammer()) 
            {
                return; 
            }

            if (!isSwinging)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float dirX = mousePos.x - transform.position.x;
                
                // Store attack direction for wave
                currentWaveDirection = dirX > 0 ? 1f : -1f;
                
                StartCoroutine(Smash(dirX > 0 ? -90f : 90f));
            }
        }
    }

    bool IsMouseOverHammer()
    {
        if (dragColliders == null || dragColliders.Length == 0) return false;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        foreach (Collider2D col in dragColliders)
        {
            if (col != null && col.OverlapPoint(mousePos))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator Smash(float targetAngle)
    {
        isSwinging = true;
        lastSwingTime = Time.time;
        hitEnemies.Clear();
        hasTriggeredWave = false;
        hasTriggeredShockwave = false;
        
        if (hammerCollider != null) hammerCollider.enabled = true;

        if (showDebugLogs) Debug.Log("[HealthHammer] Starting swing in direction: " + (currentWaveDirection > 0 ? "RIGHT" : "LEFT"));

        try 
        {
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.Euler(0, 0, targetAngle);

            // A. SWING DOWN
            float timer = 0f;
            while (timer < swingDuration)
            {
                timer += Time.deltaTime * swingSpeed;
                transform.rotation = Quaternion.Lerp(startRot, endRot, timer);
                yield return null;
            }

            // B. IMPACT - TRIGGER DAMAGE WAVE
            if (!hasTriggeredWave)
            {
                hasTriggeredWave = true;
                TriggerDamageWave();
            }

            // C. CHECK FOR GROUND IMPACT (for additional shockwave)
            if (headTransform != null && !hasTriggeredShockwave)
            {
                Collider2D groundHit = Physics2D.OverlapCircle(
                    headTransform.position, 
                    0.5f,
                    groundLayer
                );

                if (groundHit != null)
                {
                    hasTriggeredShockwave = true;
                    if (showDebugLogs) Debug.Log("[HealthHammer] Ground impact - triggering radial shockwave!");
                    TriggerShockwave();
                    
                    // Self damage on ground hit
                    if (playerHealth != null && selfDamageOnHit > 0) 
                    {
                        playerHealth.TakeDamage(selfDamageOnHit);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);

            // D. RETURN UP
            if (hammerCollider != null) hammerCollider.enabled = false; 
            
            timer = 0f;
            while (timer < swingDuration)
            {
                timer += Time.deltaTime * (swingSpeed * 0.5f); 
                transform.rotation = Quaternion.Lerp(endRot, defaultRotation, timer);
                yield return null;
            }

            transform.rotation = defaultRotation;
            
            if (showDebugLogs) Debug.Log($"[HealthHammer] Swing complete. Hit {hitEnemies.Count} enemies");
        }
        finally
        {
            ResetSwing();
        }
    }

    void TriggerDamageWave()
    {
        if (headTransform == null) return;

        // Calculate hammer reach (distance from player to hammer head)
        Vector2 playerPos = transform.position;
        Vector2 hammerHeadPos = headTransform.position;
        float hammerReach = Vector2.Distance(playerPos, hammerHeadPos);

        // Calculate wave dimensions
        float waveLength = hammerReach * waveLengthMultiplier;
        
        // Wave starts from player or hammer head
        Vector2 waveStartPos = waveStartsFromPlayer ? playerPos : hammerHeadPos;
        
        // Calculate wave center (middle of the rectangular area)
        Vector2 waveCenter = waveStartPos + new Vector2(currentWaveDirection * waveLength / 2f, 0);
        
        // Store for debug drawing
        lastWaveCenter = waveCenter;
        lastWaveSize = new Vector2(waveLength, waveWidth);

        if (showDebugLogs) 
        {
            Debug.Log($"[HealthHammer] Damage Wave - Reach: {hammerReach:F2}, Length: {waveLength:F2}, Direction: {(currentWaveDirection > 0 ? "RIGHT" : "LEFT")}");
        }

        // Use OverlapBox to detect enemies in rectangular area
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            waveCenter,
            lastWaveSize,
            0f, // No rotation
            enemyLayer
        );

        if (showDebugLogs) Debug.Log($"[HealthHammer] Damage Wave found {hits.Length} potential targets");

        int affectedEnemies = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                GameObject enemyObj = hit.gameObject;

                // Prevent double-hitting
                if (hitEnemies.Contains(enemyObj)) continue;
                hitEnemies.Add(enemyObj);

                // Deal Damage
                HealthSystem eHealth = hit.GetComponent<HealthSystem>();
                if (eHealth == null) eHealth = hit.GetComponentInParent<HealthSystem>();

                if (eHealth != null) 
                {
                    eHealth.TakeDamage(damage);
                    affectedEnemies++;
                    if (showDebugLogs) Debug.Log($"[HealthHammer] Wave damaged: {hit.name}");
                }

                // Apply Knockback
                Rigidbody2D eRb = hit.GetComponent<Rigidbody2D>();
                if (eRb == null) eRb = hit.GetComponentInParent<Rigidbody2D>();
                
                if (eRb != null)
                {
                    // Knockback in attack direction with slight upward force
                    Vector2 knockbackDir = new Vector2(currentWaveDirection, 0.3f).normalized;
                    eRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        if (showDebugLogs) Debug.Log($"[HealthHammer] Damage Wave affected {affectedEnemies} enemies");
    }

    void TriggerShockwave()
    {
        if (headTransform == null) return;

        Vector2 shockwaveCenter = headTransform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(shockwaveCenter, shockwaveRadius, enemyLayer);
        
        if (showDebugLogs) Debug.Log($"[HealthHammer] Radial Shockwave found {hits.Length} potential targets");

        int affectedEnemies = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                GameObject enemyObj = hit.gameObject;
                
                // Don't double-hit if already hit by wave
                if (hitEnemies.Contains(enemyObj)) continue;
                hitEnemies.Add(enemyObj);

                // Find HealthSystem
                HealthSystem eHealth = hit.GetComponent<HealthSystem>();
                if (eHealth == null) eHealth = hit.GetComponentInParent<HealthSystem>();

                if (eHealth != null) 
                {
                    eHealth.TakeDamage(shockwaveDamage);
                    affectedEnemies++;
                    if (showDebugLogs) Debug.Log($"[HealthHammer] Shockwave damaged: {hit.name}");
                }

                // Find Rigidbody2D
                Rigidbody2D eRb = hit.GetComponent<Rigidbody2D>();
                if (eRb == null) eRb = hit.GetComponentInParent<Rigidbody2D>();
                
                if (eRb != null)
                {
                    Vector2 dir = ((Vector2)hit.transform.position - shockwaveCenter).normalized;
                    dir.y = Mathf.Max(dir.y, 0.5f); // Ensure upward force
                    eRb.AddForce(dir * shockwaveKnockback, ForceMode2D.Impulse);
                }
            }
        }

        if (showDebugLogs) Debug.Log($"[HealthHammer] Radial Shockwave affected {affectedEnemies} enemies");
    }

    void ResetSwing()
    {
        isSwinging = false;
        if (hammerCollider != null) hammerCollider.enabled = false;
        transform.rotation = defaultRotation;
    }

    public void HandleCollision(Collider2D collision)
    {
        // Keep this for direct collider hits (backup system)
        if (!isSwinging) return;

        if (collision.CompareTag("Enemy"))
        {
            GameObject enemyObj = collision.gameObject;

            if (hitEnemies.Contains(enemyObj)) return;
            hitEnemies.Add(enemyObj);

            if (showDebugLogs) Debug.Log($"[HealthHammer] Direct collision with: {collision.name}");

            HealthSystem enemyHealth = collision.GetComponent<HealthSystem>();
            if (enemyHealth == null) enemyHealth = collision.GetComponentInParent<HealthSystem>();
            
            if (enemyHealth != null) 
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (headTransform != null)
        {
            // Draw radial shockwave radius (ground hit bonus)
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f); 
            Gizmos.DrawSphere(headTransform.position, shockwaveRadius);

            // Draw ground check radius
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawWireSphere(headTransform.position, 0.5f);
        }

        // Draw the damage wave box (only during play mode when wave is active)
        if (showWaveGizmos && Application.isPlaying && hasTriggeredWave)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(lastWaveCenter, lastWaveSize);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(lastWaveCenter, lastWaveSize);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Preview wave size in editor
        if (!Application.isPlaying && headTransform != null && transform != null)
        {
            Vector2 playerPos = transform.position;
            Vector2 hammerHeadPos = headTransform.position;
            float hammerReach = Vector2.Distance(playerPos, hammerHeadPos);
            float waveLength = hammerReach * waveLengthMultiplier;

            // Preview for right attack
            Vector2 previewCenter = playerPos + new Vector2(waveLength / 2f, 0);
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawCube(previewCenter, new Vector2(waveLength, waveWidth));

            // Preview for left attack
            Vector2 previewCenterLeft = playerPos + new Vector2(-waveLength / 2f, 0);
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube(previewCenterLeft, new Vector2(waveLength, waveWidth));
        }
    }
}