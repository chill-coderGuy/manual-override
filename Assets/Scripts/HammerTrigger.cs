using UnityEngine;
using UnityEngine.Events;

public class HammerTrigger : MonoBehaviour
{
    private HealthHammer parentScript;

    void Start()
    {
        // Finds the script on the grandparent/parent pivot
        parentScript = GetComponentInParent<HealthHammer>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (parentScript != null)
        {
            parentScript.HandleCollision(collision);
            Debug.Log("collision detected");
        }
    }
}