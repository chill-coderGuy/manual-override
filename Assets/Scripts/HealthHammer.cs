using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthHammer : MonoBehaviour
{
    [Header("References")]
    public Collider2D[] dragColliders; 
    public PlayerFallDamage playerHealthScript; 
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
    [Tooltip("Width of the damage wave")]
    public float waveWidth = 2.0f;
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
    private float currentWaveDirection = 1f;
    private Vector2 lastWaveCenter;
    private Vector2 lastWaveSize;
    public float minHeight => headHeightAtMin;
    public float maxHeight => headHeightAtMax;

    void Start()
    {
        defaultRotation = Quaternion.Euler(0, 0, 0); 
                if (playerHealthScript == null) 
        {
            playerHealthScript = GetComponentInParent<PlayerFallDamage>();
        }
        
        if (hammerCollider != null) hammerCollider.enabled = false;

        if (playerHealthScript != null)
        {
            float hpPercent = (float)playerHealthScript.currentHealth / (float)playerHealthScript.maxHealth;
            UpdateHammerScale(hpPercent);
        }
    }

    void Update()
    {
        if (isSwinging && Time.time - lastSwingTime > 1.0f)
        {
            if (showDebugLogs) Debug.LogWarning("[HealthHammer] Swing timeout - resetting");
            ResetSwing();
        }
        if (playerHealthScript != null && handleTransform != null && headTransform != null)
        {
            float healthPercent = (float)playerHealthScript.currentHealth / (float)playerHealthScript.maxHealth;
            UpdateHammerScale(Mathf.Clamp01(healthPercent));
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverHammer()) return; 

            if (!isSwinging)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float dirX = mousePos.x - transform.position.x;
                
                currentWaveDirection = dirX > 0 ? 1f : -1f;
                
                StartCoroutine(Smash(dirX > 0 ? -90f : 90f));
            }
        }
    }
    public void SyncState(float targetHeight)
    {
        if (handleTransform == null || headTransform == null) return;
        float healthPercent = Mathf.InverseLerp(headHeightAtMin, headHeightAtMax, targetHeight);
        UpdateHammerScale(Mathf.Clamp01(healthPercent));
    }
    void UpdateHammerScale(float healthPercent)
    {
        if (handleTransform == null || headTransform == null) return;
        float currentScale = Mathf.Lerp(handleScaleAtMin, handleScaleAtMax, healthPercent);
        Vector3 newScale = handleTransform.localScale;
        newScale.y = currentScale;
        newScale.x = 0.2f; 
        newScale.z = 1f;
        handleTransform.localScale = newScale;
        float currentHeadY = Mathf.Lerp(headHeightAtMin, headHeightAtMax, healthPercent);
        headTransform.localPosition = new Vector3(0, currentHeadY, 0);
                if (heartsize > 0)
            headTransform.localScale = Vector3.one / heartsize;
    }
    IEnumerator Smash(float targetAngle)
    {
        isSwinging = true;
        lastSwingTime = Time.time;
        hitEnemies.Clear();
        hasTriggeredWave = false;
        hasTriggeredShockwave = false;
        
        if (hammerCollider != null) hammerCollider.enabled = true;

        try 
        {
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.Euler(0, 0, targetAngle);
            float timer = 0f;
            while (timer < swingDuration)
            {
                timer += Time.deltaTime * swingSpeed;
                transform.rotation = Quaternion.Lerp(startRot, endRot, timer);
                yield return null;
            }
            if (!hasTriggeredWave)
            {
                hasTriggeredWave = true;
                TriggerDamageWave();
            }
            if (headTransform != null && !hasTriggeredShockwave)
            {
                Collider2D groundHit = Physics2D.OverlapCircle(headTransform.position, 0.5f, groundLayer);

                if (groundHit != null)
                {
                    hasTriggeredShockwave = true;
                    if (showDebugLogs) Debug.Log("[HealthHammer] BOOM! Ground Impact.");
                    TriggerShockwave();
                    if (playerHealthScript != null && selfDamageOnHit > 0) 
                    {
                        playerHealthScript.ApplyDamage((int)selfDamageOnHit);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
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
            ResetSwing();
        }
    }
    void TriggerDamageWave()
    {
        if (headTransform == null) return;

        Vector2 playerPos = transform.position;
        Vector2 hammerHeadPos = headTransform.position;
        float hammerReach = Vector2.Distance(playerPos, hammerHeadPos);

        float waveLength = hammerReach * waveLengthMultiplier;
        Vector2 waveStartPos = waveStartsFromPlayer ? playerPos : hammerHeadPos;
        Vector2 waveCenter = waveStartPos + new Vector2(currentWaveDirection * waveLength / 2f, 0);
        
        lastWaveCenter = waveCenter;
        lastWaveSize = new Vector2(waveLength, waveWidth);

        Collider2D[] hits = Physics2D.OverlapBoxAll(waveCenter, lastWaveSize, 0f, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            DamageEnemy(hit, damage);
        }
    }

    void TriggerShockwave()
    {
        if (headTransform == null) return;

        Vector2 shockwaveCenter = headTransform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(shockwaveCenter, shockwaveRadius, enemyLayer);
        
        foreach (Collider2D hit in hits)
        {
            DamageEnemy(hit, shockwaveDamage);
            Rigidbody2D eRb = hit.GetComponent<Rigidbody2D>();
            if (eRb == null) eRb = hit.GetComponentInParent<Rigidbody2D>();
            
            if (eRb != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - shockwaveCenter).normalized;
                dir.y = Mathf.Max(dir.y, 0.5f); // Always pop them up a bit
                eRb.AddForce(dir * shockwaveKnockback, ForceMode2D.Impulse);
            }
        }
    }
    void DamageEnemy(Collider2D hit, float dmgAmount)
    {
        if (hit == null) return;
        if (!hit.CompareTag("Enemy")) return;

        GameObject enemyObj = hit.gameObject;
        if (hitEnemies.Contains(enemyObj)) return;
        hitEnemies.Add(enemyObj);
        HealthSystem eHealth = hit.GetComponent<HealthSystem>();
        if (eHealth == null) eHealth = hit.GetComponentInParent<HealthSystem>();

        if (eHealth != null) 
        {
            eHealth.TakeDamage(dmgAmount);
            if (showDebugLogs) Debug.Log($"[HealthHammer] Hit {hit.name} for {dmgAmount}");
        }
    }

    void ResetSwing()
    {
        isSwinging = false;
        if (hammerCollider != null) hammerCollider.enabled = false;
        transform.rotation = defaultRotation;
    }

    bool IsMouseOverHammer()
    {
        if (dragColliders == null || dragColliders.Length == 0) return false;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        foreach (Collider2D col in dragColliders)
        {
            if (col != null && col.OverlapPoint(mousePos)) return true;
        }
        return false;
    }

    public void HandleCollision(Collider2D collision)
    {
        if (!isSwinging) return;
        DamageEnemy(collision, damage);
    }
    
    void OnDrawGizmos()
    {
        if (headTransform != null)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f); 
            Gizmos.DrawSphere(headTransform.position, shockwaveRadius);
        }

        if (showWaveGizmos && Application.isPlaying && hasTriggeredWave)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(lastWaveCenter, lastWaveSize);
            Gizmos.DrawWireCube(lastWaveCenter, lastWaveSize);
        }
    }
}