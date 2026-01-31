using UnityEngine;

public class HammerWorldDrag : MonoBehaviour
{
    public HammerSystemManager manager;
    private bool isDragging = false;

    // --- NEW: SAFETY SWITCH ---
    // When mouse touches the hammer, DISABLE the attack script
    void OnMouseEnter()
    {
        if (manager.worldScript != null)
        {
            manager.worldScript.enabled = false;
        }
    }

    // When mouse leaves the hammer, RE-ENABLE the attack script
    void OnMouseExit()
    {
        // Only re-enable if we are NOT currently dragging it
        if (!isDragging && manager.worldScript != null)
        {
            manager.worldScript.enabled = true;
        }
    }

    // --- EXISTING DRAG LOGIC ---

    void OnMouseDown()
    {
        isDragging = true;

        // 1. VISUAL SWAP
        manager.worldHammerPivot.SetActive(false); 
        manager.uiHammerObject.SetActive(true);    

        // 2. MOVE UI TO MOUSE
        manager.uiHammerObject.transform.position = Input.mousePosition;
        
        // 3. Make UI transparent
        var canvasGroup = manager.uiHammerObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            manager.uiHammerObject.transform.position = Input.mousePosition;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        
        var canvasGroup = manager.uiHammerObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        if (manager.IsMouseNearInventory())
        {
            // SNAP HOME
            manager.EquipToUI();
            
            // Make sure attack script stays disabled while in bag
            if (manager.worldScript != null) manager.worldScript.enabled = false;
        }
        else
        {
            // RE-EQUIP TO WORLD
            manager.EquipToWorld();
            
            // Re-enable attack script immediately so you can fight
            if (manager.worldScript != null) manager.worldScript.enabled = true;
        }
    }
}