using UnityEngine;

public class HammerWorldDrag : MonoBehaviour
{
    public HammerSystemManager manager;
    private bool isDragging = false;

    void OnMouseEnter()
    {
        if (manager.worldScript != null)
        {
            manager.worldScript.enabled = false;
        }
    }
    void OnMouseExit()
    {
        if (!isDragging && manager.worldScript != null)
        {
            manager.worldScript.enabled = true;
        }
    }
    void OnMouseDown()
    {
        isDragging = true;

        manager.worldHammerPivot.SetActive(false); 
        manager.uiHammerObject.SetActive(true);    
        manager.uiHammerObject.transform.position = Input.mousePosition;
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
            manager.EquipToUI();
            if (manager.worldScript != null) manager.worldScript.enabled = false;
        }
        else
        {
            manager.EquipToWorld();
            if (manager.worldScript != null) manager.worldScript.enabled = true;
        }
    }
}