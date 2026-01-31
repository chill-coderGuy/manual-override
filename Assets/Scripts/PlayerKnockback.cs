using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    private Rigidbody2D rb;
    // We will automatically try to find your movement script.
    // Ideally, drag your specific PlayerMovement script here in the Inspector.
    public MonoBehaviour playerMovementScript; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Try to auto-find movement script if you didn't assign it
        if (playerMovementScript == null)
        {
            // Assuming your movement script contains the word "Player" or "Movement"
            // If this fails, drag it in manually in the Inspector!
            playerMovementScript = GetComponent<PlayerController>(); 
            // Note: If your script is named something else (e.g. "Controller"), change the line above.
        }
    }

    public void ApplyKnockback(Vector2 forceVector)
    {
        // 1. Reset current momentum so the hit feels heavy
        rb.linearVelocity = Vector2.zero; 

        // 2. Apply the Force
        rb.AddForce(forceVector, ForceMode2D.Impulse);

        // 3. Disable Controls momentarily
        if (playerMovementScript != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisableControlsRoutine());
        }
    }

    IEnumerator DisableControlsRoutine()
    {
        playerMovementScript.enabled = false; // Turn off inputs
        yield return new WaitForSeconds(0.2f); // Wait for the "Stun"
        playerMovementScript.enabled = true;  // Give control back
    }
}