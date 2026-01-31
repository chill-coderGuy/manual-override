using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthHammer : MonoBehaviour
{
    [Header("References")]
    public HealthSystem playerHealth; 
    public Transform handleTransform; 
    public Transform headTransform;   
    public Collider2D hammerCollider; 

    [Header("Calibration")]
    public float handleScaleAtMin = 0.5f; 
    public float handleScaleAtMax = 3.0f; 
    public float headHeightAtMin = 0.5f;
    public float headHeightAtMax = 3.0f;

    [Header("Shockwave Settings")]
    public float shockwaveRadius = 3.0f; 
    public float shockwaveDamage = 10f;    
    public float shockwaveKnockback = 10f; 
    public LayerMask enemyLayer; 

    [Header("Combat Stats")]
    public float damage = 25f; 
    public float knockbackForce = 15f;
    public float selfDamageOnHit = 0f; 
    public float swingSpeed = 10f; 
    public float swingDuration = 0.2f;

    private bool isSwinging = false;
    private float lastSwingTime = 0f;
    private Quaternion defaultRotation;
    private List<GameObject> hitEnemies = new List<GameObject>();

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
            isSwinging = false;
            if (hammerCollider != null) hammerCollider.enabled = false;
            transform.rotation = defaultRotation;
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
            newScale.x = 0.2f; newScale.z = 1f;
            handleTransform.localScale = newScale;

            // Head Position (Force Scale 1 to prevent distortion)
            float currentHeadY = Mathf.Lerp(headHeightAtMin, headHeightAtMax, healthPercent);
            headTransform.localPosition = new Vector3(0, currentHeadY, 0);
            headTransform.localScale = Vector3.one; 
        }

        // 3. INPUT
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float dirX = mousePos.x - transform.position.x;
            StartCoroutine(Smash(dirX > 0 ? -90f : 90f));
        }
    }

    IEnumerator Smash(float targetAngle)
    {
        isSwinging = true;
        lastSwingTime = Time.time;
        hitEnemies.Clear(); 
        
        if (hammerCollider != null) hammerCollider.enabled = true; 

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

            // B. IMPACT
            TriggerShockwave();
            if (playerHealth != null) playerHealth.TakeDamage(selfDamageOnHit); 

            yield return new WaitForSeconds(0.1f);

            // C. RETURN UP
            if (hammerCollider != null) hammerCollider.enabled = false; 
            
            timer = 0f;
            while (timer < swingDuration)
            {
                timer += Time.deltaTime * (swingSpeed * 0.5f); 
                transform.rotation = Quaternion.Lerp(endRot, defaultRotation, timer);
                yield return null;
            }
            transform.rotation = defaultRotation;
        }
        finally
        {
            isSwinging = false; 
            transform.rotation = defaultRotation; 
        }
    }

    void TriggerShockwave()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(headTransform.position, shockwaveRadius, enemyLayer);
        Debug.Log($"💥 Shockwave Hits: {hits.Length}");

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                // --- THE FIX: SEARCH PARENTS FOR HEALTH ---
                HealthSystem eHealth = hit.GetComponent<HealthSystem>();
                if (eHealth == null) eHealth = hit.GetComponentInParent<HealthSystem>();

                if (eHealth != null) 
                {
                    eHealth.TakeDamage(shockwaveDamage);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Hit enemy '{hit.name}' but could NOT find HealthSystem script!");
                }

                // Knockback
                Rigidbody2D eRb = hit.GetComponent<Rigidbody2D>();
                if (eRb == null) eRb = hit.GetComponentInParent<Rigidbody2D>();
                
                if (eRb != null)
                {
                    Vector2 dir = (hit.transform.position - headTransform.position).normalized;
                    dir.y = 1f; 
                    eRb.AddForce(dir * shockwaveKnockback, ForceMode2D.Impulse);
                }
            }
        }
    }

    // Direct Hit Logic (Also needs the Parent fix)
    public void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !hitEnemies.Contains(collision.gameObject))
        {
            hitEnemies.Add(collision.gameObject); 
            
            HealthSystem enemyHealth = collision.GetComponent<HealthSystem>();
            if (enemyHealth == null) enemyHealth = collision.GetComponentInParent<HealthSystem>();
            
            if (enemyHealth != null) enemyHealth.TakeDamage(damage);
        }
    }
    
    void OnDrawGizmos()
    {
        if (headTransform != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); 
            Gizmos.DrawSphere(headTransform.position, shockwaveRadius);
        }
    }
}