using UnityEngine;

public class HammerTrigger : MonoBehaviour
{
    private HealthHammer parentScript;

    void Start()
    {
        parentScript = GetComponentInParent<HealthHammer>();
        
        if (parentScript == null)
        {
            Debug.LogError("[HammerTrigger] No HealthHammer script found in parent!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (parentScript != null)
        {
            parentScript.HandleCollision(collision);
        }
    }

    // Optional: Keep collider enabled to show in Scene view
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}