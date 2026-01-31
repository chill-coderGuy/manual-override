using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HammerUIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public HammerSystemManager manager;
    
    [Header("Settings")]
    public CanvasGroup groupCanvas; // Drag 'HeathUI' here

    // This remembers where the bar was before you dragged it
    private Vector3 startPosition;
    private Transform originalParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Remember starting spot
        startPosition = transform.localPosition;
        
        // 2. Make it transparent to clicks so we can drop it on the player
        if (groupCanvas != null) groupCanvas.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 3. MOVE THE ACTUAL OBJECT (Not a ghost!)
        // This moves the Parent, so the Icon AND Bar move together.
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 4. Turn clicks back on
        if (groupCanvas != null) groupCanvas.blocksRaycasts = true;

        // 5. Check if we dropped it on the Player
        if (manager.IsMouseNearPlayer())
        {
            // YES: Equip the weapon!
            manager.EquipToWorld();
        }
        else
        {
            // NO: Snap back to the inventory box
            transform.localPosition = startPosition;
            if (groupCanvas != null) groupCanvas.alpha = 1f;
        }
    }
}