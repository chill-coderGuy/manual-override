using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    private Rigidbody2D rb;
    public MonoBehaviour playerMovementScript; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (playerMovementScript == null)
        {
            playerMovementScript = GetComponent<PlayerController>(); 
        }
    }

    public void ApplyKnockback(Vector2 forceVector)
    {
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(forceVector, ForceMode2D.Impulse);
        if (playerMovementScript != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisableControlsRoutine());
        }
    }

    IEnumerator DisableControlsRoutine()
    {
        playerMovementScript.enabled = false; 
        yield return new WaitForSeconds(0.2f); 
        playerMovementScript.enabled = true;  
    }
}