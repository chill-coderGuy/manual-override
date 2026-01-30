using UnityEngine;

public class UniversalLogicLeak : MonoBehaviour
{
    [Header("State Detection")]
    public float interfaceYThreshold = -5f; // Boundary between Menu and Game World
    public bool isInGame = false;

    [Header("Internal Interaction")]
    public Transform internalHandle; // e.g., the Triangle or the Cog center

    private bool isDraggingObject = false;
    private bool isInteractingInternal = false;
    private Vector3 dragOffset;

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 1. STATE CHECK: Detect if the object has been "Leaked" into the game
        isInGame = transform.position.y > interfaceYThreshold;

        // 2. INPUT HANDLING
        HandleMouseInput(mousePos);

        // 3. EXECUTION
        if (isDraggingObject)
        {
            transform.position = mousePos + dragOffset;
        }
        
        // This is where you would hook in your specific mechanics (Volume, Gravity, etc.)
        if (isInteractingInternal)
        {
            ProcessInternalLogic(mousePos);
        }
    }

    private void HandleMouseInput(Vector3 mousePos)
    {
        // LEFT CLICK: Drag the whole physical object
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDraggingObject = true;
                dragOffset = transform.position - mousePos;
            }
        }
        if (Input.GetMouseButtonUp(0)) isDraggingObject = false;

        // RIGHT CLICK: Interact with the UI "Handle" inside the object
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == internalHandle)
            {
                isInteractingInternal = true;
            }
        }
        if (Input.GetMouseButtonUp(1)) isInteractingInternal = false;
    }

    private void ProcessInternalLogic(Vector3 mousePos)
    {
        // Move the internal handle relative to the parent object
        // This keeps the interaction "local" to the tool itself
        float localY = transform.InverseTransformPoint(mousePos).y;
        internalHandle.localPosition = new Vector3(0, localY, 0);
    }
}