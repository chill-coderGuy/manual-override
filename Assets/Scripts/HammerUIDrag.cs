using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HammerUIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public HammerSystemManager manager;
    
    [Header("Settings")]
    public CanvasGroup groupCanvas;
    private Vector3 startPosition;
    private Transform originalParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.localPosition;
        if (groupCanvas != null) groupCanvas.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (groupCanvas != null) groupCanvas.blocksRaycasts = true;
        if (manager.IsMouseNearPlayer())
        {
            manager.EquipToWorld();
        }
        else
        {
            transform.localPosition = startPosition;
            if (groupCanvas != null) groupCanvas.alpha = 1f;
        }
    }
}