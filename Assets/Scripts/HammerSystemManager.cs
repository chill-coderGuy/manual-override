using UnityEngine;
using UnityEngine.UI;

public class HammerSystemManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public GameObject uiHammerObject;    // The 'HeathUI' Group
    public GameObject worldHammerPivot;  
    public HealthHammer worldScript;     

    [Header("Inventory Settings")]
    public RectTransform inventoryAnchor; // Drag 'InventoryBox' here
    public float snapDistance = 100f;     // Range to snap back to box

    [Header("Equip Settings")]
    public float equipRange = 2.0f;       // Range to equip to player

    void Start()
    {
        EquipToUI(); // Start in bag
    }

    // --- STATE SWITCHING ---

    public void EquipToWorld()
    {
        uiHammerObject.SetActive(false);
        worldHammerPivot.SetActive(true);
        if (worldScript != null) worldScript.enabled = true;
    }

    public void EquipToUI()
    {
        uiHammerObject.SetActive(true);
        worldHammerPivot.SetActive(false);
        
        // SNAP TO HOME
        if (uiHammerObject != null)
        {
            uiHammerObject.transform.localPosition = Vector3.zero;
        }
    }

    // --- DISTANCE CHECKS ---

    // 1. Check if Mouse is near Player (For Equipping)
    public bool IsMouseNearPlayer()
    {
        if (playerTransform == null) return false;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; 

        return Vector2.Distance(mouseWorldPos, playerTransform.position) <= equipRange;
    }

    // 2. Check if Mouse is near Inventory Box (For Returning)
    public bool IsMouseNearInventory()
    {
        if (inventoryAnchor == null) return false;
        
        // UI uses Screen Space, so we compare mouse position directly
        return Vector2.Distance(Input.mousePosition, inventoryAnchor.position) < snapDistance;
    }

    // Visual Debugging
    void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, equipRange);
        }
    }
}