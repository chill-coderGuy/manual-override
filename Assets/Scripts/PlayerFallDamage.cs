using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFallDamage : MonoBehaviour
{
    [Header("Links")]
        public HealthHammer hammerScript; 

    [Header("Death Settings")]
    public float deathFloorY = -10f;       
    public bool instantReload = true;      
    [Header("Fall Damage Settings")]
    public float safeFallDistance = 5f;
    public int damagePerUnit = 1;
    public LayerMask groundLayer;

    [Header("Health System")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Events")]
    public UnityEvent<int> OnTakeDamage;
    private Rigidbody2D rb;
    private float highestPoint;
    private bool wasAirborne = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        highestPoint = transform.position.y;
                if (hammerScript == null)
            hammerScript = GetComponentInChildren<HealthHammer>();

        UpdateHammerSize();
    }

    void Update()
    {
        if (transform.position.y < deathFloorY)
        {
            Die(); 
            return;
        }
        bool currentlyGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);

        if (!currentlyGrounded)
        {
            wasAirborne = true;
            if (transform.position.y > highestPoint) highestPoint = transform.position.y;
        }
        
        if (currentlyGrounded && wasAirborne)
        {
            CalculateFallDamage();
            wasAirborne = false;
            highestPoint = transform.position.y; 
        }

        if (currentlyGrounded) highestPoint = transform.position.y;
    }

    void CalculateFallDamage()
    {
        float fallDistance = highestPoint - transform.position.y;
        if (fallDistance > safeFallDistance)
        {
            int damage = Mathf.RoundToInt((fallDistance - safeFallDistance) * damagePerUnit);
            if (damage > 0) ApplyDamage(damage);
        }
    }

    public void ApplyDamage(int amount)
    {
        currentHealth -= amount;
                UpdateHammerSize(); 
                OnTakeDamage.Invoke(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHammerSize()
    {
        if (hammerScript == null) return;
        float healthPercent = Mathf.Clamp01((float)currentHealth / (float)maxHealth);
        float targetHeight = Mathf.Lerp(hammerScript.minHeight, hammerScript.maxHeight, healthPercent);
        hammerScript.SyncState(targetHeight);
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}