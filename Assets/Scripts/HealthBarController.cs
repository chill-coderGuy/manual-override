using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("Connections")]
    public HealthSystem playerHealth; // Drag your Player object here
    public Image barImage;            // Drag the Green Bar image here

    void Update()
    {
        if (playerHealth != null && barImage != null)
        {
            // 1. Calculate percentage (0.0 to 1.0)
            float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
            
            // 2. Update the bar fill
            barImage.fillAmount = Mathf.Clamp01(healthPercent);
        }
    }
}