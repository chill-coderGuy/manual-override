using UnityEngine;
using UnityEngine.UI;

public class HammerSystemManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public GameObject uiHammerObject;   
    public GameObject worldHammerPivot;  
    public HealthHammer worldScript;     

    [Header("Inventory Settings")]
    public RectTransform inventoryAnchor; 
     public float snapDistance = 100f;     
    [Header("Equip Settings")]
    public float equipRange = 2.0f;       

    void Start()
    {
        EquipToUI(); 
    }
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
        if (uiHammerObject != null)
        {
            uiHammerObject.transform.localPosition = Vector3.zero;
        }
    }
    public bool IsMouseNearPlayer()
    {
        if (playerTransform == null) return false;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; 

        return Vector2.Distance(mouseWorldPos, playerTransform.position) <= equipRange;
    }
    public bool IsMouseNearInventory()
    {
        if (inventoryAnchor == null) return false;
        return Vector2.Distance(Input.mousePosition, inventoryAnchor.position) < snapDistance;
    }
}