using UnityEngine;
using UnityEngine.Audio;

public class GameVolumeControl : MonoBehaviour
{
    [Header("Links")]
    public GameObject inventoryBarObject; 
    public Transform siblingBar;          
    public Transform siblingHandle;       
    public AudioMixer masterMixer;

    [Header("Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;
    public float gameThresholdY = -3f;
    public float handleVisualOffset = 0.0f; 

    private bool isDragging = false;
    private bool isResizing = false;
    
    // VARIABLES FOR SMOOTH MOVEMENT
    private Vector3 dragOffset;       // For moving the whole tool
    private float initialMouseY;      // Where was mouse when we clicked resize?
    private float initialHeight;      // How tall was bar when we clicked resize?
    
    private float currentHeight = 1.0f;
    private BoxCollider2D barCollider; 
    private SpriteRenderer barSprite;

    void OnEnable()
    {
        isDragging = false;
        isResizing = false;

        if (siblingBar != null)
        {
            if (barCollider == null) barCollider = siblingBar.GetComponent<BoxCollider2D>();
            barSprite = siblingBar.GetComponent<SpriteRenderer>();

            // Init Height
            if (barSprite != null && barSprite.drawMode == SpriteDrawMode.Sliced)
                currentHeight = barSprite.size.y;
            else
                currentHeight = siblingBar.localScale.y;

            if (barCollider != null) barCollider.enabled = true;
        }

        if (siblingHandle != null)
        {
            var col = siblingHandle.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        UpdateVisuals(currentHeight);
    }

    public void SyncState(float syncedHeight)
    {
        currentHeight = syncedHeight;
        UpdateVisuals(currentHeight);
    }

    void Update()
    {
        if (transform.position.z != 0) 
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // --- INPUT ---
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            bool foundHandle = false;
            bool foundBar = false;

            foreach (var hit in hits)
            {
                if (hit.collider.transform == siblingHandle) foundHandle = true;
                if (hit.collider.transform == siblingBar || hit.collider.gameObject == gameObject) foundBar = true;
            }

            if (foundHandle) 
            {
                isResizing = true;
                // SMOOTH FIX 1: Record the starting state
                initialMouseY = mousePos.y;
                initialHeight = currentHeight;
            }
            else if (foundBar)
            {
                isDragging = true;
                dragOffset = transform.position - mousePos;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isResizing = false;
            if (transform.position.y < gameThresholdY) SwapToInventoryMode();
        }

        // --- EXECUTION ---
        if (isDragging)
        {
            transform.position = mousePos + dragOffset;
            UpdateVisuals(currentHeight);
        }
        else if (isResizing)
        {
            // SMOOTH FIX 2: Calculate Delta (Movement) instead of Absolute Position
            float mouseDelta = mousePos.y - initialMouseY;
            float newH = initialHeight + mouseDelta;

            currentHeight = Mathf.Clamp(newH, minHeight, maxHeight);
            UpdateVisuals(currentHeight);
            ApplyAudioEffects(currentHeight);
        }
    }

    void UpdateVisuals(float h)
    {
        if (siblingBar == null || siblingHandle == null) return;

        // 1. Scale / Size
        if (barSprite != null && barSprite.drawMode == SpriteDrawMode.Sliced)
        {
            barSprite.size = new Vector2(barSprite.size.x, h);
            siblingBar.localScale = Vector3.one; 
        }
        else
        {
            siblingBar.localScale = new Vector3(0.33f, h, 1f);
        }

        Physics2D.SyncTransforms(); 

        // 2. Position Handle
        float topY = 0f;
        if(barCollider != null) topY = barCollider.bounds.max.y;
        else topY = siblingBar.position.y + h; 

        siblingHandle.position = new Vector3(siblingBar.position.x, topY + handleVisualOffset, 0);
    }

    void ApplyAudioEffects(float h)
    {
        if (masterMixer == null) return;
        float t = Mathf.InverseLerp(minHeight, maxHeight, h);
        masterMixer.SetFloat("MusicPitch", Mathf.Lerp(0.6f, 1.0f, t));
        masterMixer.SetFloat("MusicVol", Mathf.Lerp(-40f, 0f, t));
    }

    void SwapToInventoryMode()
    {
        if (inventoryBarObject != null)
        {
            inventoryBarObject.transform.position = transform.position;
            inventoryBarObject.GetComponent<InventoryVolumeControl>().SyncState(currentHeight);
            inventoryBarObject.SetActive(true);
            gameObject.SetActive(false);
            

        }
    }
}