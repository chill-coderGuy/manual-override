using UnityEngine;
using UnityEngine.Audio;

public class InventoryVolumeControl : MonoBehaviour
{
    [Header("Links")]
    public GameObject gameBarObject; 
    public Transform siblingBar;     
    public Transform siblingHandle;  
    public AudioMixer masterMixer;

    [Header("Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;
    public float sensitivity = 0.2f; 
    public float gameThresholdY = -3f;
    public float handleVisualOffset = 0.0f; 

    private bool isDragging = false;
    private bool isResizing = false;
    private Vector3 dragOffset;
    private float currentHeight = 1.0f;
    private BoxCollider2D barCollider; 

    // --- FORCE RESET ON WAKE UP ---
    // Replaces Start() to ensure it works every time you swap back
    void OnEnable()
    {
        // 1. Reset Interaction Flags (Fixes "Second Freeze")
        isDragging = false;
        isResizing = false;

        // 2. Sync Math to Actual Visuals (Fixes "The Pop")
        if (siblingBar != null)
        {
            // Get the collider if we haven't already
            if (barCollider == null) barCollider = siblingBar.GetComponent<BoxCollider2D>();
            
            // Read the ACTUAL height so we don't snap back to 1.0
            currentHeight = siblingBar.localScale.y;
            
            // Force Bar Collider On
            if (barCollider != null) barCollider.enabled = true;
        }

        // 3. Force Handle Collider On
        if (siblingHandle != null)
        {
            var col = siblingHandle.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        // 4. Update Position Immediately
        UpdateVisuals(currentHeight);
    }

    public void SyncState(float syncedHeight)
    {
        currentHeight = syncedHeight;
        UpdateVisuals(currentHeight);
    }

    void Update()
    {
        // FIX 1: Lock Z to 0 so the object never drifts away from the Raycast
        if (transform.position.z != 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // --- INPUTS ---
        // RIGHT CLICK: Dial Volume (Handle Only)
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == siblingHandle)
            {
                isResizing = true;
            }
        }
        if (Input.GetMouseButtonUp(1)) isResizing = false;

        // LEFT CLICK: Drag Tool (Handle Only)
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == siblingHandle)
            {
                isDragging = true;
                dragOffset = transform.position - mousePos;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (transform.position.y > gameThresholdY) SwapToGameMode();
        }

        // --- EXECUTION ---
        if (isResizing)
        {
            float delta = Input.GetAxis("Mouse Y") * sensitivity;
            currentHeight += delta;
            currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
            
            UpdateVisuals(currentHeight);
            ApplyAudio(currentHeight);
        }
        else if (isDragging)
        {
            transform.position = mousePos + dragOffset;
            // FIX: Ensure visuals stay synced while dragging to avoid jitter
            UpdateVisuals(currentHeight); 
        }
    }

    void UpdateVisuals(float h)
    {
        if (siblingBar == null || siblingHandle == null) return;

        // 1. Scale the Bar
        siblingBar.localScale = new Vector3(siblingBar.localScale.x, h, 1);

        // 2. Find the TRUE top of the bar using Physics Bounds
        float topY = 0f;
        
        if (barCollider != null)
        {
            // Forces the physics system to update the bounds immediately after scaling
            Physics2D.SyncTransforms(); 
            topY = barCollider.bounds.max.y;
        }
        else
        {
            // Fallback if collider is missing
            topY = siblingBar.position.y + (h / 2); 
        }

        // 3. Move Handle
        siblingHandle.position = new Vector3(siblingBar.position.x, topY + handleVisualOffset, 0);
    }

    void ApplyAudio(float h)
    {
        if (masterMixer == null) return;
        float t = Mathf.InverseLerp(minHeight, maxHeight, h);
        masterMixer.SetFloat("MusicVol", Mathf.Lerp(-40f, 0f, t));
        masterMixer.SetFloat("MusicPitch", Mathf.Lerp(0.7f, 1.0f, t));
    }

    void SwapToGameMode()
    {
        if(gameBarObject != null)
        {
             gameBarObject.transform.position = transform.position;
             // Ensure this name matches your Game script filename!
             gameBarObject.GetComponent<GameVolumeControl>().SyncState(currentHeight);
             gameBarObject.SetActive(true);
             gameObject.SetActive(false);
        }
    }
}