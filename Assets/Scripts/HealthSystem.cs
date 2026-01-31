using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; // Drag the enemy sprite here
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;
    
    [Header("Events")]
    public UnityEvent onDeath; // Drag functions here (e.g., Play Sound, Spawn Particles)

    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // --- THE MAIN FUNCTION ---
    // The Hammer will call this function: object.GetComponent<HealthSystem>().TakeDamage(50);
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        
        // Visual Feedback (Flash Red)
        if (spriteRenderer != null) StartCoroutine(FlashColor());

        // Check for Death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        
        // Trigger any custom events (explosions, score add, etc.)
        onDeath.Invoke();

        // Default Death: Destroy object
        Destroy(gameObject);
    }

    // Simple coroutine to flash the sprite red for a split second
    System.Collections.IEnumerator FlashColor()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}