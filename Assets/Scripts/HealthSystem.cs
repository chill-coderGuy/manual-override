using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; 

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Death Settings")]
    public float deathFloorY = -10f; 
    public bool reloadSceneOnDeath = true;

    [Header("Hammer Link")]
    public HealthHammer hammerScript;

    [Header("Fall Damage")]
    public float safeFallDistance = 5f;
    public float damagePerUnit = 10f; 
    public LayerMask groundLayer;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; 
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;
    
    [Header("Events")]
    public UnityEvent onDeath; 
    private Color originalColor;
    private float highestPoint;
    private bool wasAirborne = false;
    private Rigidbody2D rb;

    void Start()
    {
        Time.timeScale = 1f; 
        currentHealth = maxHealth;
        highestPoint = transform.position.y;
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (hammerScript == null) hammerScript = GetComponentInChildren<HealthHammer>();
        UpdateHammerSize();
    }

    void Update()
    {
        if (transform.position.y < deathFloorY)
        {
            Die();
            return;
        }
        HandleFallDamage();
    }
    void HandleFallDamage()
    {
        bool currentlyGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        if (!currentlyGrounded)
        {
            wasAirborne = true;
            if (transform.position.y > highestPoint) 
            {
                highestPoint = transform.position.y;
            }
        }
        if (currentlyGrounded && wasAirborne)
        {
            float fallDistance = highestPoint - transform.position.y;

            if (fallDistance > safeFallDistance)
            {
                float excessFall = fallDistance - safeFallDistance;
                float damage = excessFall * damagePerUnit;
                
                if (damage > 0)
                {
                    Debug.Log($"⬇️ Fall Damage: {damage}");
                    TakeDamage(damage);
                }
            }
            wasAirborne = false;
            highestPoint = transform.position.y; 
        }
        if (currentlyGrounded)
        {
            highestPoint = transform.position.y;
        }
    }
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
                if (spriteRenderer != null) StartCoroutine(FlashColor());
        UpdateHammerSize();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHammerSize()
    {
        if (hammerScript != null)
        {
            float hpPercent = Mathf.Clamp01(currentHealth / maxHealth);
            float targetSize = Mathf.Lerp(hammerScript.minHeight, hammerScript.maxHeight, hpPercent);
            hammerScript.SyncState(targetSize);
        }
    }

    void Die()
    {
        Debug.Log($"💀 Death Triggered! Health: {currentHealth}, Y Pos: {transform.position.y}");
        onDeath.Invoke(); 
        if (reloadSceneOnDeath)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator FlashColor()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}