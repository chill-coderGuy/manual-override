using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("Connections")]
    public HealthSystem playerHealth; 
    public Image barImage;            

    void Update()
    {
        if (playerHealth != null && barImage != null)
        {
            float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
            barImage.fillAmount = Mathf.Clamp01(healthPercent);
        }
    }
}